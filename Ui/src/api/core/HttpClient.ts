import type { ApiResult, ProblemDetails } from "./ApiResult"
import { BodyIntent, type Intent, type QueryParams } from "./Intent"

export interface HttpClientOptions {
  /** Origin of the API; empty uses the page's origin (the Vite dev server proxies `/api`) */
  baseUrl?: string
  /** Current access token, sent as bearer token with every intent that isn't anonymous */
  getAccessToken?: () => string | null
  /** Called when an authenticated request answers 401 (token missing, expired or signed by another API start) */
  onUnauthorized?: () => void
  /** fetch implementation (tests) */
  fetch?: typeof fetch
}

/**
 * Executes intents against the API. Never throws for HTTP or network errors; every outcome is an ApiResult
 */
export class HttpClient {
  private readonly options: HttpClientOptions

  constructor(options: HttpClientOptions = {}) {
    this.options = options
  }

  async execute<T>(intent: Intent<T>, signal?: AbortSignal): Promise<ApiResult<T>> {
    const headers = new Headers({ Accept: "application/json, application/problem+json" })
    let body: string | undefined
    if (intent instanceof BodyIntent) {
      headers.set("Content-Type", "application/json")
      body = JSON.stringify(intent.body)
    }

    const token = intent.anonymous ? null : this.options.getAccessToken?.()
    if (token) {
      headers.set("Authorization", `Bearer ${token}`)
    }

    let response: Response
    try {
      const fetcher = this.options.fetch ?? globalThis.fetch.bind(globalThis)
      response = await fetcher(this.url(intent), { method: intent.method, headers, body, signal })
    } catch (error) {
      if (signal?.aborted) {
        return { ok: false, status: 0, aborted: true, problem: { title: "Request cancelled" } }
      }

      return { ok: false, status: 0, problem: { title: "Network error", detail: error instanceof Error ? error.message : String(error) } }
    }

    if (!response.ok) {
      if (response.status === 401 && !intent.anonymous) {
        this.options.onUnauthorized?.()
      }

      return { ok: false, status: response.status, problem: await HttpClient.problem(response) }
    }

    try {
      return { ok: true, status: response.status, data: (await HttpClient.read(intent, response)) as T }
    } catch (error) {
      if (signal?.aborted) {
        return { ok: false, status: 0, aborted: true, problem: { title: "Request cancelled" } }
      }

      return { ok: false, status: response.status, problem: { title: "Invalid response", detail: error instanceof Error ? error.message : String(error) } }
    }
  }

  /**
   * Full URL of an intent with its query string
   */
  url(intent: Intent<unknown>): string {
    const query = HttpClient.queryString(intent.queryParams())
    return `${this.options.baseUrl ?? ""}${intent.path}${query ? `?${query}` : ""}`
  }

  static queryString(params: QueryParams): string {
    const search = new URLSearchParams()
    for (const [key, value] of Object.entries(params)) {
      if (value !== null && value !== undefined && value !== "") {
        search.append(key, String(value))
      }
    }

    return search.toString()
  }

  private static async read(intent: Intent<unknown>, response: Response): Promise<unknown> {
    if (intent.responseKind === "none" || response.status === 204) {
      return undefined
    }

    return intent.responseKind === "blob" ? await response.blob() : await response.json()
  }

  private static async problem(response: Response): Promise<ProblemDetails> {
    const fallback: ProblemDetails = { status: response.status, title: response.statusText || `HTTP ${response.status}` }
    const type = response.headers.get("Content-Type") ?? ""
    if (!type.includes("json")) {
      return fallback
    }

    try {
      return { ...fallback, ...((await response.json()) as ProblemDetails) }
    } catch {
      return fallback
    }
  }
}
