import {
  Badge,
  Box,
  Button,
  Card,
  Checkbox,
  Flex,
  Heading,
  HStack,
  Input,
  SimpleGrid,
  Spinner,
  Stack,
  Text,
} from '@chakra-ui/react'
import { type ReactNode, useState } from 'react'
import {
  CopyToSpellbookIntent,
  GetSpellsIntent,
  type KnownSpellDto,
  ListSpellsIntent,
  problemMessages,
  SetPreparedSpellsIntent,
  type SpellcastingClassDto,
} from '@/api'
import { ApiHint } from '@/components/ApiHint'
import { InfoButton } from '@/components/InfoButton'
import { useInspector } from '@/components/inspector/useInspector'
import { Pips } from '@/components/Pips'
import { useApiAction } from '@/hooks/useApiAction'
import { useIntentQuery } from '@/hooks/useIntentQuery'
import { slotGroups } from './playState'
import { useCharacter } from './useCharacter'

const levelName = (level: number) => (level === 0 ? 'Cantrip' : `Level ${level}`)

/**
 * Spellcasting per class, clickable spell slots, the spells of each class (cantrips, prepared, spellbook) with the
 * long rest loadout editor and copying into the spellbook, and spells from species and feats
 */
export function SpellsTab() {
  const {
    id,
    state,
    sheet,
    play,
    updatePlay,
  } = useCharacter()
  const spells = useIntentQuery(() => new GetSpellsIntent(id), `${id}|${state.level}`)
  const groups = slotGroups(sheet)

  if (spells.problem) {
    return <Text color='fg.error'>{problemMessages(spells.problem).join(' ')}</Text>
  }

  if (!spells.data) {
    return <Spinner />
  }

  if (spells.data.classes.length === 0 && spells.data.other.length === 0) {
    return <Text color='fg.muted'>No spells – choose a spellcasting class or a feat like Magic Initiate.</Text>
  }

  return (
    <Stack gap='4'>
      <SimpleGrid
        minChildWidth='280px'
        gap='4'
      >
        {sheet.spellcasting.map((entry) => (
          <Card.Root key={entry.class}>
            <Card.Body gap='1'>
              <Text
                fontSize='sm'
                color='fg.muted'
              >
                {entry.class} · {entry.ability}
              </Text>
              <Heading size='md'>
                Save DC {entry.saveDc} · Attack {entry.attackBonus >= 0 ? '+' : ''}{entry.attackBonus}
              </Heading>
            </Card.Body>
          </Card.Root>
        ))}
        {groups.length > 0 && (
          <Card.Root>
            <Card.Body gap='2'>
              <HStack justify='space-between'>
                <Heading size='md'>Spell slots</Heading>
                <Text
                  fontSize='xs'
                  color='fg.muted'
                >
                  click to use
                </Text>
              </HStack>
              {groups.map((group) => (
                <Flex
                  key={group.key}
                  align='center'
                  gap='3'
                >
                  <Text flex='1'>
                    Level {group.level} {group.period === 'ShortRest' && (
                      <Text
                        as='span'
                        fontSize='sm'
                        color='fg.muted'
                      >
                        pact · short rest
                      </Text>
                    )}
                  </Text>
                  <Pips
                    label={`Level ${group.level} slots`}
                    max={group.count}
                    used={play.slotsUsed[group.key] ?? 0}
                    onChange={(value) => updatePlay((current) => ({
                      ...current,
                      slotsUsed: {
                        ...current.slotsUsed,
                        [group.key]: value,
                      },
                    }))}
                  />
                </Flex>
              ))}
            </Card.Body>
          </Card.Root>
        )}
      </SimpleGrid>

      {spells.data.classes.map((entry) => (
        <ClassSpells
          key={entry.classKey}
          entry={entry}
          onChanged={spells.reload}
        />
      ))}

      {spells.data.other.length > 0 && (
        <Card.Root>
          <Card.Body gap='3'>
            <Heading size='md'>From species and feats</Heading>
            <SpellList spells={spells.data.other} />
          </Card.Body>
        </Card.Root>
      )}

      <ApiHint>
        <code>GET /api/characters/{'{id}'}/spells</code>; loadout = <code>PUT …/prepared-spells/{'{class}'}</code>,
        copy =
        <code> POST …/spellbook</code>, candidates = <code>GET /api/rules/spells?class=&amp;maxLevel=</code>
      </ApiHint>
    </Stack>
  )
}

