import { Button, Container, Flex, Heading, Stack, Text } from "@chakra-ui/react"
import { ColorModeButton, useColorMode } from "@/components/ui/color-mode"
import styles from "./App.module.scss"

export default function App() {
  const { colorMode, toggleColorMode } = useColorMode()

  return (
    <div className={styles.shell}>
      <header className={styles.header}>
        <Flex as="nav" align="center" justify="space-between" maxW="6xl" mx="auto" px="4" h="14">
          <Heading size="md" color="brand.fg">TableTopSheets</Heading>
          <ColorModeButton />
        </Flex>
      </header>

      <Container as="main" maxW="6xl" py="10" className={styles.main}>
        <Stack gap="4" align="flex-start">
          <Heading size="2xl">Character builder</Heading>
          <Text color="fg.muted">Create and level up D&amp;D 2024 characters and print their sheets.</Text>
          <Button colorPalette="brand" onClick={toggleColorMode}>
            Switch to {colorMode === "dark" ? "light" : "dark"} mode
          </Button>
          <div className={styles.card}>Styled with SCSS, using Chakra's color tokens.</div>
        </Stack>
      </Container>
    </div>
  )
}
