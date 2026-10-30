using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Spell with its full text</summary>
public sealed record SpellDetailDto(SpellSummaryDto Spell, string Components, string? Material, string Duration, bool AttackRoll, string? SaveAbility, string Description);
