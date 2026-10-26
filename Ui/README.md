# TableTopSheets UI

React front end of TableTopSheets: Vite, TypeScript, Chakra UI v3 and SCSS, managed with pnpm.

```bash
pnpm install
pnpm dev        # http://localhost:5173, /api is proxied to the API on http://localhost:5180
pnpm build      # type check and production build into dist/
pnpm lint       # oxlint (code rules) + eslint (code style)
pnpm format     # fix the code style automatically
pnpm test       # unit tests (vitest)
API_URL=http://localhost:5180 pnpm test api.integration   # end to end against a running API
```

Start the API with `dotnet run --project Api/TableTopSheets.Api` from the repository root.

## Layout

| Path | Purpose |
|---|---|
| `src/theme.ts` | Chakra system: tokens and semantic tokens (light / dark values), e.g. the `brand` color palette |
| `src/components/ui/provider.tsx` | `ChakraProvider` + color mode provider, wraps the app in `main.tsx` |
| `src/components/ui/color-mode.tsx` | Chakra CLI snippet: `useColorMode`, `useColorModeValue`, `ColorModeButton` (next-themes) |
| `src/styles/_variables.scss` | SCSS layout variables (`@use "./styles/variables" as *;`) |
| `src/styles/global.scss` | global styles not covered by Chakra's reset |
| `src/*.module.scss` | component styles as CSS modules |

## Mockups

`mockups/ui-mockups.html` shows the planned screens with mock data: login, character list, creator and a character
with **Character sheet** (Overview with abilities, saving throws, skills, hit points, clickable resource charges and
features · Inventory with armor, melee and ranged weapons, worn items and violations · Spells; short and long rest),
**Level up** and **Shop**. Every spell, species, background, class, feat, feature and item opens a detail view.
Icons are Material Design Icons (inline SVG). Open the file in a browser; the toolbar switches screens and desktop /
mobile (390 px phone frame), the title bar toggle switches light / dark. Dashed boxes note the API routes behind a
screen and the gaps (multiple equipped weapons, coins, removing items, play state). Links:
`?screen=character&page=sheet&tab=inventory&device=mobile`.

## Code style

`eslint.config.js` holds formatting rules only (`@stylistic/eslint-plugin` and the local rule in `eslint/`):

- `}` and `else` / `catch` / `finally` on separate lines
- single quotes (also in JSX attributes); double quotes only when the string contains a single quote
- objects, interfaces, type literals and destructurings with 3 or more members, chains with 3 or more calls and lines
  over 120 characters are chopped: one member / argument / call per line, trailing commas

Run `pnpm format` after editing; `pnpm lint` fails on what can't be fixed automatically (mostly long lines).

## Light and dark mode

next-themes stores the choice in `localStorage` (default: the OS preference) and sets `class="light"` or `class="dark"`
on `<html>`. Chakra's semantic tokens switch with it. In SCSS use Chakra's CSS variables
(`var(--chakra-colors-fg)`, `var(--chakra-colors-bg-subtle)`, `var(--chakra-colors-brand-solid)` …) so styles follow the
mode automatically; for mode specific rules use `:global(.dark) & { … }`.

## Network layer (`src/api`)

Components never call `fetch`. Every API route is an **intent**: an immutable class that describes method, path, query
and body, typed with the response. The **HttpClient** executes any intent and returns an `ApiResult`.

```tsx
import { GetCharacterIntent, ListSpellsIntent, problemMessages, useApi } from "@/api"

const { execute, setAccessToken } = useApi()

const result = await execute(new GetCharacterIntent(id), controller.signal)
if (result.ok) setState(result.data)                 // CharacterStateDto
else if (!result.aborted) show(problemMessages(result.problem))   // 401 / 404 / 422 …

const spells = await execute(new ListSpellsIntent({ class: "wizard", maxLevel: 1, sort: "level,name", page: 2 }))
```

| Path | Purpose |
|---|---|
| `core/Intent.ts` | `Intent<T>` (abstract), `BodyIntent<TBody, T>` for JSON bodies, `PagedIntent<TItem, TFilter>` for lists (`page`, `pageSize`, `search`, `sort` + filters) |
| `core/HttpClient.ts` | executes intents: query string, bearer token (unless `anonymous`), JSON / blob / no content, problem details; never throws |
| `core/ApiResult.ts` | `{ ok: true, status, data }` or `{ ok: false, status, problem, aborted? }`; `problemMessages` |
| `context/` | `ApiProvider` (one client, access token in `sessionStorage`, cleared on 401) and `useApi()` |
| `dtos/` | hand-written DTOs and enums; each file names the C# source it mirrors |
| `intents/` | one intent per route, named after the API's operation id (`CreateCharacterIntent`, `LevelUpIntent` …) |

- Large choices: `GetChoiceOptionsIntent.fromUrl(pendingChoice.optionsUrl, { page, school })`.
- Sign in: `execute(new LoginIntent({ userName, password }))`, then `setAccessToken(result.data.accessToken)`.
- **When the API changes:** update the DTO in `dtos/` (and its field list in `api.integration.test.ts`) and the intent in
  `intents/`, then run the integration test against the API; it compares every DTO with the real JSON.
