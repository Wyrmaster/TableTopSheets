import { useCallback, useEffect, useRef, useState } from 'react'
import { type Intent, type ProblemDetails, useApi } from '@/api'

export interface IntentQuery<T> {
  data: T | undefined
  problem: ProblemDetails | undefined
  loading: boolean
  /** Execute the intent again */
  reload: () => void
  /** Replace the data with an answer that arrived another way (e.g. the state returned by a write); cancels a running
   * request so its older answer can't overwrite it */
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
export const useIntentQuery = <T>(create: () => Intent<T> | null, key: string): IntentQuery<T> => {
  const { execute } = useApi()
  const [state, setState] = useState<QueryState<T>>({ loading: true })
  const [version, setVersion] = useState(0)
  const running = useRef<AbortController | null>(null)

  useEffect(() => {
    const intent = create()
    if (!intent) {
      setState({ loading: false })
      return
    }

    // 1. keep the previous data while loading, so a reload doesn't flash an empty page
    const controller = new AbortController()
    running.current = controller
    setState((previous) => ({
      ...previous,
      loading: true,
    }))

    // 2. store the answer unless the request was cancelled by a newer one or by setData
    execute(intent, controller.signal).then((result) => {
      if (controller.signal.aborted) {
        return
      }

      if (result.ok) {
        setState({
          data: result.data,
          loading: false,
        })
      }
      else {
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
  const setData = useCallback((data: T) => {
    running.current?.abort()
    setState({
      data,
      loading: false,
    })
  }, [])

  return {
    data: state.data,
    problem: state.problem,
    loading: state.loading,
    reload,
    setData,
  }
}
