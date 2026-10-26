import { createSystem, defaultConfig, defineConfig } from "@chakra-ui/react"

// the one place to customize Chakra: tokens, semantic tokens (light / dark values) and recipes
const config = defineConfig({
  theme: {
    semanticTokens: {
      colors: {
        brand: {
          solid: { value: { _light: "{colors.red.700}", _dark: "{colors.red.400}" } },
          contrast: { value: { _light: "white", _dark: "{colors.gray.950}" } },
          fg: { value: { _light: "{colors.red.700}", _dark: "{colors.red.300}" } },
          muted: { value: { _light: "{colors.red.100}", _dark: "{colors.red.900}" } },
          subtle: { value: { _light: "{colors.red.50}", _dark: "{colors.red.950}" } },
          emphasized: { value: { _light: "{colors.red.200}", _dark: "{colors.red.800}" } },
          focusRing: { value: { _light: "{colors.red.700}", _dark: "{colors.red.400}" } },
        },
      },
    },
  },
})

export const system = createSystem(defaultConfig, config)
