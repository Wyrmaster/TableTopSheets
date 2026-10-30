import { Image } from '@chakra-ui/react'

/**
 * Picture of a rule entity (spell, class, species, background), cropped to a fixed aspect ratio so the layout doesn't
 * jump while it loads
 */
export function RuleImage({
  src,
  alt,
  ratio = 16 / 9,
  position = 'center',
}: {
  src: string
  alt: string
  /** width / height of the frame */
  ratio?: number
  /** which part stays visible when cropped, e.g. `top` for portraits */
  position?: string
}) {
  return (
    <Image
      src={src}
      alt={alt}
      w='full'
      aspectRatio={ratio}
      objectFit='cover'
      objectPosition={position}
      rounded='md'
      bg='bg.muted'
      loading='lazy'
    />
  )
}
