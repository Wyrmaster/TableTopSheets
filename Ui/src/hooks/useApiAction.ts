import { useCallback, useState } from 'react'
import { type ApiResult, type Intent, problemMessages, useApi } from '@/api'
import { toaster } from '@/components/ui/toaster'

export interface ActionOptions {
  /** Toast shown after success */
  success?: string
}

/**
 * Runs write intents: failures are shown as error toasts (backend messages, e.g. rule violations), success
 * optionally as a toast. `busy` is true while a request runs.
 */
export function useApiAction() {
  const { execute } = useApi()
  const [busy, setBusy] = useState(false)

  const run = useCallback(async <T>(intent: Intent<T>, options: ActionOptions = {}): Promise<ApiResult<T>> => {
    setBusy(true)
    try {
      const result = await execute(intent)
      if (result.ok) {
        if (options.success) {
          toaster.create({
            type: 'success',
            title: options.success,
          })
        }
      }
      else if (!result.aborted) {
        toaster.create({
          type: 'error',
          title: result.problem.title ?? 'Request failed',
          description: problemMessages(result.problem).join('\n'),
        })
      }

      return result
    }
    finally {
      setBusy(false)
    }
  }, [execute])

  return {
    run,
    busy,
  }
}
