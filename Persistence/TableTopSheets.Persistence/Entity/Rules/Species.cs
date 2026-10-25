namespace TableTopSheets.Persistence.Entity.Rules;

/// <summary>
///   Species of a character; all mechanics (speed, size, senses ...) are expressed through <see cref="Features"/>
/// </summary>
public class Species: RuleEntity
{
  #region Properties

  public string CreatureType { get; set; } = "Humanoid";

  public ICollection<Feature> Features { get; set; } = new List<Feature>();

  #endregion
}
