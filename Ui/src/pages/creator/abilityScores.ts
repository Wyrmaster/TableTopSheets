import type { AbilityKey } from '@/api'

export type AbilityMethod = 'StandardArray' | 'PointBuy' | 'Rolled' | 'Manual'

export type Scores = Record<AbilityKey, number>

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

export const pointsSpent = (scores: Scores) => Object.values(scores)
  .reduce((sum, score) => sum + (PointCost[score] ?? 99), 0)

/**
 * 4d6, drop the lowest die, six times (highest first)
 */
export function rollScores(): number[] {
  return Array.from({ length: 6 }, () => {
    const dice = Array.from({ length: 4 }, () => 1 + Math.floor(Math.random() * 6)).sort((a, b) => b - a)
    return dice[0] + dice[1] + dice[2]
  }).sort((a, b) => b - a)
}

/**
 * Problem of the scores for the method, or null when they are valid (the API doesn't check the method)
 */
export function scoreProblem(method: AbilityMethod, scores: Scores, pool: number[]): string | null {
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
