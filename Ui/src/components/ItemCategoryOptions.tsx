/**
 * Values of the `category` filter of the item list: weapon and armor categories, tool groups and gear categories (magic
 * weapons and armor share them), then the groups of the other magic items
 */
const Categories: [string, string][] = [
  ['simple', 'Simple weapons'],
  ['martial', 'Martial weapons'],
  ['ammunition', 'Ammunition'],
  ['light', 'Light armor'],
  ['medium', 'Medium armor'],
  ['heavy', 'Heavy armor'],
  ['shield', 'Shields'],
  ['artisan', 'Artisan\'s tools'],
  ['gaming-set', 'Gaming sets'],
  ['musical-instrument', 'Musical instruments'],
  ['arcane-focus', 'Arcane focuses'],
  ['druidic-focus', 'Druidic focuses'],
  ['holy-symbol', 'Holy symbols'],
  ['pack', 'Packs'],
  ['food-and-drink', 'Food and drink'],
  ['mount', 'Mounts and animals'],
  ['tack', 'Tack and harness'],
  ['vehicle', 'Drawn vehicles'],
  ['ship', 'Ships'],
]

const MagicCategories: [string, string][] = [
  ['wondrous-item', 'Wondrous items'],
  ['potion', 'Potions'],
  ['ring', 'Rings'],
  ['rod', 'Rods'],
  ['scroll', 'Scrolls'],
  ['staff', 'Staffs'],
  ['wand', 'Wands'],
]

const categoryOptions = (categories: [string, string][]) => categories.map(([value, label]) => (
  <option
    key={value}
    value={value}
  >
    {label}
  </option>
))

/** Options of an item category select: "All categories", then equipment and magic item groups */
export const ItemCategoryOptions = () => (
  <>
    <option value=''>All categories</option>
    <optgroup label='Equipment'>{categoryOptions(Categories)}</optgroup>
    <optgroup label='Magic items'>{categoryOptions(MagicCategories)}</optgroup>
  </>
)
