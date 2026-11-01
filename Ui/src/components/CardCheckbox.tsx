import { Box, Checkbox } from '@chakra-ui/react'
import type { CardKind, CardSelection } from '@/hooks/useCardSelection'

/**
 * Checkbox of a list row in card selection mode (nothing outside it); its clicks don't reach the row, which toggles
 * the same entry on a click of its own
 */
export const CardCheckbox = ({
  selection,
  kind,
  entryKey,
  name,
}: {
  selection: CardSelection
  kind: CardKind
  /** spell or item key */
  entryKey: string
  name: string
}) => {
  if (!selection.active) {
    return null
  }

  return (
    <Box
      display='contents'
      onClick={(event) => event.stopPropagation()}
    >
      <Checkbox.Root
        colorPalette='brand'
        checked={selection.isSelected(kind, entryKey)}
        onCheckedChange={() => selection.toggle(kind, entryKey)}
      >
        <Checkbox.HiddenInput aria-label={`Select ${name} for cards`} />
        <Checkbox.Control />
      </Checkbox.Root>
    </Box>
  )
}
