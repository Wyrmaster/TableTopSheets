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

/// <summary>
///   An item of a starting equipment alternative: a fixed item, or a pick from <see cref="Options"/>
/// </summary>
public sealed class StartingEquipmentEntry
{
  #region Properties

  /// <summary>
  ///   Key of the entry inside its alternative; requests name the pick for this entry by it
  /// </summary>
  public required string Key { get; set; }

  /// <summary>
  ///   Display name (<c>Holy Symbol</c>, <c>Artisan's Tools</c>) for picks; null uses the item name
  /// </summary>
  public string? Name { get; set; }

  /// <summary>
  ///   Fixed item; null when the item is picked from <see cref="Options"/>
  /// </summary>
  public string? ItemKey { get; set; }

  /// <summary>
  ///   Item keys to pick from
  /// </summary>
  public List<string> Options { get; set; } = [];

  /// <summary>
  ///   Choice whose selection is the pick ("same as above": the tool chosen for the tool proficiency)
  /// </summary>
  public string? SameAs { get; set; }

  /// <summary>
  ///   Pieces (2 daggers, 20 arrows, 10 sheets of parchment)
  /// </summary>
  public int Quantity { get; set; } = 1;

  /// <summary>
  ///   Note stored with the item (<c>prayers</c> for a book of prayers)
  /// </summary>
  public string? Note { get; set; }

  #endregion
}
