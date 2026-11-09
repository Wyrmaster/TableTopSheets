using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Api.Contracts;

/// <summary>Mastery property of a weapon (Nick, Sap, …) with its rule text</summary>
public sealed record WeaponMasteryDto(string Key, string Name, string Description)
{
  /// <summary>Structure of the description (paragraphs, headings, lists, tables) for rich display</summary>
  public IReadOnlyList<TextBlock> DescriptionBlocks { get; init; } = [];
}
