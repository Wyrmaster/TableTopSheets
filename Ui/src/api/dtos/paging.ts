// Source: Api/Library/Contracts/Paging.cs

export const DefaultPageSize = 50
export const MaxPageSize = 200

/**
 * Paging, search and sort parameters shared by every list route
 */
export interface ListQuery {
  /** 1-based page number */
  page?: number
  /** entries per page (1 - 200, default 50) */
  pageSize?: number
  /** case-insensitive search in name and key */
  search?: string
  /** comma separated sort fields; a leading `-` sorts descending (e.g. `level,-name`) */
  sort?: string
}

/**
 * One page of a list
 */
export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}
