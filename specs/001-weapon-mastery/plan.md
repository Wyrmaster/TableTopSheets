# Weapon Mastery on the sheet: plan

Status: Done

Detailed: [Spec](spec.md) · [Plan](plan.md) · [Tasks](tasks.md) — Readable: [Spec](spec-humanreadable.md) · [Plan](plan-humanreadable.md) · [Tasks](tasks-humanreadable.md)

## Decisions

- The mastery badge opens the existing detail panel (inspector) with a new `mastery` view; the panel already is a
  bottom drawer on the phone and a floating panel on desktop ([FR-1](spec.md#fr-1-mastery-details)).
- The mastery block of the Weapon Mastery class feature is built once in the engine; the PDF and the overview tab
  both show it ([FR-6](spec.md#fr-6-weapon-mastery-block)).
- Tactical Master grants Push, Sap and Slow through seed effects, not through a hardcoded feature key
  ([FR-7](spec.md#fr-7-tactical-master)).
- Magic weapons count as their base weapon: a weapon's mastery is its own, or else its base item's (`BaseItemId`), and
  mastery is checked against `evaluation.BaseKey(weapon)` as today ([FR-5](spec.md#fr-5-magic-weapons)).

## Seed

Covers [FR-7](spec.md#fr-7-tactical-master) · Tasks [T-1](tasks.md#t-1), [T-2](tasks.md#t-2)

- New stat prefix `masteryProperty.<mastery-key>`: the character can use this mastery property on any weapon it has
  mastery with (Tactical Master). It's registered in `StatKeys`. A separate prefix is needed because
  `StatKeysWithPrefix("mastery.")` already lists the mastered weapons.
- `dnddata/Rules/classes/fighter.json`: `fighter.tactical-master` gets three effects, `masteryProperty.push`,
  `masteryProperty.sap` and `masteryProperty.slow` (`Grant`, `1`).

## Engine

Files: `Engine/Library/Sheet/CharacterSheetBuilder.cs`, `Common/Sheet`

Covers [FR-2](spec.md#fr-2-inventory-badge), [FR-5](spec.md#fr-5-magic-weapons),
[FR-6](spec.md#fr-6-weapon-mastery-block), [FR-7](spec.md#fr-7-tactical-master) · Tasks [T-3](tasks.md#t-3) to
[T-8](tasks.md#t-8)

- **Weapon mastery data on the sheet** ([T-3](tasks.md#t-3), [T-4](tasks.md#t-4)):
  - A helper returns a weapon's mastery (own or base item's).
  - `WeaponMasteryKeys` (base weapon keys the character has mastery with), next to the existing `WeaponMasteries`
    (names). The UI uses it for "mastered or not".
- **Attacks** ([T-5](tasks.md#t-5)): `AttackSheet` gets `Mastery` and `MasteryKey` (name and key of the mastery when
  the character can use it, else null; the key lets the badge open the details). The "Mastery: <name>" text moves out of `Notes`, so the web app can show a badge instead. The PDF adds
  "Mastery: <name>" to the notes again when drawing the weapon box ([T-13](tasks.md#t-13)).
- **Mastery block** ([T-6](tasks.md#t-6)): a helper builds the list of masteries for the class feature:
  1. Masteries of the equipped weapons (main hand, off hand, ready) the character has mastery with, grouped by
     mastery: `Nick (Scimitar, Dagger)`.
  2. If that list is empty: all chosen masteries, grouped the same way, with the names of the mastered weapons.
  3. Plus the masteries granted by `masteryProperty.*` that aren't listed yet (Tactical Master), without weapons.
  4. Each entry has the mastery's rule text.
- **Class features** ([T-7](tasks.md#t-7)): the first feature with key ending in `.weapon-mastery` gets its
  description replaced by the mastery block (plain text and `DescriptionBlocks`: one paragraph per mastery, the name
  and weapons bold). Further Weapon Mastery features keep their name and an empty description. Its name keeps the
  chosen weapons as today (`Weapon Mastery (Scimitar, Longsword)`).
- The catalog already holds the masteries (`RulesCatalog.Masteries`) with `DescriptionHtml`.

## API

Files: `Api/Library`

Covers [FR-1](spec.md#fr-1-mastery-details), [FR-3](spec.md#fr-3-shop-badge), [FR-4](spec.md#fr-4-items-page-badge),
[FR-5](spec.md#fr-5-magic-weapons), [FR-8](spec.md#fr-8-item-cards) · Tasks [T-9](tasks.md#t-9) to
[T-12](tasks.md#t-12)

- **Rules endpoint** ([T-9](tasks.md#t-9)): `GET /api/rules/masteries/{key}` returns
  `WeaponMasteryDto(Key, Name, Description)` with `DescriptionBlocks`, for the inspector.
- **Item summary** ([T-10](tasks.md#t-10)): `ItemSummaryDto` gets `MasteryKey`, `MasteryName` (own or base item's
  mastery, null for non-weapons) and `BaseKey` (null for mundane items). The item list queries include
  `BaseItem.Mastery`. The item detail contains the summary, so the inspector gets the mastery key from there.
- **Item cards** ([T-11](tasks.md#t-11)): a weapon's card appends the mastery's rule text to the description, as a
  paragraph starting with the bold mastery name. The existing "Mastery" fact stays.
- **Character sheet DTO:** passes `weaponMasteryKeys` and `attacks[].mastery` through.

## UI

Files: `Ui/src`

Covers [FR-1](spec.md#fr-1-mastery-details) to [FR-5](spec.md#fr-5-magic-weapons) · Tasks [T-15](tasks.md#t-15) to
[T-23](tasks.md#t-23)

- **API client** ([T-15](tasks.md#t-15)): update `dtos/rules.ts` (`ItemSummaryDto`, new `WeaponMasteryDto`),
  `dtos/sheet.ts` (`weaponMasteryKeys`, `AttackSheet.mastery`) and add a `GetWeaponMasteryIntent`.
- **Inspector** ([T-16](tasks.md#t-16)): new target `{ kind: 'mastery', key }`; `InspectorContent` loads the mastery
  and shows name and rule text. It takes an optional status (`'mastered' | 'not-mastered'`) and shows
  "You have mastery" or "Not mastered".
- **`MasteryBadge` component** ([T-17](tasks.md#t-17)): badge with the mastery name, opens the inspector and stops
  the row click. Props: mastery key/name and an optional status. Colored when mastered or without status,
  gray/outlined when not mastered.
- **Mastered check** ([T-18](tasks.md#t-18)): helper `isMastered(sheet, item)` compares `weaponMasteryKeys` with
  `item.baseKey ?? item.key`.
- **Inventory tab** ([T-19](tasks.md#t-19)): an item row shows the badge only when the weapon is mastered (equipped
  or not); the attack line no longer contains the mastery text.
- **Shop page** ([T-20](tasks.md#t-20)): `ItemRow` shows the badge on every weapon with a mastery, with the status of
  the character.
- **Items page** ([T-21](tasks.md#t-21)): `ItemRow` shows the badge on every weapon with a mastery, without status.
- **Overview tab:** nothing to change; it shows the new feature description from the sheet
  ([AC-6.5](spec.md#ac-6-5)).

## PDF

Files: `Pdf/Library`

Covers [FR-6](spec.md#fr-6-weapon-mastery-block), [FR-8](spec.md#fr-8-item-cards) · Tasks [T-13](tasks.md#t-13),
[T-14](tasks.md#t-14)

- The class features section prints the new description of the Weapon Mastery feature (built by the engine). Check
  that the longer text still fits, or flows into the continuation box like other long features.
- `CharacterSheetDocument`: weapon box notes get "Mastery: <name>" again from `AttackSheet.Mastery`.
- Item cards get the rule text through `CardMapper`; check that the text fits on the card.

## Tests

Tasks [T-8](tasks.md#t-8), [T-12](tasks.md#t-12), [T-18](tasks.md#t-18), [T-22](tasks.md#t-22),
[T-24](tasks.md#t-24), [T-25](tasks.md#t-25)

- **Engine:**
  - Fighter with an equipped Scimitar and Dagger: one Nick entry listing both.
  - Mastered weapons not equipped: the fallback lists all chosen masteries.
  - Fighter/Barbarian multiclass: one combined block; the second feature has no description.
  - Fighter level 9: Push, Sap and Slow listed once, also when a weapon already uses Sap.
  - +1 Longsword equipped: listed as Sap.
  - `AttackSheet.Mastery` set for a mastered weapon, null otherwise; `Notes` without mastery text.
- **API/mapper:**
  - The summary of a weapon (and of a magic variant) carries mastery key, name and base key.
  - A weapon card has the rule text.
  - The masteries endpoint returns name and description.
- **UI:**
  - Unit test for `isMastered` (mundane, magic variant, not mastered).
  - The API integration test checks the new sheet and item fields.
- **Manual:** run the app, check the badges on the inventory, shop and items pages on phone and desktop width, open
  the inspector, generate the PDF and the item cards and look at them (screenshots).

## Risks

- Text length: the mastery block (up to 5+3 rule texts) can overflow the PDF class features area; the continuation
  box should catch it, which the manual check verifies ([T-14](tasks.md#t-14)).
- Magic variants: if a variant doesn't carry its own `MasteryId`, every place has to fall back to the base item; the
  shared helper prevents mismatches ([T-3](tasks.md#t-3)).
