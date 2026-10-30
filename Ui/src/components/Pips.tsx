import { chakra, HStack } from '@chakra-ui/react'

/**
 * Clickable charges: a filled pip is available (click spends it), an empty one is spent (click restores it)
 */
export const Pips = ({
  max,
  used,
  onChange,
  square = false,
  label,
}: {
  max: number
  used: number
  onChange: (used: number) => void
  square?: boolean
  label: string
}) => {
  const available = max - used
  return (
    <HStack
      gap='1.5'
      wrap='wrap'
    >
      {Array.from({ length: max }, (_, index) => {
        const filled = index < available
        return (
          <chakra.button
            key={index}
            type='button'
            aria-label={`${label}: ${filled ? 'use one' : 'restore one'}`}
            title={filled ? 'Use' : 'Restore'}
            boxSize='4.5'
            borderWidth='2px'
            borderColor='brand.solid'
            rounded={square ? 'sm' : 'full'}
            bg={filled ? 'brand.solid' : 'transparent'}
            cursor='pointer'
            onClick={() => onChange(filled ? used + 1 : used - 1)}
          />
        )
      })}
    </HStack>
  )
}
