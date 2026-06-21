# CardEditor Ecosystem

A modular WPF-based solution for Yu-Gi-Oh! card data management, scripting, and tooling.
This repository contains multiple interconnected projects forming the CardEditor ecosystem.

## Projects

### CardEditor

Main desktop application for card data management, editing, and tooling.

See full documentation: 📄 [CardEditor README](./CardEditor/README.md)

### ScriptSupport

Embedded scripting support module used by CardEditor for Lua scripting, scrapiyard lookup, and IDE-like features.

See full documentation: 📄 [ScriptSupport README](./ScriptSupport/README.md)

## Solution Structure

```text
CardEditor.sln
│
├── CardEditor/              # Main WPF Application
├── ScriptSupport/          # Embedded scripting module
├── Scrapiyard.Core/
├── Scrapiyard.Converter/
├── Character.Core/
└── Character.UI/
```

## Overview
- CardEditor acts as the main application shell
- ScriptSupport is integrated as an embedded feature module
- Shared libraries provide core data and scripting infrastructure
- Designed for extensibility and modular tooling

## Getting Started

Open CardEditor.sln in Visual Studio 2019/2022 and build the solution.

For module-specific setup, refer to each project’s documentation:

📄 CardEditor setup → 📄 [CardEditor README](./CardEditor/README.md)

📄 ScriptSupport setup → 📄 [ScriptSupport README](./ScriptSupport/README.md)
