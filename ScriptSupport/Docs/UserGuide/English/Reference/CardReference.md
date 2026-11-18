# Card 

## 1. Overview

Card Reference provides tools for searching and browsing Card information from the configured Data Source.

It supports two primary search methods:

- **Card Description Search:** Searches Card Text records by Name, Description, and String.
- **Card Script Search:** Searches the contents of Lua Script Files.

Users can also use Advanced Search to find Cards based on textual information and Card properties, then view their Card Description and associated Script Files.

## 2. Card Data Loading

Before using Card Reference, it is useful to understand how Script Support loads and organizes Card information from the Data Source.

### 2.1. Data Source

A Data Source can contain multiple Database Files and multiple Script Files.

During the loading process, Script Support reads the available Database Files to retrieve Card information and builds a lookup structure for Script Files.

The loading process produces two main groups of data:

- **Card Information:** Card Data and Card Text loaded from Database Files.
- **Card Scripts:** A lookup structure associating Card IDs with their corresponding Script File paths.

### 2.2. YGO Database Structure

The standard YGO Database uses two primary tables to store Card information:

| Table   | Description |
|---------|-------------|
| `datas` | Stores Card Data, including `scope`, `type`, `race`, `attribute`, and other gameplay properties. |
| `texts` | Stores Card Text, including `name`, `desc`, and `str`. |

Both tables are associated through the `id` column, which represents the Card ID.

### 2.3. Default YGO Game Engine Behavior

By default, the YGO Game Engine uses an overwrite-based loading mechanism.

When multiple Card records with the same ID are loaded, the record loaded later replaces the previously loaded Card information.

As a result, only the information from the last loaded version remains available.

### 2.4. Script Support Loading Logic

Script Support does not follow the default overwrite behavior. Instead, Card Data and Card Text are processed independently.

**Card Data**

- Each Card ID retains only one Card Data record.
- When multiple Data records share the same Card ID, the first loaded record is retained.
- Subsequent records with the same ID are ignored and do not overwrite the existing Data.

**Card Text**

- Each Card ID maintains a `List<CardText>` containing its loaded Text records.
- Every Text record read from a Database is added to the corresponding list.
- No filtering or deduplication is performed based on language, text content, or other attributes.

Consequently, multiple Text records can coexist under the same Card ID, including records with identical languages or identical content.

This approach allows Script Support to retain all available Card Text records while maintaining a single Card Data record for each Card ID.

### 2.5. Card Script Loading

Card Scripts are stored as individual plain-text Lua files.

The standard file naming convention is:

```text
c{id}.lua
```

For example:

```text
c1001.lua
c1002.lua
c1003.lua
```

During loading, Script Support builds the following lookup structure:

```csharp
Dictionary<ulong, IReadOnlyList<string>> CardScripts;
```

The dictionary uses:

- **Key (`ulong`):** Card ID.
- **Value (`IReadOnlyList<string>`):** A list of file paths associated with that Card ID.

A Card ID can therefore be associated with multiple Script Files, and all corresponding paths are retained in its list.

This structure allows Script Support to locate and display all Script Files associated with a selected Card.

## 3. Card Search

Card Reference provides two search methods: Card Description Search and Card Script Search.

### 3.1. Search Card Description

The `Search Card Desc` search box allows users to find Cards based on their textual information.

The search is performed across all loaded Card Text records belonging to all Cards in the Data Source.

The following fields are searched:

| Field  | Description |
|--------|-------------|
| `Name` | Card name. |
| `Desc` | Card description and effect text. |
| `Str`  | Additional Card text. |

Because each Card ID can contain multiple Card Text records, the search includes every loaded record in every Card's `List<CardText>`.

This means a Card can be found through any of its loaded Text records, regardless of language or whether its content duplicates another record.

### 3.2. Search Card Script

The `Search Script` search box allows users to find Cards based on the contents of their Script Files.

Unlike Card Description Search, Script Search does not search a Script field in the Database. Instead, it searches for the specified text string within the contents of Lua Script Files.

The search is performed across the available Script File paths in the Data Source.

Since each Script File is an individual plain-text file, the search examines the contents of those files to identify matching results.

### 3.3. Search Results

Both search methods display matching Cards in their respective result lists.

Users can open a result using the following mouse interactions:

| Mouse Interaction | Behavior     |
|-------------------|--------------|
| Single Left-click | Opens the selected content in a Temporary Tab. |
| Double Left-click | Opens the selected content in a Permanent Tab. |

For Card Description Search, selecting a Card displays its available Card Description.

For Card Script Search, selecting a result opens the corresponding Script content.

## 4. Advanced Search

Advanced Search provides additional criteria for finding Cards.

It is divided into three sections: Card Text, Card Data, and Card Information.

### 4.1. Card Text

The Card Text section allows users to search Cards using textual information.

Available search fields include:

| Field       | Description          |
|-------------|----------------------|
| Card ID     | Searches using the Card ID. |
| Card Alias  | Searches using the Card Alias. |
| Card Name   | Searches using the Card Name. |
| Card Desc   | Searches within Card Description. |
| Card Str    | Searches within additional Card text. |

These fields provide more specific search options than the basic Card Description Search.

### 4.2. Card Data

The Card Data section allows users to search Cards based on their gameplay properties.

Available properties include:

- Scope
- Type
- Race
- Attribute
- Other supported Card Data properties.

This section is useful when searching for Cards based on their characteristics rather than their textual content.

### 4.3. Card Information

The Card Information section displays detailed information about the currently selected Card.

Depending on the available data, this may include:

- Card Name
- Card Type
- Card Image
- Other Card properties.

This section is intended for inspecting Card information rather than defining search criteria.

## 5. Viewing Card Information

After selecting a Card, Script Support displays its available information in the Result Panel.

### 5.1. Card Description

The Card Description is displayed in the `Desc Result` tab.

The displayed content corresponds to a selected Card Text record belonging to the current Card ID.

If multiple Card Text records are available for the same Card ID, users can choose which record to display.

### 5.2. Description Versions

A Card ID can have multiple Card Text records because Script Support retains every loaded Text record instead of replacing or filtering them.

A ComboBox above the Description result area allows users to select the Text record to display.

The selection is based on the available loaded records. Script Support does not automatically identify, categorize, or remove records based on language or duplicate content.

### 5.3. Card Script

When a Card is selected, Script Support uses its Card ID to look up the corresponding Script File paths in `CardScripts`.

<!--

The lookup operation is conceptually equivalent to:

```csharp
if (CardScripts.TryGetValue(cardId, out var paths))
{
    // Display the corresponding Script Files.
}
```
-->

Each path represents an individual Script File.

All associated Script Files are displayed in `Script Result`, with each file opened in its own TabItem.

Therefore:

- If a Card ID has one associated Script File, one TabItem is displayed.
- If a Card ID has multiple associated Script Files, multiple TabItems are displayed.
- If no Script File is associated with the Card ID, there is no corresponding Script content to display.

---

Next: [Scrapi Reference →](../Reference/ScrapiReference.md)

Previous:

[← Interface Overview](../Interface/InterfaceOverview.md)

[← Getting Started](../GettingStarted.md)
