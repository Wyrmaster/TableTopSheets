using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace TableTopSheets.Seed;

public static partial class SeedKeys
{
  #region Methods

  /// <summary>
  ///   Key of a name: lower case, apostrophes removed, everything else that is not a letter or digit becomes a dash
  ///   (<c>Bigby's Hand</c> → <c>bigbys-hand</c>)
  /// </summary>
  public static string Slug(string name)
  {
    string text = name
      .ToLowerInvariant()
      .Replace("'", string.Empty)
      .Replace("’", string.Empty);
    return NonAlphaNumericRegex()
      .Replace(text, "-")
      .Trim('-');
  }

  /// <summary>
  ///   Deterministic id of a seeded entity so re-seeding keeps references of characters intact
  /// </summary>
  public static Guid Id(string type, string key)
  {
    byte[] hash = SHA1.HashData(Encoding.UTF8.GetBytes($"tabletopsheets:{type}:{key}"));
    byte[] bytes = hash[..16];
    bytes[6] = (byte)((bytes[6] & 0x0F) | 0x50);
    bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);
    return new Guid(bytes, bigEndian: true);
  }

  [GeneratedRegex("[^a-z0-9]+")]
  private static partial Regex NonAlphaNumericRegex();

  #endregion
}
