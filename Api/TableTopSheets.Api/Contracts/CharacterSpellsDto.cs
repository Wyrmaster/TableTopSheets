using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>
///   Spells of a character: one entry per spellcasting class, plus spells granted outside a class (species, feats)
/// </summary>
/// <param name="Classes">spellcasting classes with their spells</param>
/// <param name="Other">spells granted by species traits or feats (Elven Lineage, Magic Initiate …)</param>
public sealed record CharacterSpellsDto(IReadOnlyList<SpellcastingClassDto> Classes, IReadOnlyList<KnownSpellDto> Other);
