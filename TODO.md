# TODO

Granular task list for active development. Tracks Phase 1 in detail; later phases are tracked at a higher level until they become active.

---

## Setup & Scaffolding

- [ ] Create solution: `WinChecker.sln`
- [ ] Add projects:
  - [ ] `WinChecker.App` — WinUI 3 application
  - [ ] `WinChecker.Core` — shared models, interfaces, utilities
  - [ ] `WinChecker.PE` — PE parsing wrapper
  - [ ] `WinChecker.Enumeration` — app discovery
  - [ ] `WinChecker.Detection` — framework detection
  - [ ] `WinChecker.Data` — SQLite, repositories, snapshot engine
- [ ] Add NuGet packages:
  - [ ] `AsmResolver.PE`
  - [ ] `dnlib`
  - [ ] `Microsoft.Windows.CsWin32`
  - [ ] `Microsoft.Data.Sqlite`
  - [ ] `Dapper`
  - [ ] `LiveChartsCore.SkiaSharpView.WinUI`
  - [ ] `CommunityToolkit.WinUI`
  - [ ] `CommunityToolkit.Mvvm`
- [ ] Configure `NativeAOT` or self-contained publish profile
- [ ] Set up `.editorconfig` and code style rules
- [ ] Initialize SQLite schema migrations (versioned SQL files)
- [ ] Wire up basic dependency injection (Microsoft.Extensions.DI)

---

## App Enumeration (`WinChecker.Enumeration`)

- [ ] `Win32AppEnumerator`: scan `HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall`
- [ ] `Win32AppEnumerator`: scan `HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall`
- [ ] `Win32AppEnumerator`: scan 32-bit hive (`HKLM\Software\WOW6432Node\...`)
- [ ] Extract fields: DisplayName, DisplayVersion, Publisher, InstallDate, InstallLocation, EstimatedSize
- [ ] `UwpAppEnumerator`: use `PackageManager` API to list MSIX/UWP packages
- [ ] Extract UWP fields: PackageFamilyName, version, install location, logo asset path
- [ ] `PortableAppEnumerator` (stretch for Phase 1): scan user-configured folders for `.exe` files
- [ ] Merge all sources into unified `InstalledApp` model
- [ ] Resolve app icon: extract from `.exe` resource or UWP asset
- [ ] Cache enumeration results to SQLite on first run; detect stale entries on subsequent runs

---

## PE Parsing (`WinChecker.PE`)

- [ ] `PeParser` service wrapping AsmResolver
- [ ] Read machine architecture (`IMAGE_FILE_MACHINE_*`) → map to `Architecture` enum (x86/x64/ARM/ARM64)
- [ ] Read PE subsystem (Console / Windows GUI / etc.)
- [ ] Read linker version
- [ ] Read compile timestamp from PE header
- [ ] Read imported DLL names (`ImportDirectory`)
- [ ] Resolve each imported DLL to a full filesystem path (see resolution logic below)
- [ ] Read exported function names (`ExportDirectory`)
- [ ] Read embedded manifest (`RT_MANIFEST` resource)
- [ ] Read version info resource (`VS_VERSIONINFO`)
- [ ] Handle graceful failure: corrupted PE, access denied, packed binary

### DLL Resolution Logic
- [ ] Check `KnownDLLs` registry key
- [ ] Check same directory as the executable (DLL search order step 1)
- [ ] Check `System32` / `SysWOW64`
- [ ] Walk `PATH` environment variable
- [ ] Parse app manifest for `dependentAssembly` (SxS)
- [ ] Identify `api-ms-win-*` virtual DLLs (mark as "API Set — system resolved")
- [ ] Flag as `Missing` if not found after all steps

---

## Data Layer (`WinChecker.Data`)

