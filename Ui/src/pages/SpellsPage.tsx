import {
  Box,
  ButtonGroup,
  Flex,
  Heading,
  IconButton,
  Pagination,
  Spinner,
  Stack,
  Text,
} from '@chakra-ui/react'
import { useRef, useState } from 'react'
import { type DamageTypeEnum, DamageTypes, ListSpellsIntent, problemMessages } from '@/api'
import { ApiHint } from '@/components/ApiHint'
import { CardCheckbox } from '@/components/CardCheckbox'
import { CardSelectionBar } from '@/components/CardSelectionBar'
import { FilterBar, FilterSelect, FilterToggle } from '@/components/FilterBar'
import { Icon } from '@/components/Icon'
import { nextIcon, previousIcon } from '@/components/icons'
import { SceneBanner } from '@/components/SceneBanner'
import { SpellRow } from '@/components/SpellRow'
import { useCardSelection } from '@/hooks/useCardSelection'
import { useIntentQuery } from '@/hooks/useIntentQuery'

const PageSizes = [25, 50, 100]

/** Cantrips need no spell slot, leveled spells (`Spell`) do */
type SpellKind = 'all' | 'cantrip' | 'leveled'

/** Class spell lists of the data (`lists` of a spell) */
const SpellLists: [string, string][] = [
  ['', 'All spell lists'],
  ['artificer', 'Artificer'],
  ['bard', 'Bard'],
  ['cleric', 'Cleric'],
  ['druid', 'Druid'],
  ['paladin', 'Paladin'],
  ['ranger', 'Ranger'],
  ['sorcerer', 'Sorcerer'],
  ['warlock', 'Warlock'],
  ['wizard', 'Wizard'],
]

const Schools = [
  'abjuration',
  'conjuration',
  'divination',
  'enchantment',
  'evocation',
  'illusion',
  'necromancy',
  'transmutation',
]

/** Value of the damage select for "any spell that deals damage" (otherwise a damage type) */
const AnyDamage = 'any'

const capitalize = (text: string) => text.charAt(0).toUpperCase() + text.slice(1)

/**
 * Every spell of the rules, without a character: search and filters (cantrip or leveled, level, spell list, school,
 * damage type, ritual, concentration); a spell opens in the detail view. In card selection mode a click selects the
 * spell for the card PDF instead (kept across pages and filters).
 */
