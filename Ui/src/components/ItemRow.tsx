import {
  Badge,
  Box,
  Flex,
  HStack,
  Text,
} from '@chakra-ui/react'
import type { ReactNode } from 'react'
import type { ItemSummaryDto } from '@/api'
import { InfoButton } from '@/components/InfoButton'
import { useInspector } from '@/components/inspector/useInspector'
import { RarityBadge } from '@/components/RarityBadge'
import { Thumbnail } from '@/components/Thumbnail'
import { formatPrice } from '@/pages/character/coins'

/** Price of an item; items sold in bundles show the bundle size (`1 gp / 20`), artifacts have none */
const priceLabel = (item: ItemSummaryDto) => item.priceless
  ? 'Priceless'
  : `${formatPrice(item.costCp)}${item.bundle > 1 ? ` / ${item.bundle}` : ''}`

/**
 * One item in a list: picture, name with magic / rarity / attunement badges, kind · category · weight, price and an
 * optional control in front (checkbox) and actions behind (buttons). A row click opens the details; with `onClick` it
 * calls that instead, e.g. to select the item. The control and the actions keep their own clicks
 */
export const ItemRow = ({
  item,
  control,
  actions,
  onClick,
  selected,
}: {
  item: ItemSummaryDto
  control?: ReactNode
  /** buttons at the end of the row */
  actions?: ReactNode
  /** click on the row instead of opening the details */
  onClick?: () => void
  /** highlights the row, e.g. when it is selected */
  selected?: boolean
}) => {
  const { inspect } = useInspector()

  return (
    <Flex
      align='center'
      gap='3'
      px='3'
      py='2.5'
      borderTopWidth='1px'
      _first={{ borderTopWidth: '0' }}
      cursor='pointer'
      bg={selected ? 'brand.subtle' : undefined}
      _hover={selected ? undefined : { bg: 'bg.subtle' }}
      onClick={onClick ?? (() => inspect({
        kind: 'item',
        key: item.key,
      }))}
    >
      {control && (
        <Box
          display='contents'
          onClick={(event) => event.stopPropagation()}
        >
          {control}
        </Box>
      )}
      <Thumbnail src={item.thumbnailUrl} />
      <Box
        flex='1'
        minW='0'
      >
        <HStack wrap='wrap'>
          <Text fontWeight='semibold'>{item.name}</Text>
          {item.magic && <Badge colorPalette='brand'>magic</Badge>}
          <RarityBadge rarity={item.rarity} />
          {item.requiresAttunement && <Badge>attunement</Badge>}
        </HStack>
        <Text
          fontSize='sm'
          color='fg.muted'
        >
          {[item.kind, item.category, item.weight && `${item.weight} lb`].filter(Boolean).join(' · ')}
        </Text>
      </Box>
      <Text fontWeight='semibold'>{priceLabel(item)}</Text>
      <InfoButton target={{
        kind: 'item',
        key: item.key,
      }}
      />
      {actions && (
        <Box
          display='contents'
          onClick={(event) => event.stopPropagation()}
        >
          {actions}
        </Box>
      )}
    </Flex>
  )
}
