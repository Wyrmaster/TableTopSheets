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
 *
 * A derived intent sets `method`, implements `path` and, when the route takes query parameters, overrides `queryParams`.
 */
export abstract class Intent<TResponse> {
  /** HTTP method of the route */
  abstract readonly method: HttpMethod

  /** Path below the API root, starting with `/api/`; ids and keys escaped with `segment` */
  abstract get path(): string

  /** `blob` for files (PDF), `none` for routes without content (204) */
  readonly responseKind: ResponseKind = "json"

  /** True for routes that don't need a bearer token (rules catalog, login, register) */
  readonly anonymous: boolean = false

  /** Type-only marker that lets TypeScript infer the response type of an intent; never set */
  declare readonly __response?: TResponse

  /**
   * Query parameters of the request; none by default
   */
  queryParams(): QueryParams {
    return {}
  }
}

/**
 * Intent that sends a JSON body
 */
export abstract class BodyIntent<TBody, TResponse> extends Intent<TResponse> {
  /** Only methods with a body */
  abstract override readonly method: "POST" | "PUT" | "PATCH"

  /** Request body, serialized as JSON by the HttpClient */
  readonly body: TBody

  /**
   * @param body request body (a DTO)
   */
  protected constructor(body: TBody) {
    super()
    this.body = body
  }
}

/**
 * Intent of a list route: paging (`page`, `pageSize`), `search`, `sort` and the route's typed filters. The answer is a
 * `PagedResult` of the item type
 */
export abstract class PagedIntent<TItem, TFilter extends object = object> extends Intent<PagedResult<TItem>> {
  readonly method = "GET"

  /** Paging, search, sort and filters; unset values are left out of the query string */
  readonly query: ListQuery & TFilter

  /**
   * @param query paging and filters, e.g. `{ page: 2, pageSize: 20, sort: "-level,name", class: "wizard" }`
   */
  protected constructor(query: ListQuery & TFilter) {
    super()
    this.query = query
  }

  /**
   * The query as parameters of the request
   */
  override queryParams(): QueryParams {
    // filters are flat interfaces of query values; TypeScript can't prove that for the generic TFilter
    return { ...this.query } as QueryParams
  }
}
