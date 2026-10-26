import { Box, Button, Card, Flex, Grid, Heading, HStack, IconButton, SimpleGrid, Stack, Text } from '@chakra-ui/react'
import { type AbilitySheet, type FeatureSheet, ProficiencyLevels } from '@/api'
import { ApiHint } from '@/components/ApiHint'
import { Icon } from '@/components/Icon'
import { featuresIcon, heartIcon, minusIcon, plusIcon, resourcesIcon } from '@/components/icons'
import { useInspector } from '@/components/inspector/useInspector'
import { Pips } from '@/components/Pips'
import { useCharacter } from './useCharacter'

const signed = (value: number) => (value >= 0 ? `+${value}` : `${value}`)

/**
 * Abilities with saving throws and skills, hit points, resources and features
 */
export function OverviewTab() {
  const { sheet } = useCharacter()

  return (
    <Stack gap='4'>
      <SimpleGrid
        minChildWidth='320px'
        gap='4'
      >
        {sheet.abilities.map((ability) => (
          <AbilityBox
            key={ability.key}
            ability={ability}
          />
        ))}
      </SimpleGrid>
      <SimpleGrid
        minChildWidth='320px'
        gap='4'
      >
        <HitPointsCard />
        <ResourcesCard />
      </SimpleGrid>
      <FeaturesCard />
      <ApiHint>
        sheet from <code>GET /api/characters/{'{id}'}/sheet</code>; current HP, charges and hit dice aren't stored by
        the
        API yet – they live in this browser
      </ApiHint>
    </Stack>
  )
}

/** Proficiency marker: empty, half, filled (proficient) or ringed (expertise) */
function Marker({ level }: { level: number }) {
  const proficient = level >= ProficiencyLevels.Proficient
  return (
    <Box
      flexShrink='0'
      boxSize='2.5'
      rounded='full'
      borderWidth='1.5px'
      borderColor={level > 0 ? 'brand.solid' : 'fg.subtle'}
      bg={proficient ? 'brand.solid' : level > 0 ? 'brand.muted' : 'transparent'}
      outline={level >= ProficiencyLevels.Expertise ? '1.5px solid' : undefined}
      outlineColor='brand.solid'
      outlineOffset='2px'
    />
  )
}

function AbilityBox({ ability }: { ability: AbilitySheet }) {
  return (
    <Card.Root
      overflow='hidden'
      flexDirection='row'
    >
      <Grid
        w='24'
        flexShrink='0'
        placeItems='center'
        alignContent='center'
        gap='1.5'
        py='4'
        bg='brand.subtle'
        borderRightWidth='1px'
      >
        <Text
          fontSize='2xs'
          fontWeight='bold'
          letterSpacing='widest'
          color='brand.fg'
          textTransform='uppercase'
        >
          {ability.key}
        </Text>
        <Text
          fontSize='3xl'
          fontWeight='extrabold'
          lineHeight='1'
        >
          {signed(ability.modifier)}
        </Text>
        <Box
          px='3'
          borderWidth='1px'
          rounded='full'
          bg='bg'
          fontWeight='semibold'
          title='Score'
        >
          {ability.score}
        </Box>
      </Grid>
      <Stack
        flex='1'
        gap='2'
        px='4'
        py='3'
      >
        <Text fontWeight='semibold'>{ability.name}</Text>
        <HStack
          px='2.5'
          py='1.5'
          borderWidth='1px'
          rounded='md'
          bg='bg.muted'
          fontSize='sm'
          fontWeight='semibold'
        >
          <Marker level={ability.saveProficiency} />
          Saving throw
          <Box flex='1' />
          {signed(ability.saveBonus)}
        </HStack>
        {ability.skills.length > 0 && (
          <Text
            fontSize='2xs'
            fontWeight='bold'
            letterSpacing='wider'
            color='fg.muted'
            textTransform='uppercase'
          >
            Skills
          </Text>
        )}
        {ability.skills.map((skill) => (
          <HStack
            key={skill.key}
            fontSize='sm'
          >
            <Marker level={skill.proficiency} />
            {skill.name}
            {skill.advantage && <Text
              as='span'
              color='green.fg'
              title='Advantage'
            >
              ▲
            </Text>}
            {skill.disadvantage && <Text
              as='span'
              color='fg.error'
              title='Disadvantage'
            >
              ▼
            </Text>}
            <Box flex='1' />
            {signed(skill.bonus)}
          </HStack>
        ))}
      </Stack>
    </Card.Root>
  )
}

function CardTitle({
  icon,
  title,
  hint,
}: {
  icon: string,
  title: string,
  hint?: string
}) {
  return (
    <HStack
      justify='space-between'
      mb='2'
    >
      <Heading size='md'>
        <HStack>
          <Icon path={icon} />
          {title}
        </HStack>
      </Heading>
      {hint && <Text
        fontSize='xs'
        color='fg.muted'
      >
        {hint}
      </Text>}
    </HStack>
  )
}

