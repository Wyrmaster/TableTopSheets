import {
  Alert,
  Badge,
  Button,
  HStack,
  IconButton,
  NativeSelect,
  NumberInput,
  SegmentGroup,
  SimpleGrid,
  Stack,
  Text,
} from '@chakra-ui/react'
import { AbilityKeys } from '@/api'
import { Icon } from '@/components/Icon'
import { diceIcon, minusIcon, plusIcon } from '@/components/icons'
import {
  type AbilityMethod,
  AbilityNames,
  type DraftScores,
  PointBuyBudget,
  PointCost,
  pointsSpent,
  rollScores,
  scoreProblem,
  signedModifier,
  StandardArray,
} from './abilityScores'

/**
 * Base ability scores before the background increases: standard array, point buy, rolled or manual. Each method starts
 * without scores (point buy at 8 everywhere); a value of the array or the roll can be given to one ability only
 */
export function AbilityScoresStep({
  method,
  scores,
  rolled,
  onMethod,
  onScores,
  onRolled,
}: {
  method: AbilityMethod
  scores: DraftScores
  rolled: number[]
  onMethod: (method: AbilityMethod) => void
  onScores: (scores: DraftScores) => void
  onRolled: (values: number[]) => void
}) {
  const pool = method === 'Rolled' ? rolled : StandardArray
  const problem = scoreProblem(method, scores, pool)
  const spent = pointsSpent(scores)
  const set = (key: keyof DraftScores, value: number | null) => onScores({
    ...scores,
    [key]: value,
  })

  // a value is taken when the other abilities use all of its copies (a roll can have the same value twice)
  const taken = (key: keyof DraftScores, value: number) => AbilityKeys
    .filter((other) => other !== key && scores[other] === value)
    .length >= pool.filter((entry) => entry === value).length

  return (
    <Stack gap='4'>
      <SegmentGroup.Root
        value={method}
        onValueChange={(details) => onMethod(details.value as AbilityMethod)}
      >
        <SegmentGroup.Indicator />
        <SegmentGroup.Items
          items={[
            {
              value: 'StandardArray',
              label: 'Standard array',
            },
            {
              value: 'PointBuy',
              label: 'Point buy',
            },
            {
              value: 'Rolled',
              label: 'Roll 4d6',
            },
            {
              value: 'Manual',
              label: 'Manual',
            },
          ]}
        />
      </SegmentGroup.Root>

      {(method === 'StandardArray' || method === 'Rolled') && (
        <HStack wrap='wrap'>
          {method === 'Rolled' && (
            <Button
              size='sm'
              variant='outline'
              onClick={() => onRolled(rollScores())}
            >
              <Icon path={diceIcon} />
              Roll
            </Button>
          )}
          <Text
            fontSize='sm'
            color='fg.muted'
          >
            Values
          </Text>
          {pool.map((value, index) => <Badge key={`${value}-${index}`}>{value}</Badge>)}
          {pool.length === 0 && (
            <Text
              fontSize='sm'
              color='fg.muted'
            >
              – roll first
            </Text>
          )}
        </HStack>
      )}
      {method === 'PointBuy' && (
        <Text>
          <b>{PointBuyBudget - spent}</b> of {PointBuyBudget} points left · scores 8 – 15
        </Text>
      )}

      <SimpleGrid
        minChildWidth='200px'
        gap='3'
      >
        {AbilityKeys.map((key) => (
          <Stack
            key={key}
            gap='1.5'
            p='3'
            borderWidth='1px'
            rounded='md'
          >
            <HStack justify='space-between'>
              <Text fontWeight='medium'>{AbilityNames[key]}</Text>
              <Badge>{signedModifier(scores[key])}</Badge>
            </HStack>
            {(method === 'StandardArray' || method === 'Rolled') && (
              <NativeSelect.Root size='sm'>
                <NativeSelect.Field
                  aria-label={AbilityNames[key]}
                  value={scores[key] === null ? '' : String(scores[key])}
                  onChange={(event) => set(key, event.target.value ? Number(event.target.value) : null)}
                >
                  <option value=''>–</option>
                  {[...new Set(pool)].map((value) => <option
                    key={value}
                    value={value}
                    disabled={taken(key, value)}
                  >
                    {value}
                  </option>)}
                </NativeSelect.Field>
                <NativeSelect.Indicator />
              </NativeSelect.Root>
            )}
            {method === 'PointBuy' && (
              <PointBuyControl
                name={AbilityNames[key]}
                score={scores[key] ?? 8}
                spent={spent}
                onChange={(value) => set(key, value)}
              />
            )}
            {method === 'Manual' && (
              <NumberInput.Root
                size='sm'
                min={3}
                max={18}
                value={scores[key] === null ? '' : String(scores[key])}
                onValueChange={(details) => set(
                  key,
                  Number.isNaN(details.valueAsNumber) ? null : details.valueAsNumber,
                )}
              >
                <NumberInput.Control />
                <NumberInput.Input
                  aria-label={AbilityNames[key]}
                  placeholder='3 – 18'
                />
              </NumberInput.Root>
            )}
          </Stack>
        ))}
      </SimpleGrid>

      {problem && (
        <Alert.Root status='warning'>
          <Alert.Indicator />
          <Alert.Title>{problem}</Alert.Title>
        </Alert.Root>
      )}
    </Stack>
  )
}

/**
 * Lower / raise one score of the point buy (8 – 15, within the budget)
 */
function PointBuyControl({
  name,
  score,
  spent,
  onChange,
}: {
  name: string
  score: number
  spent: number
  onChange: (score: number) => void
}) {
  return (
    <HStack>
      <IconButton
        size='xs'
        variant='outline'
        aria-label={`Lower ${name}`}
        disabled={score <= 8}
        onClick={() => onChange(score - 1)}
      >
        <Icon path={minusIcon} />
      </IconButton>
      <Text
        minW='6'
        textAlign='center'
        fontWeight='bold'
      >
        {score}
      </Text>
      <IconButton
        size='xs'
        variant='outline'
        aria-label={`Raise ${name}`}
        disabled={score >= 15 || spent - PointCost[score] + PointCost[score + 1] > PointBuyBudget}
        onClick={() => onChange(score + 1)}
      >
        <Icon path={plusIcon} />
      </IconButton>
    </HStack>
  )
}
