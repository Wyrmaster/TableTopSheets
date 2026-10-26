import {
  Badge,
  Box,
  Button,
  Card,
  Dialog,
  Flex,
  Heading,
  HStack,
  IconButton,
  Input,
  Portal,
  SimpleGrid,
  Spinner,
  Stack,
  Text,
} from '@chakra-ui/react'
import { useState } from 'react'
import { useNavigate } from 'react-router'
import { type CharacterSummaryDto, DeleteCharacterIntent, ListCharactersIntent, problemMessages } from '@/api'
import { ApiHint } from '@/components/ApiHint'
import { Icon } from '@/components/Icon'
import { deleteIcon } from '@/components/icons'
import { useApiAction } from '@/hooks/useApiAction'
import { useIntentQuery } from '@/hooks/useIntentQuery'

/**
 * The signed in user's characters; a card opens the character sheet (level 0 drafts open the creator)
 */
export function CharactersPage() {
  const [search, setSearch] = useState('')
  const characters = useIntentQuery(
    () => new ListCharactersIntent({
      search: search || undefined,
      sort: 'name',
      pageSize: 100,
    }),
    search,
  )

  return (
    <Stack
      gap='5'
      px={{
        base: '4',
        md: '6',
      }}
      py={{
        base: '5',
        md: '7',
      }}
    >
      <Flex
        wrap='wrap'
        gap='3'
        align='center'
        justify='space-between'
      >
        <Box>
          <Heading size='2xl'>Your characters</Heading>
          <Text color='fg.muted'>
            {characters.data ? `${characters.data.totalCount} characters · open one to see its sheet` : ' '}
          </Text>
        </Box>
        <Input
          maxW='260px'
          placeholder='Search…'
          value={search}
          onChange={(event) => setSearch(event.target.value)}
        />
      </Flex>

      {characters.problem && <Text color='fg.error'>{problemMessages(characters.problem).join(' ')}</Text>}
      {characters.loading && !characters.data && <Spinner />}
      {characters.data?.totalCount === 0 && (
        <Text color='fg.muted'>No characters yet – create one with “New character” in the title bar.</Text>
      )}

      <SimpleGrid
        minChildWidth='270px'
        gap='4'
      >
        {characters.data?.items.map((character) => (
          <CharacterCard
            key={character.id}
            character={character}
            onDeleted={characters.reload}
          />
        ))}
      </SimpleGrid>
      <ApiHint>
        <code>GET /api/characters?search=&amp;sort=name</code>, <code>DELETE /api/characters/{'{id}'}</code>
      </ApiHint>
    </Stack>
  )
}

function CharacterCard({ character, onDeleted }: { character: CharacterSummaryDto, onDeleted: () => void }) {
  const navigate = useNavigate()
  const classes = character.classes
    .map((entry) => `${entry.name} ${entry.level}${entry.subclass ? ` (${entry.subclass})` : ''}`)
    .join(' / ')

  return (
    <Card.Root
      cursor='pointer'
      _hover={{ borderColor: 'brand.solid' }}
      onClick={() => navigate(character.level ? `/characters/${character.id}` : `/characters/new/${character.id}`)}
    >
      <Card.Body gap='3'>
        <HStack gap='3'>
          <Flex
            boxSize='11'
            rounded='lg'
            align='center'
            justify='center'
            bg='brand.muted'
            color='brand.fg'
            fontSize='xl'
            fontWeight='bold'
          >
            {character.name[0]}
          </Flex>
          <Box flex='1'>
            <Heading size='md'>{character.name}</Heading>
            <Text
              fontSize='sm'
              color='fg.muted'
            >
              {character.species} · {character.background}
            </Text>
          </Box>
          <Badge>Lvl {character.level}</Badge>
          <DeleteButton
            character={character}
            onDeleted={onDeleted}
          />
        </HStack>
        <Text>{classes || 'no class yet'}</Text>
        {!character.level && <Badge
          colorPalette='brand'
          alignSelf='start'
        >
          Draft
        </Badge>}
      </Card.Body>
    </Card.Root>
  )
}

/**
 * Deletes the character after a confirmation; clicks inside never open the character (React events bubble out of the
 * dialog's portal to the card)
 */
function DeleteButton({ character, onDeleted }: { character: CharacterSummaryDto, onDeleted: () => void }) {
  const { run, busy } = useApiAction()

  const remove = async () => {
    const result = await run(new DeleteCharacterIntent(character.id), { success: `${character.name} deleted` })
    if (result.ok) {
      onDeleted()
    }
  }

  return (
    <Box onClick={(event) => event.stopPropagation()}>
      <Dialog.Root
        placement='center'
        role='alertdialog'
      >
        <Dialog.Trigger asChild>
          <IconButton
            size='sm'
            variant='ghost'
            colorPalette='red'
            aria-label={`Delete ${character.name}`}
            title='Delete character'
          >
            <Icon path={deleteIcon} />
          </IconButton>
        </Dialog.Trigger>
        <Portal>
          <Dialog.Backdrop />
          <Dialog.Positioner>
            <Dialog.Content>
              <Dialog.Header>
                <Dialog.Title>Delete {character.name}?</Dialog.Title>
              </Dialog.Header>
              <Dialog.Body>
                <Text color='fg.muted'>
                  The character with all its levels, choices, items and spells is removed. This can't be undone.
                </Text>
              </Dialog.Body>
              <Dialog.Footer>
                <Dialog.ActionTrigger asChild>
                  <Button variant='outline'>Cancel</Button>
                </Dialog.ActionTrigger>
                <Button
                  colorPalette='red'
                  loading={busy}
                  onClick={remove}
                >
                  Delete
                </Button>
              </Dialog.Footer>
            </Dialog.Content>
          </Dialog.Positioner>
        </Portal>
      </Dialog.Root>
    </Box>
  )
}
