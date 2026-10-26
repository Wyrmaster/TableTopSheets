import { Box } from '@chakra-ui/react'
import { ProficiencyLevels } from '@/api'

/** Proficiency marker: empty, half, filled (proficient) or ringed (expertise) */
export function ProficiencyMarker({ level }: { level: number }) {
  const proficient = level >= ProficiencyLevels.Proficient
  return (
    <Box
      flexShrink='0'
      boxSize='2.5'
      rounded='full'
      borderWidth='1.5px'
      borderColor={level > 0 ? 'brand.solid' : 'fg.subtle'}
      bg={proficient ? 'brand.solid' : level > 0 ? 'brand.muted' : 'transparent'}
      outline={level >= ProficiencyLevels.Expertise ? '1.5px solid' : undefined}
      outlineColor='brand.solid'
      outlineOffset='2px'
    />
  )
}