- [ ] SQLite schema v1:
  - [ ] `apps` table (id, name, version, publisher, architecture, install_date, install_path, source)
  - [ ] `dependencies` table (app_id, dll_name, resolved_path, is_missing)
  - [ ] `frameworks` table (id, name, version, description, rules_ref)
  - [ ] `app_frameworks` join table
  - [ ] `snapshots` table (id, taken_at, label)
  - [ ] `snapshot_apps` (snapshot_id, app_id, state_json)
- [ ] `AppRepository`: CRUD for apps + dependencies
- [ ] `SnapshotRepository`: save/load/list/delete snapshots
- [ ] SQLite FTS5 virtual table over `apps` (name, publisher)
- [ ] Schema migration runner (sequential SQL files, version tracked in `pragma user_version`)

---

## Detection Engine (`WinChecker.Detection`)

- [ ] `DetectionResult` model: framework name, version (nullable), confidence, evidence
- [ ] `IDllNameDetector`: detect framework from imported DLL names (e.g. `mscoree.dll` → .NET Framework)
- [ ] `IFilePresenceDetector`: detect framework from files present alongside the binary
- [ ] `DieCliShim`: shell out to `die.exe` / `diec.exe`, parse JSON output
- [ ] Map DIE results to internal `FrameworkTag` records
- [ ] .NET version detector (via dnlib: read `TargetFrameworkAttribute` or `<TargetFramework>`)
- [ ] VC++ runtime detector (MSVCR/MSVCP DLL name + version suffix)
- [ ] Electron detector: `electron.exe` in same dir, or `ELECTRON_RUN_AS_NODE` in resources
- [ ] Qt detector: `Qt5Core.dll` / `Qt6Core.dll` presence + version from PE version resource

---

## UI (`WinChecker.App`)

### App List Page
- [ ] `AppListViewModel`: exposes observable collection of apps, search query, sort/filter state
- [ ] `AppListPage`: ListView/ItemsRepeater showing app rows
- [ ] App row: icon, name, publisher, version, architecture badge, framework tags
- [ ] Search box bound to FTS5 query
- [ ] Sort dropdown (name / size / install date / architecture)
- [ ] Filter flyout (architecture checkboxes, framework multi-select)
- [ ] Loading state with progress ring during initial scan
- [ ] Empty state view

### App Detail Page
- [ ] Navigation from list row → detail page (pass app id)
- [ ] **Info tab**: name, version, publisher, install path, size, install date, source badge
- [ ] **Dependencies tab**: TreeView of DLLs; resolved path; missing badge in red
- [ ] **Imports/Exports tab**: ListView of functions grouped by DLL
- [ ] **Features tab**: framework tag chips, architecture, signing status, installer type
- [ ] **Resources tab**: manifest text view, version info key-value table, icon preview

### Stats Page
- [ ] Architecture distribution pie chart (LiveChartsCore)
- [ ] Top frameworks bar chart (count of apps per framework)
- [ ] Total app count, total dependency count, missing dependency count

### Shell / Navigation
- [ ] `NavigationView` shell with pages: Apps, Stats, Snapshots, Settings
- [ ] Title bar customization (WinUI 3 custom title bar)
- [ ] Window size/position persistence

---

## Phase 2–6 (High-Level, Not Yet Granular)

### Phase 2 — Framework Detection
- [ ] Complete detection rules for all frameworks listed in README
- [ ] Rules bundle SQLite import pipeline
- [ ] Library stats page

### Phase 3 — Deep Inspection
- [ ] Code signing via WinVerifyTrust
- [ ] Installer type detection
- [ ] UWP capability declarations

### Phase 4 — Snapshots & Diff
- [ ] Snapshot engine
- [ ] Diff computation and UI

### Phase 5 — Search & Sources
- [ ] Install source detection (winget, Chocolatey, Store)
- [ ] ripgrep integration
- [ ] Advanced filter panel

### Phase 6 — Own the Rules Engine
- [ ] Custom detection rules format and engine
- [ ] Community rules bundle repo
- [ ] Rules update mechanism