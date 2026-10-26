using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

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
