# Architecture

## Overview

WinChecker is a layered .NET 9 desktop application built on WinUI 3. The architecture separates concerns into discrete projects within a single solution: UI, domain logic, PE inspection, app enumeration, detection, and data persistence. Layers only communicate inward — the UI knows about Core, but Core does not know about the UI.

```
┌──────────────────────────────────────────────┐
│              WinChecker.App (WinUI 3)         │  ← Presentation
│   Pages, ViewModels, Controls, Navigation    │
└────────────────────┬─────────────────────────┘
                     │ uses
┌────────────────────▼─────────────────────────┐
│              WinChecker.Core                  │  ← Domain
│   Models, Interfaces, Service contracts      │
└──┬──────────────┬──────────────┬─────────────┘
   │              │              │
   ▼              ▼              ▼
WinChecker   WinChecker    WinChecker
   .PE       .Enumeration  .Detection
(PE parsing) (app discovery) (framework tags)
   │              │              │
   └──────────────┴──────────────┘
                  │ all persist via
         ┌────────▼────────┐
         │ WinChecker.Data │  ← Persistence
         │ SQLite + Dapper │
         └─────────────────┘
```

---

## Projects

### `WinChecker.Core`
The shared kernel. Contains:
- **Models**: `InstalledApp`, `Dependency`, `FrameworkTag`, `PeInfo`, `Snapshot`, `SnapshotDiff`
- **Interfaces**: `IAppEnumerator`, `IPeParser`, `IDetectionEngine`, `ISnapshotService`, `IAppRepository`
- **Enums**: `Architecture`, `InstallerType`, `InstallSource`, `SigningStatus`
- No external NuGet dependencies beyond Microsoft.Extensions abstractions

All other projects reference Core; Core references nothing else.

---

### `WinChecker.PE`
Wraps AsmResolver and dnlib for PE inspection.

**Key services:**
- `PeParser` — reads headers, imports, exports, resources, version info
- `DllResolver` — resolves DLL names to full paths using Windows loader rules
- `ManifestReader` — extracts and parses `RT_MANIFEST` from PE resources
- `DotNetMetadataReader` (dnlib) — reads `TargetFrameworkAttribute`, assembly references, .NET version

**DLL resolution order** (mirrors Windows loader):
1. KnownDLLs registry key
2. Same directory as the executable
3. `System32` / `SysWOW64`
4. `PATH` environment variable entries
5. SxS manifest `dependentAssembly` entries
6. API set virtual DLL recognition (`api-ms-win-*`)

Missing after all steps → flagged as `ResolutionStatus.Missing`.

---

### `WinChecker.Enumeration`
Discovers installed applications from all Windows sources.

**Enumerators:**
- `RegistryAppEnumerator` — scans Win32 uninstall keys in HKLM and HKCU (both 64-bit and WOW6432Node hives) via CsWin32
- `UwpAppEnumerator` — uses `Windows.Management.Deployment.PackageManager` AppX API
- `PortableAppEnumerator` — user-configured folder paths, discovers `.exe` files

All enumerators implement `IAppEnumerator` and produce `InstalledApp` records. A top-level `AppEnumerationService` merges all sources, deduplicates, and caches results to SQLite.

**Icon extraction:**
- Win32: extract `RT_ICON` / `RT_GROUP_ICON` from PE resource section
- UWP: locate `Square44x44Logo` or `StoreLogo` asset from the package manifest

---

### `WinChecker.Detection`
Identifies which frameworks and runtimes an app depends on.

**Detection pipeline** (runs in order, results merged):
1. `DllNameDetector` — matches imported DLL names against a rules table (e.g. `mscoree.dll` → `.NET Framework`, `Qt6Core.dll` → `Qt 6`)
2. `FilePresenceDetector` — checks for marker files alongside the binary (e.g. `electron.exe`, `python3X.dll`)
3. `ManifestDetector` — inspects PE manifest or UWP `AppxManifest.xml` for runtime declarations
4. `DotNetVersionDetector` — uses dnlib to read target framework moniker and assembly metadata
5. `DieCliShim` *(Phase 1–5 only)* — shells out to Detect-It-Easy for packer/compiler/installer detection, parses JSON output

Each detector implements `IFrameworkDetector` and returns `DetectionResult[]`. The engine aggregates results, deduplicates, and resolves conflicts by confidence score.

**Rules Bundle:**
Stored in SQLite (`framework_rules` table). Sourced from a community JSON file shipped with the app and imported at startup. Schema:

```
framework_rules (
  id, name, version_pattern, evidence_type,
  evidence_value, confidence, description, url
)
```

`evidence_type` is one of: `dll_name`, `dll_prefix`, `file_presence`, `manifest_attribute`, `pe_section_name`.

---

### `WinChecker.Data`
All persistence. SQLite via `Microsoft.Data.Sqlite` + `Dapper`.

**Schema (v1):**

