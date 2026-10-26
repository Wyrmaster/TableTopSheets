import {
  Alert,
  Badge,
  Box,
  Button,
  Card,
  Flex,
  Heading,
  HStack,
  NativeSelect,
  SimpleGrid,
  Stack,
  Switch,
  Text,
} from '@chakra-ui/react'
import type { ReactNode } from 'react'
import {
  AttuneItemIntent,
  type AttackSheet,
  type CharacterItemDto,
  ChooseArmorClassIntent,
  type EquipSlotEnum,
  EquipItemIntent,
  type ItemDetailDto,
} from '@/api'
import { ApiHint } from '@/components/ApiHint'
import { Icon } from '@/components/Icon'
import { alertIcon, armorIcon, bagIcon, meleeIcon, rangedIcon, ringIcon, shieldIcon } from '@/components/icons'
import { InfoButton } from '@/components/InfoButton'
import { useApiAction } from '@/hooks/useApiAction'
import { useItemDetails } from '@/hooks/useItemDetails'
import { useCharacter } from './useCharacter'

type Section = 'armor' | 'melee' | 'ranged' | 'worn' | 'backpack'

const isShield = (detail?: ItemDetailDto) => detail?.item.category?.toLowerCase() === 'shield'
  || detail?.item.key === 'shield'

/**
 * Section of an item: armor & shield, melee or ranged weapon, worn item or backpack
 */
function sectionOf(item: CharacterItemDto, detail?: ItemDetailDto): Section {
  if (item.kind === 'Armor') {
    return 'armor'
  }
  if (item.kind === 'Weapon') {
    return detail?.properties.ranged === 'True' ? 'ranged' : 'melee'
  }

  return item.requiresAttunement || item.slot === 'Worn' ? 'worn' : 'backpack'
}

/**
 * Armor & shield, melee and ranged weapons, worn items and the backpack, with equip switches and violations
 */
export function InventoryTab() {
  const {
    id,
    state,
    sheet,
    applyState,
  } = useCharacter()
  const { run } = useApiAction()
  const details = useItemDetails(state.items.map((item) => item.itemKey))
  const violations = state.violations.filter((violation) => violation.code !== 'choice.pending')

  const equip = async (item: CharacterItemDto, slot: EquipSlotEnum) => {
    const result = await run(new EquipItemIntent(id, item.id, { slot }))
    if (result.ok) {
      applyState(result.data)
    }
  }

  // the API holds one item per hand: a weapon goes to the main hand, or the off hand when the main hand is taken
  const toggle = (item: CharacterItemDto) => {
    if (item.slot !== 'None') {
      return equip(item, 'None')
    }

    const detail = details.get(item.itemKey)
    const section = sectionOf(item, detail)
    if (section === 'armor') {
      return equip(item, isShield(detail) ? 'OffHand' : 'Armor')
    }
    if (section === 'worn') {
      return equip(item, 'Worn')
    }

    const mainHandTaken = state.items.some((entry) => entry.slot === 'MainHand')
    return equip(item, mainHandTaken && !state.items.some((entry) => entry.slot === 'OffHand') ? 'OffHand' : 'MainHand')
  }

  const attune = async (item: CharacterItemDto) => {
    const result = await run(new AttuneItemIntent(id, item.id, { attuned: !item.attuned }))
    if (result.ok) {
      applyState(result.data)
    }
  }

  const chooseArmorClass = async (label: string) => {
    const result = await run(new ChooseArmorClassIntent(id, { label: label || null }))
    if (result.ok) {
      applyState(result.data)
    }
  }

  const bySection = (section: Section) => state.items
    .filter((item) => sectionOf(item, details.get(item.itemKey)) === section)
  const itemViolations = (item: CharacterItemDto) => violations
    .filter((violation) => violation.message.includes(item.name))
  const attack = (item: CharacterItemDto) => sheet.attacks.find((entry) => entry.name === item.name)

  const rows = (items: CharacterItemDto[], extra?: (item: CharacterItemDto) => ReactNode) => items.map((item) => (
    <ItemRow
      key={item.id}
      item={item}
      detail={details.get(item.itemKey)}
      attack={attack(item)}
      violations={itemViolations(item).map((violation) => violation.message)}
      onToggle={item.kind === 'Gear' && !item.requiresAttunement ? undefined : () => toggle(item)}
      extra={extra?.(item)}
    />
  ))

  const attuned = state.items.filter((item) => item.attuned).length
  const unmatched = violations.filter((violation) => !state.items.some((item) => violation.message.includes(item.name)))

  return (
    <Stack gap='4'>
      {unmatched.map((violation) => (
        <Alert.Root
          key={violation.message}
          status={violation.severity === 'Error' ? 'error' : 'warning'}
        >
          <Alert.Indicator />
          <Alert.Title>{violation.message}</Alert.Title>
        </Alert.Root>
      ))}

      <SimpleGrid
        minChildWidth='320px'
        gap='4'
      >
        <Section
          icon={armorIcon}
          title='Armor & shield'
        >
          {rows(bySection('armor'))}
        </Section>
        <Card.Root>
          <Card.Body gap='3'>
            <HStack justify='space-between'>
              <Heading size='md'>
                <HStack>
                  <Icon path={shieldIcon} />
                  Armor class
                </HStack>
              </Heading>
              <Text
                fontSize='2xl'
                fontWeight='bold'
              >
                {sheet.armorClass}
              </Text>
            </HStack>
            <Text
              fontSize='sm'
              fontWeight='medium'
            >
              Calculation
            </Text>
            <NativeSelect.Root size='sm'>
              <NativeSelect.Field onChange={(event) => chooseArmorClass(event.target.value)}>
                <option value=''>Best</option>
                {sheet.armorClassOptions.map((option) => (
                  <option
                    key={option.label}
                    value={option.label}
                  >
                    {option.label} ({option.value})
                  </option>
                ))}
              </NativeSelect.Field>
              <NativeSelect.Indicator />
            </NativeSelect.Root>
            {sheet.armorClassSources.length > 0 && (
              <Text
                fontSize='sm'
                color='fg.muted'
              >
                {sheet.armorClassSources
                  .map((source) => `${source.label} ${source.value >= 0 ? '+' : ''}${source.value}`)
                  .join(' · ')}
              </Text>
            )}
          </Card.Body>
        </Card.Root>
      </SimpleGrid>

      <SimpleGrid
        minChildWidth='320px'
        gap='4'
      >
        <Section
          icon={meleeIcon}
          title='Weapons – melee'
          hint={`${bySection('melee').filter((item) => item.slot !== 'None').length} in hand`}
        >
          {rows(bySection('melee'))}
        </Section>
        <Section
          icon={rangedIcon}
          title='Weapons – ranged'
          hint={`${bySection('ranged').filter((item) => item.slot !== 'None').length} in hand`}
        >
          {rows(bySection('ranged'))}
        </Section>
      </SimpleGrid>

      <SimpleGrid
        minChildWidth='320px'
        gap='4'
      >
        <Section
          icon={ringIcon}
          title='Worn & attuned'
          hint={`Attuned ${attuned} / 3`}
        >
          {rows(bySection('worn'), (item) => item.requiresAttunement && item.slot !== 'None' && (
            <Button
              size='xs'
              variant='outline'
              onClick={() => attune(item)}
            >
              {item.attuned ? 'End attunement' : 'Attune'}
            </Button>
          ))}
        </Section>
        <Section
          icon={bagIcon}
          title='Backpack'
        >
          {rows(bySection('backpack'))}
        </Section>
      </SimpleGrid>

      <ApiHint>
        equip = <code>PUT /api/characters/{'{id}'}/items/{'{itemId}'}/slot</code> (one item per hand: a second weapon
        goes to
        the off hand, a third replaces the main hand – any number of ready weapons needs an API change), attunement =
        <code> PUT …/attunement</code>, AC calculation = <code>PUT …/armor-class</code>
      </ApiHint>
    </Stack>
  )
}

