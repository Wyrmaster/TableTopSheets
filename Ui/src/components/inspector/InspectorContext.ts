import { createContext } from 'react'

/** What the detail view shows: a rule entity loaded from the API, or a feature text that is already at hand */
export type InspectTarget =
  | { kind: 'spell' | 'species' | 'background' | 'feat' | 'class' | 'item', key: string }
  | {
    kind: 'feature',
    name: string,
    source: string,
    description: string
  }

export interface InspectorContextValue {
  inspect: (target: InspectTarget) => void
  close: () => void
}

export const InspectorContext = createContext<InspectorContextValue | null>(null)
