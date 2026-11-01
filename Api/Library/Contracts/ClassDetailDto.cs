using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Api.Contracts;

/// <summary>Class with its features per level, subclasses and the starting equipment of a character that starts with it</summary>
public sealed record ClassDetailDto
(
  ClassSummaryDto Class,
  string Description,
  IReadOnlyList<FeatureDto> Features,
  IReadOnlyList<RuleDetailDto> Subclasses,
  IReadOnlyList<StartingEquipmentOptionDto> StartingEquipment
)
{
  /// <summary>Structure of the description (paragraphs, headings, lists, tables) for rich display</summary>
  public IReadOnlyList<TextBlock> DescriptionBlocks { get; init; } = [];
}