function Section({
  icon,
  title,
  hint,
  children,
}: {
  icon: string,
  title: string,
  hint?: string,
  children: ReactNode[]
}) {
  return (
    <Card.Root>
      <Card.Body gap='3'>
        <HStack justify='space-between'>
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
        {children.length > 0
          ? (
            <Box
              borderWidth='1px'
              rounded='md'
              overflow='hidden'
            >
              {children}
            </Box>
          )
          : <Text color='fg.muted'>Nothing here – buy items in the shop.</Text>}
      </Card.Body>
    </Card.Root>
  )
}

function ItemRow({
  item,
  detail,
  attack,
  violations,
  onToggle,
  extra,
}: {
  item: CharacterItemDto
  detail?: ItemDetailDto
  attack?: AttackSheet
  violations: string[]
  onToggle?: () => void
  extra?: ReactNode
}) {
  const properties = detail?.properties ?? {}
  const description = attack
    ? [
      `${attack.attackBonus >= 0 ? '+' : ''}${attack.attackBonus} to hit`,
      `${attack.damage} ${attack.damageType?.toLowerCase() ?? ''}`,
      attack.notes,
    ].filter(Boolean).join(' · ')
    : [
      properties.baseAc && `AC ${properties.baseAc}`,
      detail?.item.category,
      detail && `${detail.item.weight} lb`,
    ].filter(Boolean).join(' · ')
  const slotLabel: Partial<Record<EquipSlotEnum, string>> = {
    MainHand: 'main hand',
    OffHand: 'off hand',
  }

  return (
    <Flex
      wrap='wrap'
      align='center'
      gap='3'
      px='3'
      py='2.5'
      borderTopWidth='1px'
      _first={{ borderTopWidth: '0' }}
      borderLeftWidth='3px'
      borderLeftColor={violations.length ? 'orange.solid' : 'transparent'}
      bg={violations.length ? 'orange.subtle' : undefined}
    >
      {onToggle && (
        <Switch.Root
          colorPalette='brand'
          checked={item.slot !== 'None'}
          onCheckedChange={onToggle}
        >
          <Switch.HiddenInput aria-label={`Equip ${item.name}`} />
          <Switch.Control />
        </Switch.Root>
      )}
      <Box
        flex='1'
        minW='160px'
      >
        <HStack gap='2'>
          <Text fontWeight='semibold'>{item.name}</Text>
          {item.quantity > 1 && <Badge>×{item.quantity}</Badge>}
          {slotLabel[item.slot] && <Badge colorPalette='brand'>{slotLabel[item.slot]}</Badge>}
          {item.attuned && <Badge colorPalette='brand'>attuned</Badge>}
        </HStack>
        <Text
          fontSize='sm'
          color='fg.muted'
        >
          {description}
        </Text>
      </Box>
      {extra}
      <InfoButton target={{
        kind: 'item',
        key: item.itemKey,
      }}
      />
      {violations.map((message) => (
        <HStack
          key={message}
          flexBasis='100%'
          gap='1.5'
          fontSize='xs'
          color='orange.fg'
        >
          <Icon path={alertIcon} />
          {message}
        </HStack>
      ))}
    </Flex>
  )
}
