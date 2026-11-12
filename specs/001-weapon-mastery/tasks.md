# Weapon Mastery on the sheet: tasks

Detailed: [Spec](spec.md) · [Plan](plan.md) · [Tasks](tasks.md) — Readable: [Spec](spec-humanreadable.md) · [Plan](plan-humanreadable.md) · [Tasks](tasks-humanreadable.md)

## Seed and engine

- [x] <a id="t-1"></a>**T-1** Register the `masteryProperty.<key>` stat prefix in `StatKeys`
      ([FR-7](spec.md#fr-7-tactical-master))
- [x] <a id="t-2"></a>**T-2** Add the Push, Sap and Slow effects to `fighter.tactical-master` in `fighter.json`
      ([FR-7](spec.md#fr-7-tactical-master), [AC-7.1](spec.md#ac-7-1))
- [x] <a id="t-3"></a>**T-3** Helper for a weapon's mastery (its own or its base item's)
      ([FR-5](spec.md#fr-5-magic-weapons), [AC-5.1](spec.md#ac-5-1))
- [x] <a id="t-4"></a>**T-4** `CharacterSheet.WeaponMasteryKeys` ([FR-5](spec.md#fr-5-magic-weapons),
      [AC-5.2](spec.md#ac-5-2))
- [x] <a id="t-5"></a>**T-5** `AttackSheet.Mastery`; remove the mastery text from `Notes`
      ([FR-2](spec.md#fr-2-inventory-badge), [AC-2.3](spec.md#ac-2-3))
- [x] <a id="t-6"></a>**T-6** Mastery block: equipped masteries, fallback to all chosen ones, Tactical Master extras
      ([FR-6](spec.md#fr-6-weapon-mastery-block), [AC-6.1](spec.md#ac-6-1), [AC-6.2](spec.md#ac-6-2),
      [AC-6.3](spec.md#ac-6-3), [AC-7.1](spec.md#ac-7-1))
- [x] <a id="t-7"></a>**T-7** Replace the description of the first Weapon Mastery feature; empty description for
      further ones ([FR-6](spec.md#fr-6-weapon-mastery-block), [AC-6.4](spec.md#ac-6-4), [AC-6.5](spec.md#ac-6-5))
- [x] <a id="t-8"></a>**T-8** Engine tests: equipped, fallback, multiclass, Tactical Master, magic variant, attack
      mastery ([FR-5](spec.md#fr-5-magic-weapons), [FR-6](spec.md#fr-6-weapon-mastery-block),
      [FR-7](spec.md#fr-7-tactical-master))

## API

- [x] <a id="t-9"></a>**T-9** `WeaponMasteryDto` and `GET /api/rules/masteries/{key}`
      ([FR-1](spec.md#fr-1-mastery-details), [AC-1.1](spec.md#ac-1-1))
- [x] <a id="t-10"></a>**T-10** `ItemSummaryDto`: `MasteryKey`, `MasteryName`, `BaseKey`; include `BaseItem.Mastery`
      in the item queries ([FR-3](spec.md#fr-3-shop-badge), [FR-4](spec.md#fr-4-items-page-badge),
      [FR-5](spec.md#fr-5-magic-weapons))
- [x] <a id="t-11"></a>**T-11** `CardMapper`: mastery rule text on weapon cards ([FR-8](spec.md#fr-8-item-cards),
      [AC-8.1](spec.md#ac-8-1))
- [x] <a id="t-12"></a>**T-12** API/mapper tests ([FR-1](spec.md#fr-1-mastery-details),
      [FR-5](spec.md#fr-5-magic-weapons), [FR-8](spec.md#fr-8-item-cards))

## PDF

- [x] <a id="t-13"></a>**T-13** Weapon box notes get "Mastery: <name>" from `AttackSheet.Mastery`
      ([AC-6.6](spec.md#ac-6-6))
- [x] <a id="t-14"></a>**T-14** Check that the class features section and the item cards fit the longer text
      ([FR-6](spec.md#fr-6-weapon-mastery-block), [FR-8](spec.md#fr-8-item-cards))

## UI

- [x] <a id="t-15"></a>**T-15** Update the DTOs and add `GetWeaponMasteryIntent` ([FR-1](spec.md#fr-1-mastery-details))
- [x] <a id="t-16"></a>**T-16** Inspector: `mastery` target with name, rule text and optional status
      ([AC-1.1](spec.md#ac-1-1), [AC-1.2](spec.md#ac-1-2), [AC-3.3](spec.md#ac-3-3), [AC-4.2](spec.md#ac-4-2))
- [x] <a id="t-17"></a>**T-17** `MasteryBadge` component ([AC-1.3](spec.md#ac-1-3), [AC-3.2](spec.md#ac-3-2))
- [x] <a id="t-18"></a>**T-18** `isMastered` helper and its unit test ([AC-5.2](spec.md#ac-5-2))
- [x] <a id="t-19"></a>**T-19** Inventory tab: badge on mastered weapons, no mastery text in the attack line
      ([FR-2](spec.md#fr-2-inventory-badge), [AC-2.1](spec.md#ac-2-1), [AC-2.2](spec.md#ac-2-2),
      [AC-2.3](spec.md#ac-2-3))
- [x] <a id="t-20"></a>**T-20** Shop page: badge with status on every weapon with a mastery
      ([FR-3](spec.md#fr-3-shop-badge), [AC-3.1](spec.md#ac-3-1), [AC-3.2](spec.md#ac-3-2))
- [x] <a id="t-21"></a>**T-21** Items page: badge without status ([FR-4](spec.md#fr-4-items-page-badge),
      [AC-4.1](spec.md#ac-4-1), [AC-4.2](spec.md#ac-4-2))
- [x] <a id="t-22"></a>**T-22** Extend the API integration test
- [x] <a id="t-23"></a>**T-23** Run `pnpm lint` and `pnpm format`

## Verification

- [x] <a id="t-24"></a>**T-24** All tests pass (`dotnet test`, `pnpm test`)
- [x] <a id="t-25"></a>**T-25** Manual check in the app on phone and desktop width, and of the generated PDF and item
      cards (all FRs)
- [x] <a id="t-26"></a>**T-26** Tick the acceptance criteria in `spec.md` and set the status to Done
