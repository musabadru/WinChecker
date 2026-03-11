# WinChecker

> A modern Windows application inspector — browse installed apps, analyze PE binaries, detect frameworks and runtimes, and track dependency changes over time.

Inspired by [LibChecker](https://github.com/LibChecker/LibChecker) on Android, WinChecker brings the same philosophy to Windows: a clean, fast, native UI for understanding what's actually running on your machine.

---

## Features

### App Browsing
- Lists all installed applications: Win32, UWP/MSIX, and portable `.exe`s
- Shows icon, name, version, publisher, install date, and install location
- Search and filter by name, architecture, framework, or install source
- Sort by name, size, install date, or architecture

### Architecture & Binary Info
- Detects CPU architecture per binary: x86, x64, ARM, ARM64
- Reads PE header details: subsystem, linker version, compile timestamp
- Architecture distribution chart across all installed apps

### Dependency Inspection
- Lists all imported DLLs for a selected `.exe` or `.dll`
- Resolves full paths using Windows loader rules (PATH, SxS, API sets)
- Flags missing or unresolvable dependencies
- Shows exported functions per DLL

### Framework & Runtime Detection
Detects which runtimes an app relies on:
- .NET Framework / .NET Core / .NET 5+
- Visual C++ Redistributable (2015, 2019, 2022)
- DirectX / Direct3D, OpenGL, Vulkan
- Windows App SDK / WinUI
- Electron / CEF (Chromium Embedded Framework)
- Qt / wxWidgets
- Java / JRE launcher detection
- Embedded Python / Node.js runtimes

### Library Tags
- Tags recognized libraries with name, version, and description
- Links to official docs or GitHub page per library
- Community-maintainable rules bundle (SQLite-based)

### Snapshot & Diff
- Takes a snapshot of all installed apps and their libraries
- Compares two snapshots to detect changes after installs or updates
- Shows added, removed, and changed libraries in a clean diff view

### Package Characteristics
- Code signing status (signed / unsigned / expired)
- Installer type detection: MSI, MSIX, NSIS, Inno Setup, Squirrel
- Self-contained vs framework-dependent deployment
- Packed/obfuscated binary detection (via DIE)
- UWP capability declarations

### Installation Source Tracking
- Identifies install origin: Microsoft Store, winget, Chocolatey, or manual
- Links to store page or package manager entry where available

---

## Tech Stack

| Layer | Technology |
|---|---|
| UI | WinUI 3 + CommunityToolkit.WinUI |
| Architecture pattern | MVVM (CommunityToolkit.Mvvm) |
| Language | C# / .NET 9 |
| PE Parsing | AsmResolver + dnlib |
| Win32 Interop | CsWin32 |
| App Enumeration | Win32 Registry + WMI + AppX APIs |
| Framework Detection | Detect-It-Easy (CLI shim → custom rules) |
| Database | SQLite (Microsoft.Data.Sqlite + Dapper) |
| Search | ripgrep (file search) + SQLite FTS5 (app index) |
| Charts | LiveChartsCore (SkiaSharp/WinUI backend) |

---

## Getting Started

### Requirements
- Windows 10 22H2 or Windows 11
- .NET 10 Runtime
- Visual Studio 2026 with WinUI/WinAppSDK workload

### Build

```bash
git clone https://github.com/musabadru/winchecker
cd winchecker
dotnet restore
dotnet build
```

### Run

```bash
dotnet run --project src/WinChecker.App
```

---

## Project Structure

```
winchecker/
├── src/
│   ├── WinChecker.App/          # WinUI 3 frontend
│   ├── WinChecker.Core/         # Business logic, models
│   ├── WinChecker.PE/           # PE parsing layer
│   ├── WinChecker.Enumeration/  # App discovery (registry, WMI, AppX)
│   ├── WinChecker.Detection/    # Framework/runtime detection engine
│   └── WinChecker.Data/         # SQLite, rules bundle, snapshots
├── rules/                        # Community rules bundle (JSON → SQLite)
├── tests/
└── docs/
```

---

## Inspiration & Prior Art

- [LibChecker](https://github.com/LibChecker/LibChecker) — the Android original
- [Dependencies](https://github.com/lucasg/Dependencies) — open-source WPF dependency walker; reference for PE resolution logic
- [Detect-It-Easy](https://github.com/horsicq/Detect-It-Easy) — packer/compiler/framework detection signatures
- [CFF Explorer](https://ntcore.com/explorer-suite/) — classic PE editor

---

## License

MIT
