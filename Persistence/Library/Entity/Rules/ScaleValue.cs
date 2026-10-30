namespace TableTopSheets.Persistence.Entity.Rules;

/// <summary>
///   One cell of a class table column (e.g. <c>wizard.cantrips</c> at level 4 = 4)
/// </summary>
public class ScaleValue: BaseEntity
{
  #region Properties

  public required string Key { get; set; }

  public byte Level { get; set; }

  /// <summary>
  ///   Number or dice (e.g. <c>3</c>, <c>1d6</c>)
  /// </summary>
  public required string Value { get; set; }

  public Guid? ClassId { get; set; }

  public CharacterClass? Class { get; set; }

  #endregion
}
