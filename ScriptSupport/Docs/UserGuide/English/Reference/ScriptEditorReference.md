# Script Editor

## 1. Overview

The Script Editor provides an environment for viewing and editing Lua scripts used by the YGO OCG-Core.

In addition to basic text editing, it provides code assistance features that help users work with Lua scripts and access Scrapi documentation while writing code.

The Script Editor includes the following features:

- Lua Syntax Highlighting
- Auto Completion
- Hover Documentation
- Tab Management

## 2. Working with Script Files

Script Support allows users to open and edit Lua script files within the application.

### 2.1. Opening Script Files

Users can open existing script files through the **Open** command in the Menu.

Opened files are listed in the **Open** tab of the Navigation Panel.

Selecting a file from this list displays its content in the Script Editor.

### 2.2. Creating Script Files

Users can create new text files through the **New** command in the Menu.

Newly created files are listed in the **New** tab of the Navigation Panel and can be edited in the Script Editor.

### 2.3. Editing Script Files

The Script Editor provides a text editing environment for Lua scripts.

Users can modify script content directly in the editor. However, saving changes to the original file requires the Allow Save option to be enabled.

When **Allow Save** is disabled, users cannot save modified content to the original file.

This restriction helps protect existing game scripts from accidental modifications while allowing users to inspect and work with their contents.

For more information, see [Configuration](../Settings/Configuration.md).

## 3. Code Editor

### 3.1. Syntax Highlighting

The Script Editor provides syntax highlighting for Lua code.

Different syntax elements are displayed using distinct visual styles, making scripts easier to read and understand.

### 3.2. Auto Completion

Auto Completion provides suggestions while users write Lua scripts.

The completion system supports several categories of symbols, including:

- Functions
- Constants
- Enums and Enum Members
- Namespaces
- Tags
- Types
- Lua Keywords
- Variables

Users can invoke the completion list manually using **Ctrl + Space**.

The completion list can also appear automatically while typing.

The editor supports context-aware completion after the member access operator (`.`), allowing users to explore available members of recognized namespaces or expressions.

Pressing **Esc** dismisses the completion list.

### 3.3. Hover Documentation

Hover Documentation allows users to inspect Scrapi information directly from the Script Editor.

When the mouse pointer is placed over a recognized symbol or expression, the editor displays a documentation popup containing the corresponding information.

This allows users to consult Scrapi documentation without switching to the Scrapi Books section.

## 4. Editor Tabs

The Script Editor uses a tab-based interface to manage multiple open files.

### 4.1. Temporary and Permanent Tabs

The Editor supports two types of Tabs:

**Temporary Tab**

- Allows users to quickly preview the contents of multiple files without manually closing Tabs.
- The Tab title is always positioned at the far right of the Tab Control.
- The Tab title is displayed in *italic* text.
- Only one Temporary Tab can exist at a time.
- When another Script Card is opened, the current Temporary Tab is replaced with the newly selected Script Card.

**Permanent Tab**

- The Tab remains open for long-term viewing.
- The Tab title is positioned on the left side of the Tab Control.
- The Tab title is displayed in regular (non-italic) text.
- Multiple Permanent Tabs can exist at the same time.
- The Tab remains open when the user selects another Script Card.
- Its contents are not replaced by newly opened Script Cards.
- The Tab remains open until the user explicitly closes it.

If no Temporary Tab is currently open, the next Item opened with a Single Click will create a new Temporary Tab.

When another Tab is opened, the current Temporary Tab, if any, will be replaced. Permanent Tabs remain unaffected.

#### Converting a Temporary Tab into a Permanent Tab

A Temporary Tab can be converted into a Permanent Tab in either of the following ways:

1. **Double-click the Tab title.**
2. **Edit the displayed content.** Any modification to the content will automatically convert the Temporary Tab into a Permanent Tab.

Once converted, the Tab remains open and will no longer be replaced when the user selects another Script Card.

### 4.2. Tab Management

The Script Editor supports tab rearrangement through drag-and-drop operations.

Users can also tear off tabs and merge them into other tab groups, allowing the editor layout to be organized according to their workflow.

---

Previous:

[← Scrapyard](../Reference/ScrapiReference.md)

[← Getting Started](../GettingStarted.md)