function HitPointsCard() {
  const {
    sheet,
    play,
    updatePlay,
  } = useCharacter()
  const current = play.hitPoints ?? sheet.hitPointMaximum
  const change = (delta: number) => updatePlay((state) => ({
    ...state,
    hitPoints: Math.max(0, Math.min(sheet.hitPointMaximum, current + delta)),
  }))

  return (
    <Card.Root>
      <Card.Body>
        <CardTitle
          icon={heartIcon}
          title='Hit points'
        />
        <Flex
          align='center'
          gap='3'
          py='2'
        >
          <Text
            fontSize='3xl'
            fontWeight='extrabold'
            lineHeight='1'
          >
            {current}
          </Text>
          <Text color='fg.muted'>/ {sheet.hitPointMaximum}</Text>
          <Box flex='1' />
          <IconButton
            size='sm'
            variant='outline'
            aria-label='Damage 1'
            onClick={() => change(-1)}
          >
            <Icon path={minusIcon} />
          </IconButton>
          <IconButton
            size='sm'
            variant='outline'
            aria-label='Heal 1'
            onClick={() => change(1)}
          >
            <Icon path={plusIcon} />
          </IconButton>
        </Flex>
        {sheet.hitDice.map((pool) => {
          const used = play.hitDiceUsed[pool.die] ?? 0
          return (
            <Flex
              key={pool.die}
              align='center'
              gap='3'
              py='2'
              borderTopWidth='1px'
            >
              <Text flex='1'>
                Hit dice {pool.die.toLowerCase()} <Text
                  as='span'
                  fontSize='sm'
                  color='fg.muted'
                >
                  {pool.count - used} / {pool.count} left
                </Text>
              </Text>
              <Pips
                square
                label={`Hit dice ${pool.die}`}
                max={pool.count}
                used={used}
                onChange={(value) => updatePlay((state) => ({
                  ...state,
                  hitDiceUsed: {
                    ...state.hitDiceUsed,
                    [pool.die]: value,
                  },
                }))}
              />
            </Flex>
          )
        })}
      </Card.Body>
    </Card.Root>
  )
}

function ResourcesCard() {
  const {
    sheet,
    play,
    updatePlay,
  } = useCharacter()
  const recharge = (value: string) => value.replace(/([a-z])([A-Z])/g, '$1 $2').toLowerCase()

  return (
    <Card.Root>
      <Card.Body>
        <CardTitle
          icon={resourcesIcon}
          title='Resources'
          hint={sheet.resources.length ? 'click a charge to use it' : undefined}
        />
        {sheet.resources.length === 0 && <Text color='fg.muted'>No limited-use features.</Text>}
        {sheet.resources.map((resource, index) => (
          <Flex
            key={resource.key}
            align='center'
            gap='3'
            py='2.5'
            borderTopWidth={index ? '1px' : '0'}
          >
            <Box flex='1'>
              <Text fontWeight='semibold'>{resource.name}</Text>
              <Text
                fontSize='sm'
                color='fg.muted'
              >
                {[resource.die, recharge(resource.recharge)].filter(Boolean).join(' · ')}
              </Text>
            </Box>
            <Pips
              label={resource.name}
              max={resource.maximum}
              used={play.resourcesUsed[resource.key] ?? 0}
              onChange={(value) => updatePlay((state) => ({
                ...state,
                resourcesUsed: {
                  ...state.resourcesUsed,
                  [resource.key]: value,
                },
              }))}
            />
          </Flex>
        ))}
      </Card.Body>
    </Card.Root>
  )
}

function FeaturesCard() {
  const { sheet } = useCharacter()
  const groups: [string, FeatureSheet[]][] = [
    ['Class features', sheet.classFeatures],
    ['Species traits', sheet.speciesTraits],
    ['Feats', sheet.feats],
  ]

  return (
    <Card.Root>
      <Card.Body>
        <CardTitle
          icon={featuresIcon}
          title='Features'
          hint='click to read'
        />
        <Stack gap='4'>
          {groups
            .filter(([, features]) => features.length)
            .map(([title, features]) => (
              <Stack
                key={title}
                gap='2'
              >
                <Text
                  fontSize='2xs'
                  fontWeight='bold'
                  letterSpacing='wider'
                  color='fg.muted'
                  textTransform='uppercase'
                >
                  {title}
                </Text>
                <SimpleGrid
                  minChildWidth='240px'
                  gap='2'
                >
                  {features.map((feature) => (
                    <FeatureButton
                      key={`${feature.key}-${feature.source}`}
                      feature={feature}
                    />
                  ))}
                </SimpleGrid>
              </Stack>
            ))}
        </Stack>
      </Card.Body>
    </Card.Root>
  )
}

function FeatureButton({ feature }: { feature: FeatureSheet }) {
  const { inspect } = useInspector()
  return (
    <Button
      variant='outline'
      h='auto'
      py='2'
      px='3'
      justifyContent='start'
      textAlign='left'
      onClick={() => inspect({
        kind: 'feature',
        name: feature.name,
        source: feature.source,
        description: feature.description,
      })}
    >
      <Box>
        <Text fontWeight='semibold'>{feature.name}</Text>
        <Text
          fontSize='xs'
          color='fg.muted'
          fontWeight='normal'
        >
          {feature.source}
        </Text>
      </Box>
    </Button>
  )
}
