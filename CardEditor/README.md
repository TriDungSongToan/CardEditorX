# CardEditorX
CardEditor is a WPF desktop application built on .NET Framework 4.8.1, for managing, editing, importing, and exporting data, as well as scripting and development support for Yu-Gi-Oh! Card Game systems that use CDB databases and Lua scripts, such as OCG-Core-based engines.

## Core Features
CardEditor is composed of several functional modules:

### 1. DataEditor
A full database management system built on SQLite.
 - Manage structured card databases (SQLite-based)
 - Data realization from Database
 - Multi-level Search / Filter / Sort system
 - Automatic card image generation

### 2. CodeEditor
 A custom-built code editor (I tried to build it based on the Visual Studio interface)

 - Syntax Highlighting
 - Auto Suggestion / Auto Completion
 - Code Minimap
 - Change Tracking Margin
 - Parameter Info / Tooltips
 - Code Folding
 - Split View editor
 - Highly optimized AvalonEdit-based rendering

### 3. BanlistEditor
Yes, it’s just a text file display interface

### 4. DeckEditor
A complete deck construction system for Yu-Gi-Oh!  game engines.
 - Intuitive deck building interface
 - Categorizing and grouping of cards
 - Supports multi-level searching, filtering, and sorting, etc.
 - Supports drag-and-drop functionality
 - Integrated with card databases
 - Extended support for Rarity and Genesys Point systems


### 5. Rarity Manager
A tool for managing card rarity and image rarity label.
### 6. Genesys Manager
A tool for managing card genesys point

### 7. ItemEditor
Batch processing tool for card datasets.

- Replace text in Card.Desc across selected cards
- Field replacement using external database source
- Import card data from other databases
- Bulk card manipulation utilities

---

And tons of other features too.

Swear to God, I can't remember what I wrote in this mess.

---

## Getting Started
### Requirements
- Windows 10/11
- Visual Studio 2019 / 2022 (recommended)
- .NET Framework 4.8.1 Developer Pack
- Git

### Clone Repository
```bash
git clone https://github.com/TriDungSongToan/CardEditorX.git
cd CardEditorX
```

## Build
Open CardEditorX.sln in Visual Studio and:
 - Restore NuGet packages
 - Build solution (Debug/Release)

Or using MSBuild:

```bash
msbuild CardEditorX.sln /p:Configuration=Release
```

## Run
Run directly from Visual Studio:
- Set CardEditor as Startup Project
- Press `F5`

Or run executable:
```bash
bin\Release\CardEditor.exe
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