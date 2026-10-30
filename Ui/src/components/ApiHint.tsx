import { Box } from '@chakra-ui/react'
import type { ReactNode } from 'react'
import { useSettings } from '@/app/settings/useSettings'

/**
 * Developer hint naming the API route behind a section; only shown when the "API hints" setting is on
 */
export const ApiHint = ({ children }: { children: ReactNode }) => {
  const { showApiHints } = useSettings()
  if (!showApiHints) {
    return null
  }

  return (
    <Box
      px='3'
      py='2'
      borderWidth='1px'
      borderStyle='dashed'
      borderColor='fg.subtle'
      rounded='md'
      color='fg.muted'
      fontSize='xs'
      css={{ '& code': { fontSize: 'xs' } }}
    >
      <b>API:</b> {children}
    </Box>
  )
}
