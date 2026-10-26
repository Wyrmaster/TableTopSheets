using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Persistence.Entity.Rules.Items;

/// <summary>
///   Base of all equipment (table per hierarchy); magic properties are expressed through <see cref="Features"/>
/// </summary>
public class Item: RuleEntity
{
  #region Properties

  public ItemKindEnum Kind { get; set; }

  /// <summary>
  ///   Cost of one <see cref="Bundle"/> in copper pieces
  /// </summary>
  public int CostCp { get; set; }

  /// <summary>
  ///   Weight of one <see cref="Bundle"/> in pounds
  /// </summary>
  public double Weight { get; set; }

  /// <summary>
  ///   Pieces sold together for <see cref="CostCp"/> (20 arrows, 50 needles); 1 for everything sold by the piece
  /// </summary>
  public int Bundle { get; set; } = 1;

  /// <summary>
  ///   Tool group (<c>artisan</c>, <c>gaming-set</c>, <c>musical-instrument</c>, <c>other</c>) or gear category
  ///   (<c>ammunition</c>, <c>arcane-focus</c>, <c>druidic-focus</c>, <c>holy-symbol</c>, <c>pack</c>, <c>mount</c>, <c>tack</c>,
  ///   <c>vehicle</c>, <c>ship</c>, <c>food-and-drink</c>); null for plain gear
  /// </summary>
  public string? Group { get; set; }

  public bool Magic { get; set; }

  public bool RequiresAttunement { get; set; }

  public ICollection<Feature> Features { get; set; } = new List<Feature>();

  #endregion
}
