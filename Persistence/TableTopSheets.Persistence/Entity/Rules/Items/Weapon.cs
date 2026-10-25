using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Persistence.Entity.Rules.Items;

public class Weapon: Item
{
  #region Properties

  public WeaponCategoryEnum Category { get; set; }

  public bool Ranged { get; set; }

  /// <summary>
  ///   Damage dice (e.g. <c>1d8</c>, <c>2d6</c>, <c>1</c>)
  /// </summary>
  public required string Damage { get; set; }

  public DamageTypeEnum DamageType { get; set; }

  public WeaponPropertyEnum Properties { get; set; }

  public string? VersatileDamage { get; set; }

  public short? RangeNormal { get; set; }

  public short? RangeLong { get; set; }

  public Guid? MasteryId { get; set; }

  public WeaponMastery? Mastery { get; set; }

  #endregion
}
