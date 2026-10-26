using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Api.Contracts;

/// <summary>Descriptive fields; omitted fields keep their value</summary>
public sealed record UpdateCharacterDto
{
  public string? Name { get; init; }

  public AlignmentEnum? Alignment { get; init; }

  public string? Appearance { get; init; }

  public string? Backstory { get; init; }

  public int? Experience { get; init; }
}
