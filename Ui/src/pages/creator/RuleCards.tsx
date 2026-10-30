import { Card, HStack, Image, Input, SimpleGrid, Spinner, Stack, Text } from '@chakra-ui/react'
import { useState } from 'react'
import { ListBackgroundsIntent, ListClassesIntent, ListSpeciesIntent } from '@/api'
import { InfoButton } from '@/components/InfoButton'
import { useIntentQuery } from '@/hooks/useIntentQuery'

export type RuleKind = 'species' | 'background' | 'class'

interface CardEntry {
  key: string
  name: string
  info: string
  thumbnailUrl: string | null
}

/**
 * Selectable cards of species, backgrounds or classes; the info button opens the details without selecting
 */
export const RuleCards = ({
  kind,
  selected,
  onSelect,
}: {
  kind: RuleKind,
  selected: string,
  onSelect: (key: string) => void
}) => {
  const [search, setSearch] = useState('')
  const species = useIntentQuery(() => (kind === 'species' ? new ListSpeciesIntent({ pageSize: 100 }) : null), kind)
  const backgrounds = useIntentQuery(
    () => (kind === 'background' ? new ListBackgroundsIntent({ pageSize: 100 }) : null),
    kind,
  )
  const classes = useIntentQuery(() => (kind === 'class' ? new ListClassesIntent({ pageSize: 100 }) : null), kind)

  const entries: CardEntry[] = kind === 'species'
    ? (species.data?.items ?? []).map((entry) => ({
      key: entry.key,
      name: entry.name,
      info: entry.creatureType,
      thumbnailUrl: entry.thumbnailUrl,
    }))
    : kind === 'background'
      ? (backgrounds.data?.items ?? []).map((entry) => ({
        key: entry.key,
        name: entry.name,
        info: [
          `Abilities: ${entry.abilityOptions.join(', ').toUpperCase()}`,
          entry.originFeat && `Feat: ${entry.originFeat}`,
        ]
          .filter(Boolean)
          .join(' · '),
        thumbnailUrl: entry.thumbnailUrl,
      }))
      : (classes.data?.items ?? []).map((entry) => ({
        key: entry.key,
        name: entry.name,
        info: `${entry.hitDie.toLowerCase()} · ${
          entry.primaryAbilities.join(entry.primaryAbilityMode === 'Any' ? ' or ' : ' and ')}`,
        thumbnailUrl: entry.thumbnailUrl,
      }))
  const loading = species.loading || backgrounds.loading || classes.loading
  const visible = entries.filter((entry) => entry.name.toLowerCase().includes(search.toLowerCase()))

  return (
    <Stack gap='3'>
      <Input
        placeholder='Search…'
        value={search}
        onChange={(event) => setSearch(event.target.value)}
      />
      {loading && entries.length === 0 && <Spinner />}
      <SimpleGrid
        minChildWidth='220px'
        gap='3'
      >
        {visible.map((entry) => (
          <Card.Root
            key={entry.key}
            cursor='pointer'
            borderColor={entry.key === selected ? 'brand.solid' : undefined}
            bg={entry.key === selected ? 'brand.subtle' : undefined}
            overflow='hidden'
            onClick={() => onSelect(entry.key)}
          >
            {entry.thumbnailUrl && (
              <Image
                src={entry.thumbnailUrl}
                alt=''
                w='full'
                aspectRatio={kind === 'class' ? 4 / 3 : 16 / 9}
                objectFit='cover'
                objectPosition={kind === 'class' ? '50% 20%' : 'center'}
                bg='bg.muted'
                loading='lazy'
              />
            )}
            <Card.Body
              p='3.5'
              gap='1'
            >
              <HStack justify='space-between'>
                <Text fontWeight='semibold'>{entry.name}</Text>
                <InfoButton target={{
                  kind,
                  key: entry.key,
                }}
                />
              </HStack>
              <Text
                fontSize='sm'
                color='fg.muted'
              >
                {entry.info}
              </Text>
            </Card.Body>
          </Card.Root>
        ))}
      </SimpleGrid>
    </Stack>
  )
}
