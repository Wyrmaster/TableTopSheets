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
);
