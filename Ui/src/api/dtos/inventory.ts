// Source: Api/Library/Contracts/InventoryDtos.cs
import type { EquipSlotEnum } from './enums'

/** Add an item to the inventory */
export interface AddItemDto {
  itemKey: string
  /** default 1 */
  quantity?: number
  /** pay the cost from the coins (with change); 422 when not enough */
  purchase?: boolean
}

/** Pieces added to an item stack without paying (at least 1) */
export interface AddToStackDto {
  quantity: number
}

/** Coins of a character, or for a change the coins added (positive) or removed (negative) per kind */
export interface CoinsDto {
  copper: number
  silver: number
  electrum: number
  gold: number
  platinum: number
}

/** An amount to pay in copper pieces (1 gp = 100 cp); coins are spent and change is returned */
export interface CoinPaymentDto {
  amountCp: number
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
