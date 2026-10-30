import { Badge, Box, Button, HStack, Heading, Spinner, Stack, Text } from '@chakra-ui/react'
import {
  type ClassDetailDto,
  GetBackgroundIntent,
  GetClassIntent,
  GetFeatIntent,
  GetItemIntent,
  GetSpeciesIntent,
  GetSpellIntent,
  type ItemDetailDto,
  problemMessages,
  type RuleDetailDto,
  type SpellDetailDto,
} from '@/api'
import { ApiHint } from '@/components/ApiHint'
import { Facts, Prose } from '@/components/Facts'
import { RuleImage } from '@/components/RuleImage'
import { StartingEquipmentList } from '@/components/startingEquipment/StartingEquipmentList'
import { useIntentQuery } from '@/hooks/useIntentQuery'
import type { InspectTarget } from './InspectorContext'

const levelName = (level: number) => (level === 0 ? 'Cantrip' : `Level ${level}`)

/**
 * Loads and renders one inspect target
 */
export const InspectorContent = ({ target, onInspect }: {
  target: InspectTarget
  onInspect: (next: InspectTarget) => void
}) => {
  if (target.kind === 'feature') {
    return (
      <Stack gap='3'>
        <Header
          label={target.source}
          title={target.name}
        />
        <Prose text={target.description} />
      </Stack>
    )
  }

  return (
    <RuleContent
      target={target}
      onInspect={onInspect}
    />
  )
}

const Header = ({ label, title }: { label: string, title: string }) => {
  return (
    <Box pr='10'>
      <Text
        fontSize='sm'
        color='fg.muted'
      >
        {label}
      </Text>
      <Heading size='xl'>{title}</Heading>
    </Box>
  )
}

const RuleContent = (
  { target, onInspect }: { target: Extract<InspectTarget, { key: string }>, onInspect: (next: InspectTarget) => void },
) => {
  const query = useIntentQuery<unknown>(() => {
    switch (target.kind) {
      case 'spell':
        return new GetSpellIntent(target.key)
      case 'species':
        return new GetSpeciesIntent(target.key)
      case 'background':
        return new GetBackgroundIntent(target.key)
      case 'feat':
        return new GetFeatIntent(target.key)
      case 'class':
        return new GetClassIntent(target.key)
      case 'item':
        return new GetItemIntent(target.key)
    }
  }, `${target.kind}:${target.key}`)

  if (query.problem) {
    return <Text color='fg.error'>{problemMessages(query.problem).join(' ')}</Text>
  }

  if (!query.data || query.loading) {
    return <Spinner />
  }

  const route = `GET /api/rules/${target.kind === 'species' ? 'species' : `${target.kind}s`}/${target.key}`
  return (
    <Stack gap='4'>
      {target.kind === 'spell' && <SpellView spell={query.data as SpellDetailDto} />}
      {target.kind === 'class' && <ClassView detail={query.data as ClassDetailDto} />}
      {target.kind === 'item' && <ItemView detail={query.data as ItemDetailDto} />}
      {(target.kind === 'species' || target.kind === 'background' || target.kind === 'feat') && (
        <RuleView
          label={{
            species: 'Species',
            background: 'Background',
            feat: 'Feat',
          }[target.kind]}
          detail={query.data as RuleDetailDto}
          onInspect={onInspect}
        />
      )}
      <ApiHint><code>{route}</code></ApiHint>
    </Stack>
  )
}

const SpellView = ({ spell }: { spell: SpellDetailDto }) => {
  const summary = spell.spell
  const facts: [string, string][] = [
    ['Casting time', summary.castingTime],
    ['Range', summary.range],
    ['Components', spell.material ? `${spell.components} (${spell.material})` : spell.components],
    ['Duration', spell.duration],
  ]
  if (spell.attackRoll) {
    facts.push(['Attack', 'Spell attack roll'])
  }
  if (spell.saveAbility) {
    facts.push(['Saving throw', spell.saveAbility])
  }
  if (summary.damage) {
    facts.push(['Damage', `${summary.damage}${summary.damageType ? ` ${summary.damageType.toLowerCase()}` : ''}`])
  }
  facts.push(['Spell lists', summary.lists.join(', ')])

  return (
    <>
      <Header
        label={`${levelName(summary.level)} · ${summary.school}`}
        title={summary.name}
      />
      {summary.imageUrl && (
        <RuleImage
          src={summary.imageUrl}
          alt={summary.name}
        />
      )}
      <HStack>
        {summary.concentration && <Badge>Concentration</Badge>}
        {summary.ritual && <Badge>Ritual</Badge>}
      </HStack>
      <Facts items={facts} />
      <Prose text={spell.description} />
    </>
  )
}

