// Routes: Api/Library/Endpoints/CardEndpoints.cs
import { BodyIntent } from '../core/Intent'
import type { CardsPdfDto } from '../dtos/cards'

/**
 * POST /api/cards.pdf: printable cards of spells and items as PDF blob (422 for unknown keys or more than 200
 * cards)
 */
export class CreateCardsPdfIntent extends BodyIntent<CardsPdfDto, Blob> {
  readonly method = 'POST'
  override readonly responseKind = 'blob'

  /**
   * @param body keys of the spells and items to print
   */
  constructor(body: CardsPdfDto) {
    super(body)
  }

  /** Route of the intent */
  get path() {
    return '/api/cards.pdf'
  }
}
