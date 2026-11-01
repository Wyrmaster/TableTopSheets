import type { RarityEnum } from '@/api'

/** Label and color of each rarity (display only, the rarity itself comes from the API) */
export const Rarity: Record<RarityEnum, { label: string, palette: string }> = {
  Common: {
    label: 'Common',
    palette: 'gray',
  },
  Uncommon: {
    label: 'Uncommon',
    palette: 'green',
  },
  Rare: {
    label: 'Rare',
    palette: 'blue',
  },
  VeryRare: {
    label: 'Very rare',
    palette: 'purple',
  },
  Legendary: {
    label: 'Legendary',
    palette: 'orange',
  },
  Artifact: {
    label: 'Artifact',
    palette: 'red',
  },
}

/** Readable name of a rarity (`VeryRare` → "Very rare") */
export const rarityLabel = (rarity: RarityEnum) => Rarity[rarity].label
