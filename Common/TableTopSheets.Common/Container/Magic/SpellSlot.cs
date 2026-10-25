using TableTopSheets.Common.Enum.Magic;

namespace TableTopSheets.Common.Container.Magic;

/// <summary>
///   Structure Describing a Spellslot
/// </summary>
public struct SpellSlot
{
  #region Constructor
  
  public SpellSlot()
  {
    Level = 0;
  }

  #endregion

  #region Properties

  /// <summary>
  ///   Spell slot level
  /// </summary>
  public required byte Level { get; set; }

  /// <summary>
  ///   Period a spellslot refreshes itself
  /// </summary>
  public required PeriodEnum RefreshPeriod { get; set; } = PeriodEnum.LongRest;
  
  /// <summary>
  ///   Id of the source the spellslot originated from
  /// </summary>
  public Guid SourceId { get; set; }

  #endregion
}