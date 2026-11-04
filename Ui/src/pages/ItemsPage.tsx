import {
  Box,
  Heading,
  Spinner,
  Stack,
  Text,
} from '@chakra-ui/react'
import { useRef, useState } from 'react'
import {
  type ItemKindEnum,
  ListItemsIntent,
  problemMessages,
  Rarities,
  type RarityEnum,
} from '@/api'
import { ApiHint } from '@/components/ApiHint'
import { CardCheckbox } from '@/components/CardCheckbox'
import { CardSelectionBar } from '@/components/CardSelectionBar'
import {
  FilterBar,
  FilterSelect,
  FilterToggle,
  MagicSelect,
  SortSelect,
} from '@/components/FilterBar'
import { type MagicFilter, magicParam } from '@/components/magicFilter'
import { ItemCategoryOptions } from '@/components/ItemCategoryOptions'
import { ItemRow } from '@/components/ItemRow'
import { ListPagination } from '@/components/ListPagination'
import { PageSizes } from '@/components/pageSizes'
import { rarityLabel } from '@/components/rarity'
import { SceneBanner } from '@/components/SceneBanner'
import { sortParam } from '@/components/sort'
import { useCardSelection } from '@/hooks/useCardSelection'
import { useIntentQuery } from '@/hooks/useIntentQuery'

/**
 * Every item of the rules, without a character: search and filters (kind, category, rarity, magic, attunement); an item
 * opens in the detail view. In card selection mode a click selects the item for the card PDF instead (kept across pages
 * and filters).
 */
export const ItemsPage = () => {
  const [search, setSearch] = useState('')
  const [kind, setKind] = useState('')
  const [category, setCategory] = useState('')
  const [rarity, setRarity] = useState('')
  const [magic, setMagic] = useState<MagicFilter>('')
  const [attunement, setAttunement] = useState(false)
  const [sort, setSort] = useState('name')
  const [descending, setDescending] = useState(false)
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(PageSizes[0])
  const listTop = useRef<HTMLDivElement>(null)
  const cards = useCardSelection()

  // every filter change starts at the first page again
  const filter = <T, >(set: (value: T) => void) => (value: T) => {
    set(value)
    setPage(1)
  }

  const filtered = Boolean(search || kind || category || rarity || magic || attunement)
  const clear = () => {
    setSearch('')
    setKind('')
    setCategory('')
    setRarity('')
    setMagic('')
    setAttunement(false)
    setPage(1)
  }

  // a new page starts at the top of the list (the bottom pagination is far below it)
  const changePage = (next: number) => {
    setPage(next)
    if (listTop.current && listTop.current.getBoundingClientRect().top < 0) {
      listTop.current.scrollIntoView({ behavior: 'smooth' })
    }
  }

  const items = useIntentQuery(
    () => new ListItemsIntent({
      search: search || undefined,
      kind: (kind || undefined) as ItemKindEnum | undefined,
      category: category || undefined,
      rarity: (rarity || undefined) as RarityEnum | undefined,
      magic: magicParam(magic),
      attunement: attunement || undefined,
      sort: sortParam(sort, descending),
      page,
      pageSize,
    }),
    [search, kind, category, rarity, magic, attunement, sort, descending, page, pageSize].join('|'),
  )

  const total = items.data?.totalCount

  return (
    <Stack
      gap='4'
      px={{
        base: '4',
        md: '6',
      }}
      py={{
        base: '5',
        md: '7',
      }}
    >
      <SceneBanner />
      <Box>
        <Heading size='2xl'>Items</Heading>
        <Text color='fg.muted'>
          {total === undefined ? ' ' : `${total} items · open one to read it`}
        </Text>
      </Box>

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
        <FilterToggle
          checked={attunement}
          onChange={filter(setAttunement)}
        >
          Attunement
        </FilterToggle>
        <CardSelectionBar selection={cards} />
      </FilterBar>

      {items.problem && <Text color='fg.error'>{problemMessages(items.problem).join(' ')}</Text>}
      <Box ref={listTop} />
      {items.data && (
        <ListPagination
          total={items.data.totalCount}
          page={page}
          pageSize={pageSize}
          pageSizeLabel='Items per page'
          onPageChange={changePage}
          onPageSizeChange={(value) => filter(setPageSize)(value)}
        />
      )}
      <Box
        borderWidth='1px'
        rounded='md'
        overflow='hidden'
        opacity={items.loading && items.data ? 0.6 : 1}
      >
        {items.loading && !items.data && <Box p='3'><Spinner /></Box>}
        {items.data?.items.map((item) => (
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
          />
        ))}
        {items.data?.items.length === 0 && (
          <Text
            p='3'
            color='fg.muted'
          >
            No item matches
          </Text>
        )}
      </Box>

      {items.data && items.data.totalPages > 1 && (
        <ListPagination
          total={items.data.totalCount}
          page={page}
          pageSize={pageSize}
          onPageChange={changePage}
        />
      )}
      <ApiHint>
        <code>
          GET /api/rules/items?search=&amp;kind=&amp;category=&amp;rarity=&amp;magic=&amp;attunement=&amp;sort=
          &amp;page=&amp;pageSize=
        </code>, pictures from <code>GET /api/images/item/{'{key}'}?thumb=true</code>, cards =
        <code> POST /api/cards.pdf</code> with <code>{'{ spells, items }'}</code> (keys)
      </ApiHint>
    </Stack>
  )
}
