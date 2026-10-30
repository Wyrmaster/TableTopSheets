import { Box, NativeSelect, RadioGroup, Stack, Text } from '@chakra-ui/react'
import type { StartingEquipmentOptionDto, TakeStartingEquipmentDto } from '@/api'
import { openPicks, optionSummary } from './startingEquipment'

const Later = 'later'

/**
 * Choose an alternative of a starting equipment ("Choose A or B") and the items it leaves open (a Holy Symbol, a
 * Gaming Set). Without `allowLater` an alternative has to be chosen
 */
export const StartingEquipmentPicker = ({
  options,
  value,
  onChange,
  allowLater = true,
}: {
  options: StartingEquipmentOptionDto[]
  value: TakeStartingEquipmentDto | null
  onChange: (value: TakeStartingEquipmentDto | null) => void
  allowLater?: boolean
}) => {
  const selected = value && options.find((option) => option.key === value.optionKey)

  const choose = (key: string) => onChange(key === Later
    ? null
    : {
      optionKey: key,
      picks: {},
    })

  const pick = (entryKey: string, itemKey: string) => {
    if (!value) {
      return
    }

    const picks = { ...value.picks }
    if (itemKey) {
      picks[entryKey] = itemKey
    }
    else {
      delete picks[entryKey]
    }

    onChange({
      ...value,
      picks,
    })
  }

  return (
    <Stack gap='3'>
      <RadioGroup.Root
        value={value?.optionKey ?? (allowLater ? Later : '')}
        onValueChange={(details) => details.value && choose(details.value)}
        colorPalette='brand'
      >
        <Stack gap='2'>
          {options.map((option) => (
            <RadioGroup.Item
              key={option.key}
              value={option.key}
              alignItems='flex-start'
            >
              <RadioGroup.ItemHiddenInput />
              <RadioGroup.ItemIndicator mt='0.5' />
              <RadioGroup.ItemText>
                <Text
                  as='span'
                  fontWeight='semibold'
                >
                  {option.key.toUpperCase()}:{' '}
                </Text>
                {optionSummary(option)}
              </RadioGroup.ItemText>
            </RadioGroup.Item>
          ))}
          {allowLater && (
            <RadioGroup.Item value={Later}>
              <RadioGroup.ItemHiddenInput />
              <RadioGroup.ItemIndicator />
              <RadioGroup.ItemText color='fg.muted'>Decide later (inventory)</RadioGroup.ItemText>
            </RadioGroup.Item>
          )}
        </Stack>
      </RadioGroup.Root>

      {selected && openPicks(selected).map((entry) => (
        <Box
          key={entry.key}
          maxW='sm'
        >
          <Text
            fontSize='sm'
            fontWeight='medium'
            mb='1'
          >
            {entry.name}
          </Text>
          <NativeSelect.Root size='sm'>
            <NativeSelect.Field
              aria-label={entry.name}
              value={value?.picks?.[entry.key] ?? ''}
              onChange={(event) => pick(entry.key, event.target.value)}
            >
              <option value=''>Choose…</option>
              {entry.options.map((item) => (
                <option
                  key={item.key}
                  value={item.key}
                >
                  {item.name}
                </option>
              ))}
            </NativeSelect.Field>
            <NativeSelect.Indicator />
          </NativeSelect.Root>
          {entry.sameAs && (
            <Text
              fontSize='xs'
              color='fg.muted'
              mt='1'
            >
              The same as your tool proficiency – picking it here also makes that choice.
            </Text>
          )}
        </Box>
      ))}
    </Stack>
  )
}
