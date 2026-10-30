using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Class in a list</summary>
public sealed record ClassSummaryDto(string Key, string Name, DiceEnum HitDie, IReadOnlyList<string> PrimaryAbilities, PrimaryAbilityModeEnum PrimaryAbilityMode, int SubclassLevel, IReadOnlyList<SubclassSummaryDto> Subclasses)
{
  /// <summary>Picture (<c>/api/images/class/{key}</c>); null when there is none</summary>
  public string? ImageUrl { get; init; }

  /// <summary>Small version of the picture for lists and cards; null when there is none</summary>
  public string? ThumbnailUrl { get; init; }
}
