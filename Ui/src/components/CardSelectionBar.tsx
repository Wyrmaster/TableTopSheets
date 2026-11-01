import {
  Button,
  Flex,
  IconButton,
  Portal,
  Text,
  Tooltip,
} from '@chakra-ui/react'
import { CreateCardsPdfIntent } from '@/api'
import { Icon } from '@/components/Icon'
import { cardsIcon, pdfIcon } from '@/components/icons'
import { useApiAction } from '@/hooks/useApiAction'
import type { CardSelection } from '@/hooks/useCardSelection'

/**
 * "Select for cards" toggle of a spell or item list; in selection mode the number of selected entries, "Clear" and,
 * once something is selected, the "Card PDF" download (`POST /api/cards.pdf`)
 */
export const CardSelectionBar = ({
  selection,
  fileName = 'cards.pdf',
}: {
  selection: CardSelection
  /** name of the downloaded file, e.g. `Elara cards.pdf` */
  fileName?: string
}) => {
  const { run, busy } = useApiAction()
  const label = selection.active ? 'Stop selecting for cards' : 'Select for cards'

  const downloadPdf = async () => {
    const result = await run(new CreateCardsPdfIntent({
      spells: selection.spells,
      items: selection.items,
    }))
    if (result.ok) {
      const url = URL.createObjectURL(result.data)
      const link = document.createElement('a')
      link.href = url
      link.download = fileName
      link.click()
      URL.revokeObjectURL(url)
    }
  }

  return (
    <Flex
      wrap='wrap'
      gap='2'
      align='center'
      ms='auto'
    >
      {selection.active && (
        <>
          <Text
            fontSize='sm'
            color='fg.muted'
            aria-live='polite'
          >
            {selection.count} selected
          </Text>
          <Button
            variant='ghost'
            disabled={selection.count === 0}
            onClick={selection.clear}
          >
            Clear
          </Button>
          {selection.count > 0 && (
            <Button
              colorPalette='brand'
              loading={busy}
              onClick={downloadPdf}
            >
              <Icon path={pdfIcon} />
              Card PDF
            </Button>
          )}
        </>
      )}
      <Tooltip.Root openDelay={300}>
        <Tooltip.Trigger asChild>
          <IconButton
            variant={selection.active ? 'solid' : 'outline'}
            colorPalette={selection.active ? 'brand' : undefined}
            aria-label={label}
            aria-pressed={selection.active}
            onClick={() => selection.setActive(!selection.active)}
          >
            <Icon path={cardsIcon} />
          </IconButton>
        </Tooltip.Trigger>
        <Portal>
          <Tooltip.Positioner>
            <Tooltip.Content>{label}</Tooltip.Content>
          </Tooltip.Positioner>
        </Portal>
      </Tooltip.Root>
    </Flex>
  )
}
