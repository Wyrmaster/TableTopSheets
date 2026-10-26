import { Alert, Box, Button, Card, Field, Grid, Heading, Input, SegmentGroup, Stack, Text } from '@chakra-ui/react'
import { type FormEvent, useState } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router'
import { LoginIntent, type ProblemDetails, problemMessages, RegisterIntent, useApi } from '@/api'
import { ApiHint } from '@/components/ApiHint'

type Mode = 'signin' | 'register'

/**
 * Sign in or register with the database authentication of the API
 */
export function LoginPage() {
  const { accessToken, setAccessToken } = useApi()
  const { execute } = useApi()
  const navigate = useNavigate()
  const location = useLocation()
  const [mode, setMode] = useState<Mode>('signin')
  const [userName, setUserName] = useState('')
  const [password, setPassword] = useState('')
  const [email, setEmail] = useState('')
  const [problem, setProblem] = useState<ProblemDetails | null>(null)
  const [sending, setSending] = useState(false)

  if (accessToken) {
    return (
      <Navigate
        to='/characters'
        replace
      />
    )
  }

  const submit = async (event: FormEvent) => {
    event.preventDefault()
    setSending(true)
    setProblem(null)

    // 1. register or sign in; both answer with a token
    const intent = mode === 'register'
      ? new RegisterIntent({
        userName,
        password,
        email: email || null,
      })
      : new LoginIntent({ userName, password })
    const result = await execute(intent)
    setSending(false)
    if (!result.ok) {
      setProblem(result.problem)
      return
    }

    // 2. keep the token and go back to the page that sent the user here
    setAccessToken(result.data.accessToken)
    const from = (location.state as { from?: string } | null)?.from
    navigate(from && from !== '/login' ? from : '/characters', { replace: true })
  }

  const fieldErrors = (name: string) => problem?.errors?.[name]?.join(' ')

  return (
    <Grid
      templateColumns={{
        base: '1fr',
        md: '1.2fr 1fr',
      }}
      gap='8'
      px={{
        base: '4',
        md: '6',
      }}
      py={{
        base: '6',
        md: '14',
      }}
    >
      <Box
        hideBelow='md'
        display='grid'
        alignContent='end'
        gap='2'
        minH='440px'
        p='8'
        borderWidth='1px'
        rounded='xl'
        bgGradient='to-br'
        gradientFrom='brand.muted'
        gradientTo='bg.subtle'
      >
        <Heading size='2xl'>Build D&amp;D 2024 characters</Heading>
        <Text color='fg.muted'>
          Create a character step by step, level it up, manage the equipment and print the sheet.
        </Text>
      </Box>

      <Card.Root maxW='460px'>
        <Card.Body
          as='form'
          onSubmit={submit}
        >
          <Stack gap='4'>
            <SegmentGroup.Root
              value={mode}
              onValueChange={(details) => {
                setMode(details.value as Mode)
                setProblem(null)
              }}
            >
              <SegmentGroup.Indicator />
              <SegmentGroup.Items
                items={[
                  {
                    value: 'signin',
                    label: 'Sign in',
                  },
                  {
                    value: 'register',
                    label: 'Register',
                  },
                ]}
              />
            </SegmentGroup.Root>

            <Field.Root invalid={Boolean(fieldErrors('UserName'))}>
              <Field.Label>User name</Field.Label>
              <Input
                autoComplete='username'
                value={userName}
                onChange={(event) => setUserName(event.target.value)}
              />
              <Field.ErrorText>{fieldErrors('UserName')}</Field.ErrorText>
            </Field.Root>
            {mode === 'register' && (
              <Field.Root>
                <Field.Label>Email (optional)</Field.Label>
                <Input
                  type='email'
                  autoComplete='email'
                  value={email}
                  onChange={(event) => setEmail(event.target.value)}
                />
              </Field.Root>
            )}
            <Field.Root invalid={Boolean(fieldErrors('Password'))}>
              <Field.Label>Password</Field.Label>
              <Input
                type='password'
                autoComplete={mode === 'register' ? 'new-password' : 'current-password'}
                value={password}
                onChange={(event) => setPassword(event.target.value)}
              />
              {mode === 'register' && <Field.HelperText>At least 8 characters.</Field.HelperText>}
              <Field.ErrorText>{fieldErrors('Password')}</Field.ErrorText>
            </Field.Root>

            {problem && !problem.errors && (
              <Alert.Root status='error'>
                <Alert.Indicator />
                <Alert.Title>{problemMessages(problem).join(' ')}</Alert.Title>
              </Alert.Root>
            )}

            <Button
              type='submit'
              colorPalette='brand'
              loading={sending}
              disabled={!userName || !password}
            >
              {mode === 'register' ? 'Create account' : 'Sign in'}
            </Button>
            <ApiHint>
              <code>POST /api/auth/{mode === 'register' ? 'register' : 'login'}</code>; single sign-on (OIDC) shows a
              provider button instead when the API runs with <code>Authentication:Provider = Oidc</code>
            </ApiHint>
          </Stack>
        </Card.Body>
      </Card.Root>
    </Grid>
  )
}
