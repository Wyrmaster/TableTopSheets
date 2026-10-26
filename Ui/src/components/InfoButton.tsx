import { IconButton } from '@chakra-ui/react'
import { Icon } from '@/components/Icon'
import { infoIcon } from '@/components/icons'
import type { InspectTarget } from '@/components/inspector/InspectorContext'
import { useInspector } from '@/components/inspector/useInspector'

/**
 * Opens the detail view; never triggers the card or row it sits on
 */
export function InfoButton({ target }: { target: InspectTarget }) {
  const { inspect } = useInspector()
  return (
    <IconButton
      size='xs'
      variant='ghost'
      rounded='full'
      aria-label='Details'
      title='Details'
      color='fg.muted'
      onClick={(event) => {
        event.stopPropagation()
        inspect(target)
      }}
    >
      <Icon path={infoIcon} />
    </IconButton>
  )
}