```sql
CREATE TABLE apps (
  id INTEGER PRIMARY KEY,
  name TEXT NOT NULL,
  version TEXT,
  publisher TEXT,
  architecture INTEGER,  -- enum value
  install_date TEXT,
  install_path TEXT,
  install_source INTEGER, -- enum value
  source_detail TEXT      -- winget id, store urn, etc.
);

CREATE TABLE dependencies (
  id INTEGER PRIMARY KEY,
  app_id INTEGER REFERENCES apps(id),
  dll_name TEXT NOT NULL,
  resolved_path TEXT,
  is_missing INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE framework_tags (
  id INTEGER PRIMARY KEY,
  app_id INTEGER REFERENCES apps(id),
  framework_name TEXT NOT NULL,
  framework_version TEXT,
  confidence REAL
);

CREATE TABLE snapshots (
  id INTEGER PRIMARY KEY,
  label TEXT,
  taken_at TEXT NOT NULL
);

CREATE TABLE snapshot_entries (
  id INTEGER PRIMARY KEY,
  snapshot_id INTEGER REFERENCES snapshots(id),
  app_id INTEGER,
  state_json TEXT NOT NULL  -- serialized InstalledApp at snapshot time
);

CREATE VIRTUAL TABLE apps_fts USING fts5(
  name, publisher, content='apps', content_rowid='id'
);
```

**Migrations:** Sequential `.sql` files numbered `001_initial.sql`, `002_add_fts.sql`, etc. Version tracked in `PRAGMA user_version`. Migration runner applies missing migrations on startup.

**Repositories:**
- `AppRepository` — CRUD + FTS search
- `DependencyRepository`
- `FrameworkTagRepository`
- `SnapshotRepository` — save, list, load, diff, delete, prune

---

### `WinChecker.App`
WinUI 3 presentation layer. MVVM via CommunityToolkit.Mvvm.

**Pages and ViewModels:**

| Page | ViewModel | Notes |
|---|---|---|
| `AppListPage` | `AppListViewModel` | Paginated list, search, filter, sort |
| `AppDetailPage` | `AppDetailViewModel` | Tabbed detail, receives app id |
| `StatsPage` | `StatsViewModel` | Charts: arch distribution, top frameworks |
| `SnapshotsPage` | `SnapshotsViewModel` | List + trigger + diff view |
| `SettingsPage` | `SettingsViewModel` | Scan options, snapshot retention, theme |

**Navigation:** `NavigationView` shell with frame-based navigation. Routes are typed (`typeof(AppDetailPage)` + parameter).

**Background scanning:** Enumeration and detection run on a background thread via `Task.Run`. Progress reported to UI via `IProgress<ScanProgress>`. Results streamed to the list as they arrive (observable collection incrementally populated).

---

## Data Flow: App Scan

```
User launches app
       │
       ▼
AppEnumerationService.EnumerateAllAsync()
  ├── RegistryAppEnumerator
  ├── UwpAppEnumerator
  └── PortableAppEnumerator
       │
       ▼
 For each InstalledApp:
   PeParser.Parse(app.ExecutablePath)
       → architecture, imports, exports, resources
   DllResolver.Resolve(imports)
       → resolved paths, missing flags
   DetectionEngine.Detect(app, peInfo)
       → FrameworkTag[]
       │
       ▼
 AppRepository.Upsert(app, dependencies, tags)
       │
       ▼
 AppListViewModel receives updates
 → UI renders incrementally
```

---

## External Dependencies & Exit Strategy

| Dependency | Used For | Exit Strategy |
|---|---|---|
| `AsmResolver` | PE parsing | Keep long-term; replace only if edge cases demand it |
| `dnlib` | .NET metadata | Keep; too specialized to replace cheaply |
| `CsWin32` | Win32 P/Invoke | Keep permanently |
| `Dapper` | SQL mapping | Keep; thin enough to not matter |
| `LiveChartsCore` | Charts | Keep |
| `CommunityToolkit` | MVVM + UI | Keep |
| `Detect-It-Easy` (CLI shim) | Packer/framework detection | Replace in Phase 6 with native rules engine |
| `ripgrep` (CLI shim) | File search | Replace with own indexed search if needed post-v1 |

---

## Key Design Decisions

**Why not WPF?** WinUI 3 is the forward-looking stack for Windows native apps. Better Fluent design support, better accessibility, better performance on ARM64. The ecosystem immaturity is offset by CommunityToolkit.

**Why SQLite and not a flat JSON cache?** Snapshots, diffing, FTS search, and framework rules all benefit from relational structure. SQLite is a single file with zero deployment overhead.

**Why shell out to DIE instead of embedding it?** DIE's detection database is community-maintained and updated independently. Shelling out means rules updates don't require recompiling WinChecker. The cost is subprocess overhead — acceptable for background scanning, replaced in v1.0.

**Why a rules-based detection engine over pure heuristics?** Heuristics embedded in code are hard to update and impossible for the community to contribute to. A rules bundle in SQLite means detection logic can be shipped as a data update independently of the app binary — same model LibChecker uses with `lc-rules-bundle`.