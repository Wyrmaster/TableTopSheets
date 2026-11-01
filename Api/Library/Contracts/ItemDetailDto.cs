using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Api.Contracts;

/// <summary>Item with its full text and properties</summary>
public sealed record ItemDetailDto(ItemSummaryDto Item, string Description, IReadOnlyDictionary<string, string> Properties, IReadOnlyList<FeatureDto> Features)
{
  /// <summary>Structure of the description (paragraphs, headings, lists, tables) for rich display</summary>
  public IReadOnlyList<TextBlock> DescriptionBlocks { get; init; } = [];
}
