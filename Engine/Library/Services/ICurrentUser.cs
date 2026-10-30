namespace TableTopSheets.Engine.Services;

/// <summary>
///   User the characters of a <see cref="CharacterService"/> belong to
/// </summary>
public interface ICurrentUser
{
  #region Properties

  Guid UserId { get; }

  #endregion
}
