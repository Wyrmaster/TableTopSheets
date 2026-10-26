import {
  Alert,
  Badge,
  Box,
  Button,
  Card,
  Flex,
  Heading,
  HStack,
  IconButton,
  Menu,
  NativeSelect,
  Portal,
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
  RemoveItemIntent,
} from '@/api'
import { ApiHint } from '@/components/ApiHint'
import { Icon } from '@/components/Icon'
import {
  alertIcon,
  armorIcon,
  bagIcon,
  meleeIcon,
  moreIcon,
  rangedIcon,
  ringIcon,
  shieldIcon,
} from '@/components/icons'
import { InfoButton } from '@/components/InfoButton'
import { useApiAction } from '@/hooks/useApiAction'
import { StartingEquipmentCard } from '@/components/startingEquipment/StartingEquipmentCard'
import { useItemDetails } from '@/hooks/useItemDetails'
import { formatPrice } from './coins'
import { CoinsCard } from './CoinsCard'
import { useCharacter } from './useCharacter'

type Section = 'armor' | 'melee' | 'ranged' | 'worn' | 'backpack'

/** "2 equipped · 1 in hand" */
function equippedHint(items: CharacterItemDto[]) {
  const equipped = items.filter((item) => item.slot !== 'None').length
  const inHand = items.filter((item) => item.slot === 'MainHand' || item.slot === 'OffHand').length
  return `${equipped} equipped${inHand ? ` · ${inHand} in hand` : ''}`
}

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

  // the switch equips: weapons become ready (any number), armor is worn, a shield goes to the off hand
  const toggle = (item: CharacterItemDto) => {
    if (item.slot !== 'None') {
      return equip(item, 'None')
    }

    const detail = details.get(item.itemKey)
    const section = sectionOf(item, detail)
    if (section === 'armor') {
      return equip(item, isShield(detail) ? 'OffHand' : 'Armor')
    }

    return equip(item, section === 'worn' ? 'Worn' : 'Ready')
  }

  // equipped weapons can be taken into a hand; the weapon held there goes back to ready
  const handButtons = (item: CharacterItemDto) => {
    if (item.slot === 'None') {
      return null
    }

    const inHand = item.slot === 'MainHand' || item.slot === 'OffHand'
    return (
      <HStack gap='1'>
        {inHand
          ? (
            <Button
              size='xs'
              variant='outline'
              onClick={() => equip(item, 'Ready')}
            >
              Stow
            </Button>
          )
          : (
            <>
              <Button
                size='xs'
                variant='outline'
                onClick={() => equip(item, 'MainHand')}
              >
                Main hand
              </Button>
              <Button
                size='xs'
                variant='outline'
                onClick={() => equip(item, 'OffHand')}
              >
                Off hand
              </Button>
            </>
          )}
      </HStack>
    )
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

  // sell credits half the cost per piece; remove throws the item away
  const remove = async (item: CharacterItemDto, options: { quantity?: number, sell?: boolean }) => {
    const pieces = options.quantity ?? item.quantity
    const result = await run(
      new RemoveItemIntent(id, item.id, options),
      { success: `${options.sell ? 'Sold' : 'Removed'} ${pieces > 1 ? `${pieces} × ` : ''}${item.name}` },
    )
    if (result.ok) {
      applyState(result.data)
    }
  }

  const rows = (items: CharacterItemDto[], extra?: (item: CharacterItemDto) => ReactNode) => items.map((item) => (
    <ItemRow
      key={item.id}
      item={item}
      detail={details.get(item.itemKey)}
      attack={attack(item)}
      violations={itemViolations(item).map((violation) => violation.message)}
      onToggle={item.kind === 'Gear' && !item.requiresAttunement ? undefined : () => toggle(item)}
      extra={(
        <>
          {extra?.(item)}
          <ItemMenu
            item={item}
            costCp={details.get(item.itemKey)?.item.costCp}
            bundle={details.get(item.itemKey)?.item.bundle}
            onRemove={(options) => remove(item, options)}
          />
        </>
      )}
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

      <StartingEquipmentCard
        state={state}
        onState={applyState}
      />

      <SimpleGrid
        minChildWidth='320px'
        gap='4'
      >
        <CoinsCard />
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
          hint={equippedHint(bySection('melee'))}
        >
          {rows(bySection('melee'), handButtons)}
        </Section>
        <Section
          icon={rangedIcon}
          title='Weapons – ranged'
          hint={equippedHint(bySection('ranged'))}
        >
          {rows(bySection('ranged'), handButtons)}
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
        equip = <code>PUT /api/characters/{'{id}'}/items/{'{itemId}'}/slot</code> with slot <code>Ready</code>
        (any number of weapons), <code>MainHand</code> / <code>OffHand</code> (one item each), <code>Armor</code>,
        <code> Worn</code> or
        <code> None</code>; attunement = <code>PUT …/attunement</code>, AC calculation = <code>PUT …/armor-class</code>
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
      // not equipped: the sheet lists no attack, so the damage comes from the item details
      properties.damage && `${properties.damage} ${properties.damageType?.toLowerCase() ?? ''}`,
      properties.baseAc && `AC ${properties.baseAc}`,
      properties.range && `range ${properties.range}`,
      detail?.item.category,
      detail && `${detail.item.weight} lb${detail.item.bundle > 1 ? ` per ${detail.item.bundle}` : ''}`,
    ].filter(Boolean).join(' · ')
  const slotLabel: Partial<Record<EquipSlotEnum, string>> = {
    MainHand: 'main hand',
    OffHand: 'off hand',
    Ready: 'ready',
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
          {item.notes && <Text color='fg.muted'>({item.notes})</Text>}
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

/**
 * Sell or remove an item: one piece of a stack, or all of it; selling credits half the cost (per piece of a bundle)
 */
function ItemMenu({
  item,
  costCp,
  bundle = 1,
  onRemove,
}: {
  item: CharacterItemDto
  costCp?: number
  bundle?: number
  onRemove: (options: { quantity?: number, sell?: boolean }) => void
}) {
  // the same rounding as the API: half the cost of the pieces, rounded down
  const sellValue = (pieces: number) => (costCp === undefined ? 0 : Math.floor(costCp * pieces / bundle / 2))
  const sellPrice = sellValue(1)
  const stack = item.quantity > 1
  const actions: Record<string, { quantity?: number, sell?: boolean }> = {
    'sell-one': {
      quantity: 1,
      sell: true,
    },
    'sell-all': { sell: true },
    'remove-one': { quantity: 1 },
    'remove-all': {},
  }

  return (
    <Menu.Root onSelect={(details) => onRemove(actions[details.value])}>
      <Menu.Trigger asChild>
        <IconButton
          size='xs'
          variant='ghost'
          aria-label={`More actions for ${item.name}`}
          title='Sell or remove'
        >
          <Icon path={moreIcon} />
        </IconButton>
      </Menu.Trigger>
      <Portal>
        <Menu.Positioner>
          <Menu.Content>
            {sellPrice > 0 && (
              <Menu.Item value={stack ? 'sell-one' : 'sell-all'}>
                Sell {stack ? 'one ' : ''}for {formatPrice(sellPrice)}
              </Menu.Item>
            )}
            {sellValue(item.quantity) > 0 && stack && (
              <Menu.Item value='sell-all'>
                Sell all {item.quantity} for {formatPrice(sellValue(item.quantity))}
              </Menu.Item>
            )}
            {stack && <Menu.Item value='remove-one'>Remove one</Menu.Item>}
            <Menu.Item
              value='remove-all'
              color='fg.error'
            >
              {stack ? `Remove all ${item.quantity}` : 'Remove'}
            </Menu.Item>
          </Menu.Content>
        </Menu.Positioner>
      </Portal>
    </Menu.Root>
  )
}
