import { SimpleGrid, Text } from '@chakra-ui/react'
import type { CharacterStateDto, PendingChoiceDto } from '@/api'
import { ChoiceCard } from './ChoiceCard'

/**
 * All pending choices of a character; follow-up choices appear after their parent was saved
 */
export const ChoiceList = ({
  characterId,
  choices,
  onState,
  empty = 'Nothing left to choose.',
}: {
  characterId: string
  choices: PendingChoiceDto[]
  onState: (state: CharacterStateDto) => void
  empty?: string
}) => {
  if (choices.length === 0) {
    return <Text color='fg.muted'>{empty}</Text>
  }

  return (
    <SimpleGrid
      minChildWidth='340px'
      gap='4'
      alignItems='start'
    >
      {choices.map((choice) => (
        <ChoiceCard
          key={`${choice.id}|${choice.selected}`}
          characterId={characterId}
          choice={choice}
          onState={onState}
        />
      ))}
    </SimpleGrid>
  )
}
