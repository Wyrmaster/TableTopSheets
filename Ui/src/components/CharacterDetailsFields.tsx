import {
  Field,
  Grid,
  Input,
  NativeSelect,
  Stack,
  Textarea,
} from '@chakra-ui/react'
import { Alignments, type AlignmentEnum } from '@/api'
import { alignmentName, type CharacterDetails } from './characterDetails'

/**
 * Name, alignment, appearance and backstory of a character
 */
export const CharacterDetailsFields = ({
  details,
  onChange,
}: {
  details: CharacterDetails
  onChange: (details: CharacterDetails) => void
}) => {
  const set = (change: Partial<CharacterDetails>) => onChange({ ...details, ...change })

  return (
    <Stack gap='4'>
      <Grid
        templateColumns={{
          base: '1fr',
          md: '1fr 1fr',
        }}
        gap='4'
      >
        <Field.Root>
          <Field.Label>Name</Field.Label>
          <Input
            value={details.name}
            placeholder='Character name'
            onChange={(event) => set({ name: event.target.value })}
          />
        </Field.Root>
        <Field.Root>
          <Field.Label>Alignment</Field.Label>
          <NativeSelect.Root>
            <NativeSelect.Field
              value={details.alignment}
              onChange={(event) => set({ alignment: event.target.value as AlignmentEnum })}
            >
              {Alignments.map((value) => <option
                key={value}
                value={value}
              >
                {alignmentName(value)}
              </option>)}
            </NativeSelect.Field>
            <NativeSelect.Indicator />
          </NativeSelect.Root>
        </Field.Root>
      </Grid>
      <Field.Root>
        <Field.Label>Appearance</Field.Label>
        <Textarea
          value={details.appearance}
          autoresize
          onChange={(event) => set({ appearance: event.target.value })}
        />
      </Field.Root>
      <Field.Root>
        <Field.Label>Backstory</Field.Label>
        <Textarea
          value={details.backstory}
          autoresize
          minH='32'
          onChange={(event) => set({ backstory: event.target.value })}
        />
      </Field.Root>
    </Stack>
  )
}
