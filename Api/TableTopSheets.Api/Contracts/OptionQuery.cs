using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Filters for the options of a choice</summary>
public sealed record OptionQuery
{
  /// <summary>Parent selection of the choice instance (the <c>parentRef</c> of the pending choice)</summary>
  public string? ParentRef { get; init; }

  /// <summary>Evaluate against a level up preview: <c>class</c> or <c>class:subclass</c></summary>
  public string? Preview { get; init; }

  public int? Level { get; init; }

  public int? MinLevel { get; init; }

  public int? MaxLevel { get; init; }

  public string? School { get; init; }

  public bool? Ritual { get; init; }

  public bool? Damage { get; init; }

  public string? Category { get; init; }

  /// <summary>Also list options that exist but can't be selected right now (with the reason)</summary>
  public bool? IncludeDisabled { get; init; }

  /// <summary>Include the full description of each option</summary>
  public bool? IncludeDescriptions { get; init; }
}
