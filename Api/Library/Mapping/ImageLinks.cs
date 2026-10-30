using TableTopSheets.Api.Contracts;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Mapping;

/// <summary>
///   Routes of the images (<c>/api/images/{category}/{key}</c>) and the image links of rule contracts
/// </summary>
public static class ImageLinks
{
  #region Methods

  /// <summary>
  ///   Route segment of a category: <c>spell</c>, <c>class</c>, <c>species</c>, <c>background</c>, <c>scene</c>,
  ///   <c>item</c>, <c>coin</c>
  /// </summary>
  public static string Segment(ImageCategoryEnum category) => category
    .ToString()
    .ToLowerInvariant();

  public static string Url(ImageCategoryEnum category, string key, bool thumbnail = false)
    => $"/api/images/{Segment(category)}/{Uri.EscapeDataString(key)}" + (thumbnail ? "?thumb=true" : string.Empty);

  /// <param name="owners">keys that have an image in the category</param>
  public static SpellSummaryDto WithImage(this SpellSummaryDto dto, IReadOnlySet<string> owners)
    => owners.Contains(dto.Key)
      ? dto with
      {
        ImageUrl = Url(ImageCategoryEnum.Spell, dto.Key),
        ThumbnailUrl = Url(ImageCategoryEnum.Spell, dto.Key, true),
      }
      : dto;

  /// <param name="owners">keys that have an image in the category</param>
  public static SpeciesSummaryDto WithImage(this SpeciesSummaryDto dto, IReadOnlySet<string> owners)
    => owners.Contains(dto.Key)
      ? dto with
      {
        ImageUrl = Url(ImageCategoryEnum.Species, dto.Key),
        ThumbnailUrl = Url(ImageCategoryEnum.Species, dto.Key, true),
      }
      : dto;

  /// <param name="owners">keys that have an image in the category</param>
  public static BackgroundSummaryDto WithImage(this BackgroundSummaryDto dto, IReadOnlySet<string> owners)
    => owners.Contains(dto.Key)
      ? dto with
      {
        ImageUrl = Url(ImageCategoryEnum.Background, dto.Key),
        ThumbnailUrl = Url(ImageCategoryEnum.Background, dto.Key, true),
      }
      : dto;

  /// <param name="owners">keys that have an image in the category</param>
  public static ClassSummaryDto WithImage(this ClassSummaryDto dto, IReadOnlySet<string> owners)
    => owners.Contains(dto.Key)
      ? dto with
      {
        ImageUrl = Url(ImageCategoryEnum.Class, dto.Key),
        ThumbnailUrl = Url(ImageCategoryEnum.Class, dto.Key, true),
      }
      : dto;

  /// <param name="owners">keys that have an image in the category</param>
  public static ItemSummaryDto WithImage(this ItemSummaryDto dto, IReadOnlySet<string> owners)
    => owners.Contains(dto.Key)
      ? dto with
      {
        ImageUrl = Url(ImageCategoryEnum.Item, dto.Key),
        ThumbnailUrl = Url(ImageCategoryEnum.Item, dto.Key, true),
      }
      : dto;

  /// <param name="category">category of the entity (species or background)</param>
  /// <param name="owners">keys that have an image in the category</param>
  public static RuleDetailDto WithImage(this RuleDetailDto dto, ImageCategoryEnum category, IReadOnlySet<string> owners)
    => owners.Contains(dto.Key)
      ? dto with
      {
        ImageUrl = Url(category, dto.Key),
        ThumbnailUrl = Url(category, dto.Key, true),
      }
      : dto;

  #endregion
}
