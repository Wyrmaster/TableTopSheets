# Weapon Mastery on the sheet: what changes

Status: Done

Detailed: [Spec](spec.md) · [Plan](plan.md) · [Tasks](tasks.md) — Readable: [Spec](spec-humanreadable.md) · [Plan](plan-humanreadable.md) · [Tasks](tasks-humanreadable.md)

## Why

You can pick your Weapon Mastery weapons, but nowhere tells you what "Nick" or "Sap" actually does. You have to look
it up in the book during play, and you can't easily judge whether a looted weapon is worth keeping.

## What you'll get

### A mastery badge you can tap → [FR-1](spec.md#fr-1-mastery-details)

Weapons show a small badge like "Nick". Tap it and the rule text opens: from the bottom on the phone, as a side panel
on desktop.

- [x] Tapping the badge shows the rule → [AC-1.1](spec.md#ac-1-1)
- [x] Drawer on the phone, panel on desktop → [AC-1.2](spec.md#ac-1-2)
- [x] Tapping the badge doesn't also open the item → [AC-1.3](spec.md#ac-1-3)

### Inventory → [FR-2](spec.md#fr-2-inventory-badge)

Only weapons you have mastery with get the badge, also the ones in your backpack. The "Mastery: Nick" text in the
attack line goes away, because the badge replaces it.

- [x] Your mastered weapons have the badge → [AC-2.1](spec.md#ac-2-1)
- [x] Other weapons don't → [AC-2.2](spec.md#ac-2-2)
- [x] No more "Mastery: …" text in the attack line → [AC-2.3](spec.md#ac-2-3)

### Shop → [FR-3](spec.md#fr-3-shop-badge)

Every weapon shows its mastery. Colored means you can use it, gray means you can't. The details say it in words.

- [x] Every weapon shows its mastery → [AC-3.1](spec.md#ac-3-1)
- [x] Colored or gray depending on your mastery → [AC-3.2](spec.md#ac-3-2)
- [x] Details say "You have mastery" or "Not mastered" → [AC-3.3](spec.md#ac-3-3)

### Items page → [FR-4](spec.md#fr-4-items-page-badge)

Every weapon shows its mastery. This page isn't about a character, so there's no colored/gray difference.

- [x] Every weapon shows its mastery → [AC-4.1](spec.md#ac-4-1)
- [x] Nothing about "mastered or not" → [AC-4.2](spec.md#ac-4-2)

### Magic weapons → [FR-5](spec.md#fr-5-magic-weapons)

A magic weapon counts as its normal version: a +1 Longsword has Sap, and you can use it if you have mastery with
Longswords.

- [x] Magic weapons show the mastery of their normal version → [AC-5.1](spec.md#ac-5-1)
- [x] They count as mastered when the normal version is → [AC-5.2](spec.md#ac-5-2)

### PDF and overview tab → [FR-6](spec.md#fr-6-weapon-mastery-block)

The Weapon Mastery feature lists the masteries of the weapons you have equipped, each with its rule text. If none of
your equipped weapons match, it lists all your chosen masteries instead, which helps when deciding about loot.

- [x] Each mastery of your equipped weapons appears once, with its weapons and rule → [AC-6.1](spec.md#ac-6-1)
- [x] Unequipped weapons don't appear while an equipped one matches → [AC-6.2](spec.md#ac-6-2)
- [x] No match: all your chosen masteries are listed → [AC-6.3](spec.md#ac-6-3)
- [x] Multiclassed into two classes with Weapon Mastery: only one list → [AC-6.4](spec.md#ac-6-4)
- [x] The overview tab shows the same list as the PDF → [AC-6.5](spec.md#ac-6-5)
- [x] The PDF weapon box still says "Mastery: Nick" → [AC-6.6](spec.md#ac-6-6)

### Fighters from level 9 → [FR-7](spec.md#fr-7-tactical-master)

Tactical Master lets a Fighter swap a mastery for Push, Sap or Slow, so these three are listed too.

- [x] Push, Sap and Slow are listed, each only once → [AC-7.1](spec.md#ac-7-1)

### Printable item cards → [FR-8](spec.md#fr-8-item-cards)

- [x] A weapon's card shows its mastery's rule text → [AC-8.1](spec.md#ac-8-1)

## Not part of this

- Dice or damage from masteries (e.g. Graze damage); they're only shown, not calculated
- Changing your chosen masteries; that works as it does today
- A PDF list for weapons you carry but haven't equipped (except when nothing equipped matches)
- The Weapon Master feat, which isn't in the rules data yet
