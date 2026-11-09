import { createContext } from 'react'
import type { TextBlock } from '@/api'
import type { MasteryStatus } from '@/components/mastery'

/** What the detail view shows: a rule entity loaded from the API, or a feature text that is already at hand */
export type InspectTarget =
  | { kind: 'spell' | 'species' | 'background' | 'feat' | 'class' | 'item', key: string }
  | {
    kind: 'mastery',
    key: string,
    /** whether the character can use it; left out where no character is involved */
    status?: MasteryStatus
  }
  | {
    kind: 'feature',
    name: string,
    source: string,
    description: string,
    /** structure of the description (paragraphs, headings, lists, tables), when known */
    descriptionBlocks?: TextBlock[] | null
  }

export interface InspectorContextValue {
  inspect: (target: InspectTarget) => void
  close: () => void
}

export const InspectorContext = createContext<InspectorContextValue | null>(null)
