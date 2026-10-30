import type { StartingEquipmentItemDto, StartingEquipmentOptionDto, TakeStartingEquipmentDto } from '@/api'

/** Coins of an alternative as text (always whole gold pieces in the rule book) */
export const formatGold = (cp: number) => (cp % 100 === 0 ? `${cp / 100} gp` : `${cp} cp`)

/** Entries the player has to pick an item for (no fixed item and no tool chosen through "same as above") */
export const openPicks = (option: StartingEquipmentOptionDto) => option.items.filter((entry) => !entry.itemKey)

/** Name of an entry: the item, or the item already chosen for a pick, with quantity and note */
export const entryLabel = (entry: StartingEquipmentItemDto) => {
  const chosen = entry.options.length && entry.itemKey
    ? entry.options.find((option) => option.key === entry.itemKey)?.name
    : null
  const name = chosen ?? entry.name
  return [
    entry.quantity > 1 ? `${entry.quantity} × ${name}` : name,
    entry.note && `(${entry.note})`,
  ]
    .filter(Boolean)
    .join(' ')
}

/** The alternative as one line: `Spear, 2 × Dagger, Arcane Focus (Crystal) + 28 gp` */
export const optionSummary = (option: StartingEquipmentOptionDto) => {
  const items = option.items.map(entryLabel).join(', ')
  const coins = option.coinsCp ? formatGold(option.coinsCp) : ''
  return [items, coins].filter(Boolean).join(' + ') || 'nothing'
}

/** True when nothing is chosen (decide later) or every pick of the chosen alternative is made */
export const equipmentComplete = (options: StartingEquipmentOptionDto[], value: TakeStartingEquipmentDto | null) => {
  const option = value && options.find((entry) => entry.key === value.optionKey)
  return !value || Boolean(option && openPicks(option).every((entry) => value.picks?.[entry.key]))
}
