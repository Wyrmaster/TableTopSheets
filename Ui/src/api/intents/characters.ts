// Routes: Api/Library/Endpoints/CharacterEndpoints.cs
import { BodyIntent, Intent, PagedIntent, segment } from '../core/Intent'
import type {
  CharacterFilter,
  CharacterStateDto,
  CharacterSummaryDto,
  CreateCharacterDto,
  UpdateCharacterDto,
} from '../dtos/character'
import type { Guid } from '../dtos/common'
import type { ListQuery } from '../dtos/paging'
import type { CharacterSheet } from '../dtos/sheet'

/**
 * Path of a character route
 *
 * @param characterId id of the character
 * @param suffix rest of the route, e.g. `/choices`
 */
export const characterPath = (characterId: Guid, suffix = ''): string => {
  return `/api/characters/${segment(characterId)}${suffix}`
}

/** GET /api/characters: characters of the signed in user (sort: name, level, species) */
export class ListCharactersIntent extends PagedIntent<CharacterSummaryDto, CharacterFilter> {
  /**
   * @param query paging, search, sort and filters (`class`, `species`, `minLevel`, `maxLevel`)
   */
  constructor(query: ListQuery & CharacterFilter = {}) {
    super(query)
  }

  /** Route of the intent */
  get path() {
    return '/api/characters'
  }
}

/** POST /api/characters: create a draft from its origin; the answer lists the pending choices (201) */
export class CreateCharacterIntent extends BodyIntent<CreateCharacterDto, CharacterStateDto> {
  readonly method = 'POST'

  /**
   * @param body name, species, background and base ability scores; origin choices may be sent later
   */
  constructor(body: CreateCharacterDto) {
    super(body)
  }

  /** Route of the intent */
  get path() {
    return '/api/characters'
  }
}

/** GET /api/characters/{id}: current state with pending choices */
export class GetCharacterIntent extends Intent<CharacterStateDto> {
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
    return characterPath(this.characterId)
  }
}

/** PATCH /api/characters/{id}: name, alignment, appearance, backstory or experience */
export class UpdateCharacterIntent extends BodyIntent<UpdateCharacterDto, CharacterStateDto> {
  readonly method = 'PATCH'

  readonly characterId: Guid

  /**
   * @param characterId id of the character
   * @param body fields to change; omitted fields keep their value
   */
  constructor(characterId: Guid, body: UpdateCharacterDto) {
    super(body)
    this.characterId = characterId
  }

  /** Route of the intent */
  get path() {
    return characterPath(this.characterId)
  }
}

/** DELETE /api/characters/{id} (204) */
export class DeleteCharacterIntent extends Intent<void> {
  readonly method = 'DELETE'
  override readonly responseKind = 'none'

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
    return characterPath(this.characterId)
  }
}

/** GET /api/characters/{id}/sheet: every computed value of the character sheet */
export class GetCharacterSheetIntent extends Intent<CharacterSheet> {
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
    return characterPath(this.characterId, '/sheet')
  }
}

/** GET /api/characters/{id}/sheet.pdf: printable character sheet as PDF blob */
export class GetCharacterSheetPdfIntent extends Intent<Blob> {
  readonly method = 'GET'
  override readonly responseKind = 'blob'

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
    return characterPath(this.characterId, '/sheet.pdf')
  }
}
