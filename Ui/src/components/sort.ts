/**
 * Sort parameter of a list route: the direction applies to the first field only, the others stay ascending as tie
 * breakers (`level,name` descending → `-level,name`: highest level first, names A–Z within it)
 */
export const sortParam = (fields: string, descending: boolean) => descending ? `-${fields}` : fields
