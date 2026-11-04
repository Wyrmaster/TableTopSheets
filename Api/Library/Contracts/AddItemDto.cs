using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>
///   Add pieces of an item to the inventory (<c>quantity</c> 1 to 1000); <c>purchase</c> pays their cost from the coins
///   (with change, 422 when too expensive)
/// </summary>
public sealed record AddItemDto(string ItemKey, int Quantity = 1, bool Purchase = false);
