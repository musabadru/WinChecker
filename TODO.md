# TODO

Granular task list for active development. Tracks Phase 1 in detail; later phases are tracked at a higher level until they become active.

---

## Setup & Scaffolding

- [x] Create solution: `WinChecker.sln`
- [x] Add projects:
  - [x] `WinChecker.App` — WinUI 3 application
  - [x] `WinChecker.Core` — shared models, interfaces, utilities
  - [x] `WinChecker.PE` — PE parsing wrapper
  - [x] `WinChecker.Enumeration` — app discovery
  - [x] `WinChecker.Detection` — framework detection
  - [x] `WinChecker.Data` — SQLite, repositories, snapshot engine
- [x] Add NuGet packages:
  - [x] `AsmResolver.PE`
  - [x] `dnlib`
  - [x] `Microsoft.Windows.CsWin32`
  - [x] `Microsoft.Data.Sqlite`
  - [x] `Dapper`
  - [x] `LiveChartsCore.SkiaSharpView.WinUI`
  - [x] `CommunityToolkit.WinUI`
  - [x] `CommunityToolkit.Mvvm`
- [ ] Configure `NativeAOT` or self-contained publish profile
- [x] Set up `.editorconfig` and code style rules
- [x] Initialize SQLite schema migrations (versioned SQL files)
- [x] Wire up basic dependency injection (Microsoft.Extensions.DI)
- [x] Set up Versionize and Conventional Commits

---

## App Enumeration (`WinChecker.Enumeration`)

- [x] `Win32AppEnumerator`: scan `HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall`
- [x] `Win32AppEnumerator`: scan `HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall`
- [x] `Win32AppEnumerator`: scan 32-bit hive (`HKLM\Software\WOW6432Node\...`)
- [x] Extract fields: DisplayName, DisplayVersion, Publisher, InstallDate, InstallLocation, EstimatedSize
- [x] `UwpAppEnumerator`: use `PackageManager` API to list MSIX/UWP packages
- [x] Extract UWP fields: PackageFamilyName, version, install location, logo asset path
- [ ] `PortableAppEnumerator` (stretch for Phase 1): scan user-configured folders for `.exe` files
- [x] Merge all sources into unified `InstalledApp` model
- [ ] Resolve app icon: extract from `.exe` resource or UWP asset
- [ ] Cache enumeration results to SQLite on first run; detect stale entries on subsequent runs

---

## PE Parsing (`WinChecker.PE`)

- [x] `PeParser` service wrapping AsmResolver
- [x] Read machine architecture (`IMAGE_FILE_MACHINE_*`) → map to `Architecture` enum (x86/x64/ARM/ARM64)
- [x] Read PE subsystem (Console / Windows GUI / etc.)
- [x] Read linker version
- [x] Read compile timestamp from PE header
- [x] Read imported DLL names (`ImportDirectory`)
- [ ] Resolve each imported DLL to a full filesystem path (see resolution logic below)
- [x] Read exported function names (`ExportDirectory`)
- [x] Read embedded manifest (`RT_MANIFEST` resource)
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

- [x] SQLite schema v1:
  - [x] `apps` table (id, name, version, publisher, architecture, install_date, install_path, source)
  - [x] `dependencies` table (app_id, dll_name, resolved_path, is_missing)
  - [x] `frameworks` table (id, name, version, description, rules_ref)
  - [x] `app_frameworks` join table
  - [x] `snapshots` table (id, taken_at, label)
  - [x] `snapshot_apps` (snapshot_id, app_id, state_json)
- [x] `AppRepository`: CRUD for apps + dependencies
- [ ] `SnapshotRepository`: save/load/list/delete snapshots
- [x] SQLite FTS5 virtual table over `apps` (name, publisher)
- [x] Schema migration runner (sequential SQL files, version tracked in `pragma user_version`)

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
