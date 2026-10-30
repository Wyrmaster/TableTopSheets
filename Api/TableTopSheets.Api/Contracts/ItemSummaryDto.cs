using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Item in a list; cost and weight are per <c>bundle</c> (20 arrows for 1 GP), quantities count pieces</summary>
public sealed record ItemSummaryDto
(
  string Key,
  string Name,
  ItemKindEnum Kind,
  string? Category,
  int CostCp,
  double Weight,
  bool Magic,
  bool RequiresAttunement,
  int Bundle
)
{
  /// <summary>Picture (<c>/api/images/item/{key}</c>); null when there is none</summary>
  public string? ImageUrl { get; init; }

  /// <summary>Small version of the picture for lists and rows; null when there is none</summary>
  public string? ThumbnailUrl { get; init; }
}
