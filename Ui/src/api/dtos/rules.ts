// Sources: Api/TableTopSheets.Api/Contracts/RuleDtos.cs, Contracts/Filters.cs
import type {
  DamageTypeEnum,
  DiceEnum,
  ItemKindEnum,
  OptionSourceEnum,
  PrimaryAbilityModeEnum,
  ReplacePolicyEnum,
} from './enums'

/** Species in a list */
export interface SpeciesSummaryDto {
  key: string
  name: string
  creatureType: string
}

/** Background in a list */
export interface BackgroundSummaryDto {
  key: string
  name: string
  abilityOptions: string[]
  originFeat: string | null
}

/** Subclass of a class */
export interface SubclassSummaryDto {
  key: string
  name: string
}

/** Class in a list */
export interface ClassSummaryDto {
  key: string
  name: string
  hitDie: DiceEnum
  primaryAbilities: string[]
  primaryAbilityMode: PrimaryAbilityModeEnum
  subclassLevel: number
  subclasses: SubclassSummaryDto[]
}

/** Feat in a list */
export interface FeatSummaryDto {
  key: string
  name: string
  category: string
  repeatable: boolean
  hasPrerequisite: boolean
}

/** Spell in a list */
export interface SpellSummaryDto {
  key: string
  name: string
  level: number
  school: string
  castingTime: string
  range: string
  ritual: boolean
  concentration: boolean
  damage: string | null
  damageType: DamageTypeEnum | null
  lists: string[]
}

/** Item in a list */
export interface ItemSummaryDto {
  key: string
  name: string
  kind: ItemKindEnum
  category: string | null
  costCp: number
  weight: number
  magic: boolean
  requiresAttunement: boolean
}

/** Definition of a choice as authored in the rules (independent of a character) */
export interface ChoiceDefinitionDto {
  key: string
  name: string
  /** count as rule expression, e.g. `2` or `scale('wizard.cantrips')` */
  count: string
  optionSource: OptionSourceEnum
  optional: boolean
  allowDuplicates: boolean
  maxPerOption: number
  replacePolicy: ReplacePolicyEnum
}

/** A feature (class feature, species trait, feat benefit ...) with the choices it offers */
export interface FeatureDto {
  key: string
  name: string
  level: number | null
  description: string
  choices: ChoiceDefinitionDto[]
}

/** Full description of a rule entity with its features (species, background, feat, subclass) */
export interface RuleDetailDto {
  key: string
  name: string
  description: string
  features: FeatureDto[]
}

/** Class with its features per level and subclasses */
export interface ClassDetailDto {
  class: ClassSummaryDto
  description: string
  features: FeatureDto[]
  subclasses: RuleDetailDto[]
}

/** Spell with its full text */
export interface SpellDetailDto {
  spell: SpellSummaryDto
  components: string
  material: string | null
  duration: string
  attackRoll: boolean
  saveAbility: string | null
  description: string
}

/** Item with its full text and properties */
export interface ItemDetailDto {
  item: ItemSummaryDto
  description: string
  properties: Record<string, string>
  features: FeatureDto[]
}

/** Filters of the spell list */
export interface SpellFilter {
  /** class spell list, e.g. `wizard` */
  class?: string
  level?: number
  minLevel?: number
  maxLevel?: number
  /** school key, e.g. `evocation` */
  school?: string
  ritual?: boolean
  concentration?: boolean
  /** only spells that deal (or don't deal) damage */
  damage?: boolean
  damageType?: DamageTypeEnum
  /** text the casting time contains, e.g. `reaction` */
  castingTime?: string
}

/** Filters of the feat list */
export interface FeatFilter {
  /** `origin`, `general`, `fighting-style` or `epic-boon` */
  category?: string
  repeatable?: boolean
  hasPrerequisite?: boolean
}

/** Filters of the item list */
export interface ItemFilter {
  kind?: ItemKindEnum
  /** weapon category (`simple`, `martial`), armor category (`light` ...) or tool group */
  category?: string
  magic?: boolean
  attunement?: boolean
}
