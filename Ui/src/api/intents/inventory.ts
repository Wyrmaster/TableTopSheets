// Routes: Api/TableTopSheets.Api/Endpoints/InventoryEndpoints.cs
import { BodyIntent, segment } from "../core/Intent"
import type { CharacterStateDto } from "../dtos/character"
import type { Guid } from "../dtos/common"
import type { AddItemDto, ArmorClassDto, AttuneDto, EquipDto, ToggleDto } from "../dtos/inventory"
import { characterPath } from "./characters"

/** POST /api/characters/{id}/items: add an item to the inventory */
export class AddItemIntent extends BodyIntent<AddItemDto, CharacterStateDto> {
  readonly method = "POST"

  readonly characterId: Guid

  constructor(characterId: Guid, body: AddItemDto) {
    super(body)
    this.characterId = characterId
  }

  get path() {
    return characterPath(this.characterId, "/items")
  }
}

/** PUT /api/characters/{id}/items/{itemId}/slot: equip an item (slot None unequips it) */
export class EquipItemIntent extends BodyIntent<EquipDto, CharacterStateDto> {
  readonly method = "PUT"

  readonly characterId: Guid
  readonly itemId: Guid

  constructor(characterId: Guid, itemId: Guid, body: EquipDto) {
    super(body)
    this.characterId = characterId
    this.itemId = itemId
  }

  get path() {
    return characterPath(this.characterId, `/items/${segment(this.itemId)}/slot`)
  }
}

/** PUT /api/characters/{id}/items/{itemId}/attunement: attune to an item or end the attunement (max 3) */
export class AttuneItemIntent extends BodyIntent<AttuneDto, CharacterStateDto> {
  readonly method = "PUT"

  readonly characterId: Guid
  readonly itemId: Guid

  constructor(characterId: Guid, itemId: Guid, body: AttuneDto) {
    super(body)
    this.characterId = characterId
    this.itemId = itemId
  }

  get path() {
    return characterPath(this.characterId, `/items/${segment(this.itemId)}/attunement`)
  }
}

/** PUT /api/characters/{id}/toggles/{featureKey}: switch a toggleable feature (Rage, Mage Armor ...) */
export class ToggleFeatureIntent extends BodyIntent<ToggleDto, CharacterStateDto> {
  readonly method = "PUT"

  readonly characterId: Guid
  readonly featureKey: string

  constructor(characterId: Guid, featureKey: string, body: ToggleDto) {
    super(body)
    this.characterId = characterId
    this.featureKey = featureKey
  }

  get path() {
    return characterPath(this.characterId, `/toggles/${segment(this.featureKey)}`)
  }
}

/** PUT /api/characters/{id}/armor-class: choose the armor class calculation by label (null = the best) */
export class ChooseArmorClassIntent extends BodyIntent<ArmorClassDto, CharacterStateDto> {
  readonly method = "PUT"

  readonly characterId: Guid

  constructor(characterId: Guid, body: ArmorClassDto) {
    super(body)
    this.characterId = characterId
  }

  get path() {
    return characterPath(this.characterId, "/armor-class")
  }
}
