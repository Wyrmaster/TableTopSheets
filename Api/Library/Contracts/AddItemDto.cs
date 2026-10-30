using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Add an item to the inventory; <c>purchase</c> pays its cost from the coins (with change, 422 when too expensive)</summary>
public sealed record AddItemDto(string ItemKey, int Quantity = 1, bool Purchase = false);
