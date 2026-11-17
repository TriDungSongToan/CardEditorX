# Scrapi Reference

## 1. Overview

Scrapi Reference provides documentation for the functions, constants, namespaces, enums, tags, and types defined by the YGO OCG-Core.

Script Support organizes these components into searchable categories, allowing users to look up their names, descriptions, and related documentation without having to search through the source files manually.

The Scrapi data is presented through the **Scrapi Books** section in the Navigation Panel, while the corresponding documentation is displayed in the **Scrapi Result** area.

## 2. Scrapi Structure

Script Support organizes Scrapi into six categories:

| Category | Description |
|---|---|
| Functions | Functions provided by YGO OCG-Core. |
| Constants | Constant values defined by YGO OCG-Core. |
| Namespaces | Namespaces used to organize related components. |
| Enums | Enumerations and their associated members. |
| Tags | Tags defined by YGO OCG-Core. |
| Types | Types referenced by the Scrapi definitions. |

These categories are collectively referred to as **Scrapi** throughout this User Guide.

## 3. Searching Scrapi

The Scrapi Books section provides two search tabs: **Scr Name** and **Scr Desc**.

### 3.1. Search by Name

The **Scr Name** tab allows users to search for Scrapi components by name.

Enter a search term in the **Search Name** box. The result list displays matching Scrapi entries.

### 3.2. Search by Description

The **Scr Desc** tab allows users to search for Scrapi components by their descriptions.

Enter a search term in the **Search Desc** box. The result list displays matching entries based on their documentation descriptions.

## 4. Viewing Scrapi Information

When a Scrapi entry is selected from either search tab, its corresponding documentation is displayed in the **Scrapi Result** area.

This area provides the information associated with the selected entry, allowing users to inspect its documentation while working with Card Scripts.

## 5. Related Card Scripts

Script Support can help users identify Card Scripts that use a particular Scrapi component.

### 5.1. Auto Search

The **Auto Search** option automatically searches for Card Scripts containing the name of the currently selected Scrapi entry.

When this option is enabled, selecting a Scrapi entry triggers a text search across the contents of the `.lua` files available in the configured Data Source.

The search identifies scripts whose text contains the selected Scrapi name. The matching scripts are displayed in the Script Result area, allowing users to inspect examples of how the Scrapi component appears in Card Scripts.

This feature performs a text-based search. It does not parse Lua syntax or analyze expressions to determine whether a Scrapi component is actually being invoked or referenced.

To enable or disable this feature, navigate to:

`Settings -> Configuration -> Data Handling -> Auto Search`

When Auto Search is disabled, selecting a Scrapi entry does not automatically trigger a related Card Script search.

---
Next: [Script Editor Reference →](../Reference/ScriptEditorReference.md)

Previous:

[← Card Reference](../Reference/CardReference.md)

[← Getting Started](../GettingStarted.md)
