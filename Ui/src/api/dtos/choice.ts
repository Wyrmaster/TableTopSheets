// Source: Api/TableTopSheets.Api/Contracts/ChoiceDtos.cs
import type { Guid } from "./common"
import type { ChoiceInputKind, OptionSourceEnum } from "./enums"

/**
 * Limits of a choice instance, enough to validate before sending
 */
export interface ChoiceConstraintsDto {
  /** selections that have to be sent before the character is complete */
  min: number
  /** selections that may still be sent */
  max: number
  /** how often one option may be picked */
  maxPerOption: number
  allowDuplicates: boolean
  /** for distributions: the allowed splits of the remaining picks, e.g. `[[2,1],[1,1,1]]` */
  allowedPatterns: number[][] | null
}

/** A selection made for a choice instance */
export interface SelectedOptionDto {
  selectionId: Guid
  optionKey: string
  name: string
}

/** Typed details of an option; which fields are set depends on the option source */
export interface OptionMetaDto {
  /** spell level or feature level */
  level: number | null
  /** spell school key */
  school: string | null
  ritual: boolean | null
  damage: boolean | null
  /** feat category, language rarity, tool group, weapon category ... */
  category: string | null
  /** value the option carries (e.g. the damage type of a draconic ancestry) */
  value: string | null
  repeatable: boolean | null
  /** prerequisite as rule expression (e.g. `score('cha') >= 13`) */
  prerequisite: string | null
  /** class spell lists of a spell */
  lists: string[] | null
}

/** An option of a choice */
export interface OptionDto {
  key: string
  name: string
  /** first sentence of the description */
  summary: string | null
  /** full description (only with `includeDescriptions`) */
  description: string | null
  /** true when the option exists but can't be selected right now (only with `includeDisabled`) */
  disabled: boolean
  disabledReason: string | null
  /** names of the features the option grants */
  grants: string[]
  meta: OptionMetaDto
}

/**
 * A choice instance that still needs selections
 */
export interface PendingChoiceDto {
  /** stable id of the instance (`choiceKey` or `choiceKey@parentRef`) */
  id: string
  choiceKey: string
  name: string
  /** feature that offers the choice, e.g. "Ability Score Improvement (Wizard 4)" */
  source: string
  description: string
  /** send back as `parentRef` of the selections (null for top level choices) */
  parentRef: string | null
  input: ChoiceInputKind
  optionSource: OptionSourceEnum
  optional: boolean
  total: number
  selected: number
  remaining: number
  constraints: ChoiceConstraintsDto
  /** filters the options route accepts for this choice */
  optionFilters: string[]
  /** number of options that can be selected right now */
  optionCount: number
  /** embedded available options when there are few; otherwise null and `optionsUrl` lists them */
  options: OptionDto[] | null
  /** paged, filterable list of the options (path with query, see GetChoiceOptionsIntent.fromUrl) */
  optionsUrl: string
  selectedOptions: SelectedOptionDto[]
}

/** Filters of the options of a choice */
export interface OptionQuery {
  /** parent selection of the choice instance (the `parentRef` of the pending choice) */
  parentRef?: string
  /** evaluate against a level up preview: `class` or `class:subclass` */
  preview?: string
  level?: number
  minLevel?: number
  maxLevel?: number
  school?: string
  ritual?: boolean
  damage?: boolean
  category?: string
  /** also list options that exist but can't be selected right now (with the reason) */
  includeDisabled?: boolean
  /** include the full description of each option */
  includeDescriptions?: boolean
}
