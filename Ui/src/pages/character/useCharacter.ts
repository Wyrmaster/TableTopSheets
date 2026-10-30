import { useContext } from 'react'
import { CharacterContext, type CharacterContextValue } from './CharacterContext'

/**
 * State, sheet and play state of the open character (inside CharacterLayout)
 */
export const useCharacter = (): CharacterContextValue => {
  const value = useContext(CharacterContext)
  if (!value) {
    throw new Error('useCharacter needs the CharacterLayout above the component')
  }

  return value
}
