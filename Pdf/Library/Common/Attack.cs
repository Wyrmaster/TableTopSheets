using TableTopSheets.Common.Enum;

namespace TableTopSheets.Pdf.Common;

/// <summary>
///   Container describing an Attack
/// </summary>
public record Attack
{
  #region Properties

  /// <summary>
  ///   Name of the Attack Weapon Cantrip...
  /// </summary>
  public string Name { get; set; } = string.Empty;

  /// <summary>
  ///   Dice to throw for an Attack roll
  /// </summary>
  public sbyte AttackRollModifier { get; set; }

  /// <summary>
  ///   Dice to throw for the Damage roll
  /// </summary>
  public DiceEnum[] DamageRoll { get; set; } = [];

  /// <summary>
  ///   Damage Type of the Attack
  /// </summary>
  public DamageTypeEnum DamageType { get; set; }

  /// <summary>
  ///   Additional Notes for this attack
  /// </summary>
  public string Notes { get; set; } = string.Empty;

  #endregion
}