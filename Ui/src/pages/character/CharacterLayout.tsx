import {
  Badge,
  Box,
  Button,
  Flex,
  Grid,
  Heading,
  HStack,
  Popover,
  Portal,
  Spinner,
  Stack,
  Text,
} from '@chakra-ui/react'
import { useMemo } from 'react'
import { NavLink, Outlet, useLocation, useParams } from 'react-router'
import {
  type CharacterSheet,
  type CharacterStateDto,
  GetCharacterIntent,
  GetCharacterSheetIntent,
  GetCharacterSheetPdfIntent,
  problemMessages,
} from '@/api'
import { Icon } from '@/components/Icon'
import { alertIcon, levelUpIcon, pdfIcon, sheetIcon, shopIcon } from '@/components/icons'
import { useApiAction } from '@/hooks/useApiAction'
import { useIntentQuery } from '@/hooks/useIntentQuery'
import { CharacterContext, type CharacterContextValue } from './CharacterContext'
import { usePlayState } from './playState'

const pages = [
  {
    to: 'sheet/overview',
    match: 'sheet',
    label: 'Character sheet',
    short: 'Sheet',
    icon: sheetIcon,
  },
  {
    to: 'level-up',
    match: 'level-up',
    label: 'Level up',
    short: 'Level up',
    icon: levelUpIcon,
  },
  {
    to: 'shop',
    match: 'shop',
    label: 'Shop',
    short: 'Shop',
    icon: shopIcon,
  },
]

/**
 * Loads a character (state and sheet) and frames its pages: header with stats, Character sheet · Level up · Shop
 */
export const CharacterLayout = () => {
  const { id = '' } = useParams()
  const state = useIntentQuery(() => new GetCharacterIntent(id), id)
  const sheet = useIntentQuery(() => new GetCharacterSheetIntent(id), id)
  const { play, updatePlay } = usePlayState(id)

  const context = useMemo<CharacterContextValue | null>(() => {
    if (!state.data || !sheet.data) {
      return null
    }

    return {
      id,
      state: state.data,
      sheet: sheet.data,
      applyState: (next: CharacterStateDto) => {
        state.setData(next)
        sheet.reload()
      },
      play,
      updatePlay,
    }
  }, [id, state, sheet, play, updatePlay])

  const problem = state.problem ?? sheet.problem
  if (problem) {
    return (
      <Box p='6'>
        <Text color='fg.error'>{problemMessages(problem).join(' ')}</Text>
      </Box>
    )
  }

  if (!context) {
    return (
      <Box p='6'>
        <Spinner />
      </Box>
    )
  }

  return (
    <CharacterContext.Provider value={context}>
      <Header
        state={context.state}
        sheet={context.sheet}
        hitPoints={play.hitPoints ?? context.sheet.hitPointMaximum}
      />
      <Box
        px={{
          base: '4',
          md: '6',
        }}
        py={{
          base: '4',
          md: '6',
        }}
        pb={{
          base: '24',
          md: '10',
        }}
      >
        <Outlet />
      </Box>
      <MobileNavigation />
    </CharacterContext.Provider>
  )
}

const Header = ({
  state,
  sheet,
  hitPoints,
}: {
  state: CharacterStateDto,
  sheet: CharacterSheet,
  hitPoints: number
}) => {
  const { run, busy } = useApiAction()
  const { pathname } = useLocation()
  const violations = state.violations.filter((violation) => violation.code !== 'choice.pending')
  const open = state.pendingChoices.filter((choice) => !choice.optional).length
  const classes = state.classes
    .map((entry) => `${entry.name} ${entry.level}${entry.subclass ? ` (${entry.subclass})` : ''}`)
    .join(' / ')
  const stats: [string | number, string][] = [
    [sheet.armorClass, 'AC'],
    [`${hitPoints}/${sheet.hitPointMaximum}`, 'HP'],
    [`+${sheet.proficiencyBonus}`, 'PB'],
    [sheet.speed, 'Speed'],
    [sheet.initiative >= 0 ? `+${sheet.initiative}` : sheet.initiative, 'Init'],
    [sheet.passivePerception, 'Pass. perc.'],
    [sheet.level, 'Level'],
  ]

  const downloadPdf = async () => {
    const result = await run(new GetCharacterSheetPdfIntent(state.id))
    if (result.ok) {
      const url = URL.createObjectURL(result.data)
      const link = document.createElement('a')
      link.href = url
      link.download = `${state.name}.pdf`
      link.click()
      URL.revokeObjectURL(url)
    }
  }

  return (
    <Stack
      gap='4'
      px={{
        base: '4',
        md: '6',
      }}
      pt={{
        base: '4',
        md: '5',
      }}
      borderBottomWidth='1px'
    >
      <Flex
        wrap='wrap'
        gap='3'
        align='center'
      >
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
          {state.name[0]}
        </Flex>
        <Box
          flex='1'
          minW='180px'
        >
          <Heading size='xl'>{state.name}</Heading>
          <Text
            fontSize='sm'
            color='fg.muted'
          >
            {[state.species, state.background, classes].filter(Boolean).join(' · ')}
          </Text>
        </Box>
        {violations.length > 0 && (
          <Badge
            colorPalette={violations.some((violation) => violation.severity === 'Error') ? 'red' : 'yellow'}
            title={violations.map((violation) => violation.message).join('\n')}
          >
            <Icon path={alertIcon} />
            {violations.length} violation{violations.length > 1 ? 's' : ''}
          </Badge>
        )}
        {open > 0 && (
          <NavLink to='level-up'>
            <Badge colorPalette='brand'>{open} open choice{open > 1 ? 's' : ''}</Badge>
          </NavLink>
        )}
        <Button
          size='sm'
          variant='outline'
          loading={busy}
          onClick={downloadPdf}
        >
          <Icon path={pdfIcon} />
          <Text
            as='span'
            hideBelow='md'
          >
            Sheet PDF
          </Text>
        </Button>
      </Flex>

      <HStack
        gap='2'
        overflowX='auto'
        pb='1'
      >
        {stats.map(([value, label]) => label === 'AC'
          ? (
            <ArmorClassStat
              key={label}
              sheet={sheet}
            />
          )
          : (
            <Stat
              key={label}
              value={value}
              label={label}
            />
          ))}
      </HStack>

      <HStack
        gap='1'
        hideBelow='md'
      >
        {pages.map((page) => (
          <NavLink
            key={page.to}
            to={page.to}
          >
            {({ isActive }) => <PageTab
              active={isActive || pathname.includes(`/${page.match}`)}
              icon={page.icon}
              label={page.label}
            />}
          </NavLink>
        ))}
      </HStack>
    </Stack>
  )
}

