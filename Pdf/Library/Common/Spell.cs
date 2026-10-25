namespace TableTopSheets.Pdf.Common;

/// <summary>
///   Spell container of a spell data to render
/// </summary>
public record Spell
{
  #region Properties

  /// <summary>
  ///   Name of a Spell
  /// </summary>
  public string Name { get; set; } = string.Empty;

  /// <summary>
  ///   Notes of a Spell
  /// </summary>
  public string Notes { get; set; } = string.Empty;

  /// <summary>
  ///   Level of a Spell
  /// </summary>
  public string Level { get; set; } = string.Empty;

  /// <summary>
  ///   Casting Time of a Spell
  /// </summary>
  public string CastingTime { get; set; } = string.Empty;

  /// <summary>
  ///   Range of a Spell
  /// </summary>
  public string Range { get; set; }

  /// <summary>
  ///   Flag to show if the spell needs concentration
  /// </summary>
  public bool Concentration { get; set; }
  
  /// <summary>
  ///   Flag to show if a Spell can be cast as a ritual
  /// </summary>
  public bool Ritual { get; set; }
  
  /// <summary>
  ///   Flag to show if a Spell needs materials
  /// </summary>
  public bool Material { get; set; }

  #endregion
}