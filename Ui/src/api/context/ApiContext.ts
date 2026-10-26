import { createContext } from 'react'
import type { ApiResult } from '../core/ApiResult'
import type { HttpClient } from '../core/HttpClient'
import type { Intent } from '../core/Intent'

export interface ApiContextValue {
  client: HttpClient
  /** Executes an intent with the current access token */
  execute: <T>(intent: Intent<T>, signal?: AbortSignal) => Promise<ApiResult<T>>
  /** Bearer token of the signed in user; null when signed out */
  accessToken: string | null
  /** Store the token after login / register (or from the OIDC provider) */
  setAccessToken: (accessToken: string | null) => void
  signOut: () => void
}

export const ApiContext = createContext<ApiContextValue | null>(null)
