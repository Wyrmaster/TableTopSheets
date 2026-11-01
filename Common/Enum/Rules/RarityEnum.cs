namespace TableTopSheets.Common.Enum.Rules;

/// <summary>
///   Rarity of a magic item (Dungeon Master's Guide, chapter 7); it sets the price of the item
/// </summary>
public enum RarityEnum
{
  Common,
  Uncommon,
  Rare,
  VeryRare,
  Legendary,

  /// <summary>
  ///   Unique and priceless
  /// </summary>
  Artifact,
}
