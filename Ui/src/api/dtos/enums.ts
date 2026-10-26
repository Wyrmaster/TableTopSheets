// Enums of the API, serialized as their names (JsonStringEnumConverter).
// Sources: Common/TableTopSheets.Common/Enum/**, Api/TableTopSheets.Api/Contracts/ChoiceDtos.cs (ChoiceInputKind),
// Common/TableTopSheets.Common/Sheet/CharacterSheet.cs (ViolationSeverityEnum)

export const Alignments = [
  'Unaligned',
  'LawfulGood', 'NeutralGood', 'ChaoticGood',
  'LawfulNeutral', 'Neutral', 'ChaoticNeutral',
  'LawfulEvil', 'NeutralEvil', 'ChaoticEvil',
] as const
export type AlignmentEnum = (typeof Alignments)[number]

export const AbilityMethods = ['StandardArray', 'PointBuy', 'Rolled', 'Manual'] as const
export type AbilityMethodEnum = (typeof AbilityMethods)[number]

export const ItemKinds = ['Gear', 'Weapon', 'Armor', 'Tool'] as const
export type ItemKindEnum = (typeof ItemKinds)[number]

/** `Ready`: an equipped weapon that isn't held; any number of weapons can be ready */
export const EquipSlots = ['None', 'Armor', 'MainHand', 'OffHand', 'Worn', 'Ready'] as const
export type EquipSlotEnum = (typeof EquipSlots)[number]

export type DiceEnum = 'D1' | 'D4' | 'D6' | 'D8' | 'D10' | 'D12' | 'D20' | 'D100'

export const DamageTypes = [
  'Acid', 'Bludgeoning', 'Cold', 'Fire', 'Force', 'Lightning', 'Necrotic',
  'Piercing', 'Poison', 'Psychic', 'Radiant', 'Slashing', 'Thunder',
] as const
/** Damage types and, for immunities, conditions */
export type DamageTypeEnum =
  | (typeof DamageTypes)[number]
  | 'Blinded' | 'Charmed' | 'Deafened' | 'Exhaustion' | 'Frightened' | 'Grappled' | 'Incapacitated'
  | 'Invisible' | 'Paralyzed' | 'Petrified' | 'Poisoned' | 'Prone' | 'Restrained' | 'Stunned' | 'Unconscious'

export type PrimaryAbilityModeEnum = 'All' | 'Any'

export type OptionSourceEnum =
  | 'Static' | 'Ability' | 'Skill' | 'Tool' | 'Language' | 'Spell' | 'Feat' | 'FeatureCategory' | 'Item' | 'Weapon'

export type ReplacePolicyEnum = 'Never' | 'OneOnLevelUp' | 'OnLongRest' | 'OnShortRest'

/** How a client renders a choice: radio / dropdown, checkboxes, or counters per option */
export type ChoiceInputKind = 'SingleSelect' | 'MultiSelect' | 'Distribution'

export type ViolationSeverityEnum = 'Info' | 'Warning' | 'Error'

export type SizeEnum = 'Tiny' | 'Small' | 'Medium' | 'Large' | 'Huge' | 'Gargantuan'

export type PeriodEnum =
  | 'None' | 'ShortRest' | 'LongRest' | 'ProficiencyBonus' | 'Intelligence' | 'Wisdom' | 'Charisma' | 'AtWill'

/**
 * Armor training. ArmorEnum is a [Flags] enum, so it arrives as comma separated names (e.g. "Light, Medium, Shield"),
 * or "0" without any training
 */
export type ArmorFlags = string

/** Where prepared spells come from */
export type SpellPoolEnum = 'ClassList' | 'Spellbook'

/** When prepared spells change: after a long rest (loadout) or on a level up (choices) */
export type PrepareChangeEnum = 'LongRest' | 'LevelUp'

export type RitualModeEnum = 'None' | 'Prepared' | 'FromSpellbook'

/** Ability keys used in ability dictionaries and choices */
export const StartingEquipmentSources = ['Background', 'Class'] as const
export type StartingEquipmentSourceEnum = (typeof StartingEquipmentSources)[number]

export const AbilityKeys = ['str', 'dex', 'con', 'int', 'wis', 'cha'] as const
export type AbilityKey = (typeof AbilityKeys)[number]
