# ScriptSupport
ScriptSupport is a scripting support tool for Yu-Gi-Oh! game engines that use CDB databases and Lua scripts, such as OCG-Core-based engines.
## Features
- Card information lookup
- Card script lookup
- Scrapiyard lookup (constants, functions, enums, terminology, etc - OCG-Core)
- Scrapiyard symbol search by name and description
- IDE-like Lua script editor support
  - Auto Suggestion
  - Auto Completion
  - Parameter Information
  - Hover Documentation
  - ...
---
## Getting Started
#### Requirements
- Windows 10/11
- .NET 8 SDK
- Visual Studio 2022 (recommended)
#### Clone Repository
```bash
git clone https://github.com/TriDungSongToan/ScriptSupport.git
cd ScriptSupport
```
## Build

```bash
dotnet restore
dotnet build CardEditorX.Git.sln
```
## Run
Run in standalone mode:

```bash
dotnet run --project ScriptSupport/ScriptSupport.csproj
```
Run in embedded mode:
```bash
dotnet run --project ScriptSupport/ScriptSupport.csproj -- --embedded
```

## Basic Usage
### Configure Data Source
Before using the application, configure a valid data source directory:
```text
Setting -> Configuration -> User Setting
```
The selected directory should contain:
- Card database files (`*.cdb`)
- Card Script files (`c<ID>.lua`)
- Card Image files (`*.png` or `*.jpg`)
### Language Support
Available built-in languages:
- English
- Vietnamese
- Japanese

Custom languages can also be added manually by copying:
```text
<App-Folder>/data/CardData/Language/English
```
Rename the folder and translate the contents inside.
### Search
######  Card Search
- Use the **Search Card** panel in the **Card** tab to search for Card Name/Card Desc.
- For advanced card filtering options, click the **Advanced** button (right arrow icon).
######  Script Search
- Use the **Search Script** panel in the **Script** tab to search for Lua card scripts.
######  Scrapiyard Search
-  **Search Scrapi Name** to search symbol names
-  **Search Scrapi Desc** to search symbol descriptions
> Supported symbol types include:
  >  -  constants
  >  - functions
  > - enums
  > - terminology
  > - and other scrapiyard metadata
###  Automatic Script Search
If the following option is enabled:

```text
Setting -> Configuration -> DataHandling -> Auto Search
```
Selecting a scrapiyard symbol automatically searches related scripts.
### Opening Scripts

When selecting a card result from the **Card** or **Script** tab:

- **Single Click** → Open script in preview mode
- **Double Click** → Open script in permanent mode

---
### Runtime Data
On startup, the application uses runtime folders next to the executable:
- config/
- data/
- data/CardData/
- HighLight/
- ErrorLog.txt

If required data folders are missing, the application can clone or update configured data repositories.

---
---

# Technology
## Stack
- .NET 8
- WPF
- MVVM Architecture
- Dependency Injection
- AvalonEdit
- AvalonDock
- MaterialDesignInXaml
- HandyControl
- SQLite
## Architecture
ScriptSupport follows a modular WPF MVVM architecture built on .NET 8 and dependency injection.
Main architectural areas include:
- Services
- Stores / State Containers
- MVVM ViewModels
- AvalonEdit-based editor infrastructure
- Scrapiyard symbol integration
- Runtime configuration and localization systems
## Solution Structure
```text
├── ScriptSupport/          # Main WPF application
├── Scrapiyard.Core/        # Symbol models and metadata utilities
├── Scrapiyard.Converter/   # YAML-to-JSON converter
├── Character.Core/         # Character-related models/services
└── Character.UI/           # Character-related UI
```

## Third-Party Libraries
- AvalonEdit
- AvalonDock
- MaterialDesignInXaml
- HandyControl
- SDL.MultiSelectComboBox

Some controls and resource dictionaries have been customized to better fit the application's workflow and UI requirements.
