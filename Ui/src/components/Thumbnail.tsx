import { Image } from '@chakra-ui/react'

/**
 * Small square picture at the start of a list row (spell, item); nothing without a URL
 */
export function Thumbnail({ src }: { src?: string | null }) {
  if (!src) {
    return null
  }

  return (
    <Image
      src={src}
      alt=''
      boxSize='10'
      flexShrink='0'
      rounded='md'
      objectFit='cover'
      bg='bg.muted'
      loading='lazy'
    />
  )
}
