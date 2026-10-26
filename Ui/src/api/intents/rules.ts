// Routes: Api/TableTopSheets.Api/Endpoints/RulesEndpoints.cs. The rules catalog doesn't need a token
import { Intent, PagedIntent, segment } from '../core/Intent'
import type { ListQuery } from '../dtos/paging'
import type {
  BackgroundSummaryDto,
  ClassDetailDto,
  ClassSummaryDto,
  FeatFilter,
  FeatSummaryDto,
  ItemDetailDto,
  ItemFilter,
  ItemSummaryDto,
  RuleDetailDto,
  SpeciesSummaryDto,
  SpellDetailDto,
  SpellFilter,
  SpellSummaryDto,
} from '../dtos/rules'

/**
 * Details of one rule entity by key; derived classes only name their collection
 */
abstract class RuleDetailIntent<T> extends Intent<T> {
  readonly method = 'GET'
  override readonly anonymous = true

  /** Key of the entity, e.g. `wizard` or `fire-bolt` */
  readonly key: string

  /** Collection below `/api/rules`, e.g. `classes` */
  protected abstract readonly collection: string

  /**
   * @param key key of the entity, e.g. `wizard` or `fire-bolt`
   */
  constructor(key: string) {
    super()
    this.key = key
  }

  /** `/api/rules/{collection}/{key}` */
  get path() {
    return `/api/rules/${this.collection}/${segment(this.key)}`
  }
}

/** GET /api/rules/species (sort: name, key) */
export class ListSpeciesIntent extends PagedIntent<SpeciesSummaryDto> {
  override readonly anonymous = true

  /**
   * @param query paging, search and sort
   */
  constructor(query: ListQuery = {}) {
    super(query)
  }

  /** Route of the intent */
  get path() {
    return '/api/rules/species'
  }
}

/** GET /api/rules/species/{key}: species with its traits and choices */
export class GetSpeciesIntent extends RuleDetailIntent<RuleDetailDto> {
  protected readonly collection = 'species'
}

/** GET /api/rules/backgrounds (sort: name, key) */
export class ListBackgroundsIntent extends PagedIntent<BackgroundSummaryDto> {
  override readonly anonymous = true

  /**
   * @param query paging, search and sort
   */
  constructor(query: ListQuery = {}) {
    super(query)
  }

  /** Route of the intent */
  get path() {
    return '/api/rules/backgrounds'
  }
}

/** GET /api/rules/backgrounds/{key}: background with its features and choices */
export class GetBackgroundIntent extends RuleDetailIntent<RuleDetailDto> {
  protected readonly collection = 'backgrounds'
}

/** GET /api/rules/classes: classes with their subclasses (sort: name, key) */
export class ListClassesIntent extends PagedIntent<ClassSummaryDto> {
  override readonly anonymous = true

  /**
   * @param query paging, search and sort
   */
  constructor(query: ListQuery = {}) {
    super(query)
  }

  /** Route of the intent */
  get path() {
    return '/api/rules/classes'
  }
}

/** GET /api/rules/classes/{key}: class with its features per level and subclasses */
export class GetClassIntent extends RuleDetailIntent<ClassDetailDto> {
  protected readonly collection = 'classes'
}

/** GET /api/rules/feats (sort: name, key, category) */
export class ListFeatsIntent extends PagedIntent<FeatSummaryDto, FeatFilter> {
  override readonly anonymous = true

  /**
   * @param query paging, search, sort and filters (`category`, `repeatable`, `hasPrerequisite`)
   */
  constructor(query: ListQuery & FeatFilter = {}) {
    super(query)
  }

  /** Route of the intent */
  get path() {
    return '/api/rules/feats'
  }
}

/** GET /api/rules/feats/{key}: feat with its benefits and choices */
export class GetFeatIntent extends RuleDetailIntent<RuleDetailDto> {
  protected readonly collection = 'feats'
}

/** GET /api/rules/spells (sort: name, key, level, school) */
export class ListSpellsIntent extends PagedIntent<SpellSummaryDto, SpellFilter> {
  override readonly anonymous = true

  /**
   * @param query paging, search, sort and filters (`class`, levels, `school`, `ritual`, `concentration`, damage,
   *   casting time)
   */
  constructor(query: ListQuery & SpellFilter = {}) {
    super(query)
  }

  /** Route of the intent */
  get path() {
    return '/api/rules/spells'
  }
}

/** GET /api/rules/spells/{key}: spell with its full text */
export class GetSpellIntent extends RuleDetailIntent<SpellDetailDto> {
  protected readonly collection = 'spells'
}

/** GET /api/rules/items (sort: name, key, kind, cost, weight) */
export class ListItemsIntent extends PagedIntent<ItemSummaryDto, ItemFilter> {
  override readonly anonymous = true

  /**
   * @param query paging, search, sort and filters (`kind`, `category`, `magic`, `attunement`)
   */
  constructor(query: ListQuery & ItemFilter = {}) {
    super(query)
  }

  /** Route of the intent */
  get path() {
    return '/api/rules/items'
  }
}

/** GET /api/rules/items/{key}: item with its full text and properties */
export class GetItemIntent extends RuleDetailIntent<ItemDetailDto> {
  protected readonly collection = 'items'
}
