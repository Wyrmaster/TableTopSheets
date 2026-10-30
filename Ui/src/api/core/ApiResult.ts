/**
 * RFC 7807 problem details as the API answers errors with. Rule violations (422) carry the engine messages in
 * `errors.rules`, invalid queries and bodies (400) carry them under the parameter name.
 */
export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  errors?: Record<string, string[]>
}

export interface ApiSuccess<T> {
  ok: true
  status: number
  data: T
}

export interface ApiFailure {
  ok: false
  /** HTTP status, 0 when the request didn't reach the API (network error or abort) */
  status: number
  problem: ProblemDetails
  /** set when the request was cancelled through its AbortSignal */
  aborted?: true
}

/**
 * Outcome of executing an intent; check `ok` before reading `data` or `problem`
 */
export type ApiResult<T> = ApiSuccess<T> | ApiFailure

/**
 * All messages of a problem: the `errors` entries, or the detail / title when there are none
 */
export const problemMessages = (problem: ProblemDetails): string[] => {
  const errors = Object.values(problem.errors ?? {}).flat()
  if (errors.length > 0) {
    return errors
  }

  return [problem.detail ?? problem.title ?? 'Unknown error']
}
