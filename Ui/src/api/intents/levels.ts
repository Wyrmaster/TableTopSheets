// Routes: Api/TableTopSheets.Api/Endpoints/LevelEndpoints.cs
import { BodyIntent, Intent, type QueryParams } from "../core/Intent"
import type { CharacterStateDto, LevelUpDto, LevelUpPreviewDto, ValidationResultDto } from "../dtos/character"
import type { Guid } from "../dtos/common"
import { characterPath } from "./characters"

/**
 * GET /api/characters/{id}/level-up/preview: choices a level in the class would open (nothing is saved). At the subclass
 * level without a subclass only `requiresSubclass` and `subclasses` are set
 */
export class PreviewLevelUpIntent extends Intent<LevelUpPreviewDto> {
  readonly method = "GET"

  readonly characterId: Guid
  readonly classKey: string
  readonly subclassKey: string | undefined

  constructor(characterId: Guid, classKey: string, subclassKey?: string) {
    super()
    this.characterId = characterId
    this.classKey = classKey
    this.subclassKey = subclassKey
  }

  get path() {
    return characterPath(this.characterId, "/level-up/preview")
  }

  override queryParams(): QueryParams {
    return { class: this.classKey, subclass: this.subclassKey }
  }
}

/**
 * POST /api/characters/{id}/levels: gain a level. Saved even when its choices are open (unless allowIncomplete is false);
 * answers posted later belong to this level
 */
export class LevelUpIntent extends BodyIntent<LevelUpDto, CharacterStateDto> {
  readonly method = "POST"

  readonly characterId: Guid

  constructor(characterId: Guid, body: LevelUpDto) {
    super(body)
    this.characterId = characterId
  }

  get path() {
    return characterPath(this.characterId, "/levels")
  }
}

/** POST /api/characters/{id}/levels/validate: check a level up without saving it */
export class ValidateLevelUpIntent extends BodyIntent<LevelUpDto, ValidationResultDto> {
  readonly method = "POST"

  readonly characterId: Guid

  constructor(characterId: Guid, body: LevelUpDto) {
    super(body)
    this.characterId = characterId
  }

  get path() {
    return characterPath(this.characterId, "/levels/validate")
  }
}

/** DELETE /api/characters/{id}/levels/last: remove the highest level with its selections */
export class UndoLevelIntent extends Intent<CharacterStateDto> {
  readonly method = "DELETE"

  readonly characterId: Guid

  constructor(characterId: Guid) {
    super()
    this.characterId = characterId
  }

  get path() {
    return characterPath(this.characterId, "/levels/last")
  }
}
