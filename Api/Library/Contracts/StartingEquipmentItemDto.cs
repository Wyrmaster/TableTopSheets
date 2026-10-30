using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>An item of a starting equipment alternative: a fixed item (<c>itemKey</c>) or a pick from <c>options</c></summary>
/// <param name="Key">entry key; picks are sent keyed by it</param>
/// <param name="Name">item name, or the name of the pick (<c>Holy Symbol</c>)</param>
/// <param name="ItemKey">the fixed item; for a pick the item already chosen through <c>sameAs</c>, otherwise null</param>
/// <param name="Quantity">pieces (2 daggers, 20 arrows)</param>
/// <param name="Note">note stored with the item (<c>prayers</c>)</param>
/// <param name="Options">items to pick from (empty for fixed items)</param>
/// <param name="SameAs">choice the pick has to match ("same as above"); a pick answers it while it's open</param>
public sealed record StartingEquipmentItemDto
(
  string Key,
  string Name,
  string? ItemKey,
  int Quantity,
  string? Note,
  IReadOnlyList<ItemOptionDto> Options,
  string? SameAs
);
