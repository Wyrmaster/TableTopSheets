// Formatting rules only (see ~/.claude/CLAUDE.md); code-quality rules live in .oxlintrc.json.
// `pnpm format` fixes what can be fixed automatically, `pnpm lint` reports the rest.
import stylistic from '@stylistic/eslint-plugin'
import tsParser from '@typescript-eslint/parser'
import oneMemberPerLine from './eslint/one-member-per-line.js'

// chop objects, interfaces and type literals with 3+ members; keep 1-2 inline when they fit
const chopFromThree = { minProperties: 3, multiline: true, consistent: true }
const chopWhenMultiline = { multiline: true, consistent: true }

export default [
  { ignores: ['dist/**', 'node_modules/**'] },
  {
    files: ['**/*.{ts,tsx}'],
    languageOptions: {
      parser: tsParser,
      parserOptions: { ecmaFeatures: { jsx: true } },
    },
    plugins: {
      '@stylistic': stylistic,
      local: { rules: { 'one-member-per-line': oneMemberPerLine } },
    },
    rules: {
      // `}` and `else` / `catch` / `finally` on separate lines
      '@stylistic/brace-style': ['error', 'stroustrup'],

      // single quotes wherever possible
      '@stylistic/quotes': ['error', 'single', { avoidEscape: true }],
      '@stylistic/jsx-quotes': ['error', 'prefer-single'],

      // chop long constructions
      '@stylistic/object-curly-newline': ['error', {
        ObjectExpression: chopFromThree,
        ObjectPattern: chopFromThree,
        TSTypeLiteral: chopFromThree,
        TSInterfaceBody: chopFromThree,
        ImportDeclaration: chopWhenMultiline,
        ExportDeclaration: chopWhenMultiline,
      }],
      '@stylistic/object-property-newline': ['error', { allowAllPropertiesOnSameLine: true }],
      'local/one-member-per-line': 'error',
      '@stylistic/newline-per-chained-call': ['error', { ignoreChainWithDepth: 2 }],
      '@stylistic/function-paren-newline': ['error', 'multiline-arguments'],
      '@stylistic/function-call-argument-newline': ['error', 'consistent'],
      '@stylistic/max-len': ['error', {
        code: 120,
        ignoreUrls: true,
        // a line that is only a string literal can't be chopped further
        ignorePattern: String.raw`^\s*(['"\x60]).*\1,?$`,
      }],

      // keep autofixed code tidy
      '@stylistic/indent': ['error', 2],
      '@stylistic/comma-dangle': ['error', 'always-multiline'],
      '@stylistic/no-trailing-spaces': 'error',
    },
  },
]
