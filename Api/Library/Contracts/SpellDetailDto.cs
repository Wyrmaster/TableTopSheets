using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Api.Contracts;

/// <summary>Spell with its full text</summary>
public sealed record SpellDetailDto(SpellSummaryDto Spell, string Components, string? Material, string Duration, bool AttackRoll, string? SaveAbility, string Description)
{
  /// <summary>Structure of the description (paragraphs, headings, lists, tables) for rich display</summary>
  public IReadOnlyList<TextBlock> DescriptionBlocks { get; init; } = [];
}
