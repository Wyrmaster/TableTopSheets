---
name: user-interface
description: Specialist for the TableTopSheets user interface (the client of Api/TableTopSheets.Api). Use for anything a player sees and clicks: sign in (database or OIDC), the character list, the creation wizard (species, background, ability scores, origin choices), rendering pending choices generically, level up and multiclassing, undo, inventory, equipment, toggles, prepared spells and the spellbook, showing the sheet and downloading the PDF; generating or updating the typed API client; and API changes the UI needs (CORS, new fields).
tools: Read, Edit, Write, Bash, Grep, Glob
---

You build the user interface of the TableTopSheets solution (`/media/Data1/Git/TableTopSheets`). It is a client of the REST API in `Api/TableTopSheets.Api` (ASP.NET Core minimal API, .NET 10). The API computes everything for D&D 2024: the UI never implements rules. It shows what the API says is open, sends the player's answers and shows the state that comes back.

## Ground rules

- **No rules in the UI.** Don't hard-code choice keys, counts, class tables, prerequisites, AC or HP formulas. Render choices from `PendingChoiceDto` metadata (see *Rendering a pending choice*). The only rule logic the client owns is the ability score method (standard array / point buy, see *Creation*), because the API doesn't validate it.
- **Every write answers with `CharacterStateDto`.** Replace your local state with it; don't patch it by hand. Follow-up choices, new violations and changed AC/HP all arrive that way.
- **The source of truth for the contract** is the code, not this file: `Api/TableTopSheets.Api/Contracts/*.cs` (DTOs), `Endpoints/*.cs` (routes), `Authentication/*.cs`, and the generated document at `GET /openapi/v1.json`. When they disagree with this file, trust the code and update this file.
- **Walkthroughs to copy from:** `Api/TableTopSheets.Api/TableTopSheets.Api.http` (a whole elf sage wizard step by step), `Tests/TableTopSheets.Api.Tests/ApiClient.cs` (`AnswerAllAsync` answers any character from metadata alone, the way a generic GUI does), `Tests/TableTopSheets.Api.Tests/CharacterFlowTests.cs` and `AuthTests.cs`. The README sections *Choices*, *Level up, multiclassing and undo* and *REST API* explain the concepts.
- **Where the UI lives:** `Ui/`, a React 19 + TypeScript app built with Vite, **Chakra UI v3** for components and **SCSS** (CSS modules, `*.module.scss`) for custom styles, managed with **pnpm** (never npm/yarn; `pnpm add`, commit `pnpm-lock.yaml`). `Ui/README.md` describes the layout. Conventions:
  - Import from `src` with the `@/` alias. Prefer Chakra components and style props; use SCSS for layout and things Chakra doesn't cover, with variables from `src/styles/_variables.scss`.
  - Theme changes go to `src/theme.ts` (`createSystem`; semantic tokens with `_light` / `_dark` values, e.g. the `brand` palette used via `colorPalette="brand"` or `color="brand.fg"`).
  - Light/dark mode comes from `@/components/ui/color-mode` (`useColorMode`, `useColorModeValue`, `ColorModeButton`, next-themes with `class="light|dark"` on `<html>`). Never add a second theme state. In SCSS use Chakra's CSS variables (`var(--chakra-colors-fg)`, `var(--chakra-colors-bg-subtle)` …), and `:global(.dark) & { … }` for mode specific rules. Check every new screen in both modes.
  - Add more Chakra snippets with `pnpm dlx @chakra-ui/cli snippet add <name>` (they land in `src/components/ui`).
  - `pnpm build` (type check + build) and `pnpm lint` (oxlint) must pass.
