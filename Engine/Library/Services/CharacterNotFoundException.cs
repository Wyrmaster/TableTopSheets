namespace TableTopSheets.Engine.Services;

/// <summary>
///   The requested character doesn't exist
/// </summary>
public sealed class CharacterNotFoundException(Guid characterId)
  : RuleViolationException($"Character {characterId} doesn't exist")
{
  public Guid CharacterId { get; } = characterId;
}
