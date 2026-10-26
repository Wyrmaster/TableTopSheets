using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

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
