using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Characters;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Engine.Evaluation;

/// <summary>
///   A spell the character can cast or has in a spellbook
/// </summary>
/// <param name="Profile">spellcasting profile the spell belongs to (null for spells granted by species, feats ...)</param>
/// <param name="AlwaysPrepared">prepared without counting against the prepared limit</param>
/// <param name="Prepared">part of the prepared list (cantrips are always prepared)</param>
/// <param name="InSpellbook">written in a spellbook</param>
/// <param name="Ability">spellcasting ability key used for the spell</param>
public sealed record KnownSpell(Spell Spell, string Source, SpellcastingProfile? Profile, bool AlwaysPrepared, bool Prepared, bool InSpellbook, string Ability);