export const SpellsPage = () => {
  const [search, setSearch] = useState('')
  const [kind, setKind] = useState<SpellKind>('all')
  const [level, setLevel] = useState('')
  const [list, setList] = useState('')
  const [school, setSchool] = useState('')
  const [damageType, setDamageType] = useState('')
  const [ritual, setRitual] = useState(false)
  const [concentration, setConcentration] = useState(false)
  const [sort, setSort] = useState('level,name')
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(PageSizes[0])
  const listTop = useRef<HTMLDivElement>(null)
  const cards = useCardSelection()

  // every filter change starts at the first page again
  const filter = <T, >(set: (value: T) => void) => (value: T) => {
    set(value)
    setPage(1)
  }

  const filtered = Boolean(search || kind !== 'all' || level || list || school || damageType || ritual || concentration)
  const clear = () => {
    setSearch('')
    setKind('all')
    setLevel('')
    setList('')
    setSchool('')
    setDamageType('')
    setRitual(false)
    setConcentration(false)
    setPage(1)
  }

  // a new page starts at the top of the list (the bottom pagination is far below it)
  const changePage = (next: number) => {
    setPage(next)
    if (listTop.current && listTop.current.getBoundingClientRect().top < 0) {
      listTop.current.scrollIntoView({ behavior: 'smooth' })
    }
  }

  const spells = useIntentQuery(
    () => new ListSpellsIntent({
      search: search || undefined,
      level: kind === 'cantrip' ? 0 : level ? Number(level) : undefined,
      minLevel: kind === 'leveled' && !level ? 1 : undefined,
      class: list || undefined,
      school: school || undefined,
      damage: damageType ? true : undefined,
      damageType: (damageType && damageType !== AnyDamage ? damageType : undefined) as DamageTypeEnum | undefined,
      ritual: ritual || undefined,
      concentration: concentration || undefined,
      sort,
      page,
      pageSize,
    }),
    [search, kind, level, list, school, damageType, ritual, concentration, sort, page, pageSize].join('|'),
  )

  const total = spells.data?.totalCount

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
        <Heading size='2xl'>Spells</Heading>
        <Text color='fg.muted'>
          {total === undefined ? ' ' : `${total} spells · open one to read it`}
        </Text>
      </Box>

      <FilterBar
        search={search}
        placeholder='Search spells…'
        filtered={filtered}
        onSearchChange={filter(setSearch)}
        onClear={clear}
      >
        <FilterSelect
          label='Type'
          width='150px'
          value={kind}
          onChange={(value) => {
            filter(setKind)(value as SpellKind)
            setLevel('')
          }}
        >
          <option value='all'>All spells</option>
          <option value='cantrip'>Cantrip</option>
          <option value='leveled'>Spell</option>
        </FilterSelect>
        <FilterSelect
          label='Level'
          width='130px'
          value={level}
          disabled={kind === 'cantrip'}
          onChange={filter(setLevel)}
        >
          <option value=''>All levels</option>
          {[1, 2, 3, 4, 5, 6, 7, 8, 9].map((value) => (
            <option
              key={value}
              value={value}
            >
              Level {value}
            </option>
          ))}
        </FilterSelect>
        <FilterSelect
          label='Spell list'
          width='160px'
          value={list}
          onChange={filter(setList)}
        >
          {SpellLists.map(([value, label]) => (
            <option
              key={value}
              value={value}
            >
              {label}
            </option>
          ))}
        </FilterSelect>
        <FilterSelect
          label='School'
          width='170px'
          value={school}
          onChange={filter(setSchool)}
        >
          <option value=''>All schools</option>
          {Schools.map((value) => (
            <option
              key={value}
              value={value}
            >
              {capitalize(value)}
            </option>
          ))}
        </FilterSelect>
        <FilterSelect
          label='Damage type'
          width='170px'
          value={damageType}
          onChange={filter(setDamageType)}
        >
          <option value=''>Damage or not</option>
          <option value={AnyDamage}>Deals damage</option>
          {DamageTypes.map((value) => (
            <option
              key={value}
              value={value}
            >
              {value}
            </option>
          ))}
        </FilterSelect>
        <FilterSelect
          label='Sort'
          width='230px'
          value={sort}
          onChange={setSort}
        >
          <option value='level,name'>Sort: level</option>
          <option value='name'>Sort: name</option>
          <option value='school,level,name'>Sort: school</option>
          <option value='castingTime,level,name'>Sort: casting time</option>
          <option value='-concentration,level,name'>Sort: concentration first</option>
          <option value='-ritual,level,name'>Sort: ritual first</option>
          <option value='damageType,level,name'>Sort: damage type</option>
        </FilterSelect>
        <FilterToggle
          checked={ritual}
          onChange={filter(setRitual)}
        >
          Ritual
        </FilterToggle>
        <FilterToggle
          checked={concentration}
          onChange={filter(setConcentration)}
        >
          Concentration
        </FilterToggle>
        <CardSelectionBar selection={cards} />
      </FilterBar>

      {spells.problem && <Text color='fg.error'>{problemMessages(spells.problem).join(' ')}</Text>}
      <Box ref={listTop} />
      {spells.data && (
        <SpellPagination
          total={spells.data.totalCount}
          page={page}
          pageSize={pageSize}
          onPageChange={changePage}
          onPageSizeChange={(value) => filter(setPageSize)(value)}
        />
      )}
      <Box
        borderWidth='1px'
        rounded='md'
        overflow='hidden'
        opacity={spells.loading && spells.data ? 0.6 : 1}
      >
        {spells.loading && !spells.data && <Box p='3'><Spinner /></Box>}
        {spells.data?.items.map((spell) => (
          <SpellRow
            key={spell.key}
            spell={spell}
            selected={cards.active && cards.isSelected('spell', spell.key)}
            onClick={cards.active ? () => cards.toggle('spell', spell.key) : undefined}
            control={(
              <CardCheckbox
                selection={cards}
                kind='spell'
                entryKey={spell.key}
                name={spell.name}
              />
            )}
          />
        ))}
        {spells.data?.items.length === 0 && (
          <Text
            p='3'
            color='fg.muted'
          >
            No spell matches
          </Text>
        )}
      </Box>

      {spells.data && spells.data.totalPages > 1 && (
        <SpellPagination
          total={spells.data.totalCount}
          page={page}
          pageSize={pageSize}
          onPageChange={changePage}
        />
      )}
      <ApiHint>
        <code>
          GET /api/rules/spells?search=&amp;level=&amp;minLevel=&amp;class=&amp;school=&amp;damage=&amp;damageType=
          &amp;ritual=&amp;concentration=&amp;sort=&amp;page=&amp;pageSize=
        </code>, pictures from <code>GET /api/images/spell/{'{key}'}?thumb=true</code>, cards =
        <code> POST /api/cards.pdf</code> with <code>{'{ spells, items }'}</code> (keys)
      </ApiHint>
    </Stack>
  )
}

