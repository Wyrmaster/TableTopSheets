using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Filters of the spell list</summary>
public sealed record SpellFilter
{
  /// <summary>Class spell list, e.g. <c>wizard</c></summary>
  public string? Class { get; init; }

  public int? Level { get; init; }

  public int? MinLevel { get; init; }

  public int? MaxLevel { get; init; }

  /// <summary>School key, e.g. <c>evocation</c></summary>
  public string? School { get; init; }

  public bool? Ritual { get; init; }

  public bool? Concentration { get; init; }

  /// <summary>Only spells that deal (or don't deal) damage</summary>
  public bool? Damage { get; init; }

  public DamageTypeEnum? DamageType { get; init; }

  /// <summary>Text the casting time contains, e.g. <c>reaction</c></summary>
  public string? CastingTime { get; init; }
}
