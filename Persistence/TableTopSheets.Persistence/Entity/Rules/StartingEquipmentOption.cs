namespace TableTopSheets.Persistence.Entity.Rules;

/// <summary>
///   One alternative of the starting equipment of a background or class ("Choose A or B"); stored as JSON with its owner
/// </summary>
public sealed class StartingEquipmentOption
{
  #region Properties

  /// <summary>
  ///   Letter of the alternative (<c>a</c>, <c>b</c>, <c>c</c>)
  /// </summary>
  public required string Key { get; set; }

  public List<StartingEquipmentEntry> Items { get; set; } = [];

  /// <summary>
  ///   Coins of the alternative in copper pieces (paid out as gold pieces where possible)
  /// </summary>
  public int CoinsCp { get; set; }

  #endregion
}
