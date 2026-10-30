// Routes: Api/Library/Endpoints/ChoiceEndpoints.cs
import { BodyIntent, Intent, PagedIntent, segment } from '../core/Intent'
import type { CharacterStateDto, SelectionInfoDto, SelectionsDto, ValidationResultDto } from '../dtos/character'
import type { OptionDto, OptionQuery, PendingChoiceDto } from '../dtos/choice'
import type { Guid } from '../dtos/common'
import type { RestReplaceDto } from '../dtos/inventory'
import type { ListQuery } from '../dtos/paging'
import { characterPath } from './characters'

/** Query parameters of the options route that are numbers or booleans (everything else stays a string) */
const NumberOptions = new Set(['level', 'minLevel', 'maxLevel', 'page', 'pageSize'])
const BooleanOptions = new Set(['ritual', 'damage', 'includeDisabled', 'includeDescriptions'])

/** GET /api/characters/{id}/choices: choices that still need selections */
export class GetPendingChoicesIntent extends Intent<PendingChoiceDto[]> {
  readonly method = 'GET'

  readonly characterId: Guid

  /**
   * @param characterId id of the character
   */
  constructor(characterId: Guid) {
    super()
    this.characterId = characterId
  }

  /** Route of the intent */
  get path() {
    return characterPath(this.characterId, '/choices')
  }
}

/**
 * POST /api/characters/{id}/choices: answer open choices. Applied in order, all or nothing; a feat and the choices it
 * opens can be sent together with ref / parentRef (422 on rule violations)
 */
export class SelectOptionsIntent extends BodyIntent<SelectionsDto, CharacterStateDto> {
  readonly method = 'POST'

  readonly characterId: Guid

  /**
   * @param characterId id of the character
   * @param body one selection per pick (a +2 is the same option twice)
   */
  constructor(characterId: Guid, body: SelectionsDto) {
    super(body)
    this.characterId = characterId
  }

  /** Route of the intent */
  get path() {
    return characterPath(this.characterId, '/choices')
  }
}

/** POST /api/characters/{id}/choices/validate: dry run of selections; the state shows follow-up choices */
export class ValidateSelectionsIntent extends BodyIntent<SelectionsDto, ValidationResultDto> {
  readonly method = 'POST'

  readonly characterId: Guid

  /**
   * @param characterId id of the character
   * @param body the selections that would be sent to SelectOptionsIntent
   */
  constructor(characterId: Guid, body: SelectionsDto) {
    super(body)
    this.characterId = characterId
  }

  /** Route of the intent */
  get path() {
    return characterPath(this.characterId, '/choices/validate')
  }
}

/**
 * GET /api/characters/{id}/choices/{choiceKey}/options: paged, filterable options of a choice instance
 * (sort: name, key, level, school, category, available). Use `fromUrl` with the `optionsUrl` of a pending choice
 */
export class GetChoiceOptionsIntent extends PagedIntent<OptionDto, OptionQuery> {
  readonly characterId: Guid
  readonly choiceKey: string

  /**
   * @param characterId id of the character
   * @param choiceKey key of the choice, e.g. `wizard.spellbook-initial`
   * @param query paging and filters; `parentRef` selects the instance, `preview` evaluates against a level up preview
   */
  constructor(characterId: Guid, choiceKey: string, query: ListQuery & OptionQuery = {}) {
    super(query)
    this.characterId = characterId
    this.choiceKey = choiceKey
  }

  /** Route of the intent */
  get path() {
    return characterPath(this.characterId, `/choices/${segment(this.choiceKey)}/options`)
  }

  /**
   * Intent for the `optionsUrl` of a pending choice (it already carries `parentRef` and `preview`)
   *
   * @param optionsUrl `optionsUrl` of a PendingChoiceDto
   * @param query paging and filters added to (or replacing) the ones of the url
   */
  static fromUrl(optionsUrl: string, query: ListQuery & OptionQuery = {}): GetChoiceOptionsIntent {
    // 1. split the url; the base is only needed because the API sends a path without origin
    const url = new URL(optionsUrl, 'http://localhost')
    const match = /^\/api\/characters\/([^/]+)\/choices\/([^/]+)\/options$/.exec(url.pathname)
    if (!match) {
      throw new Error(`'${optionsUrl}' is not an options url`)
    }

    // 2. query parameters arrive as strings; turn numbers and flags back into their types
    const parsed: Record<string, string | number | boolean> = {}
    url.searchParams.forEach((value, key) => {
      parsed[key] = NumberOptions.has(key) ? Number(value) : BooleanOptions.has(key) ? value === 'true' : value
    })

    // 3. character id and choice key come from the path; the given query wins over the url's parameters
    return new GetChoiceOptionsIntent(
      decodeURIComponent(match[1]),
      decodeURIComponent(match[2]),
      { ...parsed, ...query },
    )
  }
}

/** GET /api/characters/{id}/selections: active selections with ids, level and whether they can be removed */
export class GetSelectionsIntent extends Intent<SelectionInfoDto[]> {
  readonly method = 'GET'

  readonly characterId: Guid

  /**
   * @param characterId id of the character
   */
  constructor(characterId: Guid) {
    super()
    this.characterId = characterId
  }

  /** Route of the intent */
  get path() {
    return characterPath(this.characterId, '/selections')
  }
}

/**
 * DELETE /api/characters/{id}/selections/{selectionId}: take back a selection of the latest level with its dependants
 */
export class RemoveSelectionIntent extends Intent<CharacterStateDto> {
  readonly method = 'DELETE'

  readonly characterId: Guid
  readonly selectionId: Guid

  /**
   * @param characterId id of the character
   * @param selectionId id of the selection (`selectedOptions[].selectionId` or GetSelectionsIntent)
   */
  constructor(characterId: Guid, selectionId: Guid) {
    super()
    this.characterId = characterId
    this.selectionId = selectionId
  }

  /** Route of the intent */
  get path() {
    return characterPath(this.characterId, `/selections/${segment(this.selectionId)}`)
  }
}

/** POST /api/characters/{id}/selections/{selectionId}/rest-replace: change a selection on a rest */
export class ReplaceSelectionOnRestIntent extends BodyIntent<RestReplaceDto, CharacterStateDto> {
  readonly method = 'POST'

  readonly characterId: Guid
  readonly selectionId: Guid

  /**
   * @param characterId id of the character
   * @param selectionId id of the selection to change (its choice allows changes on a rest)
   * @param body the new option
   */
  constructor(characterId: Guid, selectionId: Guid, body: RestReplaceDto) {
    super(body)
    this.characterId = characterId
    this.selectionId = selectionId
  }

  /** Route of the intent */
  get path() {
    return characterPath(this.characterId, `/selections/${segment(this.selectionId)}/rest-replace`)
  }
}
