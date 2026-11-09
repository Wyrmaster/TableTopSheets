import { describe, expect, it } from 'vitest'
import { isMastered, masteryStatus } from '@/components/mastery'

describe('isMastered', () => {
  const keys = ['longsword', 'scimitar']

  it('matches a mastered mundane weapon by its key', () => {
    expect(isMastered(keys, { key: 'scimitar', baseKey: null })).toBe(true)
  })

  it('matches a magic variant by its base weapon', () => {
    expect(isMastered(keys, { key: 'plus1-longsword', baseKey: 'longsword' })).toBe(true)
  })

  it('rejects weapons without mastery', () => {
    expect(isMastered(keys, { key: 'dagger', baseKey: null })).toBe(false)
    expect(isMastered(keys, { key: 'plus1-dagger', baseKey: 'dagger' })).toBe(false)
  })
})

describe('masteryStatus', () => {
  it('names the status', () => {
    expect(masteryStatus(['dagger'], { key: 'dagger', baseKey: null })).toBe('mastered')
    expect(masteryStatus([], { key: 'dagger', baseKey: null })).toBe('not-mastered')
  })
})
