import { createContext } from 'react'

export interface Settings {
  /** Show the API route behind a section (developer hint, off by default) */
  showApiHints: boolean
}

export interface SettingsContextValue extends Settings {
  setShowApiHints: (show: boolean) => void
}

export const SettingsContext = createContext<SettingsContextValue | null>(null)
