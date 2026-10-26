import { Badge, Box, Button, Card, Flex, HStack, IconButton, Input, Spinner, Stack, Text } from '@chakra-ui/react'
import { useState } from 'react'
import {
  type CharacterStateDto,
  GetChoiceOptionsIntent,
  type OptionDto,
  type PendingChoiceDto,
  RemoveSelectionIntent,
  SelectOptionsIntent,
} from '@/api'
import { Icon } from '@/components/Icon'
import { checkIcon, closeIcon, minusIcon, plusIcon } from '@/components/icons'
import { InfoButton } from '@/components/InfoButton'
import type { InspectTarget } from '@/components/inspector/InspectorContext'
import { useApiAction } from '@/hooks/useApiAction'
import { useIntentQuery } from '@/hooks/useIntentQuery'

const PageSize = 20

const inputLabel = {
  SingleSelect: 'pick one',
  MultiSelect: 'pick several',
  Distribution: 'distribute',
}

/** Detail view of an option, when the option is a rule entity */
function inspectTarget(choice: PendingChoiceDto, option: OptionDto): InspectTarget | null {
  switch (choice.optionSource) {
    case 'Spell':
      return {
        kind: 'spell',
        key: option.key,
      }
    case 'Feat':
      return {
        kind: 'feat',
        key: option.key,
      }
    case 'Item':
    case 'Weapon':
      return {
        kind: 'item',
        key: option.key,
      }
    default:
      return option.description || option.summary
        ? {
          kind: 'feature',
          name: option.name,
          source: choice.name,
          description: option.description ?? option.summary ?? '',
        }
        : null
  }
}

/**
 * One pending choice, rendered from its metadata: SingleSelect (pick one), MultiSelect (pick several), Distribution
 * (counters). Large option lists are paged and searchable. Saving sends the picks with the choice's parentRef.
 */
export function ChoiceCard({
  characterId,
  choice,
  onState,
}: {
  characterId: string
  choice: PendingChoiceDto
  onState: (state: CharacterStateDto) => void
}) {
  const { run, busy } = useApiAction()
  const [picks, setPicks] = useState<Record<string, number>>({})
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)

  // embedded options for small choices, the paged options route for large ones (spells, feats …)
  const remote = useIntentQuery(
    () => (choice.options ? null : GetChoiceOptionsIntent.fromUrl(choice.optionsUrl, {
      search: search || undefined,
      page,
      pageSize: PageSize,
      includeDisabled: true,
      sort: 'available,name',
    })),
    `${choice.optionsUrl}|${search}|${page}`,
  )
  const options = choice.options ?? remote.data?.items ?? []
  const totalPages = remote.data?.totalPages ?? 1

  const picked = Object.values(picks).reduce((sum, count) => sum + count, 0)
  const single = choice.input === 'SingleSelect'
  const distribution = choice.input === 'Distribution'

  const toggle = (option: OptionDto) => {
    if (single) {
      setPicks(picks[option.key] ? {} : { [option.key]: 1 })
      return
    }

    const next = { ...picks }
    if (next[option.key]) {
      delete next[option.key]
    }
    else if (picked < choice.remaining) {
      next[option.key] = 1
    }
    setPicks(next)
  }

  const count = (option: OptionDto, delta: number) => {
    const value = Math.max(0, (picks[option.key] ?? 0) + delta)
    setPicks({
      ...picks,
      [option.key]: value,
    })
  }

  // one selection per pick; a +2 is the same option twice
  const save = async () => {
    const selections = Object.entries(picks).flatMap(([optionKey, times]) => Array.from({ length: times }, () => ({
      choiceKey: choice.choiceKey,
      optionKey,
      parentRef: choice.parentRef,
    })))
    const result = await run(new SelectOptionsIntent(characterId, { selections }), { success: `${choice.name} saved` })
    if (result.ok) {
      setPicks({})
      onState(result.data)
    }
  }

  const remove = async (selectionId: string) => {
    const result = await run(new RemoveSelectionIntent(characterId, selectionId))
    if (result.ok) {
      onState(result.data)
    }
  }

  return (
    <Card.Root>
      <Card.Body gap='3'>
        <Flex
          wrap='wrap'
          gap='2'
          align='center'
          justify='space-between'
        >
          <Box>
            <Text fontWeight='semibold'>{choice.name}</Text>
            <Text
              fontSize='sm'
              color='fg.muted'
            >
              {choice.source}
            </Text>
          </Box>
          <HStack>
            {choice.optional && <Badge>optional</Badge>}
            <Badge colorPalette='brand'>
              {picked} / {choice.remaining} · {inputLabel[choice.input]}
            </Badge>
          </HStack>
        </Flex>
        {choice.description && (
          <Text
            fontSize='sm'
            color='fg.muted'
          >
            {choice.description}
          </Text>
        )}
        {distribution && choice.constraints.allowedPatterns && (
          <Text fontSize='sm'>
            Allowed: {choice.constraints.allowedPatterns
              .map((pattern) => pattern.map((value) => `+${value}`).join('/'))
              .join(' or ')}
          </Text>
        )}

        {choice.selectedOptions.length > 0 && (
          <HStack wrap='wrap'>
            {choice.selectedOptions.map((selected) => (
              <Badge
                key={selected.selectionId}
                size='lg'
              >
                {selected.name}
                <IconButton
                  size='2xs'
                  variant='ghost'
                  aria-label={`Remove ${selected.name}`}
                  onClick={() => remove(selected.selectionId)}
                >
                  <Icon path={closeIcon} />
                </IconButton>
              </Badge>
            ))}
          </HStack>
        )}

        {!choice.options && (
          <Input
            size='sm'
            placeholder='Search…'
            value={search}
            onChange={(event) => {
              setSearch(event.target.value)
              setPage(1)
            }}
          />
        )}

        <Box
          borderWidth='1px'
          rounded='md'
          overflow='hidden'
        >
          {remote.loading && !choice.options && (
            <Box p='3'>
              <Spinner size='sm' />
            </Box>
          )}
          {options.map((option) => (
            <OptionRow
              key={option.key}
              option={option}
              count={picks[option.key] ?? 0}
              distribution={distribution}
              canAdd={picked < choice.remaining && (picks[option.key] ?? 0) < choice.constraints.maxPerOption}
              inspect={inspectTarget(choice, option)}
              onToggle={() => toggle(option)}
              onCount={(delta) => count(option, delta)}
            />
          ))}
          {!remote.loading && options.length === 0 && <Text
            p='3'
            color='fg.muted'
          >
            No options
          </Text>}
        </Box>

        <Flex
          wrap='wrap'
          gap='2'
          align='center'
        >
          {totalPages > 1 && (
            <HStack>
              <Button
                size='xs'
                variant='outline'
                disabled={page <= 1}
                onClick={() => setPage(page - 1)}
              >
                ‹
              </Button>
              <Text fontSize='sm'>{page} / {totalPages}</Text>
              <Button
                size='xs'
                variant='outline'
                disabled={page >= totalPages}
                onClick={() => setPage(page + 1)}
              >
                ›
              </Button>
            </HStack>
          )}
          <Box flex='1' />
          <Button
            size='sm'
            colorPalette='brand'
            loading={busy}
            disabled={picked === 0}
            onClick={save}
          >
            <Icon path={checkIcon} />
            Save
          </Button>
        </Flex>
      </Card.Body>
    </Card.Root>
  )
}

