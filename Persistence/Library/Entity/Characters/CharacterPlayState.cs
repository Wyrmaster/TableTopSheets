namespace TableTopSheets.Persistence.Entity.Characters;

/// <summary>
///   Values that change during play and never influence the rules calculation (owned by <see cref="Character"/>)
/// </summary>
public class CharacterPlayState
{
  #region Properties

  /// <summary>
  ///   Current hit points (null = full)
  /// </summary>
  public short? CurrentHitPoints { get; set; }

  public short TemporaryHitPoints { get; set; }

  public byte Exhaustion { get; set; }

  public bool HeroicInspiration { get; set; }

  public List<string> Conditions { get; set; } = [];

  public int CopperPieces { get; set; }

  public int SilverPieces { get; set; }

  public int ElectrumPieces { get; set; }

  public int GoldPieces { get; set; }

  public int PlatinumPieces { get; set; }

  #endregion
}
