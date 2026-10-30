import {
  Button,
  Card,
  Flex,
  Heading,
  HStack,
  Image,
  NativeSelect,
  NumberInput,
  SimpleGrid,
  Stack,
  Text,
} from '@chakra-ui/react'
import { useState } from 'react'
import {
  ChangeCoinsIntent,
  coinImageUrl,
  type CoinsDto,
  PayCoinsIntent,
  SetCoinsIntent,
} from '@/api'
import { Icon } from '@/components/Icon'
import { bagIcon } from '@/components/icons'
import { useApiAction } from '@/hooks/useApiAction'
import { CoinKinds, coinValue, formatPrice } from './coins'
import { useCharacter } from './useCharacter'

/**
 * Coins of the character (platinum, gold, electrum, silver, copper): edit all kinds at once, add or remove coins of one
 * kind (treasure, spending exact coins), or pay an amount (coins are spent, change comes back)
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
  const [kindKey, setKindKey] = useState<keyof CoinsDto>('gold')
  const kind = CoinKinds.find((entry) => entry.key === kindKey) ?? CoinKinds[1]

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
      new PayCoinsIntent(id, { amountCp: amount * kind.value }),
      { success: `Paid ${formatPrice(amount * kind.value)}` },
    )
    if (result.ok) {
      applyState(result.data)
    }
  }

  // exactly these coins: +/- of one kind, no change
  const change = async (sign: 1 | -1) => {
    const result = await run(
      new ChangeCoinsIntent(id, { [kind.key]: sign * amount }),
      { success: `${sign > 0 ? 'Added' : 'Removed'} ${amount} ${kind.short}` },
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
            <Flex
              flex='1'
              wrap='wrap'
              gap='3'
              align='center'
              fontSize='lg'
              fontWeight='semibold'
            >
              {CoinKinds
                .filter((entry) => state.coins[entry.key])
                .map((entry) => (
                  <HStack
                    key={entry.key}
                    gap='1.5'
                    title={entry.label}
                  >
                    <CoinImage kind={entry.key} />
                    {state.coins[entry.key]} {entry.short}
                  </HStack>
                ))}
              {coinValue(state.coins) === 0 && 'no coins'}
            </Flex>
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
                  <HStack gap='1.5'>
                    <CoinImage
                      kind={kind.key}
                      size='5'
                    />
                    <Text fontSize='xs'>{kind.label}</Text>
                  </HStack>
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
          <NumberInput.Root
            size='sm'
            w='20'
            min={1}
            value={String(amount)}
            onValueChange={(details) => setAmount(details.valueAsNumber || 1)}
          >
            <NumberInput.Input aria-label='Amount' />
          </NumberInput.Root>
          <NativeSelect.Root
            size='sm'
            w='28'
          >
            <NativeSelect.Field
              aria-label='Coin'
              value={kind.key}
              onChange={(event) => setKindKey(event.target.value as keyof CoinsDto)}
            >
              {CoinKinds.map((entry) => (
                <option
                  key={entry.key}
                  value={entry.key}
                >
                  {entry.label} ({entry.short})
                </option>
              ))}
            </NativeSelect.Field>
            <NativeSelect.Indicator />
          </NativeSelect.Root>
          <Button
            size='sm'
            variant='outline'
            loading={busy}
            title={`Add ${amount} ${kind.short} (found, earned)`}
            onClick={() => change(1)}
          >
            Add
          </Button>
          <Button
            size='sm'
            variant='outline'
            loading={busy}
            disabled={amount > state.coins[kind.key]}
            title={`Remove exactly ${amount} ${kind.short}`}
            onClick={() => change(-1)}
          >
            Remove
          </Button>
          <Button
            size='sm'
            variant='outline'
            loading={busy}
            disabled={amount * kind.value > coinValue(state.coins)}
            title='Pay the amount with any coins; change comes back'
            onClick={pay}
          >
            Pay
          </Button>
          <Text
            fontSize='xs'
            color='fg.muted'
          >
            Pay uses any coins and returns change</Text>
        </HStack>
      </Card.Body>
    </Card.Root>
  )
}

/**
 * Picture of a coin kind; hidden when the API has none
 */
function CoinImage({ kind, size = '7' }: { kind: keyof CoinsDto, size?: string }) {
  const [failed, setFailed] = useState(false)
  if (failed) {
    return null
  }

  return (
    <Image
      src={coinImageUrl(kind)}
      alt=''
      boxSize={size}
      flexShrink='0'
      rounded='full'
      objectFit='cover'
      onError={() => setFailed(true)}
    />
  )
}
