import { Badge } from '@chakra-ui/react'
import { Icon } from '@/components/Icon'
import { meleeIcon } from '@/components/icons'
import { useInspector } from '@/components/inspector/useInspector'
import type { MasteryStatus } from '@/components/mastery'

/**
 * Mastery property of a weapon (Nick, Sap, …) as a badge that opens its rule text; never triggers the row it sits on.
 * With a status it's colored when the character has mastery and gray otherwise
 */
export const MasteryBadge = ({
  masteryKey,
  name,
  status,
}: {
  masteryKey: string
  name: string
  /** whether the character can use it; left out where no character is involved */
  status?: MasteryStatus
}) => {
  const { inspect } = useInspector()
  const muted = status === 'not-mastered'
  return (
    <Badge
      asChild
      colorPalette={muted ? 'gray' : 'brand'}
      variant={muted ? 'outline' : 'subtle'}
      cursor='pointer'
      _hover={{ opacity: 0.8 }}
    >
      <button
        type='button'
        title={`Mastery: ${name}${muted ? ' (not mastered)' : ''}`}
        onClick={(event) => {
          event.stopPropagation()
          inspect({
            kind: 'mastery',
            key: masteryKey,
            status,
          })
        }}
      >
        <Icon path={meleeIcon} />
        {name}
      </button>
    </Badge>
  )
}
