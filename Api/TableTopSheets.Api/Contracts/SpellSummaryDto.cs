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
);
