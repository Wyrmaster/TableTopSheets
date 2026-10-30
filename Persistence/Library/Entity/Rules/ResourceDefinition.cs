using TableTopSheets.Common.Enum.Magic;

namespace TableTopSheets.Persistence.Entity.Rules;

/// <summary>
///   Limited use pool granted by a feature (Rage, Sorcery Points, Channel Divinity ...)
/// </summary>
public class ResourceDefinition: BaseEntity
{
  #region Properties

  public required string Key { get; set; }

  public required string Name { get; set; }

  public Guid FeatureId { get; set; }

  public Feature? Feature { get; set; }

  public string MaxExpr { get; set; } = "1";

  /// <summary>
  ///   Optional die of the resource (Superiority Dice, Bardic Inspiration)
  /// </summary>
  public string? DieExpr { get; set; }

  public PeriodEnum Recharge { get; set; } = PeriodEnum.LongRest;

  /// <summary>
  ///   Optional condition; while it is true the resource recharges on a Short Rest instead (Font of Inspiration)
  /// </summary>
  public string? ShortRestExpr { get; set; }

  #endregion
}
