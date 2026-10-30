// Sources: Common/Sheet/CharacterSheet.cs,
// Common/Container/Magic/SpellSlot.cs
import type { Guid } from './common'
import type { ArmorFlags, DamageTypeEnum, DiceEnum, PeriodEnum, SizeEnum, ViolationSeverityEnum } from './enums'

/** Proficiency levels of skills and saving throws (SkillModifierEnum) */
export const ProficiencyLevels = {
  None: 0,
  Half: 5,
  Proficient: 10,
  Expertise: 20,
} as const

export interface ClassLevelSheet {
  class: string
  subclass: string | null
  level: number
  hitDie: DiceEnum
}

export interface SourceValue {
  label: string
  value: number
}

export interface DicePool {
  die: DiceEnum
  count: number
}

export interface SkillSheet {
  key: string
  name: string
  /** ProficiencyLevels: 0 none, 5 half, 10 proficient, 20 expertise */
  proficiency: number
  bonus: number
  advantage: boolean
  disadvantage: boolean
}

export interface AbilitySheet {
  key: string
  name: string
  score: number
  modifier: number
  /** ProficiencyLevels: 0 none, 5 half, 10 proficient, 20 expertise */
  saveProficiency: number
  saveBonus: number
  skills: SkillSheet[]
}

export interface AttackSheet {
  name: string
  attackBonus: number
  damage: string
  damageDice: DiceEnum[]
  damageBonus: number
  /** null when the damage type is chosen on use */
  damageType: DamageTypeEnum | null
  notes: string
  /** set instead of an attack roll for effects that force a saving throw */
  saveDc: number | null
  /** ability key of the saving throw */
  saveAbility: string | null
}

export interface SpellcastingSheet {
  class: string
  ability: string
  modifier: number
  saveDc: number
  attackBonus: number
  preparedLimit: number
  cantripLimit: number
}

/** One spell slot; Pact Magic slots refresh on a short rest */
export interface SpellSlot {
  level: number
  refreshPeriod: PeriodEnum
  sourceId: Guid
}

export interface SpellSheet {
  key: string
  name: string
  level: number
  castingTime: string
  range: string
  concentration: boolean
  ritual: boolean
  material: boolean
  source: string
  notes: string
}

export interface FeatureSheet {
  key: string
  name: string
  source: string
  description: string
  spellcasting: boolean
}

export interface ResourceSheet {
  key: string
  name: string
  maximum: number
  used: number
  die: string | null
  recharge: string
}

export interface Coins {
  copper: number
  silver: number
  electrum: number
  gold: number
  platinum: number
}

/** Rule problem found while computing a sheet */
export interface SheetViolation {
  code: string
  message: string
  severity: ViolationSeverityEnum
}

/**
 * Every computed value needed to print a character sheet
 */
export interface CharacterSheet {
  name: string
  background: string
  /** classes with levels (e.g. `Wizard 3 / Fighter 2`) */
  classes: string
  subclasses: string
  species: string
  level: number
  experience: number
  classLevels: ClassLevelSheet[]

  proficiencyBonus: number
  abilities: AbilitySheet[]

  armorClass: number
  shieldEquipped: boolean
  /** the base calculation that applies with the current equipment, followed by every bonus (shield, Defense …) */
  armorClassSources: SourceValue[]
  hitPointMaximum: number
  currentHitPoints: number | null
  temporaryHitPoints: number
  /** hit point dice per die type */
  hitDice: DicePool[]
  initiative: number
  speed: number
  otherSpeeds: SourceValue[]
  size: SizeEnum
  passivePerception: number
  senses: SourceValue[]
  attacksPerAction: number
  attacks: AttackSheet[]
  resistances: string[]
  immunities: string[]
  vulnerabilities: string[]

  armorTraining: ArmorFlags
  weaponProficiencies: string[]
  weaponMasteries: string[]
  toolProficiencies: string[]
  languages: string[]

  spellcasting: SpellcastingSheet[]
  spellSlots: SpellSlot[]
  spells: SpellSheet[]

  classFeatures: FeatureSheet[]
  speciesTraits: FeatureSheet[]
  feats: FeatureSheet[]
  resources: ResourceSheet[]

  equipment: string[]
  attunedItems: string[]
  coins: Coins

  /** rule problems (open choices, illegal loadouts, missing training ...) */
  violations: SheetViolation[]
}
