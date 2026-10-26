// Routes: Api/TableTopSheets.Api/Endpoints/CharacterEndpoints.cs
import { BodyIntent, Intent, PagedIntent, segment } from "../core/Intent"
import type { CharacterFilter, CharacterStateDto, CharacterSummaryDto, CreateCharacterDto, UpdateCharacterDto } from "../dtos/character"
import type { Guid } from "../dtos/common"
import type { ListQuery } from "../dtos/paging"
import type { CharacterSheet } from "../dtos/sheet"

/**
 * Path of a character route
 */
export function characterPath(characterId: Guid, suffix = ""): string {
  return `/api/characters/${segment(characterId)}${suffix}`
}

/** GET /api/characters: characters of the signed in user (sort: name, level, species) */
export class ListCharactersIntent extends PagedIntent<CharacterSummaryDto, CharacterFilter> {
  constructor(query: ListQuery & CharacterFilter = {}) {
    super(query)
  }

  get path() {
    return "/api/characters"
  }
}

/** POST /api/characters: create a draft from its origin; the answer lists the pending choices (201) */
export class CreateCharacterIntent extends BodyIntent<CreateCharacterDto, CharacterStateDto> {
  readonly method = "POST"

  constructor(body: CreateCharacterDto) {
    super(body)
  }

  get path() {
    return "/api/characters"
  }
}

/** GET /api/characters/{id}: current state with pending choices */
export class GetCharacterIntent extends Intent<CharacterStateDto> {
  readonly method = "GET"

  readonly characterId: Guid

  constructor(characterId: Guid) {
    super()
    this.characterId = characterId
  }

  get path() {
    return characterPath(this.characterId)
  }
}

/** PATCH /api/characters/{id}: name, alignment, appearance, backstory or experience */
export class UpdateCharacterIntent extends BodyIntent<UpdateCharacterDto, CharacterStateDto> {
  readonly method = "PATCH"

  readonly characterId: Guid

  constructor(characterId: Guid, body: UpdateCharacterDto) {
    super(body)
    this.characterId = characterId
  }

  get path() {
    return characterPath(this.characterId)
  }
}

/** DELETE /api/characters/{id} (204) */
export class DeleteCharacterIntent extends Intent<void> {
  readonly method = "DELETE"
  override readonly responseKind = "none"

  readonly characterId: Guid

  constructor(characterId: Guid) {
    super()
    this.characterId = characterId
  }

  get path() {
    return characterPath(this.characterId)
  }
}

/** GET /api/characters/{id}/sheet: every computed value of the character sheet */
export class GetCharacterSheetIntent extends Intent<CharacterSheet> {
  readonly method = "GET"

  readonly characterId: Guid

  constructor(characterId: Guid) {
    super()
    this.characterId = characterId
  }

  get path() {
    return characterPath(this.characterId, "/sheet")
  }
}

/** GET /api/characters/{id}/sheet.pdf: printable character sheet as PDF blob */
export class GetCharacterSheetPdfIntent extends Intent<Blob> {
  readonly method = "GET"
  override readonly responseKind = "blob"

  readonly characterId: Guid

  constructor(characterId: Guid) {
    super()
    this.characterId = characterId
  }

  get path() {
    return characterPath(this.characterId, "/sheet.pdf")
  }
}
