# TableTopSheets

## Spec-driven development

This project uses the spec-driven development workflow from the global CLAUDE.md (`specs/NNN-feature/` with
`spec.md`, `plan.md`, `tasks.md` and a `*-humanreadable.md` version of each; FR/AC/T IDs with links; approval after
spec and plan).

Project specifics:

- `plan.md` covers the parts of the solution a feature touches: Seed (rules data), Engine, Persistence, Api, Ui and
  Pdf, plus tests (Engine, API/mapper, UI) and a manual check of the app and the generated PDF.
- Features built before the switch (creator, level up, shop, Name & Story, PDF, seed) have no spec and are not
  backfilled.
