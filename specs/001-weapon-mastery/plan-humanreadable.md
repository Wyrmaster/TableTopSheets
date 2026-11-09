# Weapon Mastery on the sheet: how it's built

Status: Done

Detailed: [Spec](spec.md) · [Plan](plan.md) · [Tasks](tasks.md) — Readable: [Spec](spec-humanreadable.md) · [Plan](plan-humanreadable.md) · [Tasks](tasks-humanreadable.md)

## Main decisions

- **The badge opens the existing detail panel**, the same one used for spells and features, instead of a new kind of
  popup. → [Plan: Decisions](plan.md#decisions)
- **The mastery list is worked out once** by the sheet calculation; the PDF and the overview tab both just show it.
- **Tactical Master is described in the rules data** ("grants Push, Sap and Slow") rather than being a special case in
  the code, so future sources of masteries work the same way.
- **Magic weapons look at their normal version** in one shared place, so every screen agrees.

## The parts

1. **Rules data:** Tactical Master learns that it grants Push, Sap and Slow. → [Plan: Seed](plan.md#seed)
2. **Sheet calculation:** works out which masteries you can use and writes the text for the Weapon Mastery feature.
   Attacks get their mastery as a separate value instead of inside the note text, so the app can draw a badge.
   → [Plan: Engine](plan.md#engine)
3. **Server:** item lists include each weapon's mastery; there's a new address to load one mastery's rule text; item
   cards get the rule text. → [Plan: API](plan.md#api)
4. **PDF:** shows the new text and keeps "Mastery: Nick" in the weapon box. → [Plan: PDF](plan.md#pdf)
5. **App:** the badge, the detail panel for a mastery, and the badges on the inventory, shop and items pages.
   → [Plan: UI](plan.md#ui)
6. **Checks:** automated tests for each case, then a look at the app on phone and desktop and at the PDF.
   → [Plan: Tests](plan.md#tests)

## Watch out for

- The PDF's class features area might be too small for all the rule texts; the overflow should move to the
  continuation box. → [Plan: Risks](plan.md#risks)
- If magic weapons don't carry their own mastery, every place has to look at the normal version; the shared helper
  takes care of that.
