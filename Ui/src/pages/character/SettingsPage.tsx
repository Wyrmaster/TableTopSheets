import {
  Button,
  Card,
  Flex,
  Heading,
  HStack,
  Stack,
  Text,
} from '@chakra-ui/react'
import { type ChangeEvent, useRef, useState } from 'react'
import {
  RemovePortraitIntent,
  SetPortraitIntent,
  UpdateCharacterIntent,
} from '@/api'
import { ApiHint } from '@/components/ApiHint'
import { CharacterAvatar } from '@/components/CharacterAvatar'
import type { CharacterDetails } from '@/components/characterDetails'
import { CharacterDetailsFields } from '@/components/CharacterDetailsFields'
import { Icon } from '@/components/Icon'
import { deleteIcon, portraitIcon } from '@/components/icons'
import { toaster } from '@/components/ui/toaster'
import { useApiAction } from '@/hooks/useApiAction'
import { portraitData } from './portrait'
import { useCharacter } from './useCharacter'

/**
 * Settings of a character after the creator: its portrait and the descriptive fields (name, alignment, appearance,
 * backstory)
 */
export const SettingsPage = () => {
  return (
    <Stack gap='4'>
      <Heading size='lg'>Settings</Heading>
      <PortraitCard />
      <DetailsCard />
      <ApiHint>
        <code>PUT /api/characters/{'{id}'}/portrait</code> with <code>{'{ data }'}</code> (base64 JPEG, PNG or WebP,
        at most 2 MB; scaled to 512 px here first), <code>DELETE /api/characters/{'{id}'}/portrait</code>, the picture
        is <code>GET /api/characters/{'{id}'}/portrait</code>; details = <code>PATCH /api/characters/{'{id}'}</code>
      </ApiHint>
    </Stack>
  )
}

const PortraitCard = () => {
  const {
    id,
    state,
    applyState,
  } = useCharacter()
  const { run, busy } = useApiAction()
  const input = useRef<HTMLInputElement>(null)

  const upload = async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    // the same file can be picked again after a removal
    event.target.value = ''
    if (!file) {
      return
    }

    let data: string
    try {
      data = await portraitData(file)
    }
    catch (error) {
      toaster.create({
        type: 'error',
        title: 'Picture not usable',
        description: error instanceof Error ? error.message : undefined,
      })
      return
    }

    const result = await run(new SetPortraitIntent(id, { data }), { success: 'Portrait saved' })
    if (result.ok) {
      applyState(result.data)
    }
  }

  const remove = async () => {
    const result = await run(new RemovePortraitIntent(id), { success: 'Portrait removed' })
    if (result.ok) {
      applyState(result.data)
    }
  }

  return (
    <Card.Root>
      <Card.Body gap='3'>
        <Heading size='md'>Portrait</Heading>
        <Flex
          wrap='wrap'
          gap='4'
          align='center'
        >
          <CharacterAvatar
            name={state.name}
            portraitUrl={state.portraitUrl}
            size='32'
          />
          <Stack gap='2'>
            <Text
              fontSize='sm'
              color='fg.muted'
            >
              Shown in the header and on the character list. Large pictures are scaled down before the upload.
            </Text>
            <HStack wrap='wrap'>
              <input
                ref={input}
                type='file'
                accept='image/jpeg,image/png,image/webp'
                hidden
                onChange={upload}
              />
              <Button
                size='sm'
                colorPalette='brand'
                loading={busy}
                onClick={() => input.current?.click()}
              >
                <Icon path={portraitIcon} />
                {state.portraitUrl ? 'Replace picture' : 'Upload picture'}
              </Button>
              {state.portraitUrl && (
                <Button
                  size='sm'
                  variant='outline'
                  colorPalette='red'
                  disabled={busy}
                  onClick={remove}
                >
                  <Icon path={deleteIcon} />
                  Remove
                </Button>
              )}
            </HStack>
          </Stack>
        </Flex>
      </Card.Body>
    </Card.Root>
  )
}

const DetailsCard = () => {
  const {
    id,
    state,
    applyState,
  } = useCharacter()
  const { run, busy } = useApiAction()
  const saved: CharacterDetails = {
    name: state.name,
    alignment: state.alignment,
    appearance: state.appearance,
    backstory: state.backstory,
  }
  const [edited, setEdited] = useState<CharacterDetails | null>(null)
  const details = edited ?? saved
  const changed = edited !== null && JSON.stringify(edited) !== JSON.stringify(saved)

  const save = async () => {
    // empty texts are sent as they are, so a field can be cleared (null would keep the old text)
    const result = await run(new UpdateCharacterIntent(id, {
      name: details.name.trim(),
      alignment: details.alignment,
      appearance: details.appearance,
      backstory: details.backstory,
    }), { success: 'Details saved' })
    if (result.ok) {
      applyState(result.data)
      setEdited(null)
    }
  }

  return (
    <Card.Root>
      <Card.Body gap='3'>
        <Heading size='md'>Details</Heading>
        <CharacterDetailsFields
          details={details}
          onChange={setEdited}
        />
        <HStack justify='end'>
          <Button
            variant='ghost'
            disabled={!changed || busy}
            onClick={() => setEdited(null)}
          >
            Reset
          </Button>
          <Button
            colorPalette='brand'
            loading={busy}
            disabled={!changed || !details.name.trim()}
            onClick={save}
          >
            Save
          </Button>
        </HStack>
      </Card.Body>
    </Card.Root>
  )
}
