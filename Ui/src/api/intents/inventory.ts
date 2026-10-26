// Routes: Api/TableTopSheets.Api/Endpoints/InventoryEndpoints.cs
import { BodyIntent, Intent, type QueryParams, segment } from '../core/Intent'
import type { CharacterStateDto } from '../dtos/character'
import type { Guid } from '../dtos/common'
import type {
  AddItemDto,
  AddToStackDto,
  ArmorClassDto,
  AttuneDto,
  CoinPaymentDto,
  CoinsDto,
  EquipDto,
  ToggleDto,
} from '../dtos/inventory'
import type { StartingEquipmentSourceEnum } from '../dtos/enums'
import type { StartingEquipmentDto, TakeStartingEquipmentDto } from '../dtos/startingEquipment'
import { characterPath } from './characters'

/** GET /api/characters/{id}/starting-equipment: starting equipment of the background and the first class */
export class GetStartingEquipmentIntent extends Intent<StartingEquipmentDto[]> {
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
    return characterPath(this.characterId, '/starting-equipment')
  }
}

/** POST /api/characters/{id}/starting-equipment/{source}: take an alternative (once per source, 422 otherwise) */
export class TakeStartingEquipmentIntent extends BodyIntent<TakeStartingEquipmentDto, CharacterStateDto> {
  readonly method = 'POST'

  readonly characterId: Guid
  readonly source: StartingEquipmentSourceEnum

  /**
   * @param characterId id of the character
   * @param source `Background` or `Class`
   * @param body the alternative and the picks per entry key
   */
  constructor(characterId: Guid, source: StartingEquipmentSourceEnum, body: TakeStartingEquipmentDto) {
    super(body)
    this.characterId = characterId
    this.source = source
  }

  /** Route of the intent */
  get path() {
    return characterPath(this.characterId, `/starting-equipment/${segment(this.source)}`)
  }
}

/** DELETE /api/characters/{id}/items/{itemId}: remove an item or part of a stack; selling credits half the cost */
export class RemoveItemIntent extends Intent<CharacterStateDto> {
  readonly method = 'DELETE'

  readonly characterId: Guid
  readonly itemId: Guid
  readonly options: { quantity?: number, sell?: boolean }

  /**
   * @param characterId id of the character
   * @param itemId id of the owned item
   * @param options `quantity` to remove (default: the whole stack), `sell` to credit half the cost per piece
   */
  constructor(characterId: Guid, itemId: Guid, options: { quantity?: number, sell?: boolean } = {}) {
    super()
    this.characterId = characterId
    this.itemId = itemId
    this.options = options
  }

  /** Route of the intent */
  get path() {
    return characterPath(this.characterId, `/items/${segment(this.itemId)}`)
  }

  /** `quantity` and `sell` when set */
  override queryParams(): QueryParams {
    return {
      quantity: this.options.quantity,
      sell: this.options.sell,
    }
  }
}

/** PUT /api/characters/{id}/coins: replace the coins */
export class SetCoinsIntent extends BodyIntent<CoinsDto, CharacterStateDto> {
  readonly method = 'PUT'

  readonly characterId: Guid

  /**
   * @param characterId id of the character
   * @param body the new coins
   */
  constructor(characterId: Guid, body: CoinsDto) {
    super(body)
    this.characterId = characterId
  }

  /** Route of the intent */
  get path() {
    return characterPath(this.characterId, '/coins')
  }
}

/** POST /api/characters/{id}/coins/change: add (positive) or remove (negative) coins of each kind */
export class ChangeCoinsIntent extends BodyIntent<Partial<CoinsDto>, CharacterStateDto> {
  readonly method = 'POST'

  readonly characterId: Guid

  /**
   * @param characterId id of the character
   * @param body coins per kind to add or remove; missing kinds stay as they are
   */
  constructor(characterId: Guid, body: Partial<CoinsDto>) {
    super(body)
    this.characterId = characterId
  }

  /** Route of the intent */
  get path() {
    return characterPath(this.characterId, '/coins/change')
  }
}

/** POST /api/characters/{id}/coins/payments: pay an amount in copper pieces, with change (422 when not enough) */
export class PayCoinsIntent extends BodyIntent<CoinPaymentDto, CharacterStateDto> {
  readonly method = 'POST'

  readonly characterId: Guid

