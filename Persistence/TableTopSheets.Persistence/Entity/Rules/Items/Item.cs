using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Persistence.Entity.Rules.Items;

/// <summary>
///   Base of all equipment (table per hierarchy); magic properties are expressed through <see cref="Features"/>
/// </summary>
public class Item: RuleEntity
{
  #region Properties

  public ItemKindEnum Kind { get; set; }

  public int CostCp { get; set; }

  public double Weight { get; set; }

  public bool Magic { get; set; }

  public bool RequiresAttunement { get; set; }

  public ICollection<Feature> Features { get; set; } = new List<Feature>();

  #endregion
}
