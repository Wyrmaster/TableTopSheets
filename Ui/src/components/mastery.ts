import type { ItemSummaryDto } from '@/api'

/** Whether the character can use a weapon's mastery property */
export type MasteryStatus = 'mastered' | 'not-mastered'

type MasteryWeapon = Pick<ItemSummaryDto, 'key' | 'baseKey'>

/**
 * The character has mastery with the weapon; a magic variant counts as its base weapon (+1 Longsword → Longsword)
 *
 * @param weaponMasteryKeys keys of the mastered weapons, from the character sheet
 * @param item the weapon
 */
export const isMastered = (weaponMasteryKeys: string[], item: MasteryWeapon) =>
  weaponMasteryKeys.includes(item.baseKey ?? item.key)

/** Status of the weapon's mastery for the character */
export const masteryStatus = (weaponMasteryKeys: string[], item: MasteryWeapon): MasteryStatus =>
  isMastered(weaponMasteryKeys, item) ? 'mastered' : 'not-mastered'
