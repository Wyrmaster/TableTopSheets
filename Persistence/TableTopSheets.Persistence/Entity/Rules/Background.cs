namespace TableTopSheets.Persistence.Entity.Rules;

/// <summary>
///   Background of a character; ability increases, skills, tools and the origin feat are granted through <see cref="Features"/>
/// </summary>
public class Background: RuleEntity
{
  #region Properties

  /// <summary>
  ///   Ability keys the background may increase
  /// </summary>
  public List<string> AbilityOptions { get; set; } = [];

  public Guid? OriginFeatId { get; set; }

  public Feat? OriginFeat { get; set; }

  public ICollection<Feature> Features { get; set; } = new List<Feature>();

  #endregion
}
