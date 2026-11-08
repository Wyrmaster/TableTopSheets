import {
  Alert,
  Badge,
  Box,
  Button,
  Card,
  Dialog,
  Drawer,
  Flex,
  Grid,
  Heading,
  HStack,
  Portal,
  Spinner,
  Stack,
  Steps,
  Text,
} from '@chakra-ui/react'
import { type ReactNode, useState } from 'react'
import { useNavigate, useParams, useSearchParams } from 'react-router'
import {
  AbilityKeys,
  type CharacterStateDto,
  CreateCharacterIntent,
  DeleteCharacterIntent,
  GetCharacterIntent,
  GetCharacterPortraitIntent,
  GetCharacterSheetIntent,
  LevelUpIntent,
  SetPortraitIntent,
  type StartingEquipmentOptionDto,
  type TakeStartingEquipmentDto,
  UpdateCharacterIntent,
} from '@/api'
import { ApiHint } from '@/components/ApiHint'
import type { CharacterDetails } from '@/components/characterDetails'
import { CharacterDetailsFields } from '@/components/CharacterDetailsFields'
import { ChoiceList } from '@/components/choices/ChoiceList'
import { SkillProficiencies } from '@/components/SkillProficiencies'
import { Icon } from '@/components/Icon'
import { statsIcon } from '@/components/icons'
import { equipmentComplete } from '@/components/startingEquipment/startingEquipment'
import { StartingEquipmentCard } from '@/components/startingEquipment/StartingEquipmentCard'
import { StartingEquipmentPicker } from '@/components/startingEquipment/StartingEquipmentPicker'
import { useRuleStartingEquipment } from '@/components/startingEquipment/useRuleStartingEquipment'
import { useApiAction } from '@/hooks/useApiAction'
import { useIntentQuery } from '@/hooks/useIntentQuery'
import { toBase64 } from '@/pages/character/portrait'
import {
  type AbilityMethod,
  completeScores,
  type DraftScores,
  initialScores,
  scoreProblem,
  signedModifier,
  StandardArray,
} from './abilityScores'
import { AbilityScoresStep } from './AbilityScoresStep'
import { RuleCards } from './RuleCards'

const steps = ['Class', 'Species', 'Background', 'Ability scores', 'Choices', 'Details']

/**
 * The ability scores step saves the draft; from the choices step on the creator works on the saved draft. Going back
 * and changing class, origin or scores replaces the draft on the next save
 */
const SaveStep = 3
const ChoicesStep = 4

/**
 * Name of the character until it is set in the details step; the API needs a name to save the character
 */
const PlaceholderName = 'New character'

/**
 * Character creator. Class, species, background and ability scores are chosen locally; "Save" stores the character
 * (POST /api/characters) and takes level 1 of the class (POST …/levels). The remaining steps work on the saved draft
 * (/characters/new/{id}), so the creator can be left and resumed. Going back from the choices step keeps the draft
 * until it is saved with other inputs; then a new draft replaces it. The name is set in the details step; "Finish"
 * saves the details. Started over from the level up page (?replaces={id}), the new character takes over name, story
 * and portrait of the old one, which is deleted once the new one is saved.
 */
export const CreatorPage = () => {
  const { id } = useParams()
  const [searchParams] = useSearchParams()
  // the character started over from the level up page; replaced by the first save
  const replaces = id ? null : searchParams.get('replaces')
  const replaced = useIntentQuery(() => (replaces ? new GetCharacterIntent(replaces) : null), replaces ?? '')
  const navigate = useNavigate()
  const { run, busy } = useApiAction()
  const [step, setStep] = useState(id ? ChoicesStep : 0)

  // class and origin (before the draft exists)
  const [classKey, setClassKey] = useState('')
  const [classEquipment, setClassEquipment] = useState<TakeStartingEquipmentDto | null>(null)
  const classOptions = useRuleStartingEquipment('class', classKey)
  const [species, setSpecies] = useState('')
  const [background, setBackground] = useState('')
  const [backgroundEquipment, setBackgroundEquipment] = useState<TakeStartingEquipmentDto | null>(null)
  const backgroundOptions = useRuleStartingEquipment('background', background)
  const [method, setMethod] = useState<AbilityMethod>('StandardArray')
  const [rolled, setRolled] = useState<number[]>([])
  const [scores, setScores] = useState<DraftScores>(initialScores('StandardArray'))

  // the saved draft
  const draft = useIntentQuery(() => (id ? new GetCharacterIntent(id) : null), id ?? '')
  const state = draft.data

  // the inputs the draft was saved with; saving other inputs replaces the draft
  const origin = JSON.stringify({
    classKey,
    classEquipment,
    species,
    background,
    backgroundEquipment,
    method,
    scores,
  })
  const [savedOrigin, setSavedOrigin] = useState<string | null>(null)
  const [confirmReplace, setConfirmReplace] = useState(false)
  const [editedDetails, setDetails] = useState<CharacterDetails | null>(null)
  const details: CharacterDetails | null = editedDetails ?? (state
    ? {
      name: state.name === PlaceholderName ? '' : state.name,
      alignment: state.alignment,
      appearance: state.appearance,
      backstory: state.backstory,
    }
    : null)

  const scoresInvalid = Boolean(scoreProblem(method, scores, method === 'Rolled' ? rolled : StandardArray))
  const canNext = [
    Boolean(classKey) && equipmentComplete(classOptions ?? [], classEquipment),
    Boolean(species),
    Boolean(background) && equipmentComplete(backgroundOptions ?? [], backgroundEquipment),
    !scoresInvalid,
    Boolean(state && state.level > 0),
    Boolean(details?.name.trim()),
  ][step]

  // 1. the character with its origin, 2. level 1 of the class; when 2 fails, the choices step offers the class again;
  // 3. the character it replaces (a draft saved before going back, or one started over) hands over its details and is
  // deleted
  const create = async () => {
    const abilities = completeScores(scores)
    if (!abilities) {
      return
    }

    const previous = state?.id ?? replaces

    const result = await run(new CreateCharacterIntent({
      name: PlaceholderName,
      speciesKey: species,
      backgroundKey: background,
      abilityMethod: method,
      abilities,
      startingEquipment: backgroundEquipment,
    }))
    if (!result.ok) {
      return
    }

    setSavedOrigin(origin)
    navigate(`/characters/new/${result.data.id}`, { replace: true })
    draft.setData(result.data)
    setStep(ChoicesStep)
    await takeClass(result.data.id)
    if (previous && await carryOver(previous, result.data.id)) {
      await run(new DeleteCharacterIntent(previous))
    }
  }

  // name, alignment, story and portrait of a replaced character move to the new one; false keeps the old one
  const carryOver = async (fromId: string, toId: string) => {
    const from = await run(new GetCharacterIntent(fromId))
    if (!from.ok) {
      return false
    }

    const updated = await run(new UpdateCharacterIntent(toId, {
      name: from.data.name,
      alignment: from.data.alignment,
      appearance: from.data.appearance || null,
      backstory: from.data.backstory || null,
    }))
    if (!updated.ok) {
      return false
    }

    draft.setData(updated.data)
    if (!from.data.portraitUrl) {
      return true
    }

    const picture = await run(new GetCharacterPortraitIntent(fromId))
    if (!picture.ok) {
      return false
    }

    const withPortrait = await run(new SetPortraitIntent(toId, { data: await toBase64(picture.data) }))
    if (withPortrait.ok) {
      draft.setData(withPortrait.data)
    }
    return withPortrait.ok
  }

  // a saved draft is kept when nothing changed, other inputs replace it after a confirmation
  const save = () => {
    if (!state) {
      create()
    }
    else if (origin === savedOrigin) {
      setStep(ChoicesStep)
    }
    else {
      setConfirmReplace(true)
    }
  }

  const takeClass = async (characterId = id) => {
    if (!characterId) {
      return
    }

    const result = await run(
      new LevelUpIntent(characterId, {
        classKey,
        startingEquipment: classEquipment,
      }),
      { success: 'Character saved – answer the open choices' },
    )
    if (result.ok) {
      draft.setData(result.data)
    }
  }

  const finish = async () => {
    if (!state || !details) {
      return
    }

    const result = await run(new UpdateCharacterIntent(state.id, {
      name: details.name.trim(),
      alignment: details.alignment,
      appearance: details.appearance || null,
      backstory: details.backstory || null,
    }), { success: 'Character saved' })
    if (result.ok) {
      navigate(`/characters/${state.id}`)
    }
  }

  const next = () => {
    if (step === SaveStep) {
      save()
    }
    else if (step === steps.length - 1) {
      finish()
    }
    else {
      setStep(step + 1)
    }
  }

  const applyState = (next: CharacterStateDto) => draft.setData(next)

  const summary = (
    <Summary
      name={details?.name ?? ''}
      className={classKey}
      species={state?.species ?? species}
      background={state?.background ?? background}
      scores={state
        ? Object.fromEntries(state.abilities.map((ability) => [ability.key, ability.score])) as DraftScores
        : scores}
      state={state}
    />
  )

  return (
    <Box
      px={{
        base: '4',
        md: '6',
      }}
      py={{
        base: '4',
        md: '6',
      }}
    >
      {replaced.data && (
        <Alert.Root
          status='info'
          mb='4'
        >
          <Alert.Indicator />
          <Alert.Title>
            Starting over: {replaced.data.name} is replaced once the new character is saved; name, story and portrait
            carry over.
          </Alert.Title>
        </Alert.Root>
      )}
      <Steps.Root
        step={step}
        count={steps.length}
        colorPalette='brand'
        size='sm'
        mb='6'
        hideBelow='md'
      >
        <Steps.List>
          {steps.map((title, index) => (
            <Steps.Item
              key={title}
              index={index}
            >
              <Steps.Indicator />
              <Steps.Title>{title}</Steps.Title>
              <Steps.Separator />
            </Steps.Item>
          ))}
        </Steps.List>
      </Steps.Root>
      <HStack
        hideFrom='lg'
        justify='space-between'
        mb='4'
      >
        <Box hideFrom='md'>
          <Text fontWeight='bold'>{steps[step]}</Text>
          <Text
            fontSize='sm'
            color='fg.muted'
          >
            Step {step + 1} of {steps.length}
          </Text>
        </Box>
        <SummaryDrawer>{summary}</SummaryDrawer>
      </HStack>

      <Grid
        templateColumns={{
          base: '1fr',
          lg: 'minmax(0, 1fr) 300px',
        }}
        gap='6'
        alignItems='start'
      >
        <Stack gap='5'>
          {step === 0 && (
            <>
              <StepTitle
                title='Class'
                text='Hit points, saving throws and armor training. Level 1 is taken when the character is saved.'
              />
              <ClassChoice
                classKey={classKey}
                options={classOptions}
                equipment={classEquipment}
                onClass={(key) => {
                  setClassKey(key)
                  setClassEquipment(null)
                }}
                onEquipment={setClassEquipment}
              />
            </>
          )}
          {step === 1 && (
            <>
              <StepTitle
                title='Species'
                text='Size, speed and traits. Click a card to choose it, the info button shows every trait.'
              />
              <RuleCards
                kind='species'
                selected={species}
                onSelect={setSpecies}
              />
            </>
          )}
          {step === 2 && (
            <>
              <StepTitle
                title='Background'
                text='Ability score increases, two skills, a tool and an origin feat.'
              />
              <RuleCards
                kind='background'
                selected={background}
                onSelect={(key) => {
                  setBackground(key)
                  setBackgroundEquipment(null)
                }}
              />
              <EquipmentChoice
                options={background ? backgroundOptions : undefined}
                value={backgroundEquipment}
                onChange={setBackgroundEquipment}
                hint='saved with the draft (POST /api/characters with startingEquipment)'
              />
            </>
          )}
          {step === SaveStep && (
            <>
              <StepTitle
                title='Ability scores'
                text='Base scores before the background increases (next step).'
              />
              <AbilityScoresStep
                method={method}
                scores={scores}
                rolled={rolled}
                onMethod={(value) => {
                  setMethod(value)
                  setScores(initialScores(value))
                }}
                onScores={setScores}
                onRolled={(values) => {
                  setRolled(values)
                  setScores(initialScores('Rolled'))
                }}
              />
              <ApiHint>
                the API stores the scores as sent; the method rules are checked here. Save sends{' '}
                <code>POST /api/characters</code>, then <code>POST …/levels</code> with the class
              </ApiHint>
            </>
          )}
          {step === ChoicesStep && (
            <>
              <StepTitle
                title='Choices'
                text='Open choices of the origin and the class; follow-up choices appear after answering.'
              />
              {!state && <Spinner />}
              {state && state.level === 0 && (
                <>
                  {/* the level-up after the save failed, or a draft from before: take the class here */}
                  <ClassChoice
                    classKey={classKey}
                    options={classOptions}
                    equipment={classEquipment}
                    onClass={(key) => {
                      setClassKey(key)
                      setClassEquipment(null)
                    }}
                    onEquipment={setClassEquipment}
                  />
                  <Flex justify='flex-end'>
                    <Button
                      colorPalette='brand'
                      disabled={!classKey || !equipmentComplete(classOptions ?? [], classEquipment)}
                      loading={busy}
                      onClick={() => takeClass()}
                    >
                      Take level 1
                    </Button>
                  </Flex>
                </>
              )}
              {state && (
                <ChoiceList
                  characterId={state.id}
                  choices={state.pendingChoices}
                  onState={applyState}
                  empty='All choices are made.'
                />
              )}
              {state && (
                <StartingEquipmentCard
                  state={state}
                  onState={applyState}
                  sources={['Background', 'Class']}
                />
              )}
            </>
          )}
          {step === 5 && state && details && (
            <DetailsStep
              state={state}
              details={details}
              onChange={setDetails}
            />
          )}

          <Flex
            gap='3'
            justify='flex-end'
            position={{
              base: 'sticky',
              md: 'static',
            }}
            bottom='0'
            py='3'
            bg='bg'
          >
            <Button
              variant='outline'
              disabled={step === 0}
              onClick={() => setStep(step - 1)}
            >
              Back
            </Button>
            <Button
              colorPalette='brand'
              disabled={!canNext}
              loading={busy && (step === SaveStep || step === steps.length - 1)}
              onClick={next}
            >
              {step === SaveStep ? 'Save' : step === steps.length - 1 ? 'Finish' : 'Next'}
            </Button>
          </Flex>
        </Stack>

        {/* below lg the summary would end up under the steps; the drawer above shows it there */}
        <Box hideBelow='lg'>{summary}</Box>
      </Grid>
      <ReplaceDraftDialog
        open={confirmReplace}
        onClose={() => setConfirmReplace(false)}
        onReplace={() => {
          setConfirmReplace(false)
          create()
        }}
      />
    </Box>
  )
}

/**
 * Starting equipment of the selected background or class, chosen before it is saved
 */
const EquipmentChoice = ({
  options,
  value,
  onChange,
  hint,
}: {
  options: StartingEquipmentOptionDto[] | undefined
  value: TakeStartingEquipmentDto | null
  onChange: (value: TakeStartingEquipmentDto | null) => void
  hint: string
}) => {
  if (!options?.length) {
    return null
  }

  return (
    <Card.Root>
      <Card.Body gap='3'>
        <Box>
          <Heading size='md'>Starting equipment</Heading>
          <Text
            fontSize='sm'
            color='fg.muted'
          >
            Choose an alternative; the items and coins go into the inventory.
          </Text>
        </Box>
        <StartingEquipmentPicker
          options={options}
          value={value}
          onChange={onChange}
        />
        <ApiHint>{hint}</ApiHint>
      </Card.Body>
    </Card.Root>
  )
}

/**
 * Class cards with the starting equipment of the chosen class
 */
const ClassChoice = ({
  classKey,
  options,
  equipment,
  onClass,
  onEquipment,
}: {
  classKey: string
  options: StartingEquipmentOptionDto[] | undefined
  equipment: TakeStartingEquipmentDto | null
  onClass: (key: string) => void
  onEquipment: (value: TakeStartingEquipmentDto | null) => void
}) => {
  return (
    <>
      <RuleCards
        kind='class'
        selected={classKey}
        onSelect={onClass}
      />
      <EquipmentChoice
        options={classKey ? options : undefined}
        value={equipment}
        onChange={onEquipment}
        hint='saved with the level (POST …/levels with startingEquipment)'
      />
    </>
  )
}

const StepTitle = ({ title, text }: { title: string, text: string }) => {
  return (
    <Box>
      <Heading size='xl'>{title}</Heading>
      <Text color='fg.muted'>{text}</Text>
    </Box>
  )
}

/**
 * Confirmation before a draft is saved again with another class, origin or other scores
 */
const ReplaceDraftDialog = ({
  open,
  onClose,
  onReplace,
}: {
  open: boolean
  onClose: () => void
  onReplace: () => void
}) => {
  return (
    <Dialog.Root
      placement='center'
      open={open}
      onOpenChange={(details) => {
        if (!details.open) {
          onClose()
        }
      }}
    >
      <Portal>
        <Dialog.Backdrop />
        <Dialog.Positioner>
          <Dialog.Content>
            <Dialog.Header>
              <Dialog.Title>Save the changes?</Dialog.Title>
            </Dialog.Header>
            <Dialog.Body>
              <Text color='fg.muted'>
                The draft is saved again with the new class, origin and scores; the choices made so far and the starting
                equipment are lost.
              </Text>
            </Dialog.Body>
            <Dialog.Footer>
              <Dialog.ActionTrigger asChild>
                <Button variant='outline'>Cancel</Button>
              </Dialog.ActionTrigger>
              <Button
                colorPalette='brand'
                onClick={onReplace}
              >
                Save
              </Button>
            </Dialog.Footer>
          </Dialog.Content>
        </Dialog.Positioner>
      </Portal>
    </Dialog.Root>
  )
}

/**
 * The summary (scores, proficiencies, open choices) on narrow screens, opened from the step header
 */
const SummaryDrawer = ({ children }: { children: ReactNode }) => {
  return (
    <Drawer.Root
      placement='bottom'
      lazyMount
      unmountOnExit
    >
      <Drawer.Trigger asChild>
        <Button
          size='sm'
          variant='outline'
          ms='auto'
        >
          <Icon path={statsIcon} />
          Stats
        </Button>
      </Drawer.Trigger>
      <Portal>
        <Drawer.Backdrop />
        <Drawer.Positioner>
          <Drawer.Content
            maxH='85dvh'
            roundedTop='l3'
          >
            <Drawer.Body
              py='4'
              overflowY='auto'
            >
              {children}
            </Drawer.Body>
            <Drawer.Footer>
              <Drawer.ActionTrigger asChild>
                <Button variant='outline'>Close</Button>
              </Drawer.ActionTrigger>
            </Drawer.Footer>
          </Drawer.Content>
        </Drawer.Positioner>
      </Portal>
    </Drawer.Root>
  )
}

const Summary = ({
  name,
  className,
  species,
  background,
  scores,
  state,
}: {
  name: string
  /** chosen class before the save (the saved draft knows its classes) */
  className: string
  species: string
  background: string
  scores: DraftScores
  state?: CharacterStateDto
}) => {
  const open = state?.pendingChoices.filter((choice) => !choice.optional).length
  // the sheet knows the skills; it's loaded again whenever a choice is answered or taken back
  const answered = state?.pendingChoices.map((choice) => `${choice.id}:${choice.selected}`).join(',')
  const sheet = useIntentQuery(
    () => (state ? new GetCharacterSheetIntent(state.id) : null),
    state ? `${state.id}|${state.level}|${answered}` : '',
  )

  return (
    <Stack
      gap='4'
      position={{
        lg: 'sticky',
      }}
      top='20'
    >
      <Card.Root>
        <Card.Body gap='3'>
          <Box>
            <Heading size='md'>{name || 'New character'}</Heading>
            <Text
              fontSize='sm'
              color='fg.muted'
            >
              {[
                species,
                background,
                state?.classes.map((entry) => `${entry.name} ${entry.level}`).join(' / ') || className || 'draft',
              ]
                .filter(Boolean)
                .join(' · ')}
            </Text>
          </Box>
          {state && (
            <HStack>
              <Badge size='lg'>AC {state.armorClass}</Badge>
              <Badge size='lg'>HP {state.hitPointMaximum || '–'}</Badge>
              <Badge size='lg'>PB +{state.proficiencyBonus}</Badge>
            </HStack>
          )}
          <Grid
            templateColumns='repeat(3, 1fr)'
            gap='1.5'
          >
            {AbilityKeys.map((key) => (
              <Box
                key={key}
                textAlign='center'
                py='1.5'
                rounded='md'
                bg='bg.subtle'
              >
                <Text
                  fontSize='xs'
                  color='fg.muted'
                  textTransform='uppercase'
                >
                  {key}
                </Text>
                <Text fontWeight='bold'>{scores[key] ?? '–'}</Text>
                <Text fontSize='xs'>{signedModifier(scores[key])}</Text>
              </Box>
            ))}
          </Grid>
          {state && (
            <Text fontSize='sm'>
              {open ? `${open} open choice${open > 1 ? 's' : ''}` : 'All choices made'}
            </Text>
          )}
        </Card.Body>
      </Card.Root>
      {sheet.data && <SkillProficiencies sheet={sheet.data} />}
    </Stack>
  )
}

const DetailsStep = ({
  state,
  details,
  onChange,
}: {
  state: CharacterStateDto
  details: CharacterDetails
  onChange: (details: CharacterDetails) => void
}) => {
  const open = state.pendingChoices.filter((choice) => !choice.optional).length

  return (
    <Stack gap='4'>
      <StepTitle
        title='Details'
        text='Name, alignment and the story of the character. “Finish” saves them.'
      />
      <CharacterDetailsFields
        details={details}
        onChange={onChange}
      />
      <Alert.Root status={open ? 'warning' : 'success'}>
        <Alert.Indicator />
        <Alert.Title>
          {open
            ? `${open} choice${open > 1 ? 's are' : ' is'} still open – the draft is saved, finish later on the `
              + 'Level up page.'
            : 'All choices made – the character is complete.'}
        </Alert.Title>
      </Alert.Root>
    </Stack>
  )
}
