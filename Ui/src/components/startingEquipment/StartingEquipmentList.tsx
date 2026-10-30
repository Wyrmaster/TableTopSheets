import { Heading, Stack, Text } from '@chakra-ui/react'
import type { StartingEquipmentOptionDto } from '@/api'
import { optionSummary } from './startingEquipment'

/**
 * The alternatives of a starting equipment as text ("A: Spear, 2 × Dagger … + 28 gp"); nothing when there are none
 */
export const StartingEquipmentList = ({ options }: { options: StartingEquipmentOptionDto[] }) => {
  if (!options.length) {
    return null
  }

  return (
    <Stack gap='1'>
      <Heading size='sm'>Starting equipment</Heading>
      {options.map((option) => (
        <Text
          key={option.key}
          fontSize='sm'
        >
          <Text
            as='span'
            fontWeight='semibold'
          >
            {option.key.toUpperCase()}:{' '}
          </Text>
          {optionSummary(option)}
        </Text>
      ))}
    </Stack>
  )
}
