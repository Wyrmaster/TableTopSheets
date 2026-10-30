using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

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
