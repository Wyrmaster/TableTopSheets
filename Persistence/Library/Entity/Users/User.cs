using TableTopSheets.Persistence.Entity.Characters;

namespace TableTopSheets.Persistence.Entity.Users;

/// <summary>
///   Owner of characters; signed in with a password (database authentication) or by an external identity provider (OIDC)
/// </summary>
public class User: BaseEntity
{
  #region Properties

  public required string UserName { get; set; }

  public string? Email { get; set; }

  /// <summary>
  ///   Salted password hash (null for users of an external identity provider)
  /// </summary>
  public string? PasswordHash { get; set; }

  /// <summary>
  ///   Issuer (<c>iss</c>) of the external identity provider
  /// </summary>
  public string? ExternalIssuer { get; set; }

  /// <summary>
  ///   Subject (<c>sub</c>) of the user at the external identity provider
  /// </summary>
  public string? ExternalSubject { get; set; }

  public DateTime CreatedAt { get; set; }

  #endregion

  #region Navigation

  public ICollection<Character> Characters { get; set; } = new List<Character>();

  #endregion
}
