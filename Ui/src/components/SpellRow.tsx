import { Badge, Box, Button, Flex, Text } from '@chakra-ui/react'
import type { ReactNode } from 'react'
import type { KnownSpellDto } from '@/api'
import { InfoButton } from '@/components/InfoButton'
import { useInspector } from '@/components/inspector/useInspector'
import { Thumbnail } from '@/components/Thumbnail'

const levelName = (level: number) => (level === 0 ? 'Cantrip' : `Level ${level}`)

/**
 * One spell in a list: name (opens the details), level · school · casting time, badges of a known spell and an
 * optional control in front (checkbox, button); with a thumbnail URL it shows the spell's picture
 */
export function SpellRow({ spell, control }: {
  spell: {
    key: string,
    name: string,
    level: number,
    school: string,
    castingTime: string,
    ritual: boolean,
    concentration: boolean,
    thumbnailUrl?: string | null
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
      <Thumbnail src={spell.thumbnailUrl} />
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
