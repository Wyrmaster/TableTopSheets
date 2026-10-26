import { useContext } from "react"
import { ApiContext, type ApiContextValue } from "./ApiContext"

/**
 * Entry point to the API: `execute(intent)` and the access token. Needs an ApiProvider above the component
 */
export function useApi(): ApiContextValue {
  const value = useContext(ApiContext)
  if (!value) {
    throw new Error("useApi needs an <ApiProvider> above the component")
  }

  return value
}
