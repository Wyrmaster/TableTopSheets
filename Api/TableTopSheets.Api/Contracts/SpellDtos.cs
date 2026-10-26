using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>
///   Spells of a character: one entry per spellcasting class, plus spells granted outside a class (species, feats)
/// </summary>
/// <param name="Classes">spellcasting classes with their spells</param>
/// <param name="Other">spells granted by species traits or feats (Elven Lineage, Magic Initiate …)</param>
public sealed record CharacterSpellsDto(IReadOnlyList<SpellcastingClassDto> Classes, IReadOnlyList<KnownSpellDto> Other);

/// <summary>
///   Spellcasting of one class with every spell it knows, has in the spellbook or has prepared
/// </summary>
/// <param name="ClassKey">class key, e.g. <c>wizard</c></param>
/// <param name="ClassName">class name</param>
/// <param name="Ability">spellcasting ability key</param>
/// <param name="Pool">where prepared spells come from: the class spell list or the spellbook</param>
/// <param name="PrepareChange">when the prepared spells change: after a long rest (PUT prepared-spells) or on a level up (choices)</param>
/// <param name="RitualMode">how rituals are cast: not at all, when prepared, or from the spellbook</param>
/// <param name="SpellList">key of the class spell list (filter <c>class</c> of the spell list route)</param>
/// <param name="MaxSpellLevel">highest spell level the class can prepare</param>
/// <param name="PreparedLimit">how many spells can be prepared (always prepared spells don't count)</param>
/// <param name="PreparedCount">spells prepared now (without always prepared spells and cantrips)</param>
/// <param name="CantripLimit">how many cantrips the class knows</param>
/// <param name="Spells">cantrips, prepared, always prepared and spellbook spells of the class</param>
public sealed record SpellcastingClassDto
(
  string ClassKey,
  string ClassName,
  string Ability,
  SpellPoolEnum Pool,
  PrepareChangeEnum PrepareChange,
  RitualModeEnum RitualMode,
  string SpellList,
  int MaxSpellLevel,
  int PreparedLimit,
  int PreparedCount,
  int CantripLimit,
  IReadOnlyList<KnownSpellDto> Spells
);

/// <summary>
///   A spell the character knows, has prepared or has in the spellbook
/// </summary>
/// <param name="Key">spell key</param>
/// <param name="Name">spell name</param>
/// <param name="Level">spell level (0 for cantrips)</param>
/// <param name="School">school key, e.g. <c>evocation</c></param>
/// <param name="CastingTime">casting time</param>
/// <param name="Ritual">can be cast as a ritual</param>
/// <param name="Concentration">needs concentration</param>
/// <param name="Prepared">part of the prepared list (cantrips are always prepared)</param>
/// <param name="AlwaysPrepared">prepared without counting against the limit (subclass or feat spells)</param>
/// <param name="InSpellbook">written in the spellbook</param>
/// <param name="Source">class or feature the spell comes from</param>
/// <param name="Ability">spellcasting ability key used for the spell</param>
public sealed record KnownSpellDto
(
  string Key,
  string Name,
  int Level,
  string School,
  string CastingTime,
  bool Ritual,
  bool Concentration,
  bool Prepared,
  bool AlwaysPrepared,
  bool InSpellbook,
  string Source,
  string Ability
);
