import { type ReactNode, useCallback, useMemo, useState } from "react"
import { HttpClient } from "../core/HttpClient"
import { ApiContext, type ApiContextValue } from "./ApiContext"

const TokenStorageKey = "tabletopsheets.accessToken"

/**
 * Token outside of React state: the HttpClient reads it at request time, so it stays one instance when the token changes
 */
class TokenStore {
  private token: string | null

  /**
   * @param token token to start with (from the session storage)
   */
  constructor(token: string | null) {
    this.token = token
  }

  /** Current token; null when signed out */
  get(): string | null {
    return this.token
  }

  /** Replace the token; null signs out */
  set(token: string | null) {
    this.token = token
  }
}

/**
 * Token of the previous page load in this tab (survives a reload, not closing the tab)
 */
function readStoredToken(): string | null {
  try {
    return sessionStorage.getItem(TokenStorageKey)
  } catch {
    // storage blocked (private mode): start signed out
    return null
  }
}

/**
 * Keep the token for a reload; null removes it
 */
function storeToken(accessToken: string | null) {
  try {
    if (accessToken) {
      sessionStorage.setItem(TokenStorageKey, accessToken)
    } else {
      sessionStorage.removeItem(TokenStorageKey)
    }
  } catch {
    // storage blocked (private mode): the token only lives in memory
  }
}

export interface ApiProviderProps {
  children: ReactNode
  /** Origin of the API; empty uses the page's origin (the dev server proxies /api) */
  baseUrl?: string
}

/**
 * Provides the HttpClient and the access token of the signed in user to the app (see useApi)
 */
export function ApiProvider({ children, baseUrl = "" }: ApiProviderProps) {
  // 1. token: kept in the store for the client and in state so components re-render on sign in / sign out
  const [tokens] = useState(() => new TokenStore(readStoredToken()))
  const [accessToken, setAccessTokenState] = useState<string | null>(() => tokens.get())

  // 2. changing the token updates all three places: client store, session storage and React state
  const setAccessToken = useCallback((token: string | null) => {
    tokens.set(token)
    storeToken(token)
    setAccessTokenState(token)
  }, [tokens])

  const signOut = useCallback(() => setAccessToken(null), [setAccessToken])

  // 3. one client for the app; a 401 answer signs the user out
  const client = useMemo(
    () => new HttpClient({ baseUrl, getAccessToken: () => tokens.get(), onUnauthorized: signOut }),
    [baseUrl, tokens, signOut],
  )

  // 4. context value; only changes when the token (or the client) changes
  const value = useMemo<ApiContextValue>(
    () => ({ client, execute: (intent, signal) => client.execute(intent, signal), accessToken, setAccessToken, signOut }),
    [client, accessToken, setAccessToken, signOut],
  )

  return <ApiContext.Provider value={value}>{children}</ApiContext.Provider>
}
