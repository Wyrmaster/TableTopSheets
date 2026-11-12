# Weapon Mastery on the sheet

Status: Done

Detailed: [Spec](spec.md) · [Plan](plan.md) · [Tasks](tasks.md) — Readable: [Spec](spec-humanreadable.md) · [Plan](plan-humanreadable.md) · [Tasks](tasks-humanreadable.md)

## Problem

Since the PHB 2024, Barbarians, Fighters, Paladins, Rangers and Rogues get the Weapon Mastery class feature: for each
weapon kind they choose, they can use that weapon's mastery property (a Scimitar has Nick, a Longsword has Sap, …).
Players can already choose these weapons, and an attack with a mastered weapon shows the note "Mastery: Nick". But
the rule text of a mastery isn't shown anywhere, in the web app or in the PDF, so players can't look up what Nick or
Sap actually does while playing, or judge whether a looted weapon is worth keeping.

Today:
- The rules data already holds all 8 masteries with their descriptions, and each weapon is linked to its mastery.
- The character sheet knows which weapons are mastered (`WeaponMasteries`), but shows only the weapon names.
- The attack list (inventory tab and PDF weapon box) shows "Mastery: <name>" for a mastered weapon, without the
  rule text.
- Printable item cards show "Mastery: <name>" as a fact, without the rule text.

## Feature requests

| FR                                                   | Where                    |
|------------------------------------------------------|--------------------------|
| [FR-1 Mastery details](#fr-1-mastery-details)        | Web app                  |
| [FR-2 Inventory badge](#fr-2-inventory-badge)        | Web app, inventory tab   |
| [FR-3 Shop badge](#fr-3-shop-badge)                  | Web app, shop            |
| [FR-4 Items page badge](#fr-4-items-page-badge)      | Web app, items page      |
| [FR-5 Magic weapons](#fr-5-magic-weapons)            | Everywhere               |
| [FR-6 Weapon Mastery block](#fr-6-weapon-mastery-block) | PDF and overview tab  |
| [FR-7 Tactical Master](#fr-7-tactical-master)        | PDF and overview tab     |
| [FR-8 Item cards](#fr-8-item-cards)                  | Printable item cards     |

### FR-1 Mastery details

A weapon's mastery appears as a small badge with the mastery's name (e.g. "Nick"). Tapping the badge shows the
mastery's name and rule text in the detail panel: a bottom drawer on the phone, a floating panel on desktop.

- [x] <a id="ac-1-1"></a>**AC-1.1** Tapping a mastery badge shows the mastery's name and rule text
- [x] <a id="ac-1-2"></a>**AC-1.2** The details open as a bottom drawer on the phone and as a floating panel on
      desktop
- [x] <a id="ac-1-3"></a>**AC-1.3** Tapping the badge doesn't trigger the row it sits on

### FR-2 Inventory badge

On the character's inventory tab, the badge appears only on weapons the character has mastery with, equipped or not.
The "Mastery: <name>" text in an equipped weapon's attack line is replaced by the badge.

- [x] <a id="ac-2-1"></a>**AC-2.1** Weapons the character has mastery with show the badge, whether equipped or not
- [x] <a id="ac-2-2"></a>**AC-2.2** Other weapons show no badge
- [x] <a id="ac-2-3"></a>**AC-2.3** The "Mastery: <name>" text no longer appears in an equipped weapon's attack line

### FR-3 Shop badge

In the character's shop, every weapon that has a mastery shows the badge. It's colored when the character has mastery
with that weapon, gray/outlined when not. The details say "You have mastery" or "Not mastered".

- [x] <a id="ac-3-1"></a>**AC-3.1** Every weapon with a mastery shows the badge
- [x] <a id="ac-3-2"></a>**AC-3.2** The badge is colored when the character has mastery, gray/outlined otherwise
- [x] <a id="ac-3-3"></a>**AC-3.3** The details say "You have mastery" or "Not mastered"

### FR-4 Items page badge

On the items page (browsing all items, not tied to a character), every weapon that has a mastery shows the badge,
without status.

- [x] <a id="ac-4-1"></a>**AC-4.1** Every weapon with a mastery shows the badge
- [x] <a id="ac-4-2"></a>**AC-4.2** Neither the badge nor the details show a mastery status

### FR-5 Magic weapons

Magic weapons count as their base weapon: a +1 Longsword has the Longsword's mastery (Sap), and it's mastered when
the character has mastery with Longswords.

- [x] <a id="ac-5-1"></a>**AC-5.1** A magic weapon shows its base weapon's mastery in badges, details and the PDF
- [x] <a id="ac-5-2"></a>**AC-5.2** A magic weapon counts as mastered when the character has mastery with its base
      weapon

### FR-6 Weapon Mastery block

The Weapon Mastery class feature shows the masteries the character can use, each with the weapons it applies to and
its rule text. This replaces the feature's general description, in the PDF's class features section and on the
overview tab.

```
Weapon Mastery.
Nick (Scimitar, Dagger): When you make the extra attack of the Light property, …
Sap (Longsword): If you hit a creature with this weapon, that creature has Disadvantage …
```

- Normally the block lists the masteries of the equipped weapons (main hand, off hand and ready), each once.
- If no equipped weapon matches a chosen mastery, it lists all chosen masteries instead, so a player can decide
  whether to keep a piece of loot.
- With Weapon Mastery from more than one source (e.g. Fighter and Barbarian multiclass), all masteries appear in one
  combined block under the first Weapon Mastery feature.

- [x] <a id="ac-6-1"></a>**AC-6.1** The block lists each mastery of the equipped weapons once, with the weapons it
      applies to and its rule text
- [x] <a id="ac-6-2"></a>**AC-6.2** Mastered weapons that aren't equipped don't appear while an equipped weapon
      matches
- [x] <a id="ac-6-3"></a>**AC-6.3** When no equipped weapon matches, the block lists all chosen masteries with their
      weapons and rule text
- [x] <a id="ac-6-4"></a>**AC-6.4** With Weapon Mastery from several sources, one combined block is shown; the other
      Weapon Mastery features don't repeat it
- [x] <a id="ac-6-5"></a>**AC-6.5** The overview tab shows the same block as the PDF
- [x] <a id="ac-6-6"></a>**AC-6.6** The PDF weapon box keeps showing "Mastery: <name>" for mastered weapons

### FR-7 Tactical Master

A Fighter with Tactical Master (level 9) can swap a weapon's mastery for Push, Sap or Slow, so the block also lists
these three.

- [x] <a id="ac-7-1"></a>**AC-7.1** A Fighter with Tactical Master gets Push, Sap and Slow in the block, each only
      once, also when a weapon already lists one of them

### FR-8 Item cards

The printable card of a weapon with a mastery shows the mastery's rule text below its properties, in addition to the
existing "Mastery: <name>" fact.

- [x] <a id="ac-8-1"></a>**AC-8.1** A weapon's item card shows the rule text of its mastery

## Out of scope

- Any mechanical effect of masteries (e.g. Graze damage, Vex advantage); this feature only displays them
- Changing the chosen masteries; swapping them on a long rest keeps working as it does today
- A list of masteries for weapons that are carried but not equipped (apart from the fallback in
  [FR-6](#fr-6-weapon-mastery-block))
- The Weapon Master feat; it isn't in the rules data yet (once it is, it counts as one more source of masteries)

## Open questions

- None
