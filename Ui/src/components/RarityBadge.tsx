import { Badge, type BadgeProps } from '@chakra-ui/react'
import type { RarityEnum } from '@/api'
import { Rarity, rarityLabel } from './rarity'

/**
 * Rarity of a magic item as a colored badge; nothing for mundane items (`null`)
 */
export const RarityBadge = ({ rarity, ...props }: { rarity: RarityEnum | null } & BadgeProps) => {
  if (!rarity) {
    return null
  }
  return (
    <Badge
      variant='subtle'
      colorPalette={Rarity[rarity].palette}
      {...props}
    >
      {rarityLabel(rarity)}
    </Badge>
  )
}
