using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Put an item into a slot (<c>None</c> unequips it)</summary>
public sealed record EquipDto(EquipSlotEnum Slot);
