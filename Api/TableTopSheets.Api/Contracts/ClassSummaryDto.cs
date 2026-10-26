using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Class in a list</summary>
public sealed record ClassSummaryDto(string Key, string Name, DiceEnum HitDie, IReadOnlyList<string> PrimaryAbilities, PrimaryAbilityModeEnum PrimaryAbilityMode, int SubclassLevel, IReadOnlyList<SubclassSummaryDto> Subclasses);
