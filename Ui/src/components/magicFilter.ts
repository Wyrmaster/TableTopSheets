/** Value of the magic filter: every item (`''`), only mundane ones (`'false'`) or only magic ones (`'true'`) */
export type MagicFilter = '' | 'false' | 'true'

/** The magic filter as query parameter of `ListItemsIntent`; undefined for every item */
export const magicParam = (filter: MagicFilter) => (filter ? filter === 'true' : undefined)
