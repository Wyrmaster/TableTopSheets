import { type AbilityKey, AbilityKeys } from '@/api'

export type AbilityMethod = 'StandardArray' | 'PointBuy' | 'Rolled' | 'Manual'

export type Scores = Record<AbilityKey, number>

/** Scores while they are chosen: null is an ability without a score yet */
export type DraftScores = Record<AbilityKey, number | null>

export const StandardArray = [15, 14, 13, 12, 10, 8]

export const PointBuyBudget = 27

/** Point buy cost per score (8 – 15) */
export const PointCost: Record<number, number> = {
  8: 0,
  9: 1,
  10: 2,
  11: 3,
  12: 4,
  13: 5,
  14: 7,
  15: 9,
}

export const AbilityNames: Record<AbilityKey, string> = {
  str: 'Strength',
  dex: 'Dexterity',
  con: 'Constitution',
  int: 'Intelligence',
  wis: 'Wisdom',
  cha: 'Charisma',
}

export const modifier = (score: number) => Math.floor((score - 10) / 2)

/** Modifier as text (`+2`, `-1`); `–` without a score */
export const signedModifier = (score: number | null) => {
  if (score === null) {
    return '–'
  }

  const value = modifier(score)
  return value >= 0 ? `+${value}` : `${value}`
}

/**
 * Scores at the start of a method: point buy starts every ability at 8, the other methods leave them unassigned
 */
export const initialScores = (method: AbilityMethod): DraftScores => {
  const score = method === 'PointBuy' ? 8 : null
  return Object.fromEntries(AbilityKeys.map((key) => [key, score])) as DraftScores
}

/** The scores when every ability has one, otherwise null */
export const completeScores = (scores: DraftScores): Scores | null => {
  return Object.values(scores).every((score) => score !== null) ? scores as Scores : null
}

export const pointsSpent = (scores: DraftScores) => Object.values(scores)
  .reduce((sum: number, score) => sum + (score === null ? 0 : PointCost[score] ?? 99), 0)

/**
 * 4d6, drop the lowest die, six times (highest first)
 */
export const rollScores = (): number[] => {
  return Array.from({ length: 6 }, () => {
    const dice = Array.from({ length: 4 }, () => 1 + Math.floor(Math.random() * 6)).sort((a, b) => b - a)
    return dice[0] + dice[1] + dice[2]
  }).sort((a, b) => b - a)
}

/**
 * Problem of the scores for the method, or null when they are valid (the API doesn't check the method)
 */
export const scoreProblem = (method: AbilityMethod, draft: DraftScores, pool: number[]): string | null => {
  const scores = completeScores(draft)
  if (!scores) {
    return 'Assign a score to every ability.'
  }

  const values = Object.values(scores)
  if (method === 'StandardArray' || method === 'Rolled') {
    const sorted = [...values].sort((a, b) => b - a).join(',')
    return sorted === [...pool].sort((a, b) => b - a).join(',') ? null : 'Use every value exactly once.'
  }

  if (method === 'PointBuy') {
    if (values.some((score) => score < 8 || score > 15)) {
      return 'Point buy scores are between 8 and 15.'
    }

    return pointsSpent(scores) > PointBuyBudget ? `More than ${PointBuyBudget} points spent.` : null
  }

  return values.some((score) => score < 3 || score > 18) ? 'Scores are between 3 and 18.' : null
}
