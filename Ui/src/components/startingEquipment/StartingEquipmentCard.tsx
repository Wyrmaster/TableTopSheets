import { Badge, Button, Card, Flex, Heading, HStack, Spinner, Stack, Text } from '@chakra-ui/react'
import { useState } from 'react'
import {
  type CharacterStateDto,
  GetStartingEquipmentIntent,
  type StartingEquipmentDto,
  type StartingEquipmentSourceEnum,
  type TakeStartingEquipmentDto,
  TakeStartingEquipmentIntent,
} from '@/api'
import { ApiHint } from '@/components/ApiHint'
import { useApiAction } from '@/hooks/useApiAction'
import { useIntentQuery } from '@/hooks/useIntentQuery'
import { equipmentComplete } from './startingEquipment'
import { StartingEquipmentPicker } from './StartingEquipmentPicker'

/**
 * Starting equipment of a saved character that wasn't taken yet (background, first class); renders nothing when all is
 * taken. `sources` limits it to some of them
 */
export function StartingEquipmentCard({
  state,
  onState,
  sources,
}: {
  state: CharacterStateDto
  onState: (state: CharacterStateDto) => void
  sources?: StartingEquipmentSourceEnum[]
}) {
  const open = (sources ?? ['Background', 'Class']).filter((source) => (source === 'Background'
    ? state.backgroundEquipment === null
    : state.level > 0 && state.classEquipment === null))
  const offers = useIntentQuery(
    () => (open.length ? new GetStartingEquipmentIntent(state.id) : null),
    `${state.id}|${state.level}|${state.backgroundEquipment}|${state.classEquipment}|${state.pendingChoices.length}`,
  )

  if (!open.length) {
    return null
  }

  return (
    <Card.Root>
      <Card.Body gap='4'>
        <Heading size='md'>Starting equipment</Heading>
        {offers.loading && !offers.data && <Spinner size='sm' />}
        {offers.data
          ?.filter((offer) => offer.taken === null && open.includes(offer.source))
          .map((offer) => (
            <Offer
              key={offer.source}
              characterId={state.id}
              offer={offer}
              onState={onState}
            />
          ))}
        <ApiHint>
          <code>GET …/starting-equipment</code>, <code>POST …/starting-equipment/{'{background|class}'}</code> with the
          alternative and the picks
        </ApiHint>
      </Card.Body>
    </Card.Root>
  )
}

function Offer({
  characterId,
  offer,
  onState,
}: {
  characterId: string
  offer: StartingEquipmentDto
  onState: (state: CharacterStateDto) => void
}) {
  const { run, busy } = useApiAction()
  const [value, setValue] = useState<TakeStartingEquipmentDto | null>(null)

  const take = async () => {
    if (!value) {
      return
    }

    const result = await run(
      new TakeStartingEquipmentIntent(characterId, offer.source, value),
      { success: `Starting equipment of ${offer.ownerName} added` },
    )
    if (result.ok) {
      onState(result.data)
    }
  }

  return (
    <Stack gap='3'>
      <HStack>
        <Text fontWeight='semibold'>{offer.ownerName}</Text>
        <Badge>{offer.source === 'Background' ? 'background' : 'class'}</Badge>
      </HStack>
      <StartingEquipmentPicker
        options={offer.options}
        value={value}
        onChange={setValue}
        allowLater={false}
      />
      <Flex justify='flex-end'>
        <Button
          size='sm'
          colorPalette='brand'
          loading={busy}
          disabled={!value || !equipmentComplete(offer.options, value)}
          onClick={take}
        >
          Take equipment
        </Button>
      </Flex>
    </Stack>
  )
}