const PageTab = ({
  active,
  icon,
  label,
}: {
  active: boolean,
  icon: string,
  label: string
}) => {
  return (
    <HStack
      px='4'
      py='2.5'
      borderBottomWidth='2px'
      borderColor={active ? 'brand.solid' : 'transparent'}
      color={active ? 'fg' : 'fg.muted'}
      fontWeight='medium'
    >
      <Icon path={icon} />
      {label}
    </HStack>
  )
}

const MobileNavigation = () => {
  const { pathname } = useLocation()
  return (
    <Grid
      hideFrom='md'
      position='fixed'
      bottom='0'
      left='0'
      right='0'
      zIndex='sticky'
      templateColumns='repeat(3, 1fr)'
      borderTopWidth='1px'
      bg='bg'
    >
      {pages.map((page) => (
        <NavLink
          key={page.to}
          to={page.to}
        >
          {({ isActive }) => {
            const active = isActive || pathname.includes(`/${page.match}`)
            return (
              <Stack
                align='center'
                gap='0.5'
                py='2'
                fontSize='xs'
                color={active ? 'brand.fg' : 'fg.muted'}
                fontWeight={active ? 'semibold' : 'normal'}
              >
                <Icon
                  path={page.icon}
                  boxSize='6'
                />
                {page.short}
              </Stack>
            )
          }}
        </NavLink>
      ))}
    </Grid>
  )
}

const Stat = ({ value, label }: { value: string | number, label: string }) => {
  return (
    <Grid
      placeItems='center'
      minW='70px'
      px='3'
      py='2'
      borderWidth='1px'
      rounded='md'
      bg='bg.subtle'
    >
      <Text
        fontSize='xl'
        fontWeight='bold'
        lineHeight='short'
      >
        {value}
      </Text>
      <Text
        fontSize='2xs'
        color='fg.muted'
        textTransform='uppercase'
        whiteSpace='nowrap'
      >
        {label}
      </Text>
    </Grid>
  )
}

/** Armor Class with its breakdown: the calculation the current equipment uses, then every bonus */
const ArmorClassStat = ({ sheet }: { sheet: CharacterSheet }) => {
  const signed = (value: number) => (value >= 0 ? `+${value}` : `${value}`)
  const breakdown = sheet.armorClassSources
    .map((source, index) => `${source.label} ${index === 0 ? source.value : signed(source.value)}`)
    .join(' · ')

  return (
    <Popover.Root positioning={{ placement: 'bottom-start' }}>
      <Popover.Trigger asChild>
        <Box
          as='button'
          cursor='pointer'
          title={breakdown}
        >
          <Stat
            value={sheet.armorClass}
            label='AC'
          />
        </Box>
      </Popover.Trigger>
      <Portal>
        <Popover.Positioner>
          <Popover.Content width='auto'>
            <Popover.Arrow />
            <Popover.Body>
              <Stack gap='1'>
                {sheet.armorClassSources.map((source, index) => (
                  <HStack
                    key={source.label}
                    justify='space-between'
                    gap='6'
                    fontSize='sm'
                  >
                    <Text>{source.label}</Text>
                    <Text fontWeight='medium'>{index === 0 ? source.value : signed(source.value)}</Text>
                  </HStack>
                ))}
                <HStack
                  justify='space-between'
                  gap='6'
                  fontSize='sm'
                  fontWeight='bold'
                  borderTopWidth='1px'
                  pt='1'
                >
                  <Text>Armor Class</Text>
                  <Text>{sheet.armorClass}</Text>
                </HStack>
              </Stack>
            </Popover.Body>
          </Popover.Content>
        </Popover.Positioner>
      </Portal>
    </Popover.Root>
  )
}
