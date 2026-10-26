import { describe, expect, it } from 'vitest'
import { HttpClient } from '../core/HttpClient'
import { RegisterIntent } from './auth'
import { CreateCharacterIntent, ListCharactersIntent } from './characters'
import { GetChoiceOptionsIntent, RemoveSelectionIntent, SelectOptionsIntent } from './choices'
import { EquipItemIntent, ToggleFeatureIntent } from './inventory'
import { LevelUpIntent, PreviewLevelUpIntent, UndoLevelIntent } from './levels'
import { GetClassIntent, GetSpellIntent } from './rules'
import { SetPreparedSpellsIntent } from './spells'

const http = new HttpClient()

describe('intents', () => {
  it.each([
    [new RegisterIntent({ userName: 'a', password: 'b' }), 'POST', '/api/auth/register', true],
    [new GetClassIntent('wizard'), 'GET', '/api/rules/classes/wizard', true],
    [new GetSpellIntent('fire-bolt'), 'GET', '/api/rules/spells/fire-bolt', true],
    [
      new ListCharactersIntent({ class: 'wizard', minLevel: 3 }),
      'GET',
      '/api/characters?class=wizard&minLevel=3',
      false,
    ],
    [new CreateCharacterIntent({
      name: 'E',
      speciesKey: 'elf',
      backgroundKey: 'sage',
      abilities: {
        str: 8,
        dex: 14,
        con: 13,
        int: 15,
        wis: 12,
        cha: 10,
      },
    }), 'POST', '/api/characters', false],
    [new SelectOptionsIntent('c1', { selections: [] }), 'POST', '/api/characters/c1/choices', false],
    [new RemoveSelectionIntent('c1', 's1'), 'DELETE', '/api/characters/c1/selections/s1', false],
    [new PreviewLevelUpIntent('c1', 'wizard'), 'GET', '/api/characters/c1/level-up/preview?class=wizard', false],
    [
      new PreviewLevelUpIntent('c1', 'wizard', 'evoker'),
      'GET',
      '/api/characters/c1/level-up/preview?class=wizard&subclass=evoker',
      false,
    ],
    [new LevelUpIntent('c1', { classKey: 'wizard' }), 'POST', '/api/characters/c1/levels', false],
    [new UndoLevelIntent('c1'), 'DELETE', '/api/characters/c1/levels/last', false],
    [new EquipItemIntent('c1', 'i1', { slot: 'Armor' }), 'PUT', '/api/characters/c1/items/i1/slot', false],
    [
      new ToggleFeatureIntent('c1', 'barbarian.rage', { active: true }),
      'PUT',
      '/api/characters/c1/toggles/barbarian.rage',
      false,
    ],
    [
      new SetPreparedSpellsIntent('c1', 'wizard', { spellKeys: [] }),
      'PUT',
      '/api/characters/c1/prepared-spells/wizard',
      false,
    ],
  ] as const)('%o', (intent, method, url, anonymous) => {
    expect(intent.method).toBe(method)
    expect(http.url(intent)).toBe(url)
    expect(intent.anonymous).toBe(anonymous)
  })

  it('parses the optionsUrl of a pending choice', () => {
    const intent = GetChoiceOptionsIntent.fromUrl(
      '/api/characters/0c1/choices/magic-initiate.wizard.spell/options?parentRef=p1&preview=wizard%3Aevoker',
      { level: 1, includeDisabled: true },
    )

    expect(intent.characterId).toBe('0c1')
    expect(intent.choiceKey).toBe('magic-initiate.wizard.spell')
    expect(intent.query).toEqual({
      parentRef: 'p1',
      preview: 'wizard:evoker',
      level: 1,
      includeDisabled: true,
    })
    expect(http.url(intent)).toBe(
      '/api/characters/0c1/choices/magic-initiate.wizard.spell/options?parentRef=p1&preview=wizard%3Aevoker&level=1&includeDisabled=true',
    )
  })

  it('rejects urls of other routes', () => {
    expect(() => GetChoiceOptionsIntent.fromUrl('/api/characters/c1/choices')).toThrow()
  })
})
