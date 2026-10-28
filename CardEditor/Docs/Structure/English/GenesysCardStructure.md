# Card Genesys Structure

This document defines the Card Genesys data structure supported by CardEditorX, as well as the file formats available for storing and exchanging Genesys Point data.

**Genesys Manager** is a card data management module in CardEditorX. It allows users to manage the Genesys Point values assigned to individual cards.

## Genesys Card Database

The Genesys Card Database is an SQLite database containing a single table named `GenesysCard`.

### Table: `GenesysCard`

|   Column  | Data Type   | Constraints   | Description    |
|-----------|-------------|---------------|----------------|
| `id`      | `INTEGER`   | `PRIMARY KEY` | The card's unique identifier (Card ID). |
| `name`    | `TEXT`      | Optional      | The card's name (Card Name). |
| `point`   | `INTEGER`   | `NOT NULL`    | The Genesys Point value assigned to the card. |

### Notes

- Each record represents a card and its associated Genesys Point value.
- The `name` field is optional.
- The `point` field is required and stores the card's Genesys Point value as an integer.

---

[Rarity Card Structure](./RarityCardStructure.md)

Previous: [← Genesys Manager](../../UserGuide/English/Manager/GenesysManager.md)
