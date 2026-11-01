import {
  Badge,
  Box,
  Button,
  Flex,
  Heading,
  HStack,
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
import { FilterBar, FilterSelect, FilterToggle } from '@/components/FilterBar'
import { Icon } from '@/components/Icon'
import { shopIcon } from '@/components/icons'
import { InfoButton } from '@/components/InfoButton'
import { RarityBadge } from '@/components/RarityBadge'
import { rarityLabel } from '@/components/rarity'
import { useInspector } from '@/components/inspector/useInspector'
import { Thumbnail } from '@/components/Thumbnail'
import { useApiAction } from '@/hooks/useApiAction'
import { useCardSelection } from '@/hooks/useCardSelection'
import { useIntentQuery } from '@/hooks/useIntentQuery'
import { coinValue, formatCoins, formatPrice } from './coins'
import { useCharacter } from './useCharacter'

const PageSize = 25

/**
 * Values of the `category` filter of the item list: weapon and armor categories, tool groups and gear categories (magic
 * weapons and armor share them), then the groups of the other magic items
 */
const Categories: [string, string][] = [
  ['simple', 'Simple weapons'],
  ['martial', 'Martial weapons'],
  ['ammunition', 'Ammunition'],
  ['light', 'Light armor'],
  ['medium', 'Medium armor'],
  ['heavy', 'Heavy armor'],
  ['shield', 'Shields'],
  ['artisan', 'Artisan\'s tools'],
  ['gaming-set', 'Gaming sets'],
  ['musical-instrument', 'Musical instruments'],
  ['arcane-focus', 'Arcane focuses'],
  ['druidic-focus', 'Druidic focuses'],
  ['holy-symbol', 'Holy symbols'],
  ['pack', 'Packs'],
  ['food-and-drink', 'Food and drink'],
  ['mount', 'Mounts and animals'],
  ['tack', 'Tack and harness'],
  ['vehicle', 'Drawn vehicles'],
  ['ship', 'Ships'],
]

const MagicCategories: [string, string][] = [
  ['wondrous-item', 'Wondrous items'],
  ['potion', 'Potions'],
  ['ring', 'Rings'],
  ['rod', 'Rods'],
  ['scroll', 'Scrolls'],
  ['staff', 'Staffs'],
  ['wand', 'Wands'],
]

const categoryOptions = (categories: [string, string][]) => categories.map(([value, label]) => (
  <option
    key={value}
    value={value}
  >
    {label}
  </option>
))

/** Price of an item; items sold in bundles show the bundle size (`1 gp / 20`), artifacts have none */
const priceLabel = (item: ItemSummaryDto) => item.priceless
  ? 'Priceless'
  : `${formatPrice(item.costCp)}${item.bundle > 1 ? ` / ${item.bundle}` : ''}`

/** Why "Buy" is disabled, if it is */
const buyBlocker = (item: ItemSummaryDto, wealth: number) => item.priceless
  ? 'Artifacts can\'t be bought'
  : item.costCp > wealth
    ? 'Not enough coins'
    : undefined

/**
 * Item catalog with search, filters and paging; "Buy" pays with the character's coins, "Add free" doesn't. In card
 * selection mode a row click selects the item for the card PDF instead of opening its details.
 */
export const ShopPage = () => {
  const {
    id,
    state,
    applyState,
  } = useCharacter()
  const { inspect } = useInspector()
  const { run } = useApiAction()
  const [search, setSearch] = useState('')
  const [kind, setKind] = useState('')
  const [category, setCategory] = useState('')
  const [magic, setMagic] = useState(false)
  const [rarity, setRarity] = useState('')
  const [sort, setSort] = useState('name')
  const [page, setPage] = useState(1)
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
    setMagic(false)
    setRarity('')
    setPage(1)
  }

  const items = useIntentQuery(
    () => new ListItemsIntent({
      search: search || undefined,
      kind: (kind || undefined) as ItemKindEnum | undefined,
      category: category || undefined,
      magic: magic || undefined,
      rarity: (rarity || undefined) as RarityEnum | undefined,
      sort,
      page,
      pageSize: PageSize,
    }),
    [search, kind, category, magic, rarity, sort, page].join('|'),
  )

  // a purchase pays the cost (the API returns change); without it the item is free (starting equipment, loot);
  // items sold in bundles come as a whole bundle (20 arrows)
  const add = async (item: ItemSummaryDto, purchase: boolean) => {
    const name = item.bundle > 1 ? `${item.bundle} × ${item.name}` : item.name
    const result = await run(
      new AddItemIntent(id, {
        itemKey: item.key,
        quantity: item.bundle,
        purchase,
      }),
      {
        success: purchase
          ? `Bought ${name} for ${formatPrice(item.costCp)}`
          : `${name} added to the inventory`,
      },
    )
    if (result.ok) {
      applyState(result.data)
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
          <option value=''>All categories</option>
          <optgroup label='Equipment'>{categoryOptions(Categories)}</optgroup>
          <optgroup label='Magic items'>{categoryOptions(MagicCategories)}</optgroup>
        </FilterSelect>
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
        <FilterSelect
          label='Sort'
          width='160px'
          value={sort}
          onChange={setSort}
        >
          <option value='name'>Sort: name</option>
          <option value='cost'>Sort: cost</option>
          <option value='-cost'>Sort: cost ↓</option>
          <option value='weight'>Sort: weight</option>
          <option value='kind,group,name'>Sort: type</option>
          <option value='rarity,name'>Sort: rarity</option>
          <option value='-rarity,name'>Sort: rarity ↓</option>
        </FilterSelect>
        <FilterToggle
          checked={magic}
          onChange={filter(setMagic)}
        >
          Magic
        </FilterToggle>
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
          const selected = cards.active && cards.isSelected('item', item.key)
          return (
            <Flex
              key={item.key}
              align='center'
              gap='3'
              px='3'
              py='2.5'
              borderTopWidth='1px'
              _first={{ borderTopWidth: '0' }}
              cursor='pointer'
              bg={selected ? 'brand.subtle' : undefined}
              _hover={selected ? undefined : { bg: 'bg.subtle' }}
              onClick={() => cards.active
                ? cards.toggle('item', item.key)
                : inspect({
                  kind: 'item',
                  key: item.key,
                })}
            >
              <CardCheckbox
                selection={cards}
                kind='item'
                entryKey={item.key}
                name={item.name}
              />
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
              <Button
                size='xs'
                variant='ghost'
                hideBelow='md'
                title='Add without paying (starting equipment, loot)'
                onClick={(event) => {
                  event.stopPropagation()
                  add(item, false)
                }}
              >
                Add free
              </Button>
              <Button
                size='sm'
                colorPalette='brand'
                // selecting for cards on a phone needs the room of the row
                hideBelow={cards.active ? 'md' : undefined}
                disabled={buyBlocker(item, wealth) !== undefined}
                title={buyBlocker(item, wealth)}
                onClick={(event) => {
                  event.stopPropagation()
                  add(item, true)
                }}
              >
                <Icon path={shopIcon} />
                Buy
              </Button>
            </Flex>
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
        <code> POST /api/characters/{'{id}'}/items</code> with <code>purchase: true</code> (pays with change, 422 when
        the coins are not enough or the item is a priceless artifact) or without it (free); cards =
        <code> POST /api/cards.pdf</code> with <code>{'{ spells, items }'}</code> (keys)
      </ApiHint>
    </Stack>
  )
}
