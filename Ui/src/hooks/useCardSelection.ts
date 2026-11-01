import { useCallback, useMemo, useState } from 'react'

/** What a card shows: a spell or an item of the rules */
export type CardKind = 'spell' | 'item'

/** Selection state and actions of `useCardSelection` */
export interface CardSelection {
  /** whether the rows show checkboxes and a row click selects */
  active: boolean
  setActive: (active: boolean) => void
  /** selected spell keys, in the order they were picked */
  spells: string[]
  /** selected item keys, in the order they were picked */
  items: string[]
  /** number of selected spells and items */
  count: number
  isSelected: (kind: CardKind, key: string) => boolean
  toggle: (kind: CardKind, key: string) => void
  clear: () => void
}

const toggled = (keys: string[], key: string) => keys.includes(key)
  ? keys.filter((entry) => entry !== key)
  : [...keys, key]

/**
 * Spells and items picked for printable cards. The keys stay selected across pages and filter changes (the selection
 * lives in the page, not in the list); leaving selection mode keeps them as well, `clear` empties it.
 */
export const useCardSelection = (): CardSelection => {
  const [active, setActive] = useState(false)
  const [spells, setSpells] = useState<string[]>([])
  const [items, setItems] = useState<string[]>([])

  const isSelected = useCallback(
    (kind: CardKind, key: string) => (kind === 'spell' ? spells : items).includes(key),
    [spells, items],
  )

  const toggle = useCallback((kind: CardKind, key: string) => {
    if (kind === 'spell') {
      setSpells((current) => toggled(current, key))
    }
    else {
      setItems((current) => toggled(current, key))
    }
  }, [])

  const clear = useCallback(() => {
    setSpells([])
    setItems([])
  }, [])

  return useMemo(() => ({
    active,
    setActive,
    spells,
    items,
    count: spells.length + items.length,
    isSelected,
    toggle,
    clear,
  }), [active, spells, items, isSelected, toggle, clear])
}
