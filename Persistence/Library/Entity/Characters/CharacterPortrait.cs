namespace TableTopSheets.Persistence.Entity.Characters;

/// <summary>
///   Picture of a character, uploaded by its owner; kept apart from <see cref="Rules.RuleImage" /> because the ruleset
///   migrator owns those rows. Its hash is <see cref="Characters.Character.PortraitHash" />, so loading a character doesn't
///   load the picture
/// </summary>
public class CharacterPortrait: BaseEntity
{
  #region Properties

  public Guid CharacterId { get; set; }

  public Character? Character { get; set; }

  public string ContentType { get; set; } = "image/jpeg";

  public byte[] Data { get; set; } = [];

  #endregion
}
