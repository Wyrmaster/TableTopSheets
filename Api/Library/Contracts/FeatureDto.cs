using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Api.Contracts;

/// <summary>A feature (class feature, species trait, feat benefit ...) with the choices it offers</summary>
public sealed record FeatureDto(string Key, string Name, int? Level, string Description, IReadOnlyList<ChoiceDefinitionDto> Choices)
{
  /// <summary>Structure of the description (paragraphs, headings, lists, tables) for rich display</summary>
  public IReadOnlyList<TextBlock> DescriptionBlocks { get; init; } = [];
}
