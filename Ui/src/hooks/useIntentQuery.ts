import { useCallback, useEffect, useState } from 'react'
import { type Intent, type ProblemDetails, useApi } from '@/api'

export interface IntentQuery<T> {
  data: T | undefined
  problem: ProblemDetails | undefined
  loading: boolean
  /** Execute the intent again */
  reload: () => void
  /** Replace the data with an answer that arrived another way (e.g. the state returned by a write) */
  setData: (data: T) => void
}

interface QueryState<T> {
  data?: T
  problem?: ProblemDetails
  loading: boolean
}

/**
 * Executes the intent built by `create` whenever `key` changes and keeps the answer; `create` returning null skips
 * the request. Requests of a previous key are cancelled.
 */
export function useIntentQuery<T>(create: () => Intent<T> | null, key: string): IntentQuery<T> {
  const { execute } = useApi()
  const [state, setState] = useState<QueryState<T>>({ loading: true })
  const [version, setVersion] = useState(0)

  useEffect(() => {
    const intent = create()
    if (!intent) {
      setState({ loading: false })
      return
    }

    // 1. keep the previous data while loading, so a reload doesn't flash an empty page
    const controller = new AbortController()
    setState((previous) => ({
      ...previous,
      loading: true,
    }))

    // 2. store the answer unless the request was cancelled by a newer one
    execute(intent, controller.signal).then((result) => {
      if (result.ok) {
        setState({
          data: result.data,
          loading: false,
        })
      }
      else if (!result.aborted) {
        setState({
          problem: result.problem,
          loading: false,
        })
      }
    })
    return () => controller.abort()
    // the intent is identified by `key`; `create` is a new function on every render and deliberately not a dependency
  }, [key, version, execute])

  const reload = useCallback(() => setVersion((value) => value + 1), [])
  const setData = useCallback((data: T) => setState({
    data,
    loading: false,
  }), [])

  return {
    data: state.data,
    problem: state.problem,
    loading: state.loading,
    reload,
    setData,
  }
}
