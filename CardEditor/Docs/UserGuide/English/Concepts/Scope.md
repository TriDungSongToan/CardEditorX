# Scope

## Overview

**Scope** determines which items are affected when an operation is executed.

CardEditorX provides Scope options that allow users to control the range of items processed by an operation without manually selecting or filtering items beforehand.

Scope is based on the Item Collections maintained by the active Editor.

## Available Scopes

CardEditorX provides three Scope options:

| Scope              | Description |
|--------------------|-------------|
| **Selected Cards** | Processes all items currently selected in the DataGrid. |
| **Found Cards**    | Processes all items currently displayed in the DataGrid. |
| **All Cards**      | Processes all items stored in the active Editor's Item List. |

Each Editor maintains its own Item List. Operations that work with an Item List use the collection belonging to the active Editor.

### Selected Cards

**Selected Cards** refers to all items currently selected in the DataGrid.

This collection contains only selected items, regardless of how many other items are displayed.

### Found Cards

**Found Cards** refers to all items currently displayed in the DataGrid.

Its contents depend on the current filtering conditions:

- When no filter is applied, Found Cards contains all items in the Item List.
- When a filter is applied, Found Cards contains only the items matching the filter.

### All Cards

**All Cards** refers to all items currently stored in the active Editor's Item List.

This collection is independent of the current filter and selection. Items that are not displayed in the DataGrid are still included.



