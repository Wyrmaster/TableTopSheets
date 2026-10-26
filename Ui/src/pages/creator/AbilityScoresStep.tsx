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
  modifier,
  PointBuyBudget,
  PointCost,
  pointsSpent,
  rollScores,
  type Scores,
  scoreProblem,
  StandardArray,
} from './abilityScores'

const signed = (value: number) => (value >= 0 ? `+${value}` : `${value}`)

/**
 * Base ability scores before the background increases: standard array, point buy, rolled or manual
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
  scores: Scores
  rolled: number[]
  onMethod: (method: AbilityMethod) => void
  onScores: (scores: Scores) => void
  onRolled: (values: number[]) => void
}) {
  const pool = method === 'Rolled' ? rolled : StandardArray
  const problem = scoreProblem(method, scores, pool)
  const spent = pointsSpent(scores)
  const set = (key: keyof Scores, value: number) => onScores({
    ...scores,
    [key]: value,
  })

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
              <Badge>{signed(modifier(scores[key]))}</Badge>
            </HStack>
            {(method === 'StandardArray' || method === 'Rolled') && (
              <NativeSelect.Root size='sm'>
                <NativeSelect.Field
                  value={String(scores[key])}
                  onChange={(event) => set(key, Number(event.target.value))}
                >
                  {[...new Set(pool)].map((value) => <option
                    key={value}
                    value={value}
                  >
                    {value}
                  </option>)}
                </NativeSelect.Field>
                <NativeSelect.Indicator />
              </NativeSelect.Root>
            )}
            {method === 'PointBuy' && (
              <HStack>
                <IconButton
                  size='xs'
                  variant='outline'
                  aria-label={`Lower ${AbilityNames[key]}`}
                  disabled={scores[key] <= 8}
                  onClick={() => set(key, scores[key] - 1)}
                >
                  <Icon path={minusIcon} />
                </IconButton>
                <Text
                  minW='6'
                  textAlign='center'
                  fontWeight='bold'
                >
                  {scores[key]}
                </Text>
                <IconButton
                  size='xs'
                  variant='outline'
                  aria-label={`Raise ${AbilityNames[key]}`}
                  disabled={scores[key] >= 15
                    || spent - PointCost[scores[key]] + PointCost[scores[key] + 1] > PointBuyBudget}
                  onClick={() => set(key, scores[key] + 1)}
                >
                  <Icon path={plusIcon} />
                </IconButton>
              </HStack>
            )}
            {method === 'Manual' && (
              <NumberInput.Root
                size='sm'
                min={3}
                max={18}
                value={String(scores[key])}
                onValueChange={(details) => set(key, details.valueAsNumber || 3)}
              >
                <NumberInput.Control />
                <NumberInput.Input />
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
