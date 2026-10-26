using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Copy a spell into the spellbook</summary>
public sealed record SpellbookEntryDto(string SpellKey, string? Note = null);
