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
