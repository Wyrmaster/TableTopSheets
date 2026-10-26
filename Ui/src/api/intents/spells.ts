// Routes: Api/TableTopSheets.Api/Endpoints/SpellEndpoints.cs
import { BodyIntent, segment } from '../core/Intent'
import type { CharacterStateDto } from '../dtos/character'
import type { Guid } from '../dtos/common'
import type { PreparedSpellsDto, SpellbookEntryDto } from '../dtos/inventory'
import { characterPath } from './characters'

/** PUT /api/characters/{id}/prepared-spells/{classKey}: replace the prepared spells of a class (long rest) */
export class SetPreparedSpellsIntent extends BodyIntent<PreparedSpellsDto, CharacterStateDto> {
  readonly method = 'PUT'

  readonly characterId: Guid
  readonly classKey: string

  /**
   * @param characterId id of the character
   * @param classKey class whose prepared spells change, e.g. `wizard`
   * @param body the complete new list of prepared spell keys
   */
  constructor(characterId: Guid, classKey: string, body: PreparedSpellsDto) {
    super(body)
    this.characterId = characterId
    this.classKey = classKey
  }

  /** Route of the intent */
  get path() {
    return characterPath(this.characterId, `/prepared-spells/${segment(this.classKey)}`)
  }
}

/** POST /api/characters/{id}/spellbook: copy a spell into the spellbook */
export class CopyToSpellbookIntent extends BodyIntent<SpellbookEntryDto, CharacterStateDto> {
  readonly method = 'POST'

  readonly characterId: Guid

  /**
   * @param characterId id of the character
   * @param body spell key and an optional note (e.g. where the scroll was found)
   */
  constructor(characterId: Guid, body: SpellbookEntryDto) {
    super(body)
    this.characterId = characterId
  }

  /** Route of the intent */
  get path() {
    return characterPath(this.characterId, '/spellbook')
  }
}
