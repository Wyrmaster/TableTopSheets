// Sources: Api/TableTopSheets.Api/Contracts/CharacterDtos.cs, Contracts/Filters.cs (CharacterFilter)
import type { PendingChoiceDto } from './choice'
import type { Guid } from './common'
import type {
  AbilityKey,
  AbilityMethodEnum,
  AlignmentEnum,
  EquipSlotEnum,
  ItemKindEnum,
  ReplacePolicyEnum,
  ViolationSeverityEnum,
} from './enums'
import type { CoinsDto } from './inventory'
import type { SubclassSummaryDto } from './rules'
import type { TakeStartingEquipmentDto } from './startingEquipment'

/** Levels of a character in one class */
export interface ClassLevelDto {
  key: string
  name: string
  level: number
  subclass: string | null
}

/** Character in a list */
export interface CharacterSummaryDto {
  id: Guid
  name: string
  level: number
  species: string
  background: string
  classes: ClassLevelDto[]
}

/** Ability score with its modifier */
export interface AbilityDto {
  key: AbilityKey
  name: string
  score: number
  modifier: number
}

/** Rule violation of the current state (e.g. armor without training) */
export interface ViolationDto {
  code: string
  message: string
  severity: ViolationSeverityEnum
}

/** Owned item */
export interface CharacterItemDto {
  id: Guid
  itemKey: string
  name: string
  kind: ItemKindEnum
  quantity: number
  slot: EquipSlotEnum
  attuned: boolean
  requiresAttunement: boolean
  /** what the item is about (a book of `prayers`) */
  notes: string | null
}

/**
 * Everything a client needs after a change: the summary, what the user still has to decide and what is wrong.
 * Every write answers with it
 */
export interface CharacterStateDto {
  id: Guid
  name: string
  level: number
  species: string
  background: string
  alignment: AlignmentEnum
  classes: ClassLevelDto[]
  abilities: AbilityDto[]
  proficiencyBonus: number
  hitPointMaximum: number
  armorClass: number
  items: CharacterItemDto[]
  coins: CoinsDto
  /** starting equipment alternative taken from the background (null = still open) */
  backgroundEquipment: string | null
  /** starting equipment alternative taken from the first class (null = still open or no class yet) */
  classEquipment: string | null
  /** true when no required choice is open */
  complete: boolean
  /** choices that still need selections (follow-up choices appear here after their parent was selected) */
  pendingChoices: PendingChoiceDto[]
  violations: ViolationDto[]
}

/**
 * Answer for a choice. `parentRef` addresses the choice instance: the `parentRef` of the pending choice, or the `ref`
 * of another selection of the same request (choose a feat and its abilities at once)
 */
export interface SelectionDto {
  choiceKey: string
  optionKey: string
  ref?: string | null
  parentRef?: string | null
}

/** Selections for open choices */
export interface SelectionsDto {
  selections: SelectionDto[]
}

/**
 * New character. Only the origin is decided here; classes are added with level ups
 */
export interface CreateCharacterDto {
  name: string
  speciesKey: string
  backgroundKey: string
  abilityMethod?: AbilityMethodEnum
  /** base scores before increases; all six keys are required */
  abilities: Record<AbilityKey, number>
  alignment?: AlignmentEnum
  /** answers to origin choices known up front (more can be posted later) */
  selections?: SelectionDto[]
  /** starting equipment of the background; null takes it later */
  startingEquipment?: TakeStartingEquipmentDto | null
  /** save even when required choices are open (default true) */
  allowIncomplete?: boolean
}

/** Descriptive fields; omitted fields keep their value */
export interface UpdateCharacterDto {
  name?: string | null
  alignment?: AlignmentEnum | null
  appearance?: string | null
  backstory?: string | null
  experience?: number | null
}

/** Swap an existing selection on a level up */
export interface ReplacementDto {
  selectionId: Guid
  optionKey: string
}

/** Gain a level */
export interface LevelUpDto {
  classKey: string
  /** required at the class level the subclass is gained */
  subclassKey?: string | null
  /** rolled hit points (1 to the hit die); null takes the fixed value */
  hitPointRoll?: number | null
  selections?: SelectionDto[]
  replacements?: ReplacementDto[]
  /** save even when required choices are open (default true) */
  allowIncomplete?: boolean
  /** starting equipment of the class, only with the first level; null takes it later */
  startingEquipment?: TakeStartingEquipmentDto | null
}

/** Active selection of a character */
export interface SelectionInfoDto {
  id: Guid
  choiceKey: string
  choiceName: string
  optionKey: string
  optionName: string
  source: string
  level: number | null
  parentId: Guid | null
  replacePolicy: ReplacePolicyEnum
  /** can be taken back with RemoveSelectionIntent (selections of the latest level) */
  removable: boolean
}

/** What a level up in a class would ask for */
export interface LevelUpPreviewDto {
  classKey: string
  classLevel: number
  characterLevel: number
  requiresSubclass: boolean
  subclasses: SubclassSummaryDto[]
  pendingChoices: PendingChoiceDto[]
  /** selections that may be swapped with this level */
  replaceable: SelectionInfoDto[]
}

/** Result of a validation without saving */
export interface ValidationResultDto {
  valid: boolean
  errors: string[]
  state: CharacterStateDto | null
}

/** Filters of the character list */
export interface CharacterFilter {
  /** has levels in this class */
  class?: string
  species?: string
  minLevel?: number
  maxLevel?: number
}
