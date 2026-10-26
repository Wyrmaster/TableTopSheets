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
import { useCharacter } from './useCharacter'

const PageSize = 25

const price = (cp: number) => (cp >= 100 ? `${cp / 100} gp` : cp >= 10 ? `${cp / 10} sp` : `${cp} cp`)

/**
 * Item catalog with search, filters and paging; "Add" puts the item into the inventory
 */
export function ShopPage() {
  const {
    id,
    sheet,
    applyState,
  } = useCharacter()
  const { inspect } = useInspector()
  const { run } = useApiAction()
  const [search, setSearch] = useState('')
  const [kind, setKind] = useState('')
  const [magic, setMagic] = useState(false)
  const [sort, setSort] = useState('name')
  const [page, setPage] = useState(1)

  const items = useIntentQuery(
    () => new ListItemsIntent({
      search: search || undefined,
      kind: (kind || undefined) as ItemKindEnum | undefined,
      magic: magic || undefined,
      sort,
      page,
      pageSize: PageSize,
    }),
    `${search}|${kind}|${magic}|${sort}|${page}`,
  )

  const add = async (item: ItemSummaryDto) => {
    const result = await run(
      new AddItemIntent(id, { itemKey: item.key }),
      { success: `${item.name} added to the inventory` },
    )
    if (result.ok) {
      applyState(result.data)
    }
  }

  const coins = sheet.coins
  const purse = [
    coins.platinum && `${coins.platinum} pp`,
    `${coins.gold} gp`,
    coins.silver && `${coins.silver} sp`,
    coins.copper && `${coins.copper} cp`,
  ].filter(Boolean).join(' · ')

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
                {[item.kind, item.category, `${item.weight} lb`].filter(Boolean).join(' · ')}
              </Text>
            </Box>
            <Text fontWeight='semibold'>{price(item.costCp)}</Text>
            <InfoButton target={{
              kind: 'item',
              key: item.key,
            }}
            />
            <Button
              size='sm'
              colorPalette='brand'
              onClick={(event) => {
                event.stopPropagation()
                add(item)
              }}
            >
              <Icon path={shopIcon} />
              Add
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
        <code>GET /api/rules/items?search=&amp;kind=&amp;magic=&amp;sort=&amp;page=</code>,
        <code> POST /api/characters/{'{id}'}/items</code>.
        Adding doesn't charge coins yet – the API has no route to change coins (API gap)
      </ApiHint>
    </Stack>
  )
}
