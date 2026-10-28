# Card Rarity Structure

This document defines the Rarity Card data structure supported by CardEditorX and the formats that can be used to store and exchange card data.

Rarity Manager is a card rarity data management module in CardEditorX. It is designed to load, process, store, and exchange card rarity data using supported card database formats.

Rarity Manager supports two types of data sources:

- Rarity Card List Structure: A dedicated data structure designed specifically for Rarity Manager, containing card information and rarity data.
- Card List Structure: A standard card list structure compatible with DataEditor and OmegaDataEditor. These files provide only the Card ID and Card Name of each card. For details, refer to [Card List Structure](./CardListStructure.md).

## Rarity Card Database

Rarity Card Database is an SQLite database containing a single table named RareCard.

Table: RareCard

|Column Name| Data Type | Constraints |      Description                  |
|-----------|-----------|-------------|-----------------------------------|
| id        | INTEGER   | PRIMARY KEY | Unique identifier for each entry. |
| name      | TEXT      | None        | Name of card.                     |
| rare      | INTEGER   | NOT NULL    | Rarity value of card.             |


### Rarity Bit Flags

The `rare` column stores rarity data as a bit flag, where each bit represents a rarity. A card can have multiple rarities simultaneously by combining their bit flag values using a bitwise OR operation.

Each rarity is assigned a unique bit position, and its bit flag value is calculated as 2<sup>Code</sup>.

For example:

| Rarity     | Bit Flag Value | Binary Representation |
|------------|---------------:|----------------------:|
| Normal     | 1              | 001                   |
| Rare       | 2              | 010                   |
| Super Rare | 4              | 100                   |


If rare = 5, its binary representation is 101, meaning the card has both `Super Rare` and `Normal` rarities.


For the Rarity List structure and the codes assigned to each rarity, refer to Rarity List Structure.

---

[Genesys Card Structure](./GenesysCardStructure.md)

Previous: [Card List Structure](./CardListStructure.md)&nbsp;&nbsp;&nbsp;||&nbsp;&nbsp;&nbsp;[Rarity Manager](../../UserGuide/English/Manager/RarityManager.md)

Next: [Rarity List Structure →](./RarityListStructure.md)