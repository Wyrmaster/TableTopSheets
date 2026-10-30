import {
  Alert,
  Badge,
  Box,
  Button,
  Card,
  Field,
  Flex,
  Grid,
  Heading,
  HStack,
  Input,
  NativeSelect,
  Spinner,
  Stack,
  Steps,
  Text,
  Textarea,
} from '@chakra-ui/react'
import { useState } from 'react'
import { useNavigate, useParams } from 'react-router'
import {
  AbilityKeys,
  Alignments,
  type AlignmentEnum,
  type CharacterStateDto,
  CreateCharacterIntent,
  GetCharacterIntent,
  GetCharacterSheetIntent,
  LevelUpIntent,
  type StartingEquipmentOptionDto,
  type TakeStartingEquipmentDto,
  UpdateCharacterIntent,
} from '@/api'
import { ApiHint } from '@/components/ApiHint'
import { ChoiceList } from '@/components/choices/ChoiceList'
import { SkillProficiencies } from '@/components/SkillProficiencies'
import { equipmentComplete } from '@/components/startingEquipment/startingEquipment'
import { StartingEquipmentCard } from '@/components/startingEquipment/StartingEquipmentCard'
import { StartingEquipmentPicker } from '@/components/startingEquipment/StartingEquipmentPicker'
import { useRuleStartingEquipment } from '@/components/startingEquipment/useRuleStartingEquipment'
import { useApiAction } from '@/hooks/useApiAction'
import { useIntentQuery } from '@/hooks/useIntentQuery'
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

/** The ability scores step saves the draft; from the choices step on the creator works on the saved draft */
const SaveStep = 3
const ChoicesStep = 4

const alignmentName = (alignment: string) => alignment.replace(/([a-z])([A-Z])/g, '$1 $2')

/**
 * Name of the character until it is set in the details step; the API needs a name to save the character
 */
const PlaceholderName = 'New character'

interface Details {
  name: string
  alignment: AlignmentEnum
  appearance: string
  backstory: string
}

/**
 * Character creator. Class, species, background and ability scores are chosen locally; "Save" stores the character
 * (POST /api/characters) and takes level 1 of the class (POST …/levels). The remaining steps work on the saved draft
 * (/characters/new/{id}), so the creator can be left and resumed. The name is set in the details step; "Finish" saves
 * the details.
 */
export function CreatorPage() {
  const { id } = useParams()
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
  const [editedDetails, setDetails] = useState<Details | null>(null)
  const details: Details | null = editedDetails ?? (state
    ? {
      name: state.name === PlaceholderName ? '' : state.name,
      alignment: state.alignment,
      appearance: '',
      backstory: '',
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

  // 1. the character with its origin, 2. level 1 of the class; when 2 fails, the choices step offers the class again
  const create = async () => {
    const abilities = completeScores(scores)
    if (!abilities) {
      return
    }

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

    navigate(`/characters/new/${result.data.id}`, { replace: true })
    draft.setData(result.data)
    setStep(ChoicesStep)
    await takeClass(result.data.id)
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
      create()
    }
    else if (step === steps.length - 1) {
      finish()
    }
    else {
      setStep(step + 1)
    }
  }

  const applyState = (next: CharacterStateDto) => draft.setData(next)

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
        hideFrom='md'
        justify='space-between'
        mb='4'
      >
        <Text fontWeight='bold'>{steps[step]}</Text>
        <Text
          fontSize='sm'
          color='fg.muted'
        >
          Step {step + 1} of {steps.length}
        </Text>
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
              disabled={step === 0 || step === ChoicesStep}
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
      </Grid>
    </Box>
  )
}

/**
 * Starting equipment of the selected background or class, chosen before it is saved
 */
function EquipmentChoice({
  options,
  value,
  onChange,
  hint,
}: {
  options: StartingEquipmentOptionDto[] | undefined
  value: TakeStartingEquipmentDto | null
  onChange: (value: TakeStartingEquipmentDto | null) => void
  hint: string
}) {
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
function ClassChoice({
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
}) {
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

function StepTitle({ title, text }: { title: string, text: string }) {
  return (
    <Box>
      <Heading size='xl'>{title}</Heading>
      <Text color='fg.muted'>{text}</Text>
    </Box>
  )
}

function Summary({
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
}) {
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

function DetailsStep({
  state,
  details,
  onChange,
}: {
  state: CharacterStateDto
  details: Details
  onChange: (details: Details) => void
}) {
  const open = state.pendingChoices.filter((choice) => !choice.optional).length
  const set = (change: Partial<Details>) => onChange({ ...details, ...change })

  return (
    <Stack gap='4'>
      <StepTitle
        title='Details'
        text='Name, alignment and the story of the character. “Finish” saves them.'
      />
      <Grid
        templateColumns={{
          base: '1fr',
          md: '1fr 1fr',
        }}
        gap='4'
      >
        <Field.Root>
          <Field.Label>Name</Field.Label>
          <Input
            value={details.name}
            placeholder='Character name'
            onChange={(event) => set({ name: event.target.value })}
          />
        </Field.Root>
        <Field.Root>
          <Field.Label>Alignment</Field.Label>
          <NativeSelect.Root>
            <NativeSelect.Field
              value={details.alignment}
              onChange={(event) => set({ alignment: event.target.value as AlignmentEnum })}
            >
              {Alignments.map((value) => <option
                key={value}
                value={value}
              >
                {alignmentName(value)}
              </option>)}
            </NativeSelect.Field>
            <NativeSelect.Indicator />
          </NativeSelect.Root>
        </Field.Root>
      </Grid>
      <Field.Root>
        <Field.Label>Appearance</Field.Label>
        <Textarea
          value={details.appearance}
          onChange={(event) => set({ appearance: event.target.value })}
        />
      </Field.Root>
      <Field.Root>
        <Field.Label>Backstory</Field.Label>
        <Textarea
          value={details.backstory}
          onChange={(event) => set({ backstory: event.target.value })}
        />
      </Field.Root>
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
