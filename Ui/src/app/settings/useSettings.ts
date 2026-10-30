import { useContext } from 'react'
import { SettingsContext, type SettingsContextValue } from './SettingsContext'

/**
 * User settings (API hints …); needs a SettingsProvider above the component
 */
export const useSettings = (): SettingsContextValue => {
  const value = useContext(SettingsContext)
  if (!value) {
    throw new Error('useSettings needs a <SettingsProvider> above the component')
  }

  return value
}
