import {
  Button,
  Card,
  Flex,
  Heading,
  HStack,
  NativeSelect,
  NumberInput,
  SimpleGrid,
  Stack,
  Text,
} from '@chakra-ui/react'
import { useState } from 'react'
import { type CoinsDto, PayCoinsIntent, SetCoinsIntent } from '@/api'
import { Icon } from '@/components/Icon'
import { bagIcon } from '@/components/icons'
import { useApiAction } from '@/hooks/useApiAction'
import { CoinKinds, coinValue, formatCoins, formatPrice } from './coins'
import { useCharacter } from './useCharacter'

/**
 * Coins of the character: edit all kinds at once, or pay an amount (coins are spent, change comes back)
 */
export function CoinsCard() {
  const {
    id,
    state,
    applyState,
  } = useCharacter()
  const { run, busy } = useApiAction()
  const [editing, setEditing] = useState<CoinsDto | null>(null)
  const [amount, setAmount] = useState(1)
  const [unit, setUnit] = useState(100)

  const save = async () => {
    if (!editing) {
      return
    }

    const result = await run(new SetCoinsIntent(id, editing), { success: 'Coins saved' })
    if (result.ok) {
      applyState(result.data)
      setEditing(null)
    }
  }

  const pay = async () => {
    const result = await run(
      new PayCoinsIntent(id, { amountCp: amount * unit }),
      { success: `Paid ${formatPrice(amount * unit)}` },
    )
    if (result.ok) {
      applyState(result.data)
    }
  }

  return (
    <Card.Root>
      <Card.Body gap='3'>
        <HStack justify='space-between'>
          <Heading size='md'>
            <HStack>
              <Icon path={bagIcon} />
              Coins
            </HStack>
          </Heading>
          <Text
            fontSize='xs'
            color='fg.muted'
          >
            worth {formatPrice(coinValue(state.coins))}
          </Text>
        </HStack>

        {!editing && (
          <Flex
            wrap='wrap'
            gap='3'
            align='center'
          >
            <Text
              flex='1'
              fontSize='lg'
              fontWeight='semibold'
            >
              {formatCoins(state.coins)}
            </Text>
            <Button
              size='xs'
              variant='outline'
              onClick={() => setEditing({ ...state.coins })}
            >
              Edit
            </Button>
          </Flex>
        )}

        {editing && (
          <Stack gap='3'>
            <SimpleGrid
              columns={{
                base: 2,
                md: 5,
              }}
              gap='2'
            >
              {CoinKinds.map((kind) => (
                <Stack
                  key={kind.key}
                  gap='1'
                >
                  <Text fontSize='xs'>{kind.label}</Text>
                  <NumberInput.Root
                    size='sm'
                    min={0}
                    value={String(editing[kind.key])}
                    onValueChange={(details) => setEditing({
                      ...editing,
                      [kind.key]: details.valueAsNumber || 0,
                    })}
                  >
                    <NumberInput.Input aria-label={kind.label} />
                  </NumberInput.Root>
                </Stack>
              ))}
            </SimpleGrid>
            <HStack justify='flex-end'>
              <Button
                size='xs'
                variant='outline'
                onClick={() => setEditing(null)}
              >
                Cancel
              </Button>
              <Button
                size='xs'
                colorPalette='brand'
                loading={busy}
                onClick={save}
              >
                Save coins
              </Button>
            </HStack>
          </Stack>
        )}

        <HStack
          wrap='wrap'
          gap='2'
          pt='2'
          borderTopWidth='1px'
        >
          <Text fontSize='sm'>Pay</Text>
          <NumberInput.Root
            size='sm'
            w='20'
            min={1}
            value={String(amount)}
            onValueChange={(details) => setAmount(details.valueAsNumber || 1)}
          >
            <NumberInput.Input aria-label='Amount to pay' />
          </NumberInput.Root>
          <NativeSelect.Root
            size='sm'
            w='20'
          >
            <NativeSelect.Field
              aria-label='Coin'
              value={String(unit)}
              onChange={(event) => setUnit(Number(event.target.value))}
            >
              <option value='100'>gp</option>
              <option value='10'>sp</option>
              <option value='1'>cp</option>
            </NativeSelect.Field>
            <NativeSelect.Indicator />
          </NativeSelect.Root>
          <Button
            size='sm'
            variant='outline'
            loading={busy}
            disabled={amount * unit > coinValue(state.coins)}
            onClick={pay}
          >
            Pay
          </Button>
          <Text
            fontSize='xs'
            color='fg.muted'
          >
            change comes back automatically
          </Text>
        </HStack>
      </Card.Body>
    </Card.Root>
  )
}
