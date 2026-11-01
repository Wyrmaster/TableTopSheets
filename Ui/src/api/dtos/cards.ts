// Source: Api/Library/Contracts/CardDtos.cs

/** Body of `POST /api/cards.pdf`: the spells and items to print as cards (keys, at most 200 cards together) */
export interface CardsPdfDto {
  /** spell keys, e.g. `fireball` */
  spells: string[]
  /** item keys, e.g. `bag-of-holding` */
  items: string[]
}
