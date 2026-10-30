import { createSystem, defaultConfig, defineConfig } from '@chakra-ui/react'

// the one place to customize Chakra: tokens, semantic tokens (light / dark values) and recipes.
// Light mode keeps Chakra's defaults; dark mode uses a slate blue-gray instead of black and a calmer red, so the brand
// color doesn't glare (white on brand.solid and brand.fg on the backgrounds stay above 4.5:1)
const config = defineConfig({
  theme: {
    semanticTokens: {
      colors: {
        bg: {
          DEFAULT: { value: { _light: '{colors.white}', _dark: '#1a2030' } },
          subtle: { value: { _light: '{colors.gray.50}', _dark: '#1f2636' } },
          panel: { value: { _light: '{colors.white}', _dark: '#232a3b' } },
          muted: { value: { _light: '{colors.gray.100}', _dark: '#2c3447' } },
          emphasized: { value: { _light: '{colors.gray.200}', _dark: '#374055' } },
        },
        fg: {
          DEFAULT: { value: { _light: '{colors.black}', _dark: '#e8ecf3' } },
          muted: { value: { _light: '{colors.gray.600}', _dark: '#9aa4b5' } },
          subtle: { value: { _light: '{colors.gray.400}', _dark: '#7a8496' } },
        },
        border: {
          DEFAULT: { value: { _light: '{colors.gray.200}', _dark: '#374055' } },
          muted: { value: { _light: '{colors.gray.100}', _dark: '#2c3447' } },
          subtle: { value: { _light: '{colors.gray.50}', _dark: '#252c3d' } },
          emphasized: { value: { _light: '{colors.gray.300}', _dark: '#465069' } },
        },
        // the gray palette (default badges, outline and ghost buttons) on the slate too
        gray: {
          fg: { value: { _light: '{colors.gray.800}', _dark: '#d5dbe6' } },
          subtle: { value: { _light: '{colors.gray.100}', _dark: '#2c3447' } },
          muted: { value: { _light: '{colors.gray.200}', _dark: '#374055' } },
          emphasized: { value: { _light: '{colors.gray.300}', _dark: '#465069' } },
          border: { value: { _light: '{colors.gray.200}', _dark: '#374055' } },
        },
        brand: {
          solid: { value: { _light: '{colors.red.700}', _dark: '#bd5258' } },
          contrast: { value: { _light: 'white', _dark: 'white' } },
          fg: { value: { _light: '{colors.red.700}', _dark: '#eb9a9c' } },
          muted: { value: { _light: '{colors.red.100}', _dark: '#4a2f3c' } },
          subtle: { value: { _light: '{colors.red.50}', _dark: '#3a2a36' } },
          emphasized: { value: { _light: '{colors.red.200}', _dark: '#5a3644' } },
          focusRing: { value: { _light: '{colors.red.700}', _dark: '#d9787c' } },
        },
      },
    },
  },
})

export const system = createSystem(defaultConfig, config)
