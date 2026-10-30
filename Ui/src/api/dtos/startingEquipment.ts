// Source: Api/Library/Contracts/StartingEquipmentDtos.cs
import type { StartingEquipmentSourceEnum } from './enums'

/** An item that can be picked */
export interface ItemOptionDto {
  key: string
  name: string
}

/** An item of a starting equipment alternative: a fixed item (`itemKey`) or a pick from `options` */
export interface StartingEquipmentItemDto {
  /** entry key; picks are sent keyed by it */
  key: string
  /** item name, or the name of the pick (`Holy Symbol`) */
  name: string
  /** the fixed item; for a pick the item already chosen through `sameAs`, otherwise null */
  itemKey: string | null
  /** pieces (2 daggers, 20 arrows) */
  quantity: number
  /** note stored with the item (`prayers`) */
  note: string | null
  /** items to pick from (empty for fixed items) */
  options: ItemOptionDto[]
  /** choice the pick has to match ("same as above"); a pick answers it while it's open */
  sameAs: string | null
}

/** One alternative of a starting equipment ("(A) Spear, 2 Daggers ..., 28 GP") */
export interface StartingEquipmentOptionDto {
  /** letter of the alternative (`a`, `b` ...) */
  key: string
  items: StartingEquipmentItemDto[]
  /** coins in copper pieces (paid out in gold pieces) */
  coinsCp: number
}

/** Starting equipment a character can take from its background or first class */
export interface StartingEquipmentDto {
  source: StartingEquipmentSourceEnum
  /** key of the background or class */
  ownerKey: string
  /** name of the background or class */
  ownerName: string
  /** key of the alternative already taken; null while it can be taken */
  taken: string | null
  options: StartingEquipmentOptionDto[]
}

/** Take an alternative of a starting equipment */
export interface TakeStartingEquipmentDto {
  /** the alternative (`a`, `b` ...) */
  optionKey: string
  /** picked item key per entry key, for the entries with options */
  picks?: Record<string, string> | null
}