function ClassSpells({ entry, onChanged }: { entry: SpellcastingClassDto, onChanged: () => void }) {
  const [mode, setMode] = useState<'view' | 'prepare' | 'copy'>('view')
  const cantrips = entry.spells.filter((spell) => spell.level === 0)
  const prepared = entry.spells.filter((spell) => spell.level > 0 && (spell.prepared || spell.alwaysPrepared))
  const spellbook = entry.spells
    .filter((spell) => spell.level > 0 && spell.inSpellbook && !spell.prepared && !spell.alwaysPrepared)
  const longRest = entry.prepareChange === 'LongRest'
  const hasSpellbook = entry.pool === 'Spellbook'

  return (
    <Card.Root>
      <Card.Body gap='4'>
        <Flex
          wrap='wrap'
          gap='2'
          align='center'
          justify='space-between'
        >
          <Box>
            <Heading size='md'>{entry.className} spells</Heading>
            <Text
              fontSize='sm'
              color='fg.muted'
            >
              Up to level {entry.maxSpellLevel} · prepared {entry.preparedCount} / {entry.preparedLimit} · cantrips{' '}
              {cantrips.length} / {entry.cantripLimit}
              {longRest ? ' · changes after a long rest' : ' · changes on a level up'}
            </Text>
          </Box>
          <HStack>
            {longRest && (
              <Button
                size='sm'
                variant={mode === 'prepare' ? 'solid' : 'outline'}
                colorPalette='brand'
                onClick={() => setMode(mode === 'prepare' ? 'view' : 'prepare')}
              >
                Prepare spells
              </Button>
            )}
            {hasSpellbook && (
              <Button
                size='sm'
                variant={mode === 'copy' ? 'solid' : 'outline'}
                colorPalette='brand'
                onClick={() => setMode(mode === 'copy' ? 'view' : 'copy')}
              >
                Copy into spellbook
              </Button>
            )}
          </HStack>
        </Flex>

        {mode === 'prepare' && (
          <PrepareEditor
            entry={entry}
            onDone={() => {
              setMode('view')
              onChanged()
            }}
          />
        )}
        {mode === 'copy' && (
          <CopyPicker
            entry={entry}
            onDone={() => {
              setMode('view')
              onChanged()
            }}
          />
        )}

        <Group title='Cantrips'>
          <SpellList spells={cantrips} />
        </Group>
        <Group title='Prepared'>
          <SpellList spells={prepared} />
        </Group>
        {hasSpellbook && (
          <Group title='In the spellbook, not prepared'>
            <SpellList spells={spellbook} />
          </Group>
        )}
      </Card.Body>
    </Card.Root>
  )
}

function Group({ title, children }: { title: string, children: ReactNode }) {
  return (
    <Stack gap='2'>
      <Text
        fontSize='2xs'
        fontWeight='bold'
        letterSpacing='wider'
        color='fg.muted'
        textTransform='uppercase'
      >
        {title}
      </Text>
      {children}
    </Stack>
  )
}

function SpellList({ spells }: { spells: KnownSpellDto[] }) {
  if (spells.length === 0) {
    return <Text color='fg.muted'>None</Text>
  }

  return (
    <Box
      borderWidth='1px'
      rounded='md'
      overflow='hidden'
    >
      {spells.map((spell) => (
        <SpellRow
          key={`${spell.key}-${spell.source}`}
          spell={spell}
        />
      ))}
    </Box>
  )
}

function SpellRow({ spell, control }: {
  spell: {
    key: string,
    name: string,
    level: number,
    school: string,
    castingTime: string,
    ritual: boolean,
    concentration: boolean
  }
  control?: ReactNode
}) {
  const { inspect } = useInspector()
  const known = spell as Partial<KnownSpellDto>
  const tags = [
    levelName(spell.level),
    spell.school,
    spell.castingTime,
    spell.concentration && 'concentration',
    spell.ritual && 'ritual',
  ].filter(Boolean).join(' · ')

  return (
    <Flex
      align='center'
      gap='3'
      px='3'
      py='2.5'
      borderTopWidth='1px'
      _first={{ borderTopWidth: '0' }}
    >
      {control}
      <Box
        flex='1'
        minW='0'
      >
        <Button
          variant='plain'
          p='0'
          h='auto'
          fontWeight='semibold'
          onClick={() => inspect({
            kind: 'spell',
            key: spell.key,
          })}
        >
          {spell.name}
        </Button>
        <Text
          fontSize='sm'
          color='fg.muted'
        >
          {tags}
        </Text>
      </Box>
      {known.alwaysPrepared && spell.level > 0 && <Badge>always prepared</Badge>}
      {known.source && <Badge hideBelow='md'>{known.source}</Badge>}
      <InfoButton target={{
        kind: 'spell',
        key: spell.key,
      }}
      />
    </Flex>
  )
}

/**
 * Long rest loadout: from the spellbook (wizard) or the whole class list up to the highest level (cleric, druid …)
 */