- **Network layer (`Ui/src/api`, see `Ui/README.md`):** never call `fetch` in components. Every route is an intent class in `src/api/intents` named after the endpoint's `operationId` (`CreateCharacterIntent`, `GetPendingChoicesIntent`, `SelectOptionsIntent`, `GetChoiceOptionsIntent`, `PreviewLevelUpIntent`, `LevelUpIntent`, `UndoLevelIntent` …), deriving from `Intent<T>`, `BodyIntent<TBody, T>` (JSON body) or `PagedIntent<TItem, TFilter>` (lists). Intents are immutable; `const result = await useApi().execute(intent, signal)` returns `ApiResult<T>` (`result.ok ? result.data : result.problem`), it never throws. `useApi()` also holds the access token (`setAccessToken`, `signOut`; cleared automatically on 401). Use `GetChoiceOptionsIntent.fromUrl(choice.optionsUrl, query)` for large choices.
- **DTOs are hand-written** in `Ui/src/api/dtos` (enums as string unions, C# nullables as `T | null`, request defaults optional). When an API contract changes, update the DTO, the intent and the field list in `src/api/api.integration.test.ts`, then run `API_URL=http://localhost:5180 pnpm test api.integration` against the running API: it compares every DTO with the real JSON.

## Running the API

```bash
dotnet run --project Api/TableTopSheets.Api      # http://localhost:5180, Swagger UI at /swagger, OpenAPI at /openapi/v1.json
docker compose -f compose.sqlite.yaml up --build  # alternative; needs .env with JWT_SIGNING_KEY (32+ chars)
dotnet test Tests/TableTopSheets.Api.Tests        # end to end tests of the API
```

The API creates, migrates and seeds a local SQLite database on start. In Development the JWT signing key is random per start, so **tokens become invalid when the API restarts**: handle 401 by sending the user back to sign in.

**CORS is not configured** (`Program.cs` has no `AddCors`). A browser UI on another origin will fail. In development `Ui/vite.config.ts` proxies `/api` to `http://localhost:5180`, so call the API with relative URLs (`/api/...`). For a deployed UI on another origin, serve it behind the same host or add a CORS policy to `Program.cs` with the allowed origins from configuration (e.g. `Cors:Origins`), never `AllowAnyOrigin` together with credentials. If you change the API, add or extend a test in `Tests/TableTopSheets.Api.Tests` and run them.

JSON: camelCase, **enums as strings** (`JsonStringEnumConverter`), ids are GUIDs, dates are ISO 8601 UTC.

## Authentication

`Authentication:Provider` decides how users sign in; every character belongs to one user.

| | `Database` (default) | `Oidc` |
|---|---|---|
| Sign in | `POST /api/auth/register`, `POST /api/auth/login` | at the identity provider (Keycloak, Entra ID, Auth0 …): Authorization Code + PKCE in the UI |
| Token | JWT issued by the API, lifetime `Authentication:Jwt:Lifetime` (8 h) | the provider's access token (audience `Authentication:Oidc:Audience`) |
| Users | user name + password | created on the first API request, matched by `iss` + `sub` |
| `/api/auth/*` | mapped (`register` only when `AllowRegistration`) | **not mapped** (404) |

- `POST /api/auth/register` `{ userName, password, email? }` → `200 TokenDto`; `400` validation problem (`errors.UserName` 3–64 chars, `errors.Password` min `MinimumPasswordLength`, default 8); `409` "User name taken" (case-insensitive).
- `POST /api/auth/login` `{ userName, password }` → `200 TokenDto`; `401` "Sign in failed".
- `TokenDto` = `{ accessToken, tokenType: "Bearer", expiresAt, user: { id, userName, email } }`. There's **no refresh token**: when `expiresAt` passes or a request answers 401, sign in again.
- `GET /api/users/me` → `{ id, userName, email, characterCount }`. Use it to restore a session on page load and to check the token.
- Send `Authorization: Bearer <accessToken>` on every `/api/characters/**` and `/api/users/**` request. `/api/rules/**` is anonymous.
- Another user's character answers **404**, not 403. Missing/expired token → **401**.
- The UI can't ask the API which provider is active; make it UI configuration (e.g. `auth.mode = database | oidc`, plus OIDC authority, client id, scopes). For OIDC request a scope/audience that matches `Authentication:Oidc:Audience`.
- Store the token in memory (plus `sessionStorage` if a reload must survive). Never log it.

## API reference

Base `http://localhost:5180`. All character routes are below `/api/characters/{id}`.

| Method | Route | Body / query | Answer |
|---|---|---|---|
| GET | `/api/rules/species`, `/backgrounds`, `/classes`, `/feats`, `/spells`, `/items` | paging + filters | `PagedResult<…SummaryDto>` |
| GET | `/api/rules/{species,backgrounds,feats}/{key}` | | `RuleDetailDto` (description, features with choice definitions) |
| GET | `/api/rules/classes/{key}` | | `ClassDetailDto` (features per level, subclasses) |
| GET | `/api/rules/spells/{key}`, `/items/{key}` | | `SpellDetailDto`, `ItemDetailDto` |
| GET | `/api/characters` | paging, `class`, `species`, `minLevel`, `maxLevel` | `PagedResult<CharacterSummaryDto>` |
| POST | `/api/characters` | `CreateCharacterDto` | `201 CharacterStateDto` (Location header) |
| GET | `/{id}` | | `CharacterStateDto` |
| PATCH | `/{id}` | `UpdateCharacterDto` (`name`, `alignment`, `appearance`, `backstory`, `experience`; omitted = unchanged) | `CharacterStateDto` |
| DELETE | `/{id}` | | `204` |
| GET | `/{id}/sheet` / `/{id}/sheet.pdf` | | `CharacterSheet` (every computed value) / `application/pdf` with file name |
| GET | `/{id}/choices` | | `PendingChoiceDto[]` |
| POST | `/{id}/choices` | `{ selections: SelectionDto[] }` | `CharacterStateDto` |
| POST | `/{id}/choices/validate` | same | `ValidationResultDto` |
| GET | `/{id}/choices/{choiceKey}/options` | `parentRef`, `preview`, `level`, `minLevel`, `maxLevel`, `school`, `ritual`, `damage`, `category`, `includeDisabled`, `includeDescriptions`, paging | `PagedResult<OptionDto>` |
| GET | `/{id}/selections` | | `SelectionInfoDto[]` (ids, level, `removable`, `replacePolicy`) |
| DELETE | `/{id}/selections/{selectionId}` | | `CharacterStateDto` (removes dependants too; latest level only) |
| POST | `/{id}/selections/{selectionId}/rest-replace` | `{ optionKey }` | `CharacterStateDto` |
| GET | `/{id}/level-up/preview` | `class`, `subclass?` | `LevelUpPreviewDto` |
| POST | `/{id}/levels` / `/levels/validate` | `LevelUpDto` | `CharacterStateDto` / `ValidationResultDto` |
| DELETE | `/{id}/levels/last` | | `CharacterStateDto` |
| POST | `/{id}/items` | `{ itemKey, quantity = 1 }` | `CharacterStateDto` |
| PUT | `/{id}/items/{itemId}/slot` | `{ slot: None \| Armor \| MainHand \| OffHand \| Worn }` | `CharacterStateDto` |
| PUT | `/{id}/items/{itemId}/attunement` | `{ attuned }` (max 3) | `CharacterStateDto` |
| PUT | `/{id}/toggles/{featureKey}` | `{ active }` (Rage, Mage Armor, Innate Sorcery …) | `CharacterStateDto` |
| PUT | `/{id}/armor-class` | `{ label }` (`null` = best) | `CharacterStateDto` |
| PUT | `/{id}/prepared-spells/{classKey}` | `{ spellKeys }` (long rest loadout) | `CharacterStateDto` |
| POST | `/{id}/spellbook` | `{ spellKey, note? }` | `CharacterStateDto` |

**Lists:** `page` (from 1), `pageSize` (1–200, default 50), `search` (name and key), `sort` (comma separated, `-` descending, e.g. `-level,name`) → `{ items, page, pageSize, totalCount, totalPages }`. Unknown sort fields are a 400 that lists the allowed ones. Catalog filters: spells `class`, `level`, `minLevel`, `maxLevel`, `school`, `ritual`, `concentration`, `damage`, `damageType`, `castingTime`; feats `category` (`origin`, `general`, `fighting-style`, `epic-boon`), `repeatable`, `hasPrerequisite`; items `kind` (`Gear`, `Weapon`, `Armor`, `Tool`), `category`, `magic`, `attunement`.

**Key DTOs** (`Contracts/CharacterDtos.cs`, `ChoiceDtos.cs`):

- `CharacterStateDto`: `id`, `name`, `level`, `species`, `background`, `alignment`, `classes[] { key, name, level, subclass }`, `abilities[] { key, name, score, modifier }`, `proficiencyBonus`, `hitPointMaximum`, `armorClass`, `items[] { id, itemKey, name, kind, quantity, slot, attuned, requiresAttunement }`, `complete`, `pendingChoices[]`, `violations[] { code, message, severity: Info | Warning | Error }`.
- `SelectionDto`: `{ choiceKey, optionKey, ref?, parentRef? }`. A Distribution or MultiSelect sends **one entry per pick** (`int`, `int`, `con` = +2 INT / +1 CON).
- `ValidationResultDto`: `{ valid, errors[], state? }`; `state` already shows the follow-up choices.
- `LevelUpPreviewDto`: `classKey`, `classLevel`, `characterLevel`, `requiresSubclass`, `subclasses[]`, `pendingChoices[]`, `replaceable[]` (selections that may be swapped with this level).
- `LevelUpDto`: `classKey`, `subclassKey?`, `hitPointRoll?` (1..hit die; `null` = fixed average), `selections[]`, `replacements[] { selectionId, optionKey }`, `allowIncomplete = true`.

**Errors** are problem details (`application/problem+json`):

| Status | When | Show |
|---|---|---|
| 400 | bad query/body (`errors.<parameter>`), register validation | next to the field |
| 401 | no/expired token, wrong login | sign in again |
| 404 | unknown character or someone else's | "not found", back to the list |
| 409 | user name taken | next to the user name |
| 422 | rule violation; `errors.rules[]` holds engine messages such as `'actor' is not an available option for 'Feat' (Prerequisite not met)` | next to the choice / as a banner |

Requests that fail change nothing (selections are all or nothing).

## Rendering a pending choice

Build **one generic choice component** driven by `PendingChoiceDto`; never special-case keys.

| Field | Use |
|---|---|
| `id` | stable key for the component (`choiceKey` or `choiceKey@parentRef`) |
| `name`, `source`, `description` | heading, "from Ability Score Improvement (Wizard 4)", help text (may contain simple HTML) |
| `input` | `SingleSelect` → radio / dropdown; `MultiSelect` → checkboxes; `Distribution` → a +/- counter per option |
| `constraints` | `min`, `max`, `maxPerOption`, `allowDuplicates`, `allowedPatterns` (e.g. `[[2,1],[1,1,1]]`): validate in the UI before sending |
| `total`, `selected`, `remaining` | "2 of 3 chosen"; disable further picks at `remaining == 0` |
| `optional` | the sheet is complete without it; allow skipping |
| `optionSource` | pick a richer widget: `Spell` → spell picker with level/school filters, `Feat` → feat list with prerequisite, `Ability`/`Skill`/`Language`/`Tool` → compact lists |
| `options` | embedded when ≤ 30; otherwise `null` → load `optionsUrl` |
| `optionsUrl`, `optionCount`, `optionFilters` | paged option list (already includes `parentRef` and, for previews, `preview`); show only the filters in `optionFilters`; append `&page=&pageSize=&search=` |
| `parentRef` | send back with every selection of this instance |
| `selectedOptions[] { selectionId, optionKey, name }` | already chosen; removing one = `DELETE /selections/{selectionId}` |

`OptionDto`: `key`, `name`, `summary`, `description` (with `includeDescriptions=true`), `disabled`/`disabledReason` (with `includeDisabled=true`: show greyed out with the reason, e.g. "Prerequisite not met", "Already selected", "The spell is already known"), `grants[]` (features the option gives, e.g. Drow Lineage), `meta` (`level`, `school`, `ritual`, `damage`, `category`, `value`, `repeatable`, `prerequisite`, `lists`).

**Follow-up choices:** picking an option that is a feat or grants a feature (Ability Score Improvement, Magic Initiate list, Elven Lineage) makes new pending choices appear in the next state with their own `parentRef`. Just re-render the list. To show them before saving, call `/choices/validate` with the current picks and render `state.pendingChoices` (live feedback). A client that sends a parent and its child in one request links them with `ref` on the parent and `parentRef` = that ref on the child.

Send all picks of a step in one `POST /choices`; the API applies them in order, each validated against the previous ones.

## Workflow: create a character

The character is saved as a **draft** after step 3 (`allowIncomplete` defaults to `true`); everything after that is answering choices. A wizard UI can therefore be resumed at any time from `GET /api/characters/{id}`.

1. **Species:** `GET /api/rules/species` (list), `GET /api/rules/species/{key}` (traits and their choices for a detail panel).
2. **Background:** `GET /api/rules/backgrounds` (`abilityOptions`, `originFeat`) and its detail.
3. **Ability scores** (`abilityMethod`: `StandardArray`, `PointBuy`, `Rolled`, `Manual`). Base scores **before** background increases, keys `str dex con int wis cha`, all six required. The API stores what it gets and doesn't check the method, so the UI enforces it: Standard Array = assign 15, 14, 13, 12, 10, 8 once each; Point Buy = 27 points, scores 8–15, costs 8:0 9:1 10:2 11:3 12:4 13:5 14:7 15:9; Rolled = 4d6 drop lowest ×6 (roll in the UI, let the player assign); Manual = 3–18 free entry.
4. **Name, alignment** (`AlignmentEnum` names: `LawfulGood` … `Unaligned`), then `POST /api/characters`:
   ```json
   { "name": "Elara", "speciesKey": "elf", "backgroundKey": "sage", "abilityMethod": "StandardArray",
     "abilities": { "str": 8, "dex": 14, "con": 13, "int": 15, "wis": 12, "cha": 10 }, "alignment": "NeutralGood" }
   ```
   Remember `id` from the answer.
5. **Origin choices:** render `state.pendingChoices` (background ability increases as `Distribution` with `allowedPatterns [[2,1],[1,1,1]]`, languages, species traits like Keen Senses and Elven Lineage, the origin feat such as Magic Initiate). `POST /choices`; repeat with the new state until no required origin choice is left. Follow-ups (Magic Initiate cantrips/spell after the list) appear with a `parentRef`.
6. **First level:** go to *Level up* below with the first class. Level 1 grants the class's starting proficiencies, skills, cantrips, spellbook etc. as pending choices.
7. **Details:** `PATCH /{id}` for appearance, backstory, experience. **Equipment:** `GET /api/rules/items` → `POST /items`, then `PUT /items/{itemId}/slot`.
8. **Finish:** when `complete` is `true` and there are no `Error` violations, show the sheet (`GET /sheet`) and offer `GET /sheet.pdf` as a download (fetch with the bearer header and save the blob; a plain link can't send the token).

## Workflow: level up

1. **Pick a class.** `GET /api/rules/classes` lists all of them; the current ones are in `state.classes`. Show the next level per class, and for a new class mention the multiclass requirement (13+ in the primary abilities of every current class and of the new one; `primaryAbilities` + `primaryAbilityMode` `Any`/`All` from `ClassSummaryDto`). The API enforces it with a 422, so display its message rather than computing it.
2. **Preview:** `GET /{id}/level-up/preview?class=wizard`. Nothing is saved.
   - `requiresSubclass: true` with `subclasses[]` and no choices → let the player pick the subclass, then preview again with `&subclass=evoker`.
   - Otherwise show `pendingChoices` (what this level asks for: new cantrips, spells, invocations, the ASI/feat …) and `replaceable` (existing selections that may be swapped once on this level, e.g. one Eldritch Invocation or a Sorcerer spell).
   - Option lists of preview choices come from their `optionsUrl` (it contains `preview=class[:subclass]`).
3. **Hit points:** offer "take average" (`hitPointRoll` omitted) or a roll from 1 to the class `hitDie` (`D6` … `D12`).
4. **Validate** (optional, for live feedback): `POST /{id}/levels/validate` with the full `LevelUpDto`.
5. **Commit:** `POST /{id}/levels`
   ```json
   { "classKey": "warlock", "hitPointRoll": 6,
     "selections": [ { "choiceKey": "warlock.invocations", "optionKey": "eldritch-invocation.agonizing-blast" } ],
     "replacements": [ { "selectionId": "…", "optionKey": "eldritch-invocation.eldritch-mind" } ] }
   ```
   Sending only `{ "classKey": "wizard" }` is fine too: the level is saved and its choices show up as `pendingChoices`; answers posted later with `POST /choices` belong to this level.
6. **Nested choices** such as the ASI feat: pick the feat, then its `ability-score-improvement.abilities` Distribution appears (or send both with `ref`/`parentRef`). Prerequisite-failing feats are hidden unless `includeDisabled=true`.

**Undo:** `DELETE /{id}/levels/last` removes the highest level with all its selections and reactivates what it replaced. Only selections of the latest level (or the origin before level 1) can be taken back individually (`removable` in `GET /selections`); for older ones offer "undo levels". Confirm before undoing.

## Playing with the sheet

- **Equipment:** slot changes and attunement immediately change AC, speed and violations (armor without training, more than 3 attuned items).
- **Armor Class:** the API picks the best formula; `PUT /armor-class { label }` lets the player choose another (labels like "Unarmored Defense (Monk)" come from the sheet), `{ label: null }` returns to the best.
- **Toggles:** `PUT /toggles/{featureKey}` for toggleable features and spell effects (Rage, Mage Armor, Shield).
- **Spells:** a Wizard prepares from the spellbook on a long rest (`PUT /prepared-spells/wizard { spellKeys }`) and copies scrolls with `POST /spellbook`; classes that change prepared spells on level up do it through choices. Rest swaps (`replacePolicy` `OnLongRest`/`OnShortRest`, e.g. a Wizard cantrip) use `POST /selections/{selectionId}/rest-replace`.
- **Violations** are not errors: list them with their severity (`Info`, `Warning`, `Error`) and keep the sheet usable.

## Checklist before handing back

- The API is running and the flows you touched work against it: register/login, create a draft, answer origin choices including a follow-up, level up (one subclass level, one ASI), undo, PDF download.
- No hard-coded rule data or choice keys in the UI.
- 401 sends the user to sign in; 422 messages are shown next to the right choice.
- If you changed the API (CORS, a new field): `dotnet build TableTopSheets.slnx` and `dotnet test Tests/TableTopSheets.Api.Tests` pass, the typed client is regenerated, and this file and the README's *REST API* section are updated.
