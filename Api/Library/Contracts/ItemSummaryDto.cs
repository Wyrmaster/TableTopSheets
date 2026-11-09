using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>
///   Item in a list; cost and weight are per <c>bundle</c> (20 arrows for 1 GP), quantities count pieces. <c>rarity</c> is
///   null for mundane items; <c>priceless</c> items (artifacts) can't be bought, their <c>costCp</c> is 0
/// </summary>
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
  int Bundle,
  RarityEnum? Rarity,
  bool Priceless
)
{
  /// <summary>Picture (<c>/api/images/item/{key}</c>); null when there is none</summary>
  public string? ImageUrl { get; init; }

  /// <summary>Small version of the picture for lists and rows; null when there is none</summary>
  public string? ThumbnailUrl { get; init; }

  /// <summary>Key of the weapon's mastery property (<c>/api/rules/masteries/{key}</c>); null for items without one</summary>
  public string? MasteryKey { get; init; }

  /// <summary>Name of the weapon's mastery property; null for items without one</summary>
  public string? MasteryName { get; init; }

  /// <summary>
  ///   Key of the mundane item a magic variant is made from (<c>longsword</c> for a +1 Longsword); null for mundane items.
  ///   Proficiency and mastery follow it
  /// </summary>
  public string? BaseKey { get; init; }
}
