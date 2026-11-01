import { describe, expect, it } from 'vitest'
import { sortParam } from '@/components/sort'

describe('sortParam', () => {
  it('keeps an ascending sort as it is', () => {
    expect(sortParam('level,name', false)).toBe('level,name')
  })

  it('turns only the first field descending', () => {
    expect(sortParam('level,name', true)).toBe('-level,name')
    expect(sortParam('name', true)).toBe('-name')
  })
})