const ClassView = ({ detail }: { detail: ClassDetailDto }) => {
  const features = [...detail.features].sort((a, b) => (a.level ?? 0) - (b.level ?? 0))
  return (
    <>
      <Header
        label='Class'
        title={detail.class.name}
      />
      {detail.class.imageUrl && (
        <RuleImage
          src={detail.class.imageUrl}
          alt={detail.class.name}
          ratio={4 / 3}
          position='50% 20%'
        />
      )}
      <Facts
        items={[
          ['Hit die', detail.class.hitDie.toLowerCase()],
          [
            'Primary ability',
            detail.class.primaryAbilities.join(detail.class.primaryAbilityMode === 'Any' ? ' or ' : ' and '),
          ],
          ['Subclass', `at level ${detail.class.subclassLevel}`],
          ['Subclasses', detail.subclasses.map((subclass) => subclass.name).join(', ')],
        ]}
      />
      <Prose text={detail.description} />
      <StartingEquipmentList options={detail.startingEquipment} />
      <Heading size='sm'>Features by level</Heading>
      <Stack gap='2'>
        {features.map((feature) => (
          <Box
            key={feature.key}
            borderWidth='1px'
            rounded='md'
            px='3'
            py='2'
          >
            <HStack justify='space-between'>
              <Text fontWeight='semibold'>{feature.name}</Text>
              <Badge>Level {feature.level}</Badge>
            </HStack>
            {feature.choices.length > 0 && (
              <Text
                fontSize='sm'
                color='fg.muted'
              >
                Choices: {feature.choices.map((choice) => choice.name).join(', ')}
              </Text>
            )}
          </Box>
        ))}
      </Stack>
    </>
  )
}

const ItemView = ({ detail }: { detail: ItemDetailDto }) => {
  const item = detail.item
  const properties = Object.entries(detail.properties).map(([key, value]): [string, string] => [key, value])
  return (
    <>
      <Header
        label={`${item.kind}${item.category ? ` · ${item.category}` : ''}`}
        title={item.name}
      />
      {item.imageUrl && (
        <RuleImage
          src={item.imageUrl}
          alt={item.name}
        />
      )}
      <HStack>
        {item.magic && <Badge colorPalette='brand'>Magic</Badge>}
        {item.requiresAttunement && <Badge>Requires attunement</Badge>}
      </HStack>
      <Facts
        items={[
          ['Cost', `${item.costCp / 100} gp${item.bundle > 1 ? ` per ${item.bundle}` : ''}`],
          ['Weight', `${item.weight} lb`],
          ...properties,
        ]}
      />
      {detail.description && <Prose text={detail.description} />}
    </>
  )
}

const RuleView = (
  {
    label,
    detail,
    onInspect,
  }: {
    label: string,
    detail: RuleDetailDto,
    onInspect: (next: InspectTarget) => void
  },
) => {
  return (
    <>
      <Header
        label={label}
        title={detail.name}
      />
      {detail.imageUrl && (
        <RuleImage
          src={detail.imageUrl}
          alt={detail.name}
        />
      )}
      {detail.description && <Prose text={detail.description} />}
      <StartingEquipmentList options={detail.startingEquipment ?? []} />
      {detail.features.map((feature) => (
        <Box key={feature.key}>
          <Heading
            size='sm'
            mb='1'
          >
            {feature.name}
          </Heading>
          <Prose text={feature.description} />
          {feature.choices.length > 0 && (
            <Text
              mt='1'
              fontSize='sm'
              color='fg.muted'
            >
              Choices: {feature.choices.map((choice) => choice.name).join(', ')}
            </Text>
          )}
          {feature.key.startsWith('feat.') && label !== 'Feat' && (
            <Button
              mt='2'
              size='xs'
              variant='outline'
              onClick={() => onInspect({
                kind: 'feat',
                key: feature.key.slice('feat.'.length),
              })}
            >
              Show feat
            </Button>
          )}
        </Box>
      ))}
    </>
  )
}
