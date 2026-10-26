import {
  Badge,
  Box,
  Button,
  Checkbox,
  Flex,
  Heading,
  HStack,
  Input,
  NativeSelect,
  Spinner,
  Stack,
  Text,
} from '@chakra-ui/react'
import { useState } from 'react'
import { AddItemIntent, type ItemKindEnum, type ItemSummaryDto, ListItemsIntent } from '@/api'
import { ApiHint } from '@/components/ApiHint'
import { Icon } from '@/components/Icon'
import { shopIcon } from '@/components/icons'
import { InfoButton } from '@/components/InfoButton'
import { useInspector } from '@/components/inspector/useInspector'
import { useApiAction } from '@/hooks/useApiAction'
import { useIntentQuery } from '@/hooks/useIntentQuery'
import { coinValue, formatCoins, formatPrice } from './coins'
import { useCharacter } from './useCharacter'

const PageSize = 25

/** Values of the `category` filter of the item list (weapon and armor categories, tool groups, gear categories) */
const Categories: [string, string][] = [
  ['', 'All categories'],
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

/** Price of an item; items sold in bundles show the bundle size (`1 gp / 20`) */
const priceLabel = (item: ItemSummaryDto) => `${formatPrice(item.costCp)}${item.bundle > 1 ? ` / ${item.bundle}` : ''}`

/**
 * Item catalog with search, filters and paging; "Buy" pays with the character's coins, "Add free" doesn't
 */
export function ShopPage() {
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
  const [sort, setSort] = useState('name')
  const [page, setPage] = useState(1)

  const items = useIntentQuery(
    () => new ListItemsIntent({
      search: search || undefined,
      kind: (kind || undefined) as ItemKindEnum | undefined,
      category: category || undefined,
      magic: magic || undefined,
      sort,
      page,
      pageSize: PageSize,
    }),
    `${search}|${kind}|${category}|${magic}|${sort}|${page}`,
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

      <Flex
        wrap='wrap'
        gap='3'
        align='center'
      >
        <Input
          flex='1'
          minW='200px'
          placeholder='Search items…'
          value={search}
          onChange={(event) => {
            setSearch(event.target.value)
            setPage(1)
          }}
        />
        <NativeSelect.Root
          w='150px'
        >
          <NativeSelect.Field
            value={kind}
            onChange={(event) => {
              setKind(event.target.value)
              setPage(1)
            }}
          >
            <option value=''>All kinds</option>
            <option value='Weapon'>Weapons</option>
            <option value='Armor'>Armor</option>
            <option value='Gear'>Gear</option>
            <option value='Tool'>Tools</option>
          </NativeSelect.Field>
          <NativeSelect.Indicator />
        </NativeSelect.Root>
        <NativeSelect.Root w='190px'>
          <NativeSelect.Field
            aria-label='Category'
            value={category}
            onChange={(event) => {
              setCategory(event.target.value)
              setPage(1)
            }}
          >
            {Categories.map(([value, label]) => (
              <option
                key={value}
                value={value}
              >
                {label}
              </option>
            ))}
          </NativeSelect.Field>
          <NativeSelect.Indicator />
        </NativeSelect.Root>
        <NativeSelect.Root w='150px'>
          <NativeSelect.Field
            value={sort}
            onChange={(event) => setSort(event.target.value)}
          >
            <option value='name'>Sort: name</option>
            <option value='cost'>Sort: cost</option>
            <option value='-cost'>Sort: cost ↓</option>
            <option value='weight'>Sort: weight</option>
          </NativeSelect.Field>
          <NativeSelect.Indicator />
        </NativeSelect.Root>
        <Checkbox.Root
          checked={magic}
          colorPalette='brand'
          onCheckedChange={(details) => {
            setMagic(details.checked === true)
            setPage(1)
          }}
        >
          <Checkbox.HiddenInput />
          <Checkbox.Control />
          <Checkbox.Label>Magic</Checkbox.Label>
        </Checkbox.Root>
      </Flex>

      <Box
        borderWidth='1px'
        rounded='md'
        overflow='hidden'
      >
        {items.loading && !items.data && <Box p='3'><Spinner /></Box>}
        {items.data?.items.map((item) => (
          <Flex
            key={item.key}
            align='center'
            gap='3'
            px='3'
            py='2.5'
            borderTopWidth='1px'
            _first={{ borderTopWidth: '0' }}
            cursor='pointer'
            _hover={{ bg: 'bg.subtle' }}
            onClick={() => inspect({
              kind: 'item',
              key: item.key,
            })}
          >
            <Box
              flex='1'
              minW='0'
            >
              <HStack>
                <Text fontWeight='semibold'>{item.name}</Text>
                {item.magic && <Badge colorPalette='brand'>magic</Badge>}
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
              disabled={item.costCp > wealth}
              title={item.costCp > wealth ? 'Not enough coins' : undefined}
              onClick={(event) => {
                event.stopPropagation()
                add(item, true)
              }}
            >
              <Icon path={shopIcon} />
              Buy
            </Button>
          </Flex>
        ))}
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
        <code>GET /api/rules/items?search=&amp;kind=&amp;category=&amp;magic=&amp;sort=&amp;page=</code>,
        <code> POST /api/characters/{'{id}'}/items</code> with <code>purchase: true</code> (pays with change, 422 when
        the coins are not enough) or without it (free)
      </ApiHint>
    </Stack>
  )
}
