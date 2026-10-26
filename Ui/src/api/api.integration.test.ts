// End to end check of the intents and the hand-written DTOs against a running API:
//   API_URL=http://localhost:5180 pnpm test api.integration
// Skipped without API_URL. Every DTO is compared field by field with the JSON the API answers with.
import { describe, expect, it } from 'vitest'
import type { ApiResult } from './core/ApiResult'
import { HttpClient } from './core/HttpClient'
import type { Intent } from './core/Intent'
import type {
  CharacterItemDto,
  CharacterStateDto,
  LevelUpPreviewDto,
  SelectionDto,
  SelectionInfoDto,
  ClassLevelDto,
  AbilityDto,
  CharacterSummaryDto,
} from './dtos/character'
import type { OptionDto, PendingChoiceDto, ChoiceConstraintsDto, OptionMetaDto } from './dtos/choice'
import type { PagedResult } from './dtos/paging'
import type {
  ClassDetailDto,
  ClassSummaryDto,
  FeatSummaryDto,
  ItemDetailDto,
  ItemSummaryDto,
  RuleDetailDto,
  SpeciesSummaryDto,
  SpellDetailDto,
  SpellSummaryDto,
  BackgroundSummaryDto,
  FeatureDto,
  ChoiceDefinitionDto,
} from './dtos/rules'
import type { CoinsDto } from './dtos/inventory'
import type { AbilitySheet, CharacterSheet, Coins, SkillSheet } from './dtos/sheet'
import type { CharacterSpellsDto, KnownSpellDto, SpellcastingClassDto } from './dtos/spells'
import type { CurrentUserDto, TokenDto, UserDto } from './dtos/user'
import { GetCurrentUserIntent, RegisterIntent } from './intents/auth'
import {
  CreateCharacterIntent,
  DeleteCharacterIntent,
  GetCharacterSheetIntent,
  GetCharacterSheetPdfIntent,
  ListCharactersIntent,
} from './intents/characters'
import { GetChoiceOptionsIntent, GetSelectionsIntent, SelectOptionsIntent } from './intents/choices'
import {
  AddItemIntent,
  ChangeCoinsIntent,
  EquipItemIntent,
  PayCoinsIntent,
  RemoveItemIntent,
  SetCoinsIntent,
} from './intents/inventory'
import { LevelUpIntent, PreviewLevelUpIntent, UndoLevelIntent } from './intents/levels'
import { GetSpellsIntent } from './intents/spells'
import {
  GetClassIntent,
  GetItemIntent,
  GetSpeciesIntent,
  GetSpellIntent,
  ListBackgroundsIntent,
  ListClassesIntent,
  ListFeatsIntent,
  ListItemsIntent,
  ListSpeciesIntent,
  ListSpellsIntent,
} from './intents/rules'

const apiUrl = (globalThis as { process?: { env: Record<string, string | undefined> } }).process?.env.API_URL

/** Every property name of T; the compiler rejects missing and unknown names */
function keys<T>(record: Record<keyof T, true>): string[] {
  return Object.keys(record).sort()
}

function expectShape(value: unknown, expected: string[]) {
  expect(Object.keys(value as object).sort()).toEqual(expected)
}

const Shapes = {
  token: keys<TokenDto>({
    accessToken: true,
    tokenType: true,
    expiresAt: true,
    user: true,
  }),
  user: keys<UserDto>({
    id: true,
    userName: true,
    email: true,
  }),
  currentUser: keys<CurrentUserDto>({
    id: true,
    userName: true,
    email: true,
    characterCount: true,
  }),
  paged: keys<PagedResult<unknown>>({
    items: true,
    page: true,
    pageSize: true,
    totalCount: true,
    totalPages: true,
  }),
  species: keys<SpeciesSummaryDto>({
    key: true,
    name: true,
    creatureType: true,
  }),
  background: keys<BackgroundSummaryDto>({
    key: true,
    name: true,
    abilityOptions: true,
    originFeat: true,
  }),
  class: keys<ClassSummaryDto>({
    key: true,
    name: true,
    hitDie: true,
    primaryAbilities: true,
    primaryAbilityMode: true,
    subclassLevel: true,
    subclasses: true,
  }),
  classDetail: keys<ClassDetailDto>({
    class: true,
    description: true,
    features: true,
    subclasses: true,
  }),
  ruleDetail: keys<RuleDetailDto>({
    key: true,
    name: true,
    description: true,
    features: true,
  }),
  feature: keys<FeatureDto>({
    key: true,
    name: true,
    level: true,
    description: true,
    choices: true,
  }),
  choiceDefinition: keys<ChoiceDefinitionDto>({
    key: true,
    name: true,
    count: true,
    optionSource: true,
    optional: true,
    allowDuplicates: true,
    maxPerOption: true,
    replacePolicy: true,
  }),
  feat: keys<FeatSummaryDto>({
    key: true,
    name: true,
    category: true,
    repeatable: true,
    hasPrerequisite: true,
  }),
  spell: keys<SpellSummaryDto>({
    key: true,
    name: true,
    level: true,
    school: true,
    castingTime: true,
    range: true,
    ritual: true,
    concentration: true,
    damage: true,
    damageType: true,
    lists: true,
  }),
  spellDetail: keys<SpellDetailDto>({
    spell: true,
    components: true,
    material: true,
    duration: true,
    attackRoll: true,
    saveAbility: true,
    description: true,
  }),
  item: keys<ItemSummaryDto>({
    key: true,
    name: true,
    kind: true,
    category: true,
    costCp: true,
    weight: true,
    magic: true,
    requiresAttunement: true,
  }),
  itemDetail: keys<ItemDetailDto>({
    item: true,
    description: true,
    properties: true,
    features: true,
  }),
  state: keys<CharacterStateDto>({
    id: true,
    name: true,
    level: true,
    species: true,
    background: true,
    alignment: true,
    classes: true,
    abilities: true,
    proficiencyBonus: true,
    hitPointMaximum: true,
    armorClass: true,
    items: true,
    coins: true,
    complete: true,
    pendingChoices: true,
    violations: true,
  }),
  summary: keys<CharacterSummaryDto>({
    id: true,
    name: true,
    level: true,
    species: true,
    background: true,
    classes: true,
  }),
  classLevel: keys<ClassLevelDto>({
    key: true,
    name: true,
    level: true,
    subclass: true,
  }),
  ability: keys<AbilityDto>({
    key: true,
    name: true,
    score: true,
    modifier: true,
  }),
  item_: keys<CharacterItemDto>({
    id: true,
    itemKey: true,
    name: true,
    kind: true,
    quantity: true,
    slot: true,
    attuned: true,
    requiresAttunement: true,
  }),
  pending: keys<PendingChoiceDto>({
    id: true,
    choiceKey: true,
    name: true,
    source: true,
    description: true,
    parentRef: true,
    input: true,
    optionSource: true,
    optional: true,
    total: true,
    selected: true,
    remaining: true,
    constraints: true,
    optionFilters: true,
    optionCount: true,
    options: true,
    optionsUrl: true,
    selectedOptions: true,
  }),
  constraints: keys<ChoiceConstraintsDto>({
    min: true,
    max: true,
    maxPerOption: true,
    allowDuplicates: true,
    allowedPatterns: true,
  }),
  option: keys<OptionDto>({
    key: true,
    name: true,
    summary: true,
    description: true,
    disabled: true,
    disabledReason: true,
    grants: true,
    meta: true,
  }),
  meta: keys<OptionMetaDto>({
    level: true,
    school: true,
    ritual: true,
    damage: true,
    category: true,
    value: true,
    repeatable: true,
    prerequisite: true,
    lists: true,
  }),
  preview: keys<LevelUpPreviewDto>({
    classKey: true,
    classLevel: true,
    characterLevel: true,
    requiresSubclass: true,
    subclasses: true,
    pendingChoices: true,
    replaceable: true,
  }),
  selection: keys<SelectionInfoDto>({
    id: true,
    choiceKey: true,
    choiceName: true,
    optionKey: true,
    optionName: true,
    source: true,
    level: true,
    parentId: true,
    replacePolicy: true,
    removable: true,
  }),
  sheet: keys<CharacterSheet>({
    name: true,
    background: true,
    classes: true,
    subclasses: true,
    species: true,
    level: true,
    experience: true,
    classLevels: true,
    proficiencyBonus: true,
    abilities: true,
    armorClass: true,
    shieldEquipped: true,
    armorClassOptions: true,
    armorClassSources: true,
    hitPointMaximum: true,
    currentHitPoints: true,
    temporaryHitPoints: true,
    hitDice: true,
    initiative: true,
    speed: true,
    otherSpeeds: true,
    size: true,
    passivePerception: true,
    senses: true,
    attacksPerAction: true,
    attacks: true,
    resistances: true,
    immunities: true,
    vulnerabilities: true,
    armorTraining: true,
    weaponProficiencies: true,
    weaponMasteries: true,
    toolProficiencies: true,
    languages: true,
    spellcasting: true,
    spellSlots: true,
    spells: true,
    classFeatures: true,
    speciesTraits: true,
    feats: true,
    resources: true,
    equipment: true,
    attunedItems: true,
    coins: true,
    violations: true,
  }),
  abilitySheet: keys<AbilitySheet>({
    key: true,
    name: true,
    score: true,
    modifier: true,
    saveProficiency: true,
    saveBonus: true,
    skills: true,
  }),
  skillSheet: keys<SkillSheet>({
    key: true,
    name: true,
    proficiency: true,
    bonus: true,
    advantage: true,
    disadvantage: true,
  }),
  coins: keys<Coins>({
    copper: true,
    silver: true,
    electrum: true,
    gold: true,
    platinum: true,
  }),
  coinsDto: keys<CoinsDto>({
    copper: true,
    silver: true,
    electrum: true,
    gold: true,
    platinum: true,
  }),
  spells: keys<CharacterSpellsDto>({
    classes: true,
    other: true,
  }),
  spellcastingClass: keys<SpellcastingClassDto>({
    classKey: true,
    className: true,
    ability: true,
    pool: true,
    prepareChange: true,
    ritualMode: true,
    spellList: true,
    maxSpellLevel: true,
    preparedLimit: true,
    preparedCount: true,
    cantripLimit: true,
    spells: true,
  }),
  knownSpell: keys<KnownSpellDto>({
    key: true,
    name: true,
    level: true,
    school: true,
    castingTime: true,
    ritual: true,
    concentration: true,
    prepared: true,
    alwaysPrepared: true,
    inSpellbook: true,
    source: true,
    ability: true,
  }),
}

describe.skipIf(!apiUrl)('API integration', () => {
  let token: string | null = null
  const http = new HttpClient({ baseUrl: apiUrl, getAccessToken: () => token })

  async function ok<T>(intent: Intent<T>): Promise<T> {
    const result: ApiResult<T> = await http.execute(intent)
    if (!result.ok) {
      throw new Error(`${intent.method} ${intent.path} answered ${result.status}: ${JSON.stringify(result.problem)}`)
    }

    return result.data
  }

  /** Answers every pending choice with the first available options, like a generic GUI (ApiClient.AnswerAllAsync) */
  async function answerAll(state: CharacterStateDto): Promise<CharacterStateDto> {
    for (let round = 0; round < 30 && state.pendingChoices.length > 0; round++) {
      const choice = state.pendingChoices[0]
      expectShape(choice, Shapes.pending)
      expectShape(choice.constraints, Shapes.constraints)

      let options = choice.options
        ?? (await ok(GetChoiceOptionsIntent.fromUrl(choice.optionsUrl, { pageSize: 200 }))).items
      expect(options.length).toBeGreaterThan(0)
      expectShape(options[0], Shapes.option)
      expectShape(options[0].meta, Shapes.meta)
      if (choice.optionSource === 'Feat' && options.some((option) => option.key === 'ability-score-improvement')) {
        options = options.filter((option) => option.key === 'ability-score-improvement')
      }

      const perOption = choice.input === 'Distribution' ? choice.constraints.maxPerOption : 1
      const selections: SelectionDto[] = []
      for (const option of options) {
        for (let pick = 0; pick < perOption && selections.length < choice.remaining; pick++) {
          selections.push({
            choiceKey: choice.choiceKey,
            optionKey: option.key,
            parentRef: choice.parentRef,
          })
        }
      }

      state = await ok(new SelectOptionsIntent(state.id, { selections }))
    }

    return state
  }

  it('creates, levels up and deletes a character', { timeout: 120_000 }, async () => {
    const login = await ok(new RegisterIntent({
      userName: `ui-test-${crypto.randomUUID().slice(0, 8)}`,
      password: 'correct horse battery staple',
    }))
    expectShape(login, Shapes.token)
    expectShape(login.user, Shapes.user)
    token = login.accessToken
    expectShape(await ok(new GetCurrentUserIntent()), Shapes.currentUser)

    // rules catalog
    const species = await ok(new ListSpeciesIntent({ pageSize: 5, sort: 'name' }))
    expectShape(species, Shapes.paged)
    expectShape(species.items[0], Shapes.species)
    expectShape((await ok(new ListBackgroundsIntent({ search: 'sage' }))).items[0], Shapes.background)
    expectShape((await ok(new ListClassesIntent())).items[0], Shapes.class)
    expectShape((await ok(new ListFeatsIntent({ category: 'general', pageSize: 3 }))).items[0], Shapes.feat)
    const spells = await ok(new ListSpellsIntent({
      class: 'wizard',
      maxLevel: 1,
      sort: 'level,name',
      pageSize: 3,
    }))
    expectShape(spells.items[0], Shapes.spell)
    expectShape(await ok(new GetSpellIntent(spells.items[0].key)), Shapes.spellDetail)
    const items = await ok(new ListItemsIntent({ kind: 'Weapon', search: 'quarterstaff' }))
    expectShape(items.items[0], Shapes.item)
    expectShape(await ok(new GetItemIntent('quarterstaff')), Shapes.itemDetail)
    const wizard = await ok(new GetClassIntent('wizard'))
    expectShape(wizard, Shapes.classDetail)
    expectShape(wizard.features[0], Shapes.feature)
    const elf = await ok(new GetSpeciesIntent('elf'))
    expectShape(elf, Shapes.ruleDetail)
    expectShape(elf.features.flatMap((feature) => feature.choices)[0], Shapes.choiceDefinition)

    // origin
    let state = await ok(new CreateCharacterIntent({
      name: 'Elara',
      speciesKey: 'elf',
      backgroundKey: 'sage',
      abilityMethod: 'StandardArray',
      abilities: {
        str: 8,
        dex: 14,
        con: 13,
        int: 15,
        wis: 12,
        cha: 10,
      },
      alignment: 'NeutralGood',
    }))
    expectShape(state, Shapes.state)
    expectShape(state.abilities[0], Shapes.ability)
    expectShape(state.coins, Shapes.coinsDto)
    expect(state.complete).toBe(false)
    state = await answerAll(state)
    expect(state.complete).toBe(true)

    // level 1 wizard
    const preview = await ok(new PreviewLevelUpIntent(state.id, 'wizard'))
    expectShape(preview, Shapes.preview)
    expect(preview.classLevel).toBe(1)
    state = await answerAll(await ok(new LevelUpIntent(state.id, { classKey: 'wizard' })))
    expect(state.level).toBe(1)
    expect(state.complete).toBe(true)
    expectShape(state.classes[0], Shapes.classLevel)

    const selections = await ok(new GetSelectionsIntent(state.id))
    expectShape(selections[0], Shapes.selection)
    const list = await ok(new ListCharactersIntent({ class: 'wizard' }))
    expectShape(list.items[0], Shapes.summary)

    // spells per class
    const characterSpells = await ok(new GetSpellsIntent(state.id))
    expectShape(characterSpells, Shapes.spells)
    expectShape(characterSpells.classes[0], Shapes.spellcastingClass)
    expectShape(characterSpells.classes[0].spells[0], Shapes.knownSpell)
    expect(characterSpells.classes[0].pool).toBe('Spellbook')

    // equipment
    state = await ok(new AddItemIntent(state.id, { itemKey: 'quarterstaff' }))
    expectShape(state.items[0], Shapes.item_)
    state = await ok(new EquipItemIntent(state.id, state.items[0].id, { slot: 'MainHand' }))
    expect(state.items[0].slot).toBe('MainHand')

    // coins: buy a longsword (15 gp), sell it for half, pay 5 sp
    state = await ok(new SetCoinsIntent(state.id, {
      copper: 0,
      silver: 0,
      electrum: 0,
      gold: 20,
      platinum: 0,
    }))
    state = await ok(new AddItemIntent(state.id, {
      itemKey: 'longsword',
      purchase: true,
    }))
    expect(state.coins.gold).toBe(5)
    const longsword = state.items.find((item) => item.itemKey === 'longsword')!
    state = await ok(new RemoveItemIntent(state.id, longsword.id, { sell: true }))
    expect(state.items.some((item) => item.itemKey === 'longsword')).toBe(false)
    expect(state.coins).toEqual({
      copper: 0,
      silver: 5,
      electrum: 0,
      gold: 12,
      platinum: 0,
    })
    state = await ok(new PayCoinsIntent(state.id, { amountCp: 50 }))
    expect(state.coins.silver).toBe(0)
    state = await ok(new ChangeCoinsIntent(state.id, { platinum: 1 }))
    expect(state.coins.platinum).toBe(1)

    // sheet and pdf
    const sheet = await ok(new GetCharacterSheetIntent(state.id))
    expectShape(sheet, Shapes.sheet)
    expectShape(sheet.abilities[0], Shapes.abilitySheet)
    expectShape(sheet.abilities[0].skills[0] ?? sheet.abilities[1].skills[0], Shapes.skillSheet)
    expectShape(sheet.coins, Shapes.coins)
    const pdf = await ok(new GetCharacterSheetPdfIntent(state.id))
    expect(pdf.size).toBeGreaterThan(1000)

    // undo and delete
    state = await ok(new UndoLevelIntent(state.id))
    expect(state.level).toBe(0)
    await ok(new DeleteCharacterIntent(state.id))
    const gone = await http.execute(new GetSelectionsIntent(state.id))
    expect(gone.ok ? 200 : gone.status).toBe(404)
  })
})
