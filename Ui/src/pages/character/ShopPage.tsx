import {
  Badge,
  Box,
  Button,
  Flex,
  Heading,
  HStack,
  NumberInput,
  Spinner,
  Stack,
  Text,
} from '@chakra-ui/react'
import { useState } from 'react'
import {
  AddItemIntent,
  type ItemKindEnum,
  type ItemSummaryDto,
  ListItemsIntent,
  Rarities,
  type RarityEnum,
} from '@/api'
import { ApiHint } from '@/components/ApiHint'
import { CardCheckbox } from '@/components/CardCheckbox'
import { CardSelectionBar } from '@/components/CardSelectionBar'
import {
  FilterBar,
  FilterSelect,
  MagicSelect,
  SortSelect,
} from '@/components/FilterBar'
import { Icon } from '@/components/Icon'
import { freeIcon, shopIcon } from '@/components/icons'
import { ItemCategoryOptions } from '@/components/ItemCategoryOptions'
import { ItemRow } from '@/components/ItemRow'
import { type MagicFilter, magicParam } from '@/components/magicFilter'
import { rarityLabel } from '@/components/rarity'
import { sortParam } from '@/components/sort'
import { useApiAction } from '@/hooks/useApiAction'
import { useCardSelection } from '@/hooks/useCardSelection'
import { useIntentQuery } from '@/hooks/useIntentQuery'
import { coinValue, formatCoins, formatPrice } from './coins'
import { useCharacter } from './useCharacter'

const PageSize = 25

/** Most pieces bought or added at once (as the API allows) */
const MaxQuantity = 1000

/** Typed quantity as a number of pieces: 1 for an empty or invalid input */
const pieces = (typed: string | undefined) => Math.min(MaxQuantity, Math.max(1, Math.floor(Number(typed)) || 1))

/** Cost of the pieces in copper; items sold in bundles cost their share of the bundle, rounded up like the API */
const totalCost = (item: ItemSummaryDto, quantity: number) => Math.ceil(item.costCp * quantity / item.bundle)

/** Why "Buy" is disabled, if it is */
const buyBlocker = (item: ItemSummaryDto, quantity: number, wealth: number) => item.priceless
  ? 'Artifacts can\'t be bought'
  : totalCost(item, quantity) > wealth
    ? 'Not enough coins'
    : undefined

/**
 * Item catalog with search, filters and paging; "Buy" pays with the character's coins, "Add free" doesn't, both for the
 * number of pieces in the row's stepper. In card selection mode a row click selects the item for the card PDF instead
 * of opening its details.
 */