/**
 * Pages of the list: range ("26–50 of 391"), previous / page numbers / next (only previous, "2 / 16", next on small
 * screens) and, when given a handler, the page size
 */
const SpellPagination = ({
  total,
  page,
  pageSize,
  onPageChange,
  onPageSizeChange,
}: {
  total: number
  page: number
  pageSize: number
  onPageChange: (page: number) => void
  onPageSizeChange?: (pageSize: number) => void
}) => {
  const first = total === 0 ? 0 : (page - 1) * pageSize + 1
  const last = Math.min(page * pageSize, total)

  return (
    <Flex
      wrap='wrap'
      gap='3'
      align='center'
      justify='space-between'
    >
      <Text
        fontSize='sm'
        color='fg.muted'
      >
        {first}–{last} of {total}
      </Text>
      <Pagination.Root
        count={total}
        pageSize={pageSize}
        page={page}
        siblingCount={1}
        onPageChange={(details) => onPageChange(details.page)}
      >
        <ButtonGroup
          size='sm'
          variant='ghost'
        >
          <Pagination.PrevTrigger asChild>
            <IconButton aria-label='Previous page'>
              <Icon path={previousIcon} />
            </IconButton>
          </Pagination.PrevTrigger>
          {/* page numbers on wider screens, "2 / 16" on phones */}
          <Box display={{
            base: 'none',
            md: 'contents',
          }}
          >
            <Pagination.Items
              render={(item) => (
                <IconButton variant={{
                  base: 'ghost',
                  _selected: 'outline',
                }}
                >
                  {item.value}
                </IconButton>
              )}
            />
          </Box>
          <Pagination.PageText
            format='short'
            hideFrom='md'
            px='2'
          />
          <Pagination.NextTrigger asChild>
            <IconButton aria-label='Next page'>
              <Icon path={nextIcon} />
            </IconButton>
          </Pagination.NextTrigger>
        </ButtonGroup>
      </Pagination.Root>
      {onPageSizeChange && (
        <FilterSelect
          label='Spells per page'
          width='130px'
          value={String(pageSize)}
          onChange={(value) => onPageSizeChange(Number(value))}
        >
          {PageSizes.map((value) => (
            <option
              key={value}
              value={value}
            >
              {value} per page
            </option>
          ))}
        </FilterSelect>
      )}
    </Flex>
  )
}
