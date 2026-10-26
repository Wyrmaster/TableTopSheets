import type { ApiResult, ProblemDetails } from './ApiResult'
import { BodyIntent, type Intent, type QueryParams } from './Intent'

export interface HttpClientOptions {
  /** Origin of the API; empty uses the page's origin (the Vite dev server proxies `/api`) */
  baseUrl?: string
  /** Current access token, sent as bearer token with every intent that isn't anonymous */
  getAccessToken?: () => string | null
  /** Called when an authenticated request answers 401 (token missing, expired or signed by another API start) */
  onUnauthorized?: () => void
}

/**
 * Executes intents against the API. Never throws for HTTP or network errors; every outcome is an ApiResult
 */
export class HttpClient {
  private readonly options: HttpClientOptions

  /**
   * @param options base url, token source and 401 callback; all optional
   */
  constructor(options: HttpClientOptions = {}) {
    this.options = options
  }

  /**
   * Sends the request an intent describes and reads the answer.
   *
   * @param intent what to request; its type parameter is the type of `data` on success
   * @param signal cancels the request (e.g. when the component unmounts); the result then has `aborted: true`
   * @returns `{ ok: true, status, data }` on 2xx, otherwise `{ ok: false, status, problem }`
   */
  async execute<T>(intent: Intent<T>, signal?: AbortSignal): Promise<ApiResult<T>> {
    // 1. headers and body: body intents are sent as JSON, everything else without a body
    const headers = new Headers({ Accept: 'application/json, application/problem+json' })
    let body: string | undefined
    if (intent instanceof BodyIntent) {
      headers.set('Content-Type', 'application/json')
      body = JSON.stringify(intent.body)
    }

    // 2. authentication: the bearer token goes with every intent except the anonymous ones (rules, login, register)
    const token = intent.anonymous ? null : this.options.getAccessToken?.()
    if (token) {
      headers.set('Authorization', `Bearer ${token}`)
    }

    // 3. send; fetch only rejects when the API wasn't reached (offline, CORS, aborted), not for error statuses
    let response: Response
    try {
      response = await fetch(this.url(intent), {
        method: intent.method,
        headers,
        body,
        signal,
      })
    }
    catch (error) {
      return HttpClient.failure(error, signal)
    }

    // 4. error statuses: read the problem details; a 401 means the token is no longer valid
    if (!response.ok) {
      if (response.status === 401 && !intent.anonymous) {
        this.options.onUnauthorized?.()
      }

      return {
        ok: false,
        status: response.status,
        problem: await HttpClient.problem(response),
      }
    }

    // 5. success: read the body the way the intent expects it (JSON, blob or nothing)
    try {
      return {
        ok: true,
        status: response.status,
        data: (await HttpClient.read(intent, response)) as T,
      }
    }
    catch (error) {
      // the body couldn't be read: the request was cancelled while downloading, or the answer isn't valid JSON
      return signal?.aborted
        ? HttpClient.failure(error, signal)
        : {
          ok: false,
          status: response.status,
          problem: { title: 'Invalid response', detail: HttpClient.message(error) },
        }
    }
  }

  /**
   * Full URL of an intent: base url, path and query string
   */
  url(intent: Intent<unknown>): string {
    const query = HttpClient.queryString(intent.queryParams())
    return `${this.options.baseUrl ?? ''}${intent.path}${query ? `?${query}` : ''}`
  }

  /**
   * Query string without the leading `?`; `null`, `undefined` and empty strings are left out, so optional filters can
   * be passed as they are
   */
  static queryString(params: QueryParams): string {
    const search = new URLSearchParams()
    for (const [key, value] of Object.entries(params)) {
      if (value !== null && value !== undefined && value !== '') {
        search.append(key, String(value))
      }
    }

    return search.toString()
  }

  /**
   * Body of a successful answer in the form the intent asks for; 204 and `none` give `undefined`
   */
  private static async read(intent: Intent<unknown>, response: Response): Promise<unknown> {
    if (intent.responseKind === 'none' || response.status === 204) {
      return undefined
    }

    return intent.responseKind === 'blob' ? await response.blob() : await response.json()
  }

  /**
   * Problem details of an error answer. The API sends `application/problem+json`; anything else (e.g. a proxy error
   * page) falls back to the status and its text
   */
  private static async problem(response: Response): Promise<ProblemDetails> {
    const fallback: ProblemDetails = {
      status: response.status,
      title: response.statusText || `HTTP ${response.status}`,
    }
    const type = response.headers.get('Content-Type') ?? ''
    if (!type.includes('json')) {
      return fallback
    }

    try {
      // fields of the answer win; the fallback only fills what's missing
      return { ...fallback, ...((await response.json()) as ProblemDetails) }
    }
    catch {
      return fallback
    }
  }

  /**
   * Result of a request that got no (complete) answer: cancelled through its signal, or the API wasn't reachable
   */
  private static failure(error: unknown, signal: AbortSignal | undefined): ApiResult<never> {
    if (signal?.aborted) {
      return {
        ok: false,
        status: 0,
        aborted: true,
        problem: { title: 'Request cancelled' },
      }
    }

    return {
      ok: false,
      status: 0,
      problem: { title: 'Network error', detail: HttpClient.message(error) },
    }
  }

  private static message(error: unknown): string {
    return error instanceof Error ? error.message : String(error)
  }
}
