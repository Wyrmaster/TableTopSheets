import { Box, CloseButton } from '@chakra-ui/react'
import { type ReactNode, useEffect, useMemo, useRef, useState } from 'react'
import { InspectorContent } from './InspectorContent'
import { InspectorContext, type InspectorContextValue, type InspectTarget } from './InspectorContext'

/**
 * Detail view for rule entities: a floating panel (desktop) or bottom sheet (mobile) without backdrop. It is fixed to
 * the window, so it opens where the user is, and closes on a click outside or Escape.
 */
export const InspectorProvider = ({ children }: { children: ReactNode }) => {
  const [target, setTarget] = useState<InspectTarget | null>(null)
  const panel = useRef<HTMLDivElement>(null)

  const value = useMemo<InspectorContextValue>(() => ({
    inspect: setTarget,
    close: () => setTarget(null),
  }), [])

  useEffect(() => {
    if (!target) {
      return
    }

    // a press outside the panel closes it; a press on another inspect trigger opens that one right after
    const onPointerDown = (event: PointerEvent) => {
      if (!panel.current?.contains(event.target as Node)) {
        setTarget(null)
      }
    }
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        setTarget(null)
      }
    }
    document.addEventListener('pointerdown', onPointerDown)
    document.addEventListener('keydown', onKeyDown)
    return () => {
      document.removeEventListener('pointerdown', onPointerDown)
      document.removeEventListener('keydown', onKeyDown)
    }
  }, [target])

  return (
    <InspectorContext.Provider value={value}>
      {children}
      {target && (
        <Box
          ref={panel}
          role='dialog'
          aria-label='Details'
          position='fixed'
          zIndex='modal'
          bg='bg'
          borderWidth='1px'
          shadow='lg'
          overflowY='auto'
          p='5'
          top={{
            base: 'auto',
            md: '72px',
          }}
          right={{
            base: '0',
            md: '4',
          }}
          bottom={{
            base: '0',
            md: 'auto',
          }}
          left={{
            base: '0',
            md: 'auto',
          }}
          width={{
            base: 'auto',
            md: '460px',
          }}
          maxH={{
            base: '78vh',
            md: 'calc(100vh - 88px)',
          }}
          roundedTop='xl'
          roundedBottom={{
            base: '0',
            md: 'xl',
          }}
        >
          <CloseButton
            position='absolute'
            top='3'
            right='3'
            size='sm'
            onClick={value.close}
          />
          <InspectorContent
            key={JSON.stringify(target)}
            target={target}
            onInspect={setTarget}
          />
        </Box>
      )}
    </InspectorContext.Provider>
  )
}
