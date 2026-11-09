# TableTopSheets

## Spec-driven development

New features are built spec first, one folder per feature under `specs/`. Existing features without a spec stay as
they are; they are not backfilled.

### When to use it

At the start of each request, ask whether it should go through the spec flow. Do not decide alone, not even for
changes that look small.

### Layout

Feature folders are numbered kebab-case, the number is the next free one:

```
specs/
  001-short-rest/
    spec.md    what and why: problem, behavior, acceptance criteria, out of scope, open questions
    plan.md    how: changes in Engine, Seed, Persistence, Api, Ui, Pdf; data model and API changes; risks
    tasks.md   ordered checklist of implementation steps
```

### Flow

1. **Clarify**: before writing anything, ask 1–3 rounds of questions (rules edge cases, behavior on phone and
   desktop, what is out of scope, how it interacts with existing features). Prefer asking over assuming.
2. **Spec**: write `spec.md`. Remaining questions go into its "Open questions" section. **Stop and wait for
   approval.**
3. **Plan**: write `plan.md` and `tasks.md`. **Stop and wait for approval.**
4. **Implement**: work through `tasks.md` and tick off each task (`- [x]`) when it is done. If the implementation
   has to deviate from the plan, ask first and update `plan.md`.
5. **Done**: when all acceptance criteria hold, set the status to Done. Spec files are committed together with the
   feature's implementation, not in a separate commit.

Later changes to a feature with a spec update its `spec.md` (and plan/tasks for the new work) instead of starting a
new folder.

### spec.md template

```markdown
# <Feature name>

Status: Draft | Approved | In progress | Done

## Problem
Why the feature is needed and for whom.

## Behavior
What the player sees and does, step by step. Phone and desktop differences where they matter.

## Acceptance criteria
- [ ] Short, testable statement in plain language

## Out of scope
- …

## Open questions
- …
```
