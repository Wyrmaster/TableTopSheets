# TableTopSheets

Character sheets for D&D (Player's Handbook 2024), computed from rules stored in the database.

There is no C# in the database. All rules are declarative data: features, effects, choices, class tables and expressions.
A character stores only the **decisions** a player made (a journal of level ups and selections) plus play state
(inventory, equipped and attuned items, prepared spells, used resources). Every value on the sheet is **recomputed** from
the rules and that journal. Undoing a level up removes the newest journal entry, and donning armor changes `EquipSlot`.
Neither needs an inverse script.

## Projects

| Project | Purpose |
|---|---|
| `Common/TableTopSheets.Common` | Enums, rule keys, `CharacterSheet` DTO (shared with the PDF library) |
| `Persistence/TableTopSheets.Persistence` | EF Core entities, configurations, `TableTopSheetsContext` |
| `Persistence/TableTopSheets.Persistence.Postgresql` / `.Sqlite` | Providers, design-time factories, migrations |
| `Engine/TableTopSheets.Engine` | Expression language, `CharacterEvaluation`, choices, sheet builder, `CharacterService` |
| `Seed/TableTopSheets.Seed` | Seed JSON (`Data/Extracted`, `Data/Rules`), loader, builder, idempotent writer |
| `Tools/TableTopSheets.Extractor` | Generates `Data/Extracted` from the Player's Handbook markdown |
| `Pdf/Library` | PDF rendering; `CharacterSheetDocument` renders a `CharacterSheet` |
| `Tests/TableTopSheets.Engine.Tests` | Engine, rules, undo, multiclass, equipment, migration and PDF tests |

## Schema

### Rules (seeded, read only)

- **CharacterClass / Subclass / Species / Background / Feat / Spell / Item (Weapon, Armor, Tool) / Language / WeaponMastery**:
  rule entities with a unique `Key`.
- **Feature**: the universal carrier of mechanics. It is owned by a class, subclass, species, background, feat, item or spell,
  or it is an *option* (invocation, metamagic …) that becomes active when chosen. Each feature has:
  - `Level`: the class level at which it's gained.
  - `GrantMode`: `FirstClassOnly` for starting proficiencies, `MulticlassOnly` for the reduced multiclass proficiencies.
  - `Toggleable`: for Rage, Mage Armor and similar features that are switched on and off.
  - `SheetSection`: where the feature prints on the sheet.
- **Effect**: changes a stat while its feature is active.
  - `Target`: a stat key such as `ability.int.score`, `skill.stealth.prof`, `ac.base`, `speed.walk` or `spell.fireball`.
    The full list is in `Engine/Rules/StatKeys.cs`.
  - `Operation`: one of `Add`, `Set`, `Max`, `Min`, `Proficiency`, `Grant` or `Candidate`.
  - `ValueExpr`: an expression that computes the value.
  - `ConditionExpr`: an optional expression, for example `!wearing('armor') && !wielding('shield')`.
  - `StackGroup`: `Add` effects in the same group don't stack, so only the highest counts. This covers Extra Attack from
    two classes and Thirsting Blade.
  - `Candidate` effects are alternative formulas, used for Armor Class: only one of them applies.
- **Choice**: a decision while its feature is active.
  - `CountExpr`, for example `scale('warlock.invocations')`.
  - `OptionSource`: Static, Ability, Skill, Tool, Language, Spell, Feat, FeatureCategory, Item or Weapon.
  - `FilterExpr`, for example `onList('wizard') && option.level <= maxSpellLevel('wizard')`.
  - `ReplacePolicy`: `Never`, `OneOnLevelUp` (journaled swap), or `OnLongRest` / `OnShortRest` (changed in place).
  - `Choice.Effects` apply once per selection. `$option` (and `$value` of static options) is substituted in their target
    and value.
  - For spell choices, `Category` controls what a selection means: `cantrip`, `prepared` (classes that prepare on level up)
    or `spellbook`. A spell choice without a category grants an always-prepared spell, such as Magic Initiate.
- **ChoiceOption**: a static option. It can grant a feature, which allows nested choices such as Elven Lineage or a
  Magic Initiate spell list.
- **ScaleValue**: class table columns per level, for example `wizard.cantrips` or `monk.martial-arts` = `1d8`.
- **ResourceDefinition**: limited uses such as Second Wind, Sorcery Points or Luck Points, with a `MaxExpr`, an optional die
  and a recharge period.
- **SpellcastingProfile**: one per Spellcasting or Pact Magic feature, with these columns:
  - ability
  - progression: Full, Half, Third or Pact
  - slot table (the Eldritch Knight has its own)
  - spell list
  - prepared and cantrip scale keys
  - pool: class list or spellbook
  - when preparation changes: long rest or level up
  - ritual mode

  The Wizard, Sorcerer and Warlock are differences in these columns, not special code.
- **SpellSlotRow**: the `multiclass` table, which is also the single-class table for full and half casters, the `pact`
  table, and class-specific tables.

### Characters

- **Character**: base ability scores, species, background, toggles and play state.
- **CharacterLevel** is the journal. `Number` is the character level, and the row holds the class, the subclass (on the
  level that grants it) and an optional hit point roll.
- **CharacterChoice**: one selection.
  - `LevelId` is the level up that made it (null means character creation).
  - `ParentId` scopes nested choices, for example the ability choice of the ASI feat picked at level 4.
  - `SupersededByLevelId` marks a selection replaced by a later level up.
- **CharacterItem** records equip slot and attunement. Other tables: **CharacterPreparedSpell** (long rest loadout),
  **CharacterSpellbookEntry** (copied spells) and **CharacterResourceUsage**.

### Level up and undo

`CharacterService.LevelUp` runs these steps:

1. It checks the multiclass prerequisites (13+ in the primary abilities of every class, with any/all semantics per class).
2. It adds the level and applies swaps (one per `OneOnLevelUp` choice of the leveled class).
3. It applies selections one at a time against a fresh evaluation, so nested and dependent choices validate correctly.
4. It rejects the request while required choices are open.

When anything fails, the in-memory character is restored and nothing is saved.

`UndoLastLevelAsync` only removes the highest level:

- Selections made with that level are deleted.
- Selections it replaced become active again.
- Hit points, slots, proficiencies and everything else are recomputed.

Loadouts that become illegal, such as too many prepared spells or spells above the new maximum level, are reported as
`SheetViolation`s instead of being deleted. The database enforces the same rule: `CharacterChoice.LevelId` cascades, and
`SupersededByLevelId` is set to null when the level is deleted.

## Expressions

The expression language supports numbers, dice (`1d8`), strings, `+ - * / %`, comparisons, `&& || !` and `?:`.

- **Built-in functions:** `floor`, `ceil`, `min`, `max`, `oneOf`
- **Identifiers:** `level`, `pb`, `half` / `proficient` / `expertise`, `spellcaster`
- **Character functions:** `classLevel`, `subclass`, `score`, `mod`, `scale`, `stat`, `wearing`, `wielding`, `equipped`,
  `attuned`, `has`, `chosen`, `proficient`, `skillProficiency`, `active`, `maxSpellLevel`
- **Inside filters and prerequisites:** `option.key`, `option.level`, `option.school`, `option.ritual`, `option.category`,
  and `onList('wizard')`

`CatalogValidator` checks every expression, stat key and reference when seeding.

## Seed data

- `Data/Extracted` holds the rule book content generated by `TableTopSheets.Extractor`: spells, feats, species traits,
  backgrounds (including their ability, skill, tool and language mechanics), class feature text, invocation and metamagic
  text, equipment, masteries and languages.
- `Data/Rules` holds the hand-authored mechanics. Records merge by key, so a feature in `Data/Rules` with the key of an
  extracted feature adds effects and choices to the extracted text.

To regenerate the extracted data:

```
dotnet run --project Tools/TableTopSheets.Extractor -- "<path>/Player's Handbook (2024).md"
```

The markdown has no class tables, core-trait tables or subclasses. Hit dice, saves, skills, scale columns and subclass
features are therefore authored in `Data/Rules/classes/*.json`.

**Current content:**

- All 391 spells, 75 feats, 10 species, 16 backgrounds, equipment, languages and the text of every class feature.
- Full mechanics for the Wizard (Evoker), Sorcerer (Draconic), Warlock (Fiend, all invocations), Fighter (Eldritch Knight)
  and Monk (Open Hand).
- Mechanics for the Human, Elf and Dwarf, and for the most common feats.

Other classes still need their `Data/Rules/classes/<class>.json`.

## Database

```
# PostgreSQL (connection string from TABLETOPSHEETS_POSTGRESQL)
dotnet ef database update --project Persistence/TableTopSheets.Persistence.Postgresql

# then seed (repeatable; ids are deterministic so characters keep their references)
await SeedWriter.SeedAsync(context);
```
