import { type ReactNode, useCallback, useMemo, useState } from "react"
import { HttpClient } from "../core/HttpClient"
import { ApiContext, type ApiContextValue } from "./ApiContext"

const TokenStorageKey = "tabletopsheets.accessToken"

/**
 * Token outside of React state: the HttpClient reads it at request time, so it stays one instance when the token changes
 */
class TokenStore {
  private token: string | null

  constructor(token: string | null) {
    this.token = token
  }

  get(): string | null {
    return this.token
  }

  set(token: string | null) {
    this.token = token
  }
}

function readStoredToken(): string | null {
  try {
    return sessionStorage.getItem(TokenStorageKey)
  } catch {
    return null
  }
}

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
  const [tokens] = useState(() => new TokenStore(readStoredToken()))
  const [accessToken, setAccessTokenState] = useState<string | null>(() => tokens.get())

  const setAccessToken = useCallback((token: string | null) => {
    tokens.set(token)
    storeToken(token)
    setAccessTokenState(token)
  }, [tokens])

  const signOut = useCallback(() => setAccessToken(null), [setAccessToken])

  const client = useMemo(
    () => new HttpClient({ baseUrl, getAccessToken: () => tokens.get(), onUnauthorized: signOut }),
    [baseUrl, tokens, signOut],
  )

  const value = useMemo<ApiContextValue>(
    () => ({ client, execute: (intent, signal) => client.execute(intent, signal), accessToken, setAccessToken, signOut }),
    [client, accessToken, setAccessToken, signOut],
  )

  return <ApiContext.Provider value={value}>{children}</ApiContext.Provider>
}
