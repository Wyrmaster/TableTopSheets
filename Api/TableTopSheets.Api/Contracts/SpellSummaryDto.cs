using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Spell in a list</summary>
public sealed record SpellSummaryDto
(
  string Key,
  string Name,
  int Level,
  string School,
  string CastingTime,
  string Range,
  bool Ritual,
  bool Concentration,
  string? Damage,
  DamageTypeEnum? DamageType,
  IReadOnlyList<string> Lists
)
{
  /// <summary>Picture (<c>/api/images/spell/{key}</c>); null when there is none</summary>
  public string? ImageUrl { get; init; }

  /// <summary>Small version of the picture for lists and cards; null when there is none</summary>
  public string? ThumbnailUrl { get; init; }
}
