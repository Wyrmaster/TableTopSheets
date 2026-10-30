import { Box, Text } from '@chakra-ui/react'
import { useRandomScene } from '@/hooks/useRandomScene'

/**
 * A random scene (tavern, dungeon …) as a wide strip at the top of a page; renders nothing until the scenes are loaded
 */
export const SceneBanner = () => {
  const scene = useRandomScene()
  if (!scene) {
    return null
  }

  return (
    <Box
      position='relative'
      h={{
        base: '110px',
        md: '170px',
      }}
      rounded='xl'
      overflow='hidden'
      bg='bg.muted'
      bgImage={`url(${scene.imageUrl})`}
      bgSize='cover'
      backgroundPosition='center'
      role='img'
      aria-label={scene.title}
    >
      <Text
        position='absolute'
        right='3'
        bottom='2'
        fontSize='xs'
        color='white'
        textShadow='0 1px 3px rgba(0, 0, 0, 0.8)'
      >
        {scene.title}
      </Text>
    </Box>
  )
}
