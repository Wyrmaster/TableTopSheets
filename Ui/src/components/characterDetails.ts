import type { AlignmentEnum } from '@/api'

/** Descriptive fields of a character, edited in the creator and on the settings page */
export interface CharacterDetails {
  name: string
  alignment: AlignmentEnum
  appearance: string
  backstory: string
}

/** Readable alignment (`NeutralGood` → `Neutral Good`) */
export const alignmentName = (alignment: string) => alignment.replace(/([a-z])([A-Z])/g, '$1 $2')
