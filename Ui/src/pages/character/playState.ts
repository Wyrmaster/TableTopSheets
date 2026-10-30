import { useCallback, useState } from 'react'
import type { CharacterSheet } from '@/api'
import type { PlayState } from './CharacterContext'

const emptyPlayState: PlayState = {
  temporaryHitPoints: 0,
  hitDiceUsed: {},
  resourcesUsed: {},
  slotsUsed: {},
}

const storageKey = (id: string) => `tabletopsheets.play.${id}`

const readPlayState = (id: string): PlayState => {
  try {
    return {
      ...emptyPlayState,
      ...JSON.parse(localStorage.getItem(storageKey(id)) ?? '{}'),
    }
  }
  catch {
    return emptyPlayState
  }
}

/**
 * Play state of a character in local storage (until the API can store it)
 */
export const usePlayState = (id: string) => {
  const [play, setPlay] = useState<PlayState>(() => readPlayState(id))

  const updatePlay = useCallback((update: (play: PlayState) => PlayState) => {
    setPlay((previous) => {
      const next = update(previous)
      try {
        localStorage.setItem(storageKey(id), JSON.stringify(next))
      }
      catch {
        // storage blocked: the play state only lasts for this page
      }

      return next
    })
  }, [id])

  return {
    play,
    updatePlay,
  }
}

/** Key of a group of spell slots (`1:LongRest`) */
export const slotKey = (level: number, period: string) => `${level}:${period}`

/**
 * Spell slots of the sheet grouped by level and refresh period
 */
export const slotGroups = (sheet: CharacterSheet) => {
  const groups = new Map<string, {
    level: number,
    period: string,
    count: number
  }>()
  for (const slot of sheet.spellSlots) {
    const key = slotKey(slot.level, slot.refreshPeriod)
    const group = groups.get(key) ?? {
      level: slot.level,
      period: slot.refreshPeriod,
      count: 0,
    }
    group.count += 1
    groups.set(key, group)
  }

  return [...groups.entries()]
    .map(([key, group]) => ({
      key,
      ...group,
    }))
    .sort((a, b) => a.level - b.level)
}

/**
 * Short rest: resources that recharge on a short rest come back, spent hit dice heal
 */
export const shortRest = (
  play: PlayState,
  sheet: CharacterSheet,
  healed: number,
  diceSpent: Record<string, number>,
) => {
  const resourcesUsed = { ...play.resourcesUsed }
  for (const resource of sheet.resources) {
    if (resource.recharge === 'ShortRest') {
      resourcesUsed[resource.key] = 0
    }
  }

  const slotsUsed = { ...play.slotsUsed }
  for (const group of slotGroups(sheet)) {
    if (group.period === 'ShortRest') {
      slotsUsed[group.key] = 0
    }
  }

  const hitDiceUsed = { ...play.hitDiceUsed }
  for (const [die, spent] of Object.entries(diceSpent)) {
    hitDiceUsed[die] = (hitDiceUsed[die] ?? 0) + spent
  }

  const current = play.hitPoints ?? sheet.hitPointMaximum
  return {
    ...play,
    hitPoints: Math.min(sheet.hitPointMaximum, current + healed),
    resourcesUsed,
    slotsUsed,
    hitDiceUsed,
  }
}

/**
 * Long rest: everything comes back (hit points, all hit dice, resources and spell slots)
 */
export const longRest = (): PlayState => {
  return emptyPlayState
}
