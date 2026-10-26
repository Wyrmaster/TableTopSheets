using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Filters of the item list</summary>
public sealed record ItemFilter
{
  public ItemKindEnum? Kind { get; init; }

  /// <summary>Weapon category (<c>simple</c>, <c>martial</c>), armor category (<c>light</c> ...) or tool group</summary>
  public string? Category { get; init; }

  public bool? Magic { get; init; }

  public bool? Attunement { get; init; }
}
