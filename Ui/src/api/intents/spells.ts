// Routes: Api/TableTopSheets.Api/Endpoints/SpellEndpoints.cs
import { BodyIntent, segment } from "../core/Intent"
import type { CharacterStateDto } from "../dtos/character"
import type { Guid } from "../dtos/common"
import type { PreparedSpellsDto, SpellbookEntryDto } from "../dtos/inventory"
import { characterPath } from "./characters"

/** PUT /api/characters/{id}/prepared-spells/{classKey}: replace the prepared spells of a class (long rest) */
export class SetPreparedSpellsIntent extends BodyIntent<PreparedSpellsDto, CharacterStateDto> {
  readonly method = "PUT"

  readonly characterId: Guid
  readonly classKey: string

  constructor(characterId: Guid, classKey: string, body: PreparedSpellsDto) {
    super(body)
    this.characterId = characterId
    this.classKey = classKey
  }

  get path() {
    return characterPath(this.characterId, `/prepared-spells/${segment(this.classKey)}`)
  }
}

/** POST /api/characters/{id}/spellbook: copy a spell into the spellbook */
export class CopyToSpellbookIntent extends BodyIntent<SpellbookEntryDto, CharacterStateDto> {
  readonly method = "POST"

  readonly characterId: Guid

  constructor(characterId: Guid, body: SpellbookEntryDto) {
    super(body)
    this.characterId = characterId
  }

  get path() {
    return characterPath(this.characterId, "/spellbook")
  }
}
