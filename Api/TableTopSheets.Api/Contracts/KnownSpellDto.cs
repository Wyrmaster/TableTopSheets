using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

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
