using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Magic;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Seed.Model;

// Seed files are authored by hand (Data/Rules) or generated from the rule book (Data/Extracted).
// Records with the same key are merged: authored values win, extracted values fill the gaps (mostly descriptions).

public sealed class SeedItem: SeedNamed
{
  public ItemKindEnum? Kind { get; set; }

  /// <summary>
  ///   Cost in copper pieces
  /// </summary>
  public int? Cost { get; set; }

  public double? Weight { get; set; }

  public bool? Magic { get; set; }

  public bool? Attunement { get; set; }

  /// <summary>
  ///   Gear category (<c>ammunition</c>, <c>arcane-focus</c>, <c>mount</c> ...); tools use <see cref="ToolGroup"/>
  /// </summary>
  public string? Category { get; set; }

  /// <summary>
  ///   Pieces sold together for <see cref="Cost"/> (20 arrows); cost and weight are per bundle
  /// </summary>
  public int? Bundle { get; set; }

  public List<SeedFeature> Features { get; set; } = [];

  #region Weapon

  public WeaponCategoryEnum? WeaponCategory { get; set; }

  public bool? Ranged { get; set; }

  public string? Damage { get; set; }

  public DamageTypeEnum? DamageType { get; set; }

  public WeaponPropertyEnum? Properties { get; set; }

  public string? Versatile { get; set; }

  public short? RangeNormal { get; set; }

  public short? RangeLong { get; set; }

  public string? Mastery { get; set; }

  #endregion

  #region Armor

  public ArmorEnum? ArmorCategory { get; set; }

  public byte? BaseAc { get; set; }

  public byte? DexCap { get; set; }

  public byte? StrengthRequirement { get; set; }

  public bool? StealthDisadvantage { get; set; }

  #endregion

  #region Tool

  public string? ToolGroup { get; set; }

  #endregion
}