export const ShopPage = () => {
  const {
    id,
    state,
    applyState,
  } = useCharacter()
  const { run } = useApiAction()
  const [search, setSearch] = useState('')
  const [kind, setKind] = useState('')
  const [category, setCategory] = useState('')
  const [magic, setMagic] = useState<MagicFilter>('')
  const [rarity, setRarity] = useState('')
  const [sort, setSort] = useState('name')
  const [descending, setDescending] = useState(false)
  const [page, setPage] = useState(1)
  // typed quantity per item key; rows without one buy a single piece
  const [quantities, setQuantities] = useState<Record<string, string>>({})
  const cards = useCardSelection()

  // every filter change starts at the first page again
  const filter = <T,>(set: (value: T) => void) => (value: T) => {
    set(value)
    setPage(1)
  }

  const filtered = Boolean(search || kind || category || magic || rarity)
  const clear = () => {
    setSearch('')
    setKind('')
    setCategory('')
    setMagic('')
    setRarity('')
    setPage(1)
  }

  const items = useIntentQuery(
    () => new ListItemsIntent({
      search: search || undefined,
      kind: (kind || undefined) as ItemKindEnum | undefined,
      category: category || undefined,
      magic: magicParam(magic),
      rarity: (rarity || undefined) as RarityEnum | undefined,
      sort: sortParam(sort, descending),
      page,
      pageSize: PageSize,
    }),
    [search, kind, category, magic, rarity, sort, descending, page].join('|'),
  )

  // a purchase pays the cost (the API returns change); without it the item is free (starting equipment, loot);
  // the quantity counts pieces, also for items sold in bundles (arrows)
  const add = async (item: ItemSummaryDto, purchase: boolean) => {
    const quantity = pieces(quantities[item.key])
    const name = quantity > 1 ? `${quantity} × ${item.name}` : item.name
    const result = await run(
      new AddItemIntent(id, {
        itemKey: item.key,
        quantity,
        purchase,
      }),
      {
        success: purchase
          ? `Bought ${name} for ${formatPrice(totalCost(item, quantity))}`
          : `${name} added to the inventory`,
      },
    )
    if (result.ok) {
      applyState(result.data)
      setQuantities((current) => ({
        ...current,
        [item.key]: '1',
      }))
    }
  }

  const purse = formatCoins(state.coins)
  const wealth = coinValue(state.coins)

  return (
    <Stack gap='4'>
      <Flex
        wrap='wrap'
        gap='3'
        align='center'
        justify='space-between'
      >
        <Heading size='lg'>Shop</Heading>
        <Badge
          colorPalette='brand'
          size='lg'
        >
          Coins {purse}
        </Badge>
      </Flex>

      <FilterBar
        search={search}
        placeholder='Search items…'
        filtered={filtered}
        onSearchChange={filter(setSearch)}
        onClear={clear}
      >
        <FilterSelect
          label='Kind'
          width='150px'
          value={kind}
          onChange={filter(setKind)}
        >
          <option value=''>All kinds</option>
          <option value='Weapon'>Weapons</option>
          <option value='Armor'>Armor</option>
          <option value='Gear'>Gear</option>
          <option value='Tool'>Tools</option>
        </FilterSelect>
        <FilterSelect
          label='Category'
          width='200px'
          value={category}
          onChange={filter(setCategory)}
        >
          <ItemCategoryOptions />
        </FilterSelect>
        <MagicSelect
          value={magic}
          onChange={filter(setMagic)}
        />
        <FilterSelect
          label='Rarity'
          width='150px'
          value={rarity}
          onChange={filter(setRarity)}
        >
          <option value=''>All rarities</option>
          {Rarities.map((value) => (
            <option
              key={value}
              value={value}
            >
              {rarityLabel(value)}
            </option>
          ))}
        </FilterSelect>
        <SortSelect
          label='Sort'
          width='150px'
          value={sort}
          descending={descending}
          onChange={filter(setSort)}
          onDirectionChange={filter(setDescending)}
        >
          <option value='name'>Sort: name</option>
          <option value='kind,group,name'>Sort: type</option>
          <option value='cost'>Sort: cost</option>
          <option value='weight'>Sort: weight</option>
          <option value='rarity,name'>Sort: rarity</option>
        </SortSelect>
        <CardSelectionBar
          selection={cards}
          fileName={`${state.name} cards.pdf`}
        />
      </FilterBar>

      <Box
        borderWidth='1px'
        rounded='md'
        overflow='hidden'
      >
        {items.loading && !items.data && <Box p='3'><Spinner /></Box>}
        {items.data?.items.map((item) => {
          const quantity = pieces(quantities[item.key])
          const blocker = buyBlocker(item, quantity, wealth)
          return (
            <ItemRow
              key={item.key}
              item={item}
              selected={cards.active && cards.isSelected('item', item.key)}
              onClick={cards.active ? () => cards.toggle('item', item.key) : undefined}
              control={(
                <CardCheckbox
                  selection={cards}
                  kind='item'
                  entryKey={item.key}
                  name={item.name}
                />
              )}
              actions={(
                // phones show both buttons as icons, "Add free" below "Buy" and the quantity below both to save
                // width; selecting for cards there needs the room of the row
                <Flex
                  direction={{
                    base: 'column-reverse',
                    md: 'row',
                  }}
                  align='center'
                  gap={{
                    base: '1',
                    md: '3',
                  }}
                  hideBelow={cards.active ? 'md' : undefined}
                >
                  <NumberInput.Root
                    size='xs'
                    w='20'
                    min={1}
                    max={MaxQuantity}
                    value={quantities[item.key] ?? '1'}
                    onValueChange={(details) => setQuantities((current) => ({
                      ...current,
                      [item.key]: details.value,
                    }))}
                  >
                    <NumberInput.Control />
                    <NumberInput.Input aria-label={`Quantity of ${item.name}`} />
                  </NumberInput.Root>
                  <Button
                    size='xs'
                    variant='ghost'
                    aria-label='Add free'
                    title='Add without paying (starting equipment, loot)'
                    onClick={() => add(item, false)}
                  >
                    <Icon path={freeIcon} />
                    <Text
                      as='span'
                      hideBelow='md'
                    >
                      Add free
                    </Text>
                  </Button>
                  <Button
                    size='sm'
                    colorPalette='brand'
                    aria-label='Buy'
                    disabled={blocker !== undefined}
                    title={blocker ?? `Buy for ${formatPrice(totalCost(item, quantity))}`}
                    onClick={() => add(item, true)}
                  >
                    <Icon path={shopIcon} />
                    <Text
                      as='span'
                      hideBelow='md'
                    >
                      Buy
                    </Text>
                  </Button>
                </Flex>
              )}
            />
          )
        })}
        {items.data?.items.length === 0 && <Text
          p='3'
          color='fg.muted'
        >
          No item matches
        </Text>}
      </Box>

      {items.data && items.data.totalPages > 1 && (
        <HStack justify='center'>
          <Button
            size='sm'
            variant='outline'
            disabled={page <= 1}
            onClick={() => setPage(page - 1)}
          >
            ‹ Previous
          </Button>
          <Text>{page} / {items.data.totalPages}</Text>
          <Button
            size='sm'
            variant='outline'
            disabled={page >= items.data.totalPages}
            onClick={() => setPage(page + 1)}
          >
            Next ›
          </Button>
        </HStack>
      )}
      <ApiHint>
        <code>GET /api/rules/items?search=&amp;kind=&amp;category=&amp;magic=&amp;rarity=&amp;sort=&amp;page=</code>,
        <code> POST /api/characters/{'{id}'}/items</code> with <code>quantity</code> (pieces, 1–1000) and
        <code> purchase: true</code> (pays with change, 422 when the coins are not enough or the item is a priceless
        artifact) or without it (free); gear, tools and thrown weapons stack; cards =
        <code> POST /api/cards.pdf</code> with <code>{'{ spells, items }'}</code> (keys)
      </ApiHint>
    </Stack>
  )
}
