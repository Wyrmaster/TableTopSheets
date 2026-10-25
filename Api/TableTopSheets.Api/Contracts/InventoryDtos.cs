using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Add an item to the inventory</summary>
public sealed record AddItemDto(string ItemKey, int Quantity = 1);

/// <summary>Put an item into a slot (<c>None</c> unequips it)</summary>
public sealed record EquipDto(EquipSlotEnum Slot);

/// <summary>Attune to an item or end the attunement</summary>
public sealed record AttuneDto(bool Attuned);

/// <summary>Switch a toggleable feature (Rage, Mage Armor ...)</summary>
public sealed record ToggleDto(bool Active);

/// <summary>Select the armor class calculation by its label; null takes the highest</summary>
public sealed record ArmorClassDto(string? Label);

/// <summary>Long rest loadout of prepared spells of a class</summary>
public sealed record PreparedSpellsDto(IReadOnlyList<string> SpellKeys);

/// <summary>Copy a spell into the spellbook</summary>
public sealed record SpellbookEntryDto(string SpellKey, string? Note = null);

/// <summary>Change a selection on a rest</summary>
public sealed record RestReplaceDto(string OptionKey);
