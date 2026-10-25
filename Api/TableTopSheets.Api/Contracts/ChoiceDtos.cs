using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>
///   How a client should render a choice
/// </summary>
public enum ChoiceInputKind
{
  /// <summary>Pick exactly one option (radio buttons, dropdown)</summary>
  SingleSelect,

  /// <summary>Pick several different options (checkboxes)</summary>
  MultiSelect,

  /// <summary>Distribute points over options; an option can be picked up to <c>maxPerOption</c> times (counters)</summary>
  Distribution,
}

/// <summary>
///   Limits of a choice instance, enough for a client to validate before sending
/// </summary>
/// <param name="Min">selections that have to be sent before the character is complete</param>
/// <param name="Max">selections that may still be sent</param>
/// <param name="MaxPerOption">how often one option may be picked</param>
/// <param name="AllowDuplicates">whether an option may be picked more than once</param>
/// <param name="AllowedPatterns">for distributions: the allowed splits of the remaining picks, e.g. <c>[[2,1],[1,1,1]]</c></param>
public sealed record ChoiceConstraintsDto(int Min, int Max, int MaxPerOption, bool AllowDuplicates, IReadOnlyList<IReadOnlyList<int>>? AllowedPatterns);

/// <summary>
///   A choice instance that still needs selections
/// </summary>
public sealed record PendingChoiceDto
{
  /// <summary>Stable id of the instance (<c>choiceKey</c> or <c>choiceKey@parentRef</c>)</summary>
  public required string Id { get; init; }

  public required string ChoiceKey { get; init; }

  public required string Name { get; init; }

  /// <summary>Feature that offers the choice, e.g. "Ability Score Improvement (Wizard 4)"</summary>
  public required string Source { get; init; }

  public required string Description { get; init; }

  /// <summary>Send back as <c>parentRef</c> of the selections (null for top level choices)</summary>
  public string? ParentRef { get; init; }

  public required ChoiceInputKind Input { get; init; }

  public required OptionSourceEnum OptionSource { get; init; }

  public required bool Optional { get; init; }

  public required int Total { get; init; }

  public required int Selected { get; init; }

  public required int Remaining { get; init; }

  public required ChoiceConstraintsDto Constraints { get; init; }

  /// <summary>Filters the options endpoint accepts for this choice</summary>
  public required IReadOnlyList<string> OptionFilters { get; init; }

  /// <summary>Number of options that can be selected right now</summary>
  public required int OptionCount { get; init; }

  /// <summary>Embedded available options when there are few; otherwise null and <see cref="OptionsUrl"/> lists them</summary>
  public IReadOnlyList<OptionDto>? Options { get; init; }

  /// <summary>Paged, filterable list of the options</summary>
  public required string OptionsUrl { get; init; }

  /// <summary>Selections already made for this instance</summary>
  public required IReadOnlyList<SelectedOptionDto> SelectedOptions { get; init; }
}

/// <summary>A selection made for a choice instance</summary>
public sealed record SelectedOptionDto(Guid SelectionId, string OptionKey, string Name);

/// <summary>
///   An option of a choice
/// </summary>
public sealed record OptionDto
{
  public required string Key { get; init; }

  public required string Name { get; init; }

  /// <summary>First sentence of the description</summary>
  public string? Summary { get; init; }

  /// <summary>Full description (only when requested)</summary>
  public string? Description { get; init; }

  /// <summary>True when the option exists but can't be selected right now</summary>
  public bool Disabled { get; init; }

  public string? DisabledReason { get; init; }

  /// <summary>Names of the features the option grants</summary>
  public IReadOnlyList<string> Grants { get; init; } = [];

  public required OptionMetaDto Meta { get; init; }
}

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
