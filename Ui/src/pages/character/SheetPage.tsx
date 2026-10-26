import { Button, Dialog, Flex, HStack, NumberInput, Portal, SegmentGroup, Stack, Text } from '@chakra-ui/react'
import { useState } from 'react'
import { Navigate, useNavigate, useParams } from 'react-router'
import { Icon } from '@/components/Icon'
import { inventoryIcon, longRestIcon, shortRestIcon, spellsIcon, accountIcon } from '@/components/icons'
import { toaster } from '@/components/ui/toaster'
import { InventoryTab } from './InventoryTab'
import { OverviewTab } from './OverviewTab'
import { longRest, shortRest } from './playState'
import { SpellsTab } from './SpellsTab'
import { useCharacter } from './useCharacter'

const tabs = [
  {
    value: 'overview',
    label: 'Overview',
    icon: accountIcon,
  },
  {
    value: 'inventory',
    label: 'Inventory',
    icon: inventoryIcon,
  },
  {
    value: 'spells',
    label: 'Spells',
    icon: spellsIcon,
  },
]

/**
 * Character sheet: Overview · Inventory · Spells, plus short and long rest
 */
export function SheetPage() {
  const { tab = 'overview' } = useParams()
  const navigate = useNavigate()
  if (!tabs.some((entry) => entry.value === tab)) {
    return (
      <Navigate
        to='../sheet/overview'
        replace
      />
    )
  }

  return (
    <Stack gap='5'>
      <Flex
        wrap='wrap'
        gap='3'
        align='center'
        justify='space-between'
      >
        <SegmentGroup.Root
          value={tab}
          onValueChange={(details) => navigate(`../sheet/${details.value}`)}
        >
          <SegmentGroup.Indicator />
          <SegmentGroup.Items
            items={tabs.map((entry) => ({
              value: entry.value,
              label: (
                <HStack gap='1.5'>
                  <Icon path={entry.icon} />
                  {entry.label}
                </HStack>
              ),
            }))}
          />
        </SegmentGroup.Root>
        <RestButtons />
      </Flex>
      {tab === 'overview' && <OverviewTab />}
      {tab === 'inventory' && <InventoryTab />}
      {tab === 'spells' && <SpellsTab />}
    </Stack>
  )
}

function RestButtons() {
  const {
    sheet,
    updatePlay,
    play,
  } = useCharacter()
  const [spend, setSpend] = useState<Record<string, number>>({})
  const conModifier = sheet.abilities.find((ability) => ability.key === 'con')?.modifier ?? 0

  // average roll of the spent dice (die / 2 + 1) plus the CON modifier per die
  const healed = Object.entries(spend)
    .reduce((sum, [die, count]) => sum + count * (Number(die.slice(1)) / 2 + 1 + conModifier), 0)

  return (
    <HStack>
      <Dialog.Root
        placement='center'
        onOpenChange={(details) => details.open && setSpend({})}
      >
        <Dialog.Trigger asChild>
          <Button
            size='sm'
            variant='outline'
          >
            <Icon path={shortRestIcon} />
            Short rest
          </Button>
        </Dialog.Trigger>
        <Portal>
          <Dialog.Backdrop />
          <Dialog.Positioner>
            <Dialog.Content>
              <Dialog.Header>
                <Dialog.Title>Short rest</Dialog.Title>
              </Dialog.Header>
              <Dialog.Body>
                <Stack gap='3'>
                  <Text color='fg.muted'>
                    Restores resources that recharge on a short rest. Spend hit dice to heal (average roll + CON).
                  </Text>
                  {sheet.hitDice.map((pool) => {
                    const left = pool.count - (play.hitDiceUsed[pool.die] ?? 0)
                    return (
                      <HStack
                        key={pool.die}
                        justify='space-between'
                      >
                        <Text>
                          {pool.die.toLowerCase()} <Text
                            as='span'
                            color='fg.muted'
                          >
                            ({left} left)
                          </Text>
                        </Text>
                        <NumberInput.Root
                          size='sm'
                          w='24'
                          min={0}
                          max={left}
                          value={String(spend[pool.die] ?? 0)}
                          onValueChange={(details) => setSpend({
                            ...spend,
                            [pool.die]: details.valueAsNumber || 0,
                          })}
                        >
                          <NumberInput.Control />
                          <NumberInput.Input />
                        </NumberInput.Root>
                      </HStack>
                    )
                  })}
                  {healed > 0 && <Text>Heals {healed} HP.</Text>}
                </Stack>
              </Dialog.Body>
              <Dialog.Footer>
                <Dialog.ActionTrigger asChild>
                  <Button variant='outline'>Cancel</Button>
                </Dialog.ActionTrigger>
                <Dialog.ActionTrigger asChild>
                  <Button
                    colorPalette='brand'
                    onClick={() => {
                      updatePlay((current) => shortRest(current, sheet, healed, spend))
                      toaster.create({
                        type: 'success',
                        title: `Short rest${healed ? ` · +${healed} HP` : ''}`,
                      })
                    }}
                  >
                    Take short rest
                  </Button>
                </Dialog.ActionTrigger>
              </Dialog.Footer>
            </Dialog.Content>
          </Dialog.Positioner>
        </Portal>
      </Dialog.Root>

      <Dialog.Root placement='center'>
        <Dialog.Trigger asChild>
          <Button
            size='sm'
            variant='outline'
          >
            <Icon path={longRestIcon} />
            Long rest
          </Button>
        </Dialog.Trigger>
        <Portal>
          <Dialog.Backdrop />
          <Dialog.Positioner>
            <Dialog.Content>
              <Dialog.Header>
                <Dialog.Title>Long rest</Dialog.Title>
              </Dialog.Header>
              <Dialog.Body>
                <Text color='fg.muted'>
                  Regain all hit points, all spent hit dice, all spell slots and every resource.
                </Text>
              </Dialog.Body>
              <Dialog.Footer>
                <Dialog.ActionTrigger asChild>
                  <Button variant='outline'>Cancel</Button>
                </Dialog.ActionTrigger>
                <Dialog.ActionTrigger asChild>
                  <Button
                    colorPalette='brand'
                    onClick={() => {
                      updatePlay(longRest)
                      toaster.create({
                        type: 'success',
                        title: 'Long rest · everything restored',
                      })
                    }}
                  >
                    Take long rest
                  </Button>
                </Dialog.ActionTrigger>
              </Dialog.Footer>
            </Dialog.Content>
          </Dialog.Positioner>
        </Portal>
      </Dialog.Root>
    </HStack>
  )
}
