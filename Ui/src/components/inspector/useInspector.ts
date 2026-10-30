import { useContext } from 'react'
import { InspectorContext, type InspectorContextValue } from './InspectorContext'

/**
 * Opens the detail view of a spell, species, class, item … ; needs an InspectorProvider above the component
 */
export const useInspector = (): InspectorContextValue => {
  const value = useContext(InspectorContext)
  if (!value) {
    throw new Error('useInspector needs an <InspectorProvider> above the component')
  }

  return value
}
