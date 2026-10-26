# TableTopSheets UI

React front end of TableTopSheets: Vite, TypeScript, Chakra UI v3 and SCSS, managed with pnpm.

```bash
pnpm install
pnpm dev        # http://localhost:5173, /api is proxied to the API on http://localhost:5180
pnpm build      # type check and production build into dist/
pnpm lint       # oxlint
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

## Light and dark mode

next-themes stores the choice in `localStorage` (default: the OS preference) and sets `class="light"` or `class="dark"`
on `<html>`. Chakra's semantic tokens switch with it. In SCSS use Chakra's CSS variables
(`var(--chakra-colors-fg)`, `var(--chakra-colors-bg-subtle)`, `var(--chakra-colors-brand-solid)` …) so styles follow the
mode automatically; for mode specific rules use `:global(.dark) & { … }`.
