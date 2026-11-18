# Getting Started

This guide walks you through setting up Script Support for the first time and performing a basic workflow with the Card Data Lookup Library.

If you are looking for project build instructions, see the [README](../../../README.md).

## Prerequisites

Before starting, make sure you have:

- **OS:** Windows 7, 10 or 11
- **Runtime:** .NET 8
- **Internet connection:** Required for downloading the required data packages during initial setup

## Download Data Package

When ScriptSupport is launched for the first time, it will need to download and initialize the necessary data packages.

Make sure you have an active Internet connection and allow the initialization process to complete before using the application.

Do not close the application while the initial data pack download is in progress.

Once the required packages have been installed, ScriptSupport is ready for configuration.

---

## I. Initial Setup

After the required data packages have been downloaded, open **Settings** → **Configuration** to complete the initial application setup.

Complete the initial application configuration before using the available features.

For a detailed description of all available settings, see [Configuration](./Settings/Configuration.md).

## II. Interface Overview

Script Support provides several panels for browsing Card information, exploring Scrapi documentation, and working with Lua scripts.

Before proceeding, familiarize yourself with the main application interface.

For a detailed explanation of each panel and its functionality, see [Interface Overview](./Interface/InterfaceOverview.md).

## III. First Steps

After completing the initial setup, you can start exploring the main features of Script Support.

### 1. Search for a Card

You can search for Cards using their descriptions or associated Lua scripts.

To search for a Card by its description:

1. Open the **Card** tab in the Navigation Panel.
2. Enter a search term in the **Search Card Desc** box.
3. Select a matching result to view its Card Description.

The selected Card's description is displayed in **Desc Result**. If multiple Card Text entries are available for the same Card ID, you can use the ComboBox to switch between them.

To search for a Card Script:

1. Open the **Script** tab in the Navigation Panel.
2. Enter a search term in the **Search Script** box.
3. Select a matching result to open the corresponding script.

You can also select a Card to view its associated Card Scripts in **Script Result**, if available.

For more information about Card Data, Card Text, Card Scripts, and search options, see [Card](./Reference/CardReference.md).

### 2. Explore Scrapi

Scrapi Books allows you to search and inspect the components defined by YGO OCG-Core.

**To find a Scrapi entry by name:**

1. Open the **Scr Name** tab in **Scrapi Books**.
2. Enter a search term in the **Search Name** box.
3. Select a matching result to view its documentation in **Scrapi Result**.

You can also use the Scr Desc tab to search for Scrapi entries by their descriptions.

If **Auto Search** is enabled, selecting a Scrapi entry automatically searches the available Lua scripts for its name. Matching scripts are displayed in **Script Result**.

For more information, see [Scrapyard](./Reference/ScrapiReference.md).

### 3. Open and Edit a Script

Script Support allows you to open and edit Lua script files directly in the Script Editor.

To open an existing script:

1. Open the Menu.
2. Select Open and choose the Lua script file.
3. The opened file appears in the Open tab of the Navigation Panel.
4. Select the file to display its content in the Script Editor.

### Data Handling Permissions

Script Support is primarily designed as a reference and lookup tool for Yu-Gi-Oh! OCG-Core data and scripts.

To help protect the original game files from accidental modifications, Script Support provides two independent options under:

```
Settings → Configuration → Data Handling
```

- **Allow Save**: Allows users to save the content currently displayed in the Script Editor to its original file.

- **Allow New**: Allows users to create new text files, including Lua script files.

Both options can be enabled or disabled independently.

If you only intend to use Script Support for browsing and referencing game data, you can leave both options disabled.

This helps prevent accidental changes to existing game scripts and avoids creating additional files unintentionally.

For more information about editing scripts and managing editor tabs, see [Script Editor](./Reference/ScriptEditorReference.md).

---
