# Change Log

All notable changes to this project will be documented in this file. See [versionize](https://github.com/versionize/versionize) for commit guidelines.

<a name="0.1.0"></a>
## [0.1.0](https://www.github.com/musabadru/WinChecker/releases/tag/v0.1.0) (2026-03-11)

### Features

* add back navigation and restructure app detail page ([659e9d4](https://www.github.com/musabadru/WinChecker/commit/659e9d445403be7e17ce1c066c2b386d45eec3f0))
* fix DllResolver architecture handling and add more tests ([02e36e6](https://www.github.com/musabadru/WinChecker/commit/02e36e6c484fe3267f720dfcff01d1fb0d365ea7))
* implement AppListViewModel and DllResolver unit tests ([9a1c358](https://www.github.com/musabadru/WinChecker/commit/9a1c358f2705927bacc145722de5c9529142811d))
* implement Deep Inspection with AppDetailPage and technical metadata parsing ([2a4b522](https://www.github.com/musabadru/WinChecker/commit/2a4b522231c1840eef713a368fabc3e13b10e1c3))
* implement DLL resolution logic and update roadmap ([6ba872e](https://www.github.com/musabadru/WinChecker/commit/6ba872e74577a03ecc070e4aec617a1127f19491))
* implement main page UI following DESIGN_GUIDE.md ([52c30a2](https://www.github.com/musabadru/WinChecker/commit/52c30a2ceaea31198ef2644e1aaf5963f8b60b7b))
* implement PE parsing foundation ([7b2f63f](https://www.github.com/musabadru/WinChecker/commit/7b2f63f6fc4605ac645f4c0ebb97fe3054408777))
* implement prioritized icon retrieval strategy and caching ([4b850b1](https://www.github.com/musabadru/WinChecker/commit/4b850b1e580e4c445b17afcf4393c7cf69b567b9))
* implement unified AppScannerService ([c7d261b](https://www.github.com/musabadru/WinChecker/commit/c7d261bb261f52df01d7f09768d0b8d9525df54d))
* implement UWP app enumeration ([4db3920](https://www.github.com/musabadru/WinChecker/commit/4db3920e3c62329773579353d2ffe47317f3a04c))
* implement version info extraction placeholder ([e1a5359](https://www.github.com/musabadru/WinChecker/commit/e1a5359d10a9842e7e287f84477fd647d8193d96))
* implement Win32 app enumeration and data repository ([e1889ce](https://www.github.com/musabadru/WinChecker/commit/e1889ce2079755cb9abf034637acbeb0b9b9c1e1))
* improve UI/UX following winui3 guide with NavigationView and animations ([e2dd679](https://www.github.com/musabadru/WinChecker/commit/e2dd67975857dec88210350ca782479c8a4fa13f))
* Initial commit ([9b6802d](https://www.github.com/musabadru/WinChecker/commit/9b6802d29543c6ce59a3f959004436b4662b2b6c))
* integrate header into title bar and use Mica backdrop ([60c5ce3](https://www.github.com/musabadru/WinChecker/commit/60c5ce3606594020e8b6a7c8f228fc686f27f74d))

### Bug Fixes

* correct release workflow — replace missing action with gh cli, fix version capture ([1aac259](https://www.github.com/musabadru/WinChecker/commit/1aac259ddaca281b2fcfbbbb4bff8d65e913fbab))
* implement robust error logging and safer window initialization ([24abc51](https://www.github.com/musabadru/WinChecker/commit/24abc51fa9e344f1870c36a4ddbd85c81b17abea))
* remove packaged launch profile as app is unpackaged ([ba90c1b](https://www.github.com/musabadru/WinChecker/commit/ba90c1b95f427f6fc1c3f0f222dfb16ac16611c1))
* resolve all code review issues — parser, logging, streaming, SQLite cache, tests, and startup lifecycle ([d6c5591](https://www.github.com/musabadru/WinChecker/commit/d6c5591776744e4a3fa649e4b112253c4c465fcb))
* resolve blank window, icon crashes, and add Serilog + scan optimizations ([c3a389b](https://www.github.com/musabadru/WinChecker/commit/c3a389bd18cf6b60c6ec08931c3831d00ac953a2))
* resolve XamlParseException and restore modern header design ([77192e5](https://www.github.com/musabadru/WinChecker/commit/77192e5d2c18c2d3d5ec0c6f378eb4c102bf691f))
* simplify release workflow — global versionize, workflow_dispatch, fix publish glob ([cfc7a96](https://www.github.com/musabadru/WinChecker/commit/cfc7a96cd797d61217215dd376aa921c33c43695))
* simplify startup and DI to diagnose crash ([dc052be](https://www.github.com/musabadru/WinChecker/commit/dc052be51fffbc3aee2efad9fc51742c115fab62))
* use versionize 2.x subcommand syntax (release --changelog-all) ([af861fa](https://www.github.com/musabadru/WinChecker/commit/af861fa54edeba78b1a5b82ddae6e71877834333))