function OptionRow({
  option,
  count,
  distribution,
  canAdd,
  inspect,
  onToggle,
  onCount,
}: {
  option: OptionDto
  count: number
  distribution: boolean
  canAdd: boolean
  inspect: InspectTarget | null
  onToggle: () => void
  onCount: (delta: number) => void
}) {
  const meta = [
    typeof option.meta.level === 'number' && (option.meta.level === 0 ? 'cantrip' : `level ${option.meta.level}`),
    option.meta.school,
    option.meta.category,
    option.meta.ritual && 'ritual',
  ].filter(Boolean).join(' · ')
  const selected = count > 0

  return (
    <Flex
      align='center'
      gap='3'
      px='3'
      py='2'
      borderTopWidth='1px'
      _first={{ borderTopWidth: '0' }}
      bg={selected ? 'brand.subtle' : undefined}
      opacity={option.disabled ? 0.55 : 1}
      cursor={option.disabled || distribution ? 'default' : 'pointer'}
      onClick={option.disabled || distribution ? undefined : onToggle}
    >
      {!distribution && (
        <Box
          flexShrink='0'
          boxSize='4'
          rounded='full'
          borderWidth='2px'
          borderColor={selected ? 'brand.solid' : 'border.emphasized'}
          bg={selected ? 'brand.solid' : 'transparent'}
        />
      )}
      <Stack
        flex='1'
        gap='0'
      >
        <Text fontWeight='medium'>{option.name}</Text>
        {(option.summary || meta) && (
          <Text
            fontSize='sm'
            color='fg.muted'
            lineClamp={2}
          >
            {[meta, option.summary].filter(Boolean).join(' – ')}
          </Text>
        )}
        {option.grants.length > 0 && (
          <Text
            fontSize='xs'
            color='fg.muted'
          >
            Grants: {option.grants.join(', ')}
          </Text>
        )}
        {option.disabled && option.disabledReason && (
          <Text
            fontSize='xs'
            color='fg.error'
          >
            {option.disabledReason}
          </Text>
        )}
      </Stack>
      {distribution && (
        <HStack
          gap='1'
          borderWidth='1px'
          rounded='md'
        >
          <IconButton
            size='xs'
            variant='ghost'
            aria-label={`Less ${option.name}`}
            disabled={count === 0}
            onClick={() => onCount(-1)}
          >
            <Icon path={minusIcon} />
          </IconButton>
          <Text
            minW='5'
            textAlign='center'
            fontWeight='semibold'
          >
            {count}
          </Text>
          <IconButton
            size='xs'
            variant='ghost'
            aria-label={`More ${option.name}`}
            disabled={!canAdd || option.disabled}
            onClick={() => onCount(1)}
          >
            <Icon path={plusIcon} />
          </IconButton>
        </HStack>
      )}
      {inspect && <InfoButton target={inspect} />}
    </Flex>
  )
}
