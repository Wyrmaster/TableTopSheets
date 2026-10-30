import { useState } from 'react'
import { ListScenesIntent, type SceneDto } from '@/api'
import { useIntentQuery } from './useIntentQuery'

/**
 * One of the scenes, picked at random when the component mounts; undefined while loading or when there are none
 */
export const useRandomScene = (): SceneDto | undefined => {
  const scenes = useIntentQuery(() => new ListScenesIntent(), 'scenes')
  const [pick] = useState(() => Math.random())
  const list = scenes.data ?? []
  return list.length > 0 ? list[Math.floor(pick * list.length)] : undefined
}
