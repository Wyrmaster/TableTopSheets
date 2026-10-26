import { Badge, Box, Button, Card, Flex, Heading, HStack, SimpleGrid, Stack, Text } from '@chakra-ui/react'
import type { SpellSheet } from '@/api'
import { ApiHint } from '@/components/ApiHint'
import { InfoButton } from '@/components/InfoButton'
import { useInspector } from '@/components/inspector/useInspector'
import { Pips } from '@/components/Pips'
import { slotGroups } from './playState'
import { useCharacter } from './useCharacter'

const levelName = (level: number) => (level === 0 ? 'Cantrips' : `Level ${level}`)

/**
 * Spellcasting per class, clickable spell slots and the spells of the sheet grouped by level
 */
export function SpellsTab() {
  const {
    sheet,
    play,
    updatePlay,
  } = useCharacter()
  const groups = slotGroups(sheet)
  const levels = [...new Set(sheet.spells.map((spell) => spell.level))].sort((a, b) => a - b)

  if (sheet.spells.length === 0 && sheet.spellcasting.length === 0) {
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
              <Text
                fontSize='sm'
                color='fg.muted'
              >
                {entry.preparedLimit} prepared · {entry.cantripLimit} cantrips
              </Text>
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
                    Level {group.level} <Text
                      as='span'
                      fontSize='sm'
                      color='fg.muted'
                    >
                      {group.period === 'ShortRest' ? 'pact · short rest' : ''}
                    </Text>
                  </Text>
                  <Pips
                    label={`Level ${group.level} slots`}
                    max={group.count}
                    used={play.slotsUsed[group.key] ?? 0}
                    onChange={(value) => updatePlay((state) => ({
                      ...state,
                      slotsUsed: {
                        ...state.slotsUsed,
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

      {levels.map((level) => (
        <Card.Root key={level}>
          <Card.Body gap='3'>
            <Heading size='md'>{levelName(level)}</Heading>
            <Box
              borderWidth='1px'
              rounded='md'
              overflow='hidden'
            >
              {sheet.spells
                .filter((spell) => spell.level === level)
                .map((spell) => (
                  <SpellRow
                    key={`${spell.key}-${spell.source}`}
                    spell={spell}
                  />
                ))}
            </Box>
          </Card.Body>
        </Card.Root>
      ))}
      <ApiHint>
        spells from <code>GET /api/characters/{'{id}'}/sheet</code>; preparing a wizard loadout =
        <code> PUT /api/characters/{'{id}'}/prepared-spells/{'{class}'}</code> (to be added)
      </ApiHint>
    </Stack>
  )
}

function SpellRow({ spell }: { spell: SpellSheet }) {
  const { inspect } = useInspector()
  const tags = [spell.castingTime, spell.range, spell.concentration && 'concentration', spell.ritual && 'ritual']
    .filter(Boolean)
    .join(' · ')

  return (
    <Flex
      align='center'
      gap='3'
      px='3'
      py='2.5'
      borderTopWidth='1px'
      _first={{ borderTopWidth: '0' }}
    >
      <Box flex='1'>
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
      <Badge hideBelow='md'>{spell.source}</Badge>
      {spell.notes && <Badge>{spell.notes}</Badge>}
      <InfoButton target={{
        kind: 'spell',
        key: spell.key,
      }}
      />
    </Flex>
  )
}
