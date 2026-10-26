import {
  Badge,
  Box,
  Button,
  Card,
  Dialog,
  Flex,
  Heading,
  HStack,
  NativeSelect,
  NumberInput,
  Portal,
  RadioGroup,
  SimpleGrid,
  Spinner,
  Stack,
  Text,
} from '@chakra-ui/react'
import { useState } from 'react'
import { type ClassSummaryDto, LevelUpIntent, ListClassesIntent, PreviewLevelUpIntent, UndoLevelIntent } from '@/api'
import { ApiHint } from '@/components/ApiHint'
import { ChoiceList } from '@/components/choices/ChoiceList'
import { Icon } from '@/components/Icon'
import { levelUpIcon, undoIcon } from '@/components/icons'
import { InfoButton } from '@/components/InfoButton'
import { useApiAction } from '@/hooks/useApiAction'
import { useIntentQuery } from '@/hooks/useIntentQuery'
import { useCharacter } from './useCharacter'

const dieSize = (die: string) => Number(die.slice(1))

/**
 * Open choices of the character, gaining a level (class, subclass, hit points) and undoing the last level
 */
export function LevelUpPage() {
  const {
    id,
    state,
    applyState,
  } = useCharacter()

  return (
    <Stack gap='8'>
      {state.pendingChoices.length > 0 && (
        <Stack gap='3'>
          <Heading size='lg'>Open choices</Heading>
          <ChoiceList
            characterId={id}
            choices={state.pendingChoices}
            onState={applyState}
          />
          <ApiHint>
            <code>GET …/choices/{'{choiceKey}'}/options</code> for large lists,
            <code> POST /api/characters/{'{id}'}/choices</code>
            {' '}to save, <code>DELETE …/selections/{'{selectionId}'}</code> to take one back
          </ApiHint>
        </Stack>
      )}
      <GainLevel />
    </Stack>
  )
}

