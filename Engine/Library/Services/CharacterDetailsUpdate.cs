using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Engine.Services;

/// <summary>
///   Descriptive fields of a character; null keeps the current value
/// </summary>
public sealed record CharacterDetailsUpdate
{
  public string? Name { get; init; }

  public AlignmentEnum? Alignment { get; init; }

  public string? Appearance { get; init; }

  public string? Backstory { get; init; }

  public int? Experience { get; init; }
}
