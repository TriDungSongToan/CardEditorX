# Item List

## Overview

CardEditorX uses two types of Item Lists when performing data management operations: **Original List** and **Additional List**.

These terms provide a consistent way to distinguish between the existing data managed by an Editor and the external data imported for processing.

## Original List

The **Original List** refers to the collection of items maintained by the currently active Editor.

Each supported Tab Item maintains its own Original List:

| Tab Item        | Item Type     |
|-----------------|---------------|
| DataEditor      | `Card`        |
| OmegaDataEditor | `CardOmega`   |
| BanListEditor   | `CardBanList` |

The Original List contains the primary data currently managed by the active Editor.

Operations that modify existing items or add new items generally apply their changes to this list.

## Additional List

The **Additional List** refers to a separate collection of items loaded from an external data source for use in data management operations.

It is used by features such as:

- **Import Data**
- **Replace Field**

The Additional List acts as the source of imported data. Its items are compared against the Original List, typically using their IDs, to determine whether existing items should be updated or new items should be added.

The Additional List is separate from the Original List and does not represent the primary collection managed by the active Editor.

## List Usage

The following table summarizes the purpose of each list:

| List                 | Purpose          |
|----------------------|------------------|
| **Original List**    | Stores the items currently managed by the active Editor. |
| **Additional List**  | Stores items loaded from an external data source for data management operations. |

The terms Original List and Additional List are used throughout this documentation to distinguish between these two collections.