function GainLevel() {
  const {
    id,
    state,
    sheet,
    applyState,
  } = useCharacter()
  const { run, busy } = useApiAction()
  const classes = useIntentQuery(() => new ListClassesIntent({ pageSize: 50 }), 'classes')
  const [classKey, setClassKey] = useState(state.classes[0]?.key ?? '')
  const [subclassKey, setSubclassKey] = useState('')
  const [hitPoints, setHitPoints] = useState<'average' | 'roll'>('average')
  const [roll, setRoll] = useState(1)

  const preview = useIntentQuery(
    () => (classKey ? new PreviewLevelUpIntent(id, classKey, subclassKey || undefined) : null),
    `${id}|${classKey}|${subclassKey}|${state.level}`,
  )
  const selected = classes.data?.items.find((entry) => entry.key === classKey)
  const conModifier = sheet.abilities.find((ability) => ability.key === 'con')?.modifier ?? 0
  const average = selected ? dieSize(selected.hitDie) / 2 + 1 + conModifier : 0

  const choose = (key: string) => {
    setClassKey(key)
    setSubclassKey('')
    setRoll(1)
  }

  const levelUp = async () => {
    const result = await run(new LevelUpIntent(id, {
      classKey,
      subclassKey: subclassKey || null,
      hitPointRoll: hitPoints === 'roll' ? roll : null,
    }), { success: `${state.name} is now level ${state.level + 1}` })
    if (result.ok) {
      setSubclassKey('')
      applyState(result.data)
    }
  }

  const undo = async () => {
    const result = await run(new UndoLevelIntent(id), { success: `Level ${state.level} removed` })
    if (result.ok) {
      applyState(result.data)
    }
  }

  const current = (entry: ClassSummaryDto) => state.classes.find((owned) => owned.key === entry.key)
  // the classes the character already has come first
  const ordered = [...(classes.data?.items ?? [])]
    .sort((a, b) => Number(Boolean(current(b))) - Number(Boolean(current(a))))
  const needsSubclass = preview.data?.requiresSubclass && !subclassKey

  return (
    <Stack gap='4'>
      <Flex
        wrap='wrap'
        gap='3'
        align='center'
        justify='space-between'
      >
        <Box>
          <Heading size='lg'>Gain a level</Heading>
          <Text color='fg.muted'>
            Character level {state.level} → {state.level + 1}. Nothing is saved until you press “Level up”.
          </Text>
        </Box>
        {state.level > 0 && (
          <UndoButton
            level={state.level}
            onUndo={undo}
          />
        )}
      </Flex>

      {classes.loading && !classes.data && <Spinner />}
      <SimpleGrid
        minChildWidth='210px'
        gap='3'
      >
        {ordered.map((entry) => {
          const owned = current(entry)
          return (
            <Card.Root
              key={entry.key}
              cursor='pointer'
              borderColor={entry.key === classKey ? 'brand.solid' : undefined}
              bg={entry.key === classKey ? 'brand.subtle' : undefined}
              onClick={() => choose(entry.key)}
            >
              <Card.Body
                p='3.5'
                gap='1'
              >
                <HStack justify='space-between'>
                  <Text fontWeight='semibold'>
                    {owned ? `${entry.name} ${owned.level} → ${owned.level + 1}` : `+ ${entry.name} 1`}
                  </Text>
                  <InfoButton target={{
                    kind: 'class',
                    key: entry.key,
                  }}
                  />
                </HStack>
                <Text
                  fontSize='sm'
                  color='fg.muted'
                >
                  {entry.hitDie.toLowerCase()} ·{' '}
                  {entry.primaryAbilities.join(entry.primaryAbilityMode === 'Any' ? ' or ' : ' and ')}
                  {owned ? '' : state.classes.length ? ' · multiclass' : ''}
                </Text>
              </Card.Body>
            </Card.Root>
          )
        })}
      </SimpleGrid>

      {selected && (
        <Card.Root>
          <Card.Body gap='4'>
            <Heading size='md'>{selected.name} {(current(selected)?.level ?? 0) + 1}</Heading>

            {preview.data?.requiresSubclass && (
              <Box>
                <Text
                  fontWeight='medium'
                  mb='1.5'
                >
                  Subclass
                </Text>
                <NativeSelect.Root
                  size='sm'
                  maxW='sm'
                >
                  <NativeSelect.Field
                    value={subclassKey}
                    onChange={(event) => setSubclassKey(event.target.value)}
                  >
                    <option value=''>Choose a subclass…</option>
                    {(preview.data.subclasses.length ? preview.data.subclasses : selected.subclasses)
                      .map((subclass) => (
                        <option
                          key={subclass.key}
                          value={subclass.key}
                        >
                          {subclass.name}
                        </option>
                      ))}
                  </NativeSelect.Field>
                  <NativeSelect.Indicator />
                </NativeSelect.Root>
              </Box>
            )}

            <Box>
              <Text
                fontWeight='medium'
                mb='1.5'
              >
                Hit points
              </Text>
              <RadioGroup.Root
                value={hitPoints}
                onValueChange={(details) => setHitPoints(details.value as 'average' | 'roll')}
                colorPalette='brand'
              >
                <HStack
                  gap='5'
                  wrap='wrap'
                >
                  <RadioGroup.Item value='average'>
                    <RadioGroup.ItemHiddenInput />
                    <RadioGroup.ItemIndicator />
                    <RadioGroup.ItemText>Take average: {average}</RadioGroup.ItemText>
                  </RadioGroup.Item>
                  <RadioGroup.Item value='roll'>
                    <RadioGroup.ItemHiddenInput />
                    <RadioGroup.ItemIndicator />
                    <RadioGroup.ItemText>Roll {selected.hitDie.toLowerCase()}</RadioGroup.ItemText>
                  </RadioGroup.Item>
                  {hitPoints === 'roll' && (
                    <NumberInput.Root
                      size='sm'
                      w='24'
                      min={1}
                      max={dieSize(selected.hitDie)}
                      value={String(roll)}
                      onValueChange={(details) => setRoll(details.valueAsNumber || 1)}
                    >
                      <NumberInput.Control />
                      <NumberInput.Input />
                    </NumberInput.Root>
                  )}
                </HStack>
              </RadioGroup.Root>
            </Box>

            <Box>
              <Text
                fontWeight='medium'
                mb='1.5'
              >
                This level asks for
              </Text>
              {preview.loading && <Spinner size='sm' />}
              {preview.problem && <Text color='fg.error'>{preview.problem.detail ?? preview.problem.title}</Text>}
              {needsSubclass && <Text color='fg.muted'>Choose the subclass to see the choices.</Text>}
              {preview.data && !needsSubclass && (
                <HStack wrap='wrap'>
                  {preview.data.pendingChoices.length === 0 && (
                    <Text color='fg.muted'>No choices – only new features.</Text>
                  )}
                  {preview.data.pendingChoices.map((choice) => (
                    <Badge
                      key={choice.id}
                      size='lg'
                    >
                      {choice.name} · {choice.remaining}
                    </Badge>
                  ))}
                  {preview.data.replaceable.length > 0 && (
                    <Badge
                      size='lg'
                      colorPalette='brand'
                    >
                      may swap one of {preview.data.replaceable.length}
                    </Badge>
                  )}
                </HStack>
              )}
              <Text
                mt='2'
                fontSize='sm'
                color='fg.muted'
              >
                The choices are answered after the level is saved, under “Open choices”.
              </Text>
            </Box>

            <Flex justify='flex-end'>
              <Button
                colorPalette='brand'
                loading={busy}
                disabled={Boolean(needsSubclass) || Boolean(preview.problem)}
                onClick={levelUp}
              >
                <Icon path={levelUpIcon} />
                Level up
              </Button>
            </Flex>
          </Card.Body>
        </Card.Root>
      )}
      <ApiHint>
        <code>GET …/level-up/preview?class=&amp;subclass=</code>, <code>POST /api/characters/{'{id}'}/levels</code>,
        <code> DELETE …/levels/last</code>; multiclass requirements are checked by the API (422 with the reason)
      </ApiHint>
    </Stack>
  )
}

function UndoButton({ level, onUndo }: { level: number, onUndo: () => void }) {
  return (
    <Dialog.Root placement='center'>
      <Dialog.Trigger asChild>
        <Button
          size='sm'
          variant='outline'
        >
          <Icon path={undoIcon} />
          Undo last level
        </Button>
      </Dialog.Trigger>
      <Portal>
        <Dialog.Backdrop />
        <Dialog.Positioner>
          <Dialog.Content>
            <Dialog.Header>
              <Dialog.Title>Undo level {level}?</Dialog.Title>
            </Dialog.Header>
            <Dialog.Body>
              <Text color='fg.muted'>
                The highest level and every choice made with it are removed; choices it replaced become active again.
              </Text>
            </Dialog.Body>
            <Dialog.Footer>
              <Dialog.ActionTrigger asChild>
                <Button variant='outline'>Cancel</Button>
              </Dialog.ActionTrigger>
              <Dialog.ActionTrigger asChild>
                <Button
                  colorPalette='red'
                  onClick={onUndo}
                >
                  Undo level
                </Button>
              </Dialog.ActionTrigger>
            </Dialog.Footer>
          </Dialog.Content>
        </Dialog.Positioner>
      </Portal>
    </Dialog.Root>
  )
}
