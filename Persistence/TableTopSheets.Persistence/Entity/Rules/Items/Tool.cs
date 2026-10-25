namespace TableTopSheets.Persistence.Entity.Rules.Items;

public class Tool: Item
{
  #region Properties

  /// <summary>
  ///   Tool group (<c>artisan</c>, <c>gaming-set</c>, <c>musical-instrument</c>, <c>other</c>)
  /// </summary>
  public string Group { get; set; } = "other";

  #endregion
}
