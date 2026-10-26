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
  LevelUpIntent,
  UpdateCharacterIntent,
} from '@/api'
import { ApiHint } from '@/components/ApiHint'
import { ChoiceList } from '@/components/choices/ChoiceList'
import { useApiAction } from '@/hooks/useApiAction'
import { useIntentQuery } from '@/hooks/useIntentQuery'
import { type AbilityMethod, modifier, type Scores, scoreProblem, StandardArray } from './abilityScores'
import { AbilityScoresStep } from './AbilityScoresStep'
import { RuleCards } from './RuleCards'

const steps = ['Species', 'Background', 'Ability scores', 'Origin choices', 'Class', 'Details']

const alignmentName = (alignment: string) => alignment.replace(/([a-z])([A-Z])/g, '$1 $2')

/**
 * Character creator. Steps 1–3 are local; "Create draft" saves the character (POST /api/characters) and the remaining
 * steps work on the saved draft (/characters/new/{id}), so the creator can be left and resumed.
 */
export function CreatorPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { run, busy } = useApiAction()
  const [step, setStep] = useState(id ? 3 : 0)

  // origin (before the draft exists)
  const [species, setSpecies] = useState('')
  const [background, setBackground] = useState('')
  const [name, setName] = useState('')
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

  const scoresInvalid = Boolean(scoreProblem(method, scores, method === 'Rolled' ? rolled : StandardArray))
  const canNext = [
    Boolean(species),
    Boolean(background),
    Boolean(name.trim()) && !scoresInvalid,
    true,
    Boolean(state && state.level > 0),
    true,
  ][step]

  const create = async () => {
    const result = await run(new CreateCharacterIntent({
      name: name.trim(),
      speciesKey: species,
      backgroundKey: background,
      abilityMethod: method,
      abilities: scores,
    }), { success: 'Draft saved – origin choices are open' })
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
      new LevelUpIntent(id, { classKey }),
      { success: 'Level 1 added – answer its choices below' },
    )
    if (result.ok) {
      draft.setData(result.data)
    }
  }

  const next = () => {
    if (step === 2) {
      create()
    }
    else if (step === steps.length - 1) {
      navigate(`/characters/${id}`)
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
                onSelect={setBackground}
              />
            </>
          )}
          {step === 2 && (
            <>
              <StepTitle
                title='Name and ability scores'
                text='Base scores before the background increases (next step). “Create draft” saves the character.'
              />
              <Field.Root maxW='md'>
                <Field.Label>Name</Field.Label>
                <Input
                  value={name}
                  onChange={(event) => setName(event.target.value)}
                />
              </Field.Root>
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
                    onSelect={setClassKey}
                  />
                  <Flex justify='flex-end'>
                    <Button
                      colorPalette='brand'
                      disabled={!classKey}
                      loading={busy}
                      onClick={takeClass}
                    >
                      Take level 1
                    </Button>
                  </Flex>
                </>
              )}
              {state.level > 0 && (
                <ChoiceList
                  characterId={state.id}
                  choices={state.pendingChoices}
                  onState={applyState}
                  empty='All class choices are made.'
                />
              )}
            </>
          )}
          {step === 5 && state && (
            <DetailsStep
              state={state}
              onState={applyState}
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
              loading={busy && step === 2}
              onClick={next}
            >
              {step === 2 ? 'Create draft' : step === steps.length - 1 ? 'Finish' : 'Next'}
            </Button>
          </Flex>
        </Stack>

        <Summary
          name={state?.name ?? name}
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
  return (
    <Card.Root
      position={{
        lg: 'sticky',
      }}
      top='20'
    >
      <Card.Body gap='3'>
        <Box>
          <Heading size='md'>{name || 'New character'}</Heading>
          <Text
            fontSize='sm'
            color='fg.muted'
          >
            {[species, background, state?.classes.map((entry) => `${entry.name} ${entry.level}`).join(' / ') || 'draft']
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
  )
}

function DetailsStep({ state, onState }: { state: CharacterStateDto, onState: (state: CharacterStateDto) => void }) {
  const { run, busy } = useApiAction()
  const [name, setName] = useState(state.name)
  const [alignment, setAlignment] = useState<AlignmentEnum>(state.alignment)
  const [appearance, setAppearance] = useState('')
  const [backstory, setBackstory] = useState('')
  const open = state.pendingChoices.filter((choice) => !choice.optional).length

  const save = async () => {
    const result = await run(new UpdateCharacterIntent(state.id, {
      name,
      alignment,
      appearance: appearance || null,
      backstory: backstory || null,
    }), { success: 'Details saved' })
    if (result.ok) {
      onState(result.data)
    }
  }

  return (
    <Stack gap='4'>
      <StepTitle
        title='Details'
        text='Name, alignment and the story of the character.'
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
            value={name}
            onChange={(event) => setName(event.target.value)}
          />
        </Field.Root>
        <Field.Root>
          <Field.Label>Alignment</Field.Label>
          <NativeSelect.Root>
            <NativeSelect.Field
              value={alignment}
              onChange={(event) => setAlignment(event.target.value as AlignmentEnum)}
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
          value={appearance}
          onChange={(event) => setAppearance(event.target.value)}
        />
      </Field.Root>
      <Field.Root>
        <Field.Label>Backstory</Field.Label>
        <Textarea
          value={backstory}
          onChange={(event) => setBackstory(event.target.value)}
        />
      </Field.Root>
      <Flex justify='flex-end'>
        <Button
          variant='outline'
          loading={busy}
          onClick={save}
        >
          Save details
        </Button>
      </Flex>
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
