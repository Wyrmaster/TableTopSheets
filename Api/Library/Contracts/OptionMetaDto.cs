using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Typed details of an option; which fields are set depends on the option source</summary>
/// <param name="Level">spell level or feature level</param>
/// <param name="School">spell school key</param>
/// <param name="Ritual">spell can be cast as a ritual</param>
/// <param name="Damage">spell deals damage</param>
/// <param name="Category">feat category, language rarity, tool group, weapon category ...</param>
/// <param name="Value">value the option carries (e.g. the damage type of a draconic ancestry)</param>
/// <param name="Repeatable">feat can be taken more than once</param>
/// <param name="Prerequisite">prerequisite as rule expression (e.g. <c>score('cha') &gt;= 13</c>)</param>
/// <param name="Lists">class spell lists of a spell</param>
public sealed record OptionMetaDto
(
  int? Level = null,
  string? School = null,
  bool? Ritual = null,
  bool? Damage = null,
  string? Category = null,
  string? Value = null,
  bool? Repeatable = null,
  string? Prerequisite = null,
  IReadOnlyList<string>? Lists = null
);
