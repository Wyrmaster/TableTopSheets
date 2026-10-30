import { GetBackgroundIntent, GetClassIntent, type StartingEquipmentOptionDto } from '@/api'
import { useIntentQuery } from '@/hooks/useIntentQuery'

/**
 * Starting equipment alternatives of a background or class from the rules catalog (undefined while loading or
 * without a key)
 */
export const useRuleStartingEquipment = (
  kind: 'background' | 'class',
  key: string,
): StartingEquipmentOptionDto[] | undefined => {
  const background = useIntentQuery(
    () => (kind === 'background' && key ? new GetBackgroundIntent(key) : null),
    `background|${key}`,
  )
  const classDetail = useIntentQuery(
    () => (kind === 'class' && key ? new GetClassIntent(key) : null),
    `class|${key}`,
  )
  return kind === 'background' ? background.data?.startingEquipment : classDetail.data?.startingEquipment
}
