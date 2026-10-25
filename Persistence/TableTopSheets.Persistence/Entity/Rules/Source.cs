namespace TableTopSheets.Persistence.Entity.Rules;

/// <summary>
///   Rule book the data originates from
/// </summary>
public class Source: BaseEntity
{
  #region Properties

  public required string Key { get; set; }

  public required string Name { get; set; }

  #endregion
}
