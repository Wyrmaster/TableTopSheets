import { Icon as ChakraIcon, type IconProps as ChakraIconProps } from '@chakra-ui/react'

export interface IconProps extends ChakraIconProps {
  /** SVG path of a Material Design Icon (`…Icon` from `@/components/icons`) */
  path: string
}

/**
 * Material Design Icon as a Chakra icon (inherits color and size from the text)
 */
export const Icon = ({ path, ...props }: IconProps) => {
  // Chakra's Icon styles its child, so the svg element is passed in explicitly
  return (
    <ChakraIcon
      asChild
      boxSize='1.2em'
      {...props}
    >
      <svg
        viewBox='0 0 24 24'
        aria-hidden='true'
      >
        <path
          d={path}
          fill='currentColor'
        />
      </svg>
    </ChakraIcon>
  )
}