function PrepareEditor({ entry, onDone }: { entry: SpellcastingClassDto, onDone: () => void }) {
  const { id, applyState } = useCharacter()
  const { run, busy } = useApiAction()
  const [selected, setSelected] = useState<string[]>(() => entry.spells
    .filter((spell) => spell.level > 0 && spell.prepared && !spell.alwaysPrepared)
    .map((spell) => spell.key))
  const classList = useIntentQuery(
    () => (entry.pool === 'ClassList'
      ? new ListSpellsIntent({
        class: entry.spellList,
        minLevel: 1,
        maxLevel: entry.maxSpellLevel,
        sort: 'level,name',
        pageSize: 200,
      })
      : null),
    `${entry.spellList}|${entry.maxSpellLevel}|${entry.pool}`,
  )
  const always = new Set(entry.spells.filter((spell) => spell.alwaysPrepared).map((spell) => spell.key))
  const candidates = (entry.pool === 'Spellbook'
    ? entry.spells.filter((spell) => spell.inSpellbook && spell.level > 0 && spell.level <= entry.maxSpellLevel)
    : classList.data?.items ?? []).filter((spell) => !always.has(spell.key))

  const toggle = (key: string) => setSelected(selected.includes(key)
    ? selected.filter((entry) => entry !== key)
    : [...selected, key])

  const save = async () => {
    const result = await run(
      new SetPreparedSpellsIntent(id, entry.classKey, { spellKeys: selected }),
      { success: `${entry.className} spells prepared` },
    )
    if (result.ok) {
      applyState(result.data)
      onDone()
    }
  }

  return (
    <Stack
      gap='3'
      p='3'
      borderWidth='1px'
      borderColor='brand.muted'
      rounded='md'
      bg='brand.subtle'
    >
      <HStack justify='space-between'>
        <Text fontWeight='semibold'>
          Choose up to {entry.preparedLimit} spells from the {entry.pool === 'Spellbook' ? 'spellbook' : 'class list'}
        </Text>
        <Badge colorPalette={selected.length > entry.preparedLimit ? 'red' : 'brand'}>
          {selected.length} / {entry.preparedLimit}
        </Badge>
      </HStack>
      {classList.loading && entry.pool === 'ClassList' && <Spinner size='sm' />}
      <Box
        borderWidth='1px'
        rounded='md'
        overflow='hidden'
        bg='bg'
        maxH='420px'
        overflowY='auto'
      >
        {candidates.map((spell) => {
          const checked = selected.includes(spell.key)
          return (
            <SpellRow
              key={spell.key}
              spell={spell}
              control={(
                <Checkbox.Root
                  colorPalette='brand'
                  checked={checked}
                  disabled={!checked && selected.length >= entry.preparedLimit}
                  onCheckedChange={() => toggle(spell.key)}
                >
                  <Checkbox.HiddenInput aria-label={`Prepare ${spell.name}`} />
                  <Checkbox.Control />
                </Checkbox.Root>
              )}
            />
          )
        })}
      </Box>
      <HStack justify='flex-end'>
        <Button
          size='sm'
          variant='outline'
          onClick={onDone}
        >
          Cancel
        </Button>
        <Button
          size='sm'
          colorPalette='brand'
          loading={busy}
          disabled={selected.length > entry.preparedLimit}
          onClick={save}
        >
          Save loadout
        </Button>
      </HStack>
    </Stack>
  )
}

/**
 * Copy a spell of the class list (level 1 to the highest level) into the spellbook
 */
function CopyPicker({ entry, onDone }: { entry: SpellcastingClassDto, onDone: () => void }) {
  const { id, applyState } = useCharacter()
  const { run } = useApiAction()
  const [search, setSearch] = useState('')
  const candidates = useIntentQuery(
    () => new ListSpellsIntent({
      class: entry.spellList,
      minLevel: 1,
      maxLevel: entry.maxSpellLevel,
      search: search || undefined,
      sort: 'level,name',
      pageSize: 50,
    }),
    `${entry.spellList}|${entry.maxSpellLevel}|${search}`,
  )
  const inBook = new Set(entry.spells.filter((spell) => spell.inSpellbook).map((spell) => spell.key))
  const spells = (candidates.data?.items ?? []).filter((spell) => !inBook.has(spell.key))

  const copy = async (key: string, name: string) => {
    const result = await run(
      new CopyToSpellbookIntent(id, { spellKey: key }),
      { success: `${name} copied into the spellbook` },
    )
    if (result.ok) {
      applyState(result.data)
      onDone()
    }
  }

  return (
    <Stack
      gap='3'
      p='3'
      borderWidth='1px'
      borderColor='brand.muted'
      rounded='md'
      bg='brand.subtle'
    >
      <Text fontWeight='semibold'>Copy a {entry.className.toLowerCase()} spell (scroll or another book)</Text>
      <Input
        size='sm'
        bg='bg'
        placeholder='Search spells…'
        value={search}
        onChange={(event) => setSearch(event.target.value)}
      />
      <Box
        borderWidth='1px'
        rounded='md'
        overflow='hidden'
        bg='bg'
        maxH='420px'
        overflowY='auto'
      >
        {candidates.loading && !candidates.data && <Box p='3'><Spinner size='sm' /></Box>}
        {spells.map((spell) => (
          <SpellRow
            key={spell.key}
            spell={spell}
            control={(
              <Button
                size='xs'
                colorPalette='brand'
                onClick={() => copy(spell.key, spell.name)}
              >
                Copy
              </Button>
            )}
          />
        ))}
      </Box>
      <HStack justify='flex-end'>
        <Button
          size='sm'
          variant='outline'
          onClick={onDone}
        >
          Close
        </Button>
      </HStack>
    </Stack>
  )
}
