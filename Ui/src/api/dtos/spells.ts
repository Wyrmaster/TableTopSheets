// Source: Api/TableTopSheets.Api/Contracts/SpellDtos.cs
import type { PrepareChangeEnum, RitualModeEnum, SpellPoolEnum } from './enums'

/** A spell the character knows, has prepared or has in the spellbook */
export interface KnownSpellDto {
  key: string
  name: string
  /** 0 for cantrips */
  level: number
  /** school key, e.g. `evocation` */
  school: string
  castingTime: string
  ritual: boolean
  concentration: boolean
  /** part of the prepared list (cantrips are always prepared) */
  prepared: boolean
  /** prepared without counting against the limit */
  alwaysPrepared: boolean
  inSpellbook: boolean
  /** class or feature the spell comes from */
  source: string
  ability: string
}

/** Spellcasting of one class with every spell it knows, has in the spellbook or has prepared */
export interface SpellcastingClassDto {
  classKey: string
  className: string
  ability: string
  pool: SpellPoolEnum
  prepareChange: PrepareChangeEnum
  ritualMode: RitualModeEnum
  /** key of the class spell list (`class` filter of the spell list) */
  spellList: string
  maxSpellLevel: number
  preparedLimit: number
  /** prepared now, without always prepared spells and cantrips */
  preparedCount: number
  cantripLimit: number
  spells: KnownSpellDto[]
}

/** Spells of a character: per spellcasting class, plus spells from species and feats */
export interface CharacterSpellsDto {
  classes: SpellcastingClassDto[]
  other: KnownSpellDto[]
}
