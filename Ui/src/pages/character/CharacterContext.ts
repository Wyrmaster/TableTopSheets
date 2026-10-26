import { createContext } from 'react'
import type { CharacterSheet, CharacterStateDto } from '@/api'

/**
 * Play state the API doesn't store yet (kept per character in the browser): current HP, spent hit dice, resource
 * charges and spell slots
 */
export interface PlayState {
  /** current hit points; undefined means full */
  hitPoints?: number
  temporaryHitPoints: number
  /** spent hit dice per die (`D6` …) */
  hitDiceUsed: Record<string, number>
  /** spent charges per resource key */
  resourcesUsed: Record<string, number>
  /** spent slots per `level:period` */
  slotsUsed: Record<string, number>
}

export interface CharacterContextValue {
  id: string
  state: CharacterStateDto
  sheet: CharacterSheet
  /** take the state a write answered with and reload the sheet */
  applyState: (state: CharacterStateDto) => void
  play: PlayState
  updatePlay: (update: (play: PlayState) => PlayState) => void
}

export const CharacterContext = createContext<CharacterContextValue | null>(null)
