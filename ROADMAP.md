# Roadmap

WinChecker is built in focused phases. Each phase ships something independently useful before the next begins.

---

## Phase 1 — Core Loop (v0.1)
> Goal: A useful app browser with PE inspection. Shippable as a dev tool.

- [x] Enumerate all installed Win32 apps from registry (`HKLM/HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall`)
- [x] Enumerate UWP/MSIX packages via AppX APIs
- [x] Parse PE headers for each app binary (architecture, subsystem, linker version) using AsmResolver
- [ ] Show app list with icon, name, version, publisher, architecture badge
- [ ] Basic app detail page: Info tab + Dependencies tab (raw DLL list)
- [x] SQLite database bootstrapped (app index, schema v1)
- [ ] Resolve DLL paths using Windows loader rules (KnownDLLs, PATH, SxS)
- [ ] Flag missing/unresolvable dependencies
- [ ] Architecture distribution pie chart (LiveChartsCore)
- [ ] Search by app name (SQLite FTS5)

**Milestone:** Can browse all apps, inspect their PE headers, and see raw DLL imports.

---

## Phase 2 — Framework Detection (v0.2)
> Goal: Tag apps with their runtimes and frameworks. The "LibChecker feel" starts here.

- [ ] Integrate Detect-It-Easy (DIE) CLI shim for initial framework/packer detection
- [ ] Detect .NET version (Framework vs Core vs 5+ via assembly metadata via dnlib)
- [ ] Detect VC++ Redistributable dependency (by MSVCR/MSVCP DLL names + version)
- [ ] Detect Electron / CEF (chromium DLL fingerprints + `resources/` folder heuristic)
- [ ] Detect Qt (Qt5Core.dll / Qt6Core.dll presence + version)
- [ ] Detect embedded Python / Node.js runtimes
- [ ] Detect DirectX / Vulkan / OpenGL usage
- [ ] Rules bundle v1: SQLite DB with framework fingerprint rules (JSON source, imported at startup)
- [ ] Framework tags shown on app list row and detail page Features tab
- [ ] Library reference stats: count apps per framework, ranked bar chart

**Milestone:** Every app in the list has framework badges. Stats page shows top runtimes system-wide.

---

## Phase 3 — Deep Inspection (v0.3)
> Goal: Full PE inspection — imports, exports, resources, signing.

- [ ] Imports/Exports tab: full function list per DLL (AsmResolver)
- [ ] Resources tab: embedded icons, manifests, version info extraction
- [ ] Code signing status via `WinVerifyTrust` (CsWin32)
- [ ] Installer type detection: MSI, MSIX, NSIS, Inno Setup, Squirrel (DIE + heuristics)
- [ ] Self-contained vs framework-dependent .NET detection
- [ ] UWP capability declarations from `AppxManifest.xml`
- [ ] Packed/obfuscated binary detection (DIE signatures)
- [ ] PE header detail panel: compile timestamp, checksum, section table

**Milestone:** Full detail view for any app or DLL. Comparable to CFF Explorer but with a modern UI.

---

## Phase 4 — Snapshot & Diff (v0.4)
> Goal: Track what changes on your system over time.

- [ ] Snapshot engine: serialize full app+library state to SQLite
- [ ] Manual snapshot trigger from UI
- [ ] Auto-snapshot on app launch (configurable)
- [ ] Snapshot list/history view
- [ ] Diff engine: compare two snapshots, compute added/removed/changed
- [ ] Diff view: color-coded list of changes (green add, red remove, yellow modified)
- [ ] Auto-prune old snapshots on schedule (configurable retention policy)
- [ ] Export diff as Markdown or JSON report

**Milestone:** Can tell you exactly what an installer changed on your system.

---

## Phase 5 — Search & Sources (v0.5)
> Goal: Power-user search and install provenance.

- [ ] Installation source detection: winget, Chocolatey, Microsoft Store, manual
- [ ] Link to store page / winget entry from app detail
- [ ] ripgrep integration for filesystem-level binary search
- [ ] Advanced filter panel: architecture, framework, signing status, install source
- [ ] Pin / bookmark apps for quick access
- [ ] Global search across apps and libraries (FTS5 full index)

**Milestone:** Full filtering and search across the entire app ecosystem.

---

## Phase 6 — Own the Detection Engine (v1.0)
> Goal: Replace external shims with a first-party, community-extensible rules engine.

- [ ] Custom detection rules engine (replaces DIE CLI shim)
- [ ] Rules authoring format: YAML/JSON with regex, file presence, and import-match conditions
- [ ] Community rules bundle repository (separate repo, auto-imported by the app)
- [ ] Rules versioning and update mechanism (check for bundle updates on launch)
- [ ] Well-known library tags: name, version description, links (like LibChecker's `lc-rules-bundle`)
- [ ] Performance: background scanning with progress, cancellation, caching
- [ ] Settings page: scan depth, auto-snapshot, snapshot retention, rules update channel

**Milestone:** v1.0 release. Self-contained, community-extensible, no external CLI deps.

---

## Future / Unscheduled

- **Network dependency detection** — flag apps phoning home at startup
- **Side-by-side (SxS) visual tree** — render the full WinSxS resolution chain
- **API set resolution** — trace `api-ms-win-*` virtual DLLs to their actual host
- **Plugin system** — let third-party rules engines plug into detection
- **Dark/light theme + accent colors** — full WinUI theming
- **Localization** — at least English + Chinese (nod to LibChecker's community)
- **CLI mode** — `winchecker inspect myapp.exe --json` for scripting use