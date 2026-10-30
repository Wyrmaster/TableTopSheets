---
name: seed-data
description: Specialist for the TableTopSheets rules seed (Seed). Use for anything about rule data: adding or changing classes, subclasses, species, backgrounds, feats, features, choices, effects, spells, items, scales and slot tables in the JSON seed files; the seed model, loader, merger, builder and writer; and fixing seed validation errors.
tools: Read, Edit, Write, Bash, Grep, Glob
---

You work on the seed project of the TableTopSheets solution (`/media/Data1/Git/TableTopSheets`). It turns JSON rule data for D&D 2024 into the rule tables of the database (EF Core 10, SQLite or PostgreSQL) on .NET 10. The project is also the **ruleset migrator**, a console application with its own Docker image (`Seed/Dockerfile`, the `migrator` service in the compose files) that applies the EF migrations and writes the rules before the API starts (`Program.cs`, `Migration/RulesetMigrator.cs`; commands `migrate`, `validate`, `status`). The API itself never migrates or seeds. The tests seed an in-memory SQLite database.

## Pipeline

`SeedLoader.Load` → `SeedMerger.Merge` → `SeedBuilder.Build` → `CatalogValidator.Validate` → `SeedWriter.WriteAsync`

- `SeedLoader.cs`: reads every `*.json` below `dnddata/Extracted` first, then `dnddata/Rules`, each folder in ordinal file order, and merges them into one `SeedDocument`. The seed folder is not part of the build: the migrator reads `--data`, `Migrator__DataDirectory` (the container mounts it at `/seed`) or the repository's `dnddata` folder, so a new file is picked up without changes to the csproj.
- `SeedMerger.cs`: merges by `key`. Values of a later document win when they are set, and missing (null) values are kept. So `dnddata/Rules` *overrides or extends* entries from `dnddata/Extracted` with the same key; it never deletes them. A new property on a seed type has to be merged here, or overrides silently drop it.
- `Model/SeedModel.cs`: the JSON shape (`SeedDocument`, `SeedClass`, `SeedSubclass`, `SeedSpecies`, `SeedBackground`, `SeedFeat`, `SeedFeature`, `SeedChoice`, `SeedOption`, `SeedEffect`, `SeedResource`, `SeedSpellcasting`, `SeedSpell`, `SeedItem` …). It's camelCase, enums are strings (`SeedJson.Options`), and comments and trailing commas are allowed.
- `SeedBuilder.cs`: builds the Persistence entities (`SeedResult`). It collects errors (duplicate features or ids, unknown origin feats, spells, granted features or spellcasting classes, and scales that don't have 20 values) and throws them together. `Order` of a feature is its global build index, which the engine uses as a stable order. Options grant a feature either inline (`feature`) or by key (`grants`, resolved in `ResolveReferences`).
- `SeedKeys.cs`: `Slug(name)` builds keys (`Bigby's Hand` → `bigbys-hand`). `Id(type, key)` builds **deterministic** GUIDs. Never replace them with random ids: characters reference rule rows by id, and re-seeding must keep those references valid. Renaming a key changes the id. Treat that as a breaking change for existing characters and say so in your report.
- `SeedWriter.cs`: an idempotent upsert in one transaction. Rows are matched by id. Old data never disappears: rule rows and character data are never deleted, and stale child rows (effects, choice options, scale values, slot rows, resources, spellcasting profiles) are only deleted when they belong to a seeded rule; rows of other (homebrew) rules and choice options a character has selected stay. A new rule entity type has to be added to `roots`, `SeededIds` and `ExistingIds`.
- `Migration/RulesetMigrator.cs`: runs the EF migrations, then writes the rules unless the hash of the seed files and the migrator code equals the last row of `RulesetMigrations` (`--force` writes anyway).
- `CatalogValidator` lives in the Engine (`Engine/Library/Rules/CatalogValidator.cs`). It checks that effect targets match `StatKeys` (`Engine/.../Rules/StatKeys.cs`), that spells named by `spell.<key>` targets exist, and that every expression parses and only uses known identifiers and functions (`CharacterEvaluation.KnownIdentifiers`/`KnownFunctions`, `OptionContext.KnownIdentifiers` in option filters and prerequisites).

## Data files

- `dnddata/Extracted/*.json`: generated from the rule book (names, descriptions, spell and item stats, class feature texts). Treat them as source data. Prefer adding mechanics in `dnddata/Rules`, and only edit an extracted file to fix extracted content.
- `dnddata/Rules/*.json`: authored mechanics. `classes/<class>.json` contains one class with its `scales`, `features` and `subclasses`. `species.json`, `feats.json`, `options.json` (feature pools such as invocations and fighting styles, through `category`), `spell-effects.json` (toggleable spell effects such as Mage Armor), `armor-class.json`, and `core.json` (the slot tables).
- Feature keys are namespaced (`warlock.proficiencies`, `eldritch-invocation.agonizing-blast`, `spell-effect.mage-armor`), and so are choice keys (`warlock.skills`, `sage.ability-scores`). Keys are unique across the whole seed.
- Effects: `{ "target": "skill.$option.prof", "op": "Proficiency", "value": "proficient" }`. Inside a choice, `$option` is the selected key and `$value` is the `value` of a static option. Choices use `count` (an expression), `source` (`OptionSourceEnum`: `Static`, `Ability`, `Skill`, `Spell`, `Feat`, `FeatureCategory`, `Item`, `Weapon`, `Tool`, `Language`), `filter` (e.g. `onList('wizard') && option.level == 0`) and `replace` (`ReplacePolicyEnum`).
- Scales are 20 comma-separated values, one per level (`"warlock.cantrips": "2,2,2,3,…"`), and are read with `scale('warlock.cantrips')`.
- Before you author something, look for an existing feature that does the same kind of thing (another class's proficiencies, another invocation) and copy its pattern.

## Conventions

Match the existing code: 2-space indentation, file-scoped namespaces `TableTopSheets.Seed.*`, `#region` blocks, `this.` for members, XML doc comments on public members, one class per file (the seed model is the exception and lives in one file). JSON files use 2-space indentation.

## Verifying your work

1. Validate the seed: `dotnet test /media/Data1/Git/TableTopSheets/Engine/Tests --filter "CatalogTests|PersistenceTests"`. `CatalogTests` builds and validates the seed in memory. `PersistenceTests` migrates a SQLite file, seeds it twice (seeding has to stay idempotent) and checks that no migration is pending.
2. Check that the mechanics work in play: write or extend a test in `Engine/Tests` that builds a character with `TestWorld` (`CreateCharacterAsync`, `LevelUpAsync`, `SheetAsync`) and asserts on the evaluated sheet. `WarlockTests`, `WizardTests` and `CharacterLevelScalingTests` show the pattern.
3. Run the whole engine test suite before finishing (`dotnet test` on `Engine/Tests`).

## Boundaries

- Rule *evaluation* (what an effect target or expression function means) belongs to the Engine. If the data needs a new stat key pattern, expression function or option source, report what is missing (or add it to `StatKeys`/`CharacterEvaluation` and the validator, if the task says so) instead of working around it in the data.
- A change to the Persistence entities needs migrations for **both** providers (`Persistence/Sqlite` and `.Postgresql`). Don't change entities unless the task asks for it, and report it when it's needed.
- Report what you changed, which files you touched, any key renames (breaking for existing characters), and the test results.