  /**
   * @param characterId id of the character
   * @param body amount in copper pieces
   */
  constructor(characterId: Guid, body: CoinPaymentDto) {
    super(body)
    this.characterId = characterId
  }

  /** Route of the intent */
  get path() {
    return characterPath(this.characterId, '/coins/payments')
  }
}

/** POST /api/characters/{id}/items: add an item to the inventory */
export class AddItemIntent extends BodyIntent<AddItemDto, CharacterStateDto> {
  readonly method = 'POST'

  readonly characterId: Guid

  /**
   * @param characterId id of the character
   * @param body item key from the rules catalog and quantity
   */
  constructor(characterId: Guid, body: AddItemDto) {
    super(body)
    this.characterId = characterId
  }

  /** Route of the intent */
  get path() {
    return characterPath(this.characterId, '/items')
  }
}

/** POST /api/characters/{id}/items/{itemId}/stack: add pieces to a stack without paying (recovered ammunition) */
export class AddToStackIntent extends BodyIntent<AddToStackDto, CharacterStateDto> {
  readonly method = 'POST'

  readonly characterId: Guid
  readonly itemId: Guid

  /**
   * @param characterId id of the character
   * @param itemId id of the owned item
   * @param body pieces to add
   */
  constructor(characterId: Guid, itemId: Guid, body: AddToStackDto) {
    super(body)
    this.characterId = characterId
    this.itemId = itemId
  }

  /** Route of the intent */
  get path() {
    return characterPath(this.characterId, `/items/${segment(this.itemId)}/stack`)
  }
}

/** PUT /api/characters/{id}/items/{itemId}/slot: equip an item (slot None unequips it) */
export class EquipItemIntent extends BodyIntent<EquipDto, CharacterStateDto> {
  readonly method = 'PUT'

  readonly characterId: Guid
  readonly itemId: Guid

  /**
   * @param characterId id of the character
   * @param itemId id of the owned item (`items[].id` of the state, not the item key)
   * @param body the slot
   */
  constructor(characterId: Guid, itemId: Guid, body: EquipDto) {
    super(body)
    this.characterId = characterId
    this.itemId = itemId
  }

  /** Route of the intent */
  get path() {
    return characterPath(this.characterId, `/items/${segment(this.itemId)}/slot`)
  }
}

/** PUT /api/characters/{id}/items/{itemId}/attunement: attune to an item or end the attunement (max 3) */
export class AttuneItemIntent extends BodyIntent<AttuneDto, CharacterStateDto> {
  readonly method = 'PUT'

  readonly characterId: Guid
  readonly itemId: Guid

  /**
   * @param characterId id of the character
   * @param itemId id of the owned item
   * @param body attuned or not
   */
  constructor(characterId: Guid, itemId: Guid, body: AttuneDto) {
    super(body)
    this.characterId = characterId
    this.itemId = itemId
  }

  /** Route of the intent */
  get path() {
    return characterPath(this.characterId, `/items/${segment(this.itemId)}/attunement`)
  }
}

/** PUT /api/characters/{id}/toggles/{featureKey}: switch a toggleable feature (Rage, Mage Armor ...) */
export class ToggleFeatureIntent extends BodyIntent<ToggleDto, CharacterStateDto> {
  readonly method = 'PUT'

  readonly characterId: Guid
  readonly featureKey: string

  /**
   * @param characterId id of the character
   * @param featureKey key of the toggleable feature or spell effect
   * @param body on or off
   */
  constructor(characterId: Guid, featureKey: string, body: ToggleDto) {
    super(body)
    this.characterId = characterId
    this.featureKey = featureKey
  }

  /** Route of the intent */
  get path() {
    return characterPath(this.characterId, `/toggles/${segment(this.featureKey)}`)
  }
}

/** PUT /api/characters/{id}/armor-class: choose the armor class calculation by label (null = the best) */
export class ChooseArmorClassIntent extends BodyIntent<ArmorClassDto, CharacterStateDto> {
  readonly method = 'PUT'

  readonly characterId: Guid

  /**
   * @param characterId id of the character
   * @param body label of the calculation (from `armorClassOptions` of the sheet), or null for the best one
   */
  constructor(characterId: Guid, body: ArmorClassDto) {
    super(body)
    this.characterId = characterId
  }

  /** Route of the intent */
  get path() {
    return characterPath(this.characterId, '/armor-class')
  }
}
