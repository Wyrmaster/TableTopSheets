import { Flex, Image } from '@chakra-ui/react'

/**
 * Picture of a character; without one the first letter of its name on a brand coloured square
 */
export const CharacterAvatar = ({
  name,
  portraitUrl,
  size = '11',
}: {
  name: string
  portraitUrl: string | null
  /** edge length (Chakra size token) */
  size?: string
}) => {
  if (portraitUrl) {
    return (
      <Image
        src={portraitUrl}
        alt={name}
        boxSize={size}
        flexShrink='0'
        rounded='lg'
        objectFit='cover'
        bg='bg.muted'
      />
    )
  }

  return (
    <Flex
      boxSize={size}
      flexShrink='0'
      rounded='lg'
      align='center'
      justify='center'
      bg='brand.muted'
      color='brand.fg'
      fontSize='xl'
      fontWeight='bold'
    >
      {name[0]}
    </Flex>
  )
}
