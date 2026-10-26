"use client"

import { ChakraProvider } from "@chakra-ui/react"
import { system } from "@/theme"
import {
  ColorModeProvider,
  type ColorModeProviderProps,
} from "./color-mode"

export function Provider(props: ColorModeProviderProps) {
  return (
    <ChakraProvider value={system}>
      {/* next-themes' inline script only matters for server rendering; as a data block React doesn't warn about it */}
      <ColorModeProvider scriptProps={{ type: "application/json" }} {...props} />
    </ChakraProvider>
  )
}
