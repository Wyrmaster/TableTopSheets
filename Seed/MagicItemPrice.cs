using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Seed;

/// <summary>
///   Price of a magic item by rarity (Dungeon Master's Guide, Magic Item Prices); consumables cost half, a piece of ammunition
///   a tenth of a potion
/// </summary>
public static class MagicItemPrice
{
  #region Fields

  private static readonly HashSet<string> Consumables = ["potion", "scroll"];

  #endregion

  #region Methods

  /// <summary>
  ///   Price in copper pieces; null without a rarity, 0 for artifacts (priceless)
  /// </summary>
  public static int? Of(RarityEnum? rarity, string? category)
  {
    int? gold = rarity switch
    {
      RarityEnum.Common => 100,
      RarityEnum.Uncommon => 400,
      RarityEnum.Rare => 4_000,
      RarityEnum.VeryRare => 40_000,
      RarityEnum.Legendary => 200_000,
      RarityEnum.Artifact => 0,
      _ => null,
    };
    if (gold == null)
    {
      return null;
    }

    int copper = gold.Value * 100;
    return category switch
    {
      "ammunition" => copper / 20,
      _ when category != null && Consumables.Contains(category) => copper / 2,
      _ => copper,
    };
  }

  #endregion
}
