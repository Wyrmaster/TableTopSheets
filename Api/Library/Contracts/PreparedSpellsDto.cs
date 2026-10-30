using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Long rest loadout of prepared spells of a class</summary>
public sealed record PreparedSpellsDto(IReadOnlyList<string> SpellKeys);
