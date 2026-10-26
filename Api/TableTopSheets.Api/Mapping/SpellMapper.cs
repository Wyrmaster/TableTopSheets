using TableTopSheets.Api.Contracts;
using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Api.Mapping;

/// <summary>
///   Known, prepared and spellbook spells of an evaluated character
/// </summary>
public static class SpellMapper
{
  #region Methods

  public static CharacterSpellsDto Spells(CharacterEvaluation evaluation)
  {
    IReadOnlyList<KnownSpell> known = evaluation.KnownSpells();
    List<SpellcastingClassDto> classes = evaluation.SpellcastingProfiles
      .Select(profile => Class(evaluation, profile, known.Where(entry => entry.Profile?.Id == profile.Id)))
      .OrderBy(entry => entry.ClassName)
      .ToList();
    List<KnownSpellDto> other = known
      .Where(entry => entry.Profile == null)
      .Select(Spell)
      .OrderBy(entry => entry.Level)
      .ThenBy(entry => entry.Name)
      .ToList();
    return new CharacterSpellsDto(classes, other);
  }

  private static SpellcastingClassDto Class(CharacterEvaluation evaluation, SpellcastingProfile profile, IEnumerable<KnownSpell> spells)
  {
    List<KnownSpellDto> list = spells
      .Select(Spell)
      .OrderBy(entry => entry.Level)
      .ThenBy(entry => entry.Name)
      .ToList();
    return new SpellcastingClassDto
    (
      evaluation.Catalog.Classes[profile.ClassId].Key,
      evaluation.Catalog.Classes[profile.ClassId].Name,
      profile.Ability,
      profile.Pool,
      profile.PrepareChange,
      profile.RitualMode,
      profile.SpellListKey,
      evaluation.MaxSpellLevel(profile),
      Count(evaluation, profile.PreparedCountKey),
      list.Count(entry => entry.Prepared && !entry.AlwaysPrepared && entry.Level > 0),
      Count(evaluation, profile.CantripCountKey),
      list
    );
  }

  private static int Count(CharacterEvaluation evaluation, string? scaleKey) => scaleKey == null ? 0 : (int)evaluation
    .Scale(scaleKey)
    .AsNumber();

  private static KnownSpellDto Spell(KnownSpell entry) => new
  (
    entry.Spell.Key,
    entry.Spell.Name,
    entry.Spell.Level,
    RuleMapper.SchoolKey(entry.Spell),
    entry.Spell.CastingTime,
    entry.Spell.Ritual,
    entry.Spell.Concentration,
    entry.Prepared || entry.Spell.Level == 0,
    entry.AlwaysPrepared,
    entry.InSpellbook,
    entry.Source,
    entry.Ability
  );

  #endregion
}
