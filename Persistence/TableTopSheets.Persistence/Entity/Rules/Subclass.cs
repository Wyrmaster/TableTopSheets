namespace TableTopSheets.Persistence.Entity.Rules;

public class Subclass: RuleEntity
{
  #region Properties

  public Guid ClassId { get; set; }

  public CharacterClass? Class { get; set; }

  public ICollection<Feature> Features { get; set; } = new List<Feature>();

  #endregion
}
