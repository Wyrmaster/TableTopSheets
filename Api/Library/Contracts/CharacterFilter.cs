using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Filters of the character list</summary>
public sealed record CharacterFilter
{
  /// <summary>Has levels in this class</summary>
  public string? Class { get; init; }

  public string? Species { get; init; }

  public int? MinLevel { get; init; }

  public int? MaxLevel { get; init; }
}
