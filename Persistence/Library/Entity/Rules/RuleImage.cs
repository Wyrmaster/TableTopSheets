using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Persistence.Entity.Rules;

/// <summary>
///   Artwork of a rule entity (spell, class, species, background) or a scene, written by the ruleset migrator from
///   <c>Data/Images</c>
/// </summary>
public class RuleImage: BaseEntity
{
  #region Properties

  public ImageCategoryEnum Category { get; set; }

  /// <summary>
  ///   Key of the rule entity the image belongs to (<c>fireball</c>, <c>wizard</c>) or of the scene (<c>tavern</c>)
  /// </summary>
  public required string OwnerKey { get; set; }

  /// <summary>
  ///   Readable name, used as alternative text (<c>Dragon battle</c>)
  /// </summary>
  public string Title { get; set; } = string.Empty;

  public string ContentType { get; set; } = "image/webp";

  public int Width { get; set; }

  public int Height { get; set; }

  public byte[] Data { get; set; } = [];

  /// <summary>
  ///   Smaller version for lists and cards
  /// </summary>
  public byte[] ThumbnailData { get; set; } = [];

  /// <summary>
  ///   SHA-256 (hex) over image and thumbnail; the migrator rewrites the row only when it changes, the API sends it as ETag
  /// </summary>
  public required string ContentHash { get; set; }

  #endregion
}
