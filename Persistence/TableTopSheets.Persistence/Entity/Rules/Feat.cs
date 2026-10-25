using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Persistence.Entity.Rules;

public class Feat: RuleEntity
{
  #region Properties

  public FeatCategoryEnum Category { get; set; }

  public bool Repeatable { get; set; }

  /// <summary>
  ///   Expression that has to be true to select the feat (e.g. <c>level &gt;= 4 &amp;&amp; score('cha') &gt;= 13</c>)
  /// </summary>
  public string? PrerequisiteExpr { get; set; }

  public ICollection<Feature> Features { get; set; } = new List<Feature>();

  #endregion
}
