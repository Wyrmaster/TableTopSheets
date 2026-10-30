using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Filters of the feat list</summary>
public sealed record FeatFilter
{
  /// <summary><c>origin</c>, <c>general</c>, <c>fighting-style</c> or <c>epic-boon</c></summary>
  public string? Category { get; init; }

  public bool? Repeatable { get; init; }

  public bool? HasPrerequisite { get; init; }
}
