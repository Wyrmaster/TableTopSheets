using TableTopSheets.Common.Enum.Item;

namespace TableTopSheets.Persistence.Entity.Rules.Items;

/// <summary>
///   Armor and shields (<see cref="ArmorEnum.Shield"/> uses <see cref="BaseAc"/> as bonus)
/// </summary>
public class Armor: Item
{
  #region Properties

  public ArmorEnum Category { get; set; }

  public byte BaseAc { get; set; }

  /// <summary>
  ///   Maximum Dexterity modifier added (null = no limit, 0 = none)
  /// </summary>
  public byte? DexCap { get; set; }

  public byte? StrengthRequirement { get; set; }

  public bool StealthDisadvantage { get; set; }

  #endregion
}
