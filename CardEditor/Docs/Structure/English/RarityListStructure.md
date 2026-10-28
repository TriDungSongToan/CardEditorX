# Rarity List Structure

This document defines the Rarity List data structure supported by CardEditorX.

## Rarity List Database

The Rarity List Database is an SQLite database containing a single table named `RareList`.

### Table: `RareList`

| Column      | Data Type | Constraints   | Description                 |
|-------------|-----------|---------------|-----------------------------|
| `IdRare`    | `INTEGER` | `PRIMARY KEY` | The unique identifier of the rarity entry. |
| `Name`      | `TEXT`    | None          | The name of the rarity. |
| `Code`      | `INTEGER` | None          | The bit position assigned to the rarity. The corresponding bit flag value is calculated as \(2^{Code}\). |
| `ImagePath` | `TEXT`    | None          | The path to the image used as the Rarity Label for this rarity. |


## Code and Bit Flag Values

The `Code` column in the Rarity List Database stores the **decimal bit flag value** of each rarity.

However, in Rarity Manager, the `Code` displayed in the UI represents the **bit position**, starting from `0`. When loading data from the database, CardEditorX converts the stored decimal value into its corresponding bit position. When adding or modifying a rarity, the UI bit position is converted back into a decimal bit flag value before being saved to the database.

The relationship between these values is illustrated below:

| Rarity Name | Code (Database) | Code (UI) | Bit Flag Value (dec) |
|-------------|----------------:|----------:|---------------------:|
| Normal      | 1               | 0         | 1 (2<sup>0</sup>)    |
| Rare        | 2               | 1         | 2 (2<sup>1</sup>)    |
| Super Rare  | 4               | 2         | 4 (2<sup>2</sup>)    |
| Ultra Rare  | 8               | 3         | 8 (2<sup>3</sup>)    |
| Secret Rare | 16              | 4         | 16 (2<sup>4</sup>)   |

The bit flag value is calculated using the following formula:

`Bit Flag Value` = 2<sup>`Code (UI)`</sup>

### Combining Multiple Rarities

A card can have multiple rarities simultaneously. Its `rare` value in the Rarity Card Database is the sum of the corresponding bit flag values.

For example, if a card has both **Normal** and **Super Rare** rarities:

rare = 1 (Normal) + 4 (Super Rare) = 5

The decimal value `5` has a binary representation of `101`, where the bits at positions `0` and `2` are set. These correspond to Normal and Super Rare, respectively.

Due to this Structure, the Code values in the UI must start at 0 and follow consecutive integers (0, 1, 2, 3, ...). Each Code must be unique.

---

[Genesys Card Structure](./GenesysCardStructure.md)

Previous: [Card List Structure](./CardListStructure.md)&nbsp;&nbsp;&nbsp;||&nbsp;&nbsp;&nbsp;[Rarity Manager](../../UserGuide/English/Manager/RarityManager.md)

Next: [Rarity Card Structure →](./RarityCardStructure.md)
