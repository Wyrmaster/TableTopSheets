import type { ListQuery, PagedResult } from "../dtos/paging"

export type HttpMethod = "GET" | "POST" | "PUT" | "PATCH" | "DELETE"

/** How the HttpClient reads a successful answer */
export type ResponseKind = "json" | "blob" | "none"

export type QueryValue = string | number | boolean | null | undefined

/** Query parameters of a request; `null` and `undefined` values are left out */
export type QueryParams = Record<string, QueryValue>

/**
 * Escapes a key or id for use as a path segment
 */
export function segment(value: string): string {
  return encodeURIComponent(value)
}

/**
 * Immutable description of one API request: method, path and query. The type parameter is the type of a successful
 * answer, so `HttpClient.execute` returns a typed result. Intents hold no outcome and can be executed any number of times.
 */
export abstract class Intent<TResponse> {
  abstract readonly method: HttpMethod

  /** Path below the API root, starting with `/api/` */
  abstract get path(): string

  readonly responseKind: ResponseKind = "json"

  /** True for routes that don't need a bearer token (rules catalog, login, register) */
  readonly anonymous: boolean = false

  /** Type-only marker that lets TypeScript infer the response type of an intent; never set */
  declare readonly __response?: TResponse

  queryParams(): QueryParams {
    return {}
  }
}

/**
 * Intent that sends a JSON body
 */
export abstract class BodyIntent<TBody, TResponse> extends Intent<TResponse> {
  abstract override readonly method: "POST" | "PUT" | "PATCH"

  readonly body: TBody

  protected constructor(body: TBody) {
    super()
    this.body = body
  }
}

/**
 * Intent of a list route: paging (`page`, `pageSize`), `search`, `sort` and the route's typed filters
 */
export abstract class PagedIntent<TItem, TFilter extends object = object> extends Intent<PagedResult<TItem>> {
  readonly method = "GET"

  readonly query: ListQuery & TFilter

  protected constructor(query: ListQuery & TFilter) {
    super()
    this.query = query
  }

  override queryParams(): QueryParams {
    // filters are flat interfaces of query values; TypeScript can't prove that for the generic TFilter
    return { ...this.query } as QueryParams
  }
}
