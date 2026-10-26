import { type ReactNode, useMemo, useState } from 'react'
import { type Settings, SettingsContext, type SettingsContextValue } from './SettingsContext'

const StorageKey = 'tabletopsheets.settings'
const Defaults: Settings = { showApiHints: false }

function readSettings(): Settings {
  try {
    return {
      ...Defaults,
      ...JSON.parse(localStorage.getItem(StorageKey) ?? '{}'),
    }
  }
  catch {
    return Defaults
  }
}

function storeSettings(settings: Settings) {
  try {
    localStorage.setItem(StorageKey, JSON.stringify(settings))
  }
  catch {
    // storage blocked: settings only last for this page
  }
}

/**
 * User settings of this browser (kept in local storage)
 */
export function SettingsProvider({ children }: { children: ReactNode }) {
  const [settings, setSettings] = useState<Settings>(readSettings)

  const value = useMemo<SettingsContextValue>(() => ({
    ...settings,
    setShowApiHints: (showApiHints) => {
      const next = {
        ...settings,
        showApiHints,
      }
      storeSettings(next)
      setSettings(next)
    },
  }), [settings])

  return <SettingsContext.Provider value={value}>{children}</SettingsContext.Provider>
}
