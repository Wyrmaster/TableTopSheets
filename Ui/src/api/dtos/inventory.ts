// Source: Api/TableTopSheets.Api/Contracts/InventoryDtos.cs
import type { EquipSlotEnum } from "./enums"

/** Add an item to the inventory */
export interface AddItemDto {
  itemKey: string
  /** default 1 */
  quantity?: number
}

/** Put an item into a slot (`None` unequips it) */
export interface EquipDto {
  slot: EquipSlotEnum
}

/** Attune to an item or end the attunement */
export interface AttuneDto {
  attuned: boolean
}

/** Switch a toggleable feature (Rage, Mage Armor ...) */
export interface ToggleDto {
  active: boolean
}

/** Select the armor class calculation by its label; null takes the highest */
export interface ArmorClassDto {
  label: string | null
}

/** Long rest loadout of prepared spells of a class */
export interface PreparedSpellsDto {
  spellKeys: string[]
}

/** Copy a spell into the spellbook */
export interface SpellbookEntryDto {
  spellKey: string
  note?: string | null
}

/** Change a selection on a rest */
export interface RestReplaceDto {
  optionKey: string
}
