using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Api.Contracts;

/// <summary>Full description of a rule entity with its features; a background also lists its starting equipment (otherwise empty)</summary>
public sealed record RuleDetailDto
(
  string Key,
  string Name,
  string Description,
  IReadOnlyList<FeatureDto> Features,
  IReadOnlyList<StartingEquipmentOptionDto> StartingEquipment
)
{
  /// <summary>Picture of a species or background; null when there is none (feats and subclasses have none)</summary>
  public string? ImageUrl { get; init; }

  /// <summary>Structure of the description (paragraphs, headings, lists, tables) for rich display</summary>
  public IReadOnlyList<TextBlock> DescriptionBlocks { get; init; } = [];

  /// <summary>Small version of the picture for lists and cards; null when there is none</summary>
  public string? ThumbnailUrl { get; init; }
}
