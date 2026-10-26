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
import { type AbilityMethod, modifier, type Scores, scoreProblem, StandardArray } from './abilityScores'
import { AbilityScoresStep } from './AbilityScoresStep'
import { RuleCards } from './RuleCards'

const steps = ['Species', 'Background', 'Ability scores', 'Origin choices', 'Class', 'Details']

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
 * Character creator. Steps 1–3 are local; "Save" stores the character (POST /api/characters) and the remaining
 * steps work on the saved draft (/characters/new/{id}), so the creator can be left and resumed. The name is set in the
 * details step; "Finish" saves the details.
 */
export function CreatorPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { run, busy } = useApiAction()
  const [step, setStep] = useState(id ? 3 : 0)

  // origin (before the draft exists)
  const [species, setSpecies] = useState('')
  const [background, setBackground] = useState('')
  const [backgroundEquipment, setBackgroundEquipment] = useState<TakeStartingEquipmentDto | null>(null)
  const backgroundOptions = useRuleStartingEquipment('background', background)
  const [method, setMethod] = useState<AbilityMethod>('StandardArray')
  const [rolled, setRolled] = useState<number[]>(StandardArray)
  const [scores, setScores] = useState<Scores>({
    str: 8,
    dex: 14,
    con: 13,
    int: 15,
    wis: 12,
    cha: 10,
  })

  // the saved draft
  const draft = useIntentQuery(() => (id ? new GetCharacterIntent(id) : null), id ?? '')
  const state = draft.data
  const [classKey, setClassKey] = useState('')
  const [classEquipment, setClassEquipment] = useState<TakeStartingEquipmentDto | null>(null)
  const classOptions = useRuleStartingEquipment('class', classKey)
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
    Boolean(species),
    Boolean(background) && equipmentComplete(backgroundOptions ?? [], backgroundEquipment),
    !scoresInvalid,
    true,
    Boolean(state && state.level > 0),
    Boolean(details?.name.trim()),
  ][step]

  const create = async () => {
    const result = await run(new CreateCharacterIntent({
      name: PlaceholderName,
      speciesKey: species,
      backgroundKey: background,
      abilityMethod: method,
      abilities: scores,
      startingEquipment: backgroundEquipment,
    }), { success: 'Character saved – origin choices are open' })
    if (result.ok) {
      navigate(`/characters/new/${result.data.id}`, { replace: true })
      draft.setData(result.data)
      setStep(3)
    }
  }

  const takeClass = async () => {
    if (!id) {
      return
    }

    const result = await run(
      new LevelUpIntent(id, {
        classKey,
        startingEquipment: classEquipment,
      }),
      { success: 'Level 1 added – answer its choices below' },
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
    if (step === 2) {
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
          {step === 1 && (
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
          {step === 2 && (
            <>
              <StepTitle
                title='Ability scores'
                text='Base scores before the background increases (next step).'
              />
              <AbilityScoresStep
                method={method}
                scores={scores}
                rolled={rolled}
                onMethod={setMethod}
                onScores={setScores}
                onRolled={(values) => {
                  setRolled(values)
                  setScores(Object.fromEntries(AbilityKeys.map((key, index) => [key, values[index]])) as Scores)
                }}
              />
              <ApiHint>the API stores the scores as sent; the method rules are checked here</ApiHint>
            </>
          )}
          {step === 3 && (
            <>
              <StepTitle
                title='Origin choices'
                text='Every box comes from a pending choice of the API; follow-up choices appear after saving.'
              />
              {!state && <Spinner />}
              {state && (
                <ChoiceList
                  characterId={state.id}
                  choices={state.pendingChoices}
                  onState={applyState}
                  empty='All origin choices are made.'
                />
              )}
              {state && (
                <StartingEquipmentCard
                  state={state}
                  onState={applyState}
                  sources={['Background']}
                />
              )}
            </>
          )}
          {step === 4 && state && (
            <>
              <StepTitle
                title='Class (level 1)'
                text='The first class decides the saving throws and full armor training.'
              />
              {state.level === 0 && (
                <>
                  <RuleCards
                    kind='class'
                    selected={classKey}
                    onSelect={(key) => {
                      setClassKey(key)
                      setClassEquipment(null)
                    }}
                  />
                  <EquipmentChoice
                    options={classKey ? classOptions : undefined}
                    value={classEquipment}
                    onChange={setClassEquipment}
                    hint='saved with the level (POST …/levels with startingEquipment)'
                  />
                  <Flex justify='flex-end'>
                    <Button
                      colorPalette='brand'
                      disabled={!classKey || !equipmentComplete(classOptions ?? [], classEquipment)}
                      loading={busy}
                      onClick={takeClass}
                    >
                      Take level 1
                    </Button>
                  </Flex>
                </>
              )}
              {state.level > 0 && (
                <>
                  <ChoiceList
                    characterId={state.id}
                    choices={state.pendingChoices}
                    onState={applyState}
                    empty='All class choices are made.'
                  />
                  <StartingEquipmentCard
                    state={state}
                    onState={applyState}
                    sources={['Class']}
                  />
                </>
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
              disabled={step === 0 || step === 3}
              onClick={() => setStep(step - 1)}
            >
              Back
            </Button>
            <Button
              colorPalette='brand'
              disabled={!canNext}
              loading={busy && (step === 2 || step === steps.length - 1)}
              onClick={next}
            >
              {step === 2 ? 'Save' : step === steps.length - 1 ? 'Finish' : 'Next'}
            </Button>
          </Flex>
        </Stack>

        <Summary
          name={details?.name ?? ''}
          species={state?.species ?? species}
          background={state?.background ?? background}
          scores={state
            ? Object.fromEntries(state.abilities.map((ability) => [ability.key, ability.score])) as Scores
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
  species,
  background,
  scores,
  state,
}: {
  name: string
  species: string
  background: string
  scores: Scores
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
                state?.classes.map((entry) => `${entry.name} ${entry.level}`).join(' / ') || 'draft',
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
                <Text fontWeight='bold'>{scores[key]}</Text>
                <Text fontSize='xs'>{modifier(scores[key]) >= 0 ? '+' : ''}{modifier(scores[key])}</Text>
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
