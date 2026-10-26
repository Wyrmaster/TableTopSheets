import { useEffect, useState } from 'react'
import { GetItemIntent, type ItemDetailDto, useApi } from '@/api'

/** Item details don't change while the app runs, so they are kept for the session */
const cache = new Map<string, ItemDetailDto>()

/**
 * Details (properties such as `ranged`, `baseAc`) of the given item keys, loaded once per key
 */
export function useItemDetails(keys: string[]): Map<string, ItemDetailDto> {
  const { execute } = useApi()
  const [, setVersion] = useState(0)
  const missing = [...new Set(keys)].filter((key) => !cache.has(key)).sort()
    .join(',')

  useEffect(() => {
    if (!missing) {
      return
    }

    const controller = new AbortController()
    Promise.all(missing.split(',').map(async (key) => {
      const result = await execute(new GetItemIntent(key), controller.signal)
      if (result.ok) {
        cache.set(key, result.data)
      }
    })).then(() => setVersion((value) => value + 1))
    return () => controller.abort()
  }, [missing, execute])

  return cache
}
