import { Avatar, Box, Button, Flex, HStack, IconButton, Separator, Text } from '@chakra-ui/react'
import { NavLink, Outlet, useNavigate } from 'react-router'
import { GetCurrentUserIntent, useApi } from '@/api'
import { Icon } from '@/components/Icon'
import { charactersIcon, diceIcon, eyeIcon, eyeOffIcon, logoutIcon, newCharacterIcon } from '@/components/icons'
import { ColorModeButton } from '@/components/ui/color-mode'
import { Toaster } from '@/components/ui/toaster'
import { useIntentQuery } from '@/hooks/useIntentQuery'
import { useSettings } from './settings/useSettings'

/** Maximum width of the pages; wider screens center them (the title bar spans the screen, its content lines up) */
const PageWidth = '1400px'

/**
 * Title bar and page area. Signed out: logo and theme toggle only; signed in: navigation and the user section.
 */
export function AppShell() {
  const { accessToken } = useApi()

  return (
    <Flex
      direction='column'
      minH='100vh'
    >
      <Flex
        as='header'
        position='sticky'
        top='0'
        zIndex='sticky'
        align='center'
        gap={{
          base: '1',
          md: '4',
        }}
        h='14'
        px={{
          base: '3',
          md: `max(1.5rem, calc((100% - ${PageWidth}) / 2 + 1.5rem))`,
        }}
        borderBottomWidth='1px'
        bg='bg'
      >
        <NavLink to='/characters'>
          <HStack
            color='brand.fg'
            fontWeight='bold'
            fontSize='lg'
            whiteSpace='nowrap'
          >
            <Icon path={diceIcon} />
            {/* narrow screens: the icon alone, so the title bar keeps room for the actions */}
            <Text
              as='span'
              hideBelow={accessToken ? 'sm' : undefined}
            >
              TableTopSheets
            </Text>
          </HStack>
        </NavLink>
        {accessToken && <MainNavigation />}
        <Box flex='1' />
        {accessToken && <UserSection />}
        <ColorModeButton />
      </Flex>

      {/* wide screens: the pages keep a readable width, centered */}
      <Box
        as='main'
        flex='1'
        w='full'
        maxW={PageWidth}
        mx='auto'
      >
        <Outlet />
      </Box>
      <Toaster />
    </Flex>
  )
}

function MainNavigation() {
  const links = [
    {
      to: '/characters',
      label: 'Characters',
      icon: charactersIcon,
      end: true,
    },
    {
      to: '/characters/new',
      label: 'New character',
      icon: newCharacterIcon,
      end: false,
    },
  ]

  return (
    <HStack gap='1'>
      {links.map((link) => (
        <NavLink
          key={link.to}
          to={link.to}
          end={link.end}
        >
          {({ isActive }) => (
            <Button
              as='span'
              size='sm'
              variant={isActive ? 'subtle' : 'ghost'}
              px={{
                base: '2',
                md: '3',
              }}
            >
              <Icon path={link.icon} />
              <Text
                as='span'
                hideBelow='md'
              >
                {link.label}
              </Text>
            </Button>
          )}
        </NavLink>
      ))}
    </HStack>
  )
}

function UserSection() {
  const { signOut } = useApi()
  const navigate = useNavigate()
  const { showApiHints, setShowApiHints } = useSettings()
  const user = useIntentQuery(() => new GetCurrentUserIntent(), 'me')
  const name = user.data?.userName ?? ''

  return (
    <HStack gap='1'>
      <IconButton
        size='sm'
        variant='ghost'
        aria-label={showApiHints ? 'Hide API hints' : 'Show API hints'}
        title={showApiHints ? 'Hide API hints' : 'Show API hints'}
        onClick={() => setShowApiHints(!showApiHints)}
      >
        <Icon path={showApiHints ? eyeIcon : eyeOffIcon} />
      </IconButton>
      <Separator
        orientation='vertical'
        h='6'
        mx='1'
      />
      <Avatar.Root
        size='xs'
        colorPalette='brand'
        hideBelow='md'
      >
        <Avatar.Fallback name={name} />
      </Avatar.Root>
      <Box
        lineHeight='short'
        hideBelow='md'
      >
        <Text
          fontWeight='semibold'
          fontSize='sm'
        >
          {name}
        </Text>
        <Text
          fontSize='2xs'
          color='fg.muted'
        >
          Signed in
        </Text>
      </Box>
      <Button
        size='sm'
        variant='ghost'
        title='Log out'
        onClick={() => {
          signOut()
          navigate('/login')
        }}
      >
        <Icon path={logoutIcon} />
        <Text
          as='span'
          hideBelow='md'
        >
          Log out
        </Text>
      </Button>
    </HStack>
  )
}
