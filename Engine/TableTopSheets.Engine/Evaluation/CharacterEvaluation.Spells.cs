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
public sealed record KnownSpell(Spell Spell, string Source, SpellcastingProfile? Profile, bool AlwaysPrepared, bool Prepared, bool InSpellbook);

public sealed partial class CharacterEvaluation
{
  #region Constants

  /// <summary>
  ///   <c>Choice.Category</c> of spell choices that add cantrips
  /// </summary>
  public const string CantripCategory = "cantrip";

  /// <summary>
  ///   <c>Choice.Category</c> of spell choices that form the prepared list of a level up preparation class
  /// </summary>
  public const string PreparedCategory = "prepared";

  /// <summary>
  ///   <c>Choice.Category</c> of spell choices that write spells into the spellbook
  /// </summary>
  public const string SpellbookCategory = "spellbook";

  #endregion

  #region Methods

  /// <summary>
  ///   Class a feature instance counts for, following the chain of selections for option features (invocations ...)
  /// </summary>
  public CharacterClass? OwningClass(FeatureInstance instance)
  {
    CharacterClass? @class = this.Catalog.OwningClass(instance.Feature);
    if (@class != null || instance.GrantedBy == null)
    {
      return @class;
    }

    SelectionInstance? parent = this._selections.FirstOrDefault(entry => entry.Selection.Id == instance.GrantedBy.Id);
    return parent == null ? null : this.OwningClass(parent.Owner);
  }

  /// <summary>
  ///   Spells written in the spellbook (level up choices and copied spells)
  /// </summary>
  public IReadOnlySet<Guid> SpellbookSpellIds() => this._selections
    .Where(entry => entry.Choice.OptionSource == OptionSourceEnum.Spell && entry.Choice.Category == SpellbookCategory)
    .Select(entry => this.Catalog.SpellsByKey[entry.Selection.OptionKey].Id)
    .Concat(this.Character.SpellbookEntries.Select(entry => entry.SpellId))
    .ToHashSet();

  /// <summary>
  ///   Long rest loadout of a profile
  /// </summary>
  public IEnumerable<CharacterPreparedSpell> PreparedLoadout(SpellcastingProfile profile)
    => profile.PrepareChange == PrepareChangeEnum.LongRest
      ? this.Character.PreparedSpells.Where(entry => entry.ClassId == profile.ClassId)
      : [];

  /// <summary>
  ///   Every spell the character knows, has prepared or has in a spellbook
  /// </summary>
  public IReadOnlyList<KnownSpell> KnownSpells()
  {
    List<SpellcastingProfile> profiles = this.SpellcastingProfiles.ToList();
    Dictionary<(Guid Spell, Guid? Profile), KnownSpell> spells = new();

    void Add(Spell spell, string source, SpellcastingProfile? profile, bool alwaysPrepared = false, bool prepared = false, bool inSpellbook = false)
    {
      if (spells.TryGetValue((spell.Id, profile?.Id), out KnownSpell? existing))
      {
        spells[(spell.Id, profile?.Id)] = existing with
        {
          AlwaysPrepared = existing.AlwaysPrepared || alwaysPrepared,
          Prepared = existing.Prepared || prepared,
          InSpellbook = existing.InSpellbook || inSpellbook,
        };
        return;
      }

      spells[(spell.Id, profile?.Id)] = new(spell, source, profile, alwaysPrepared, prepared, inSpellbook);
    }

    foreach (SelectionInstance selection in this._selections.Where(entry => entry.Owner.Active && entry.Choice.OptionSource == OptionSourceEnum.Spell))
    {
      if (!this.Catalog.SpellsByKey.TryGetValue(selection.Selection.OptionKey, out Spell? spell))
      {
        continue;
      }

      CharacterClass? @class = this.OwningClass(selection.Owner);
      SpellcastingProfile? profile = profiles.FirstOrDefault(entry => entry.ClassId == @class?.Id);
      string source = @class?.Name ?? selection.Owner.Feature.Name;
      switch (selection.Choice.Category)
      {
        case CantripCategory:
        case PreparedCategory:
          Add(spell, source, profile, prepared: true);
          break;
        case SpellbookCategory:
          Add(spell, source, profile, inSpellbook: true);
          break;
        default:
          Add(spell, selection.Owner.Feature.Name, profile, alwaysPrepared: true, prepared: true);
          break;
      }
    }

    foreach (CharacterSpellbookEntry entry in this.Character.SpellbookEntries)
    {
      SpellcastingProfile? profile = profiles.FirstOrDefault(profile => profile.Pool == SpellPoolEnum.Spellbook);
      Add(this.Catalog.Spells[entry.SpellId], "Spellbook", profile, inSpellbook: true);
    }

    foreach (SpellcastingProfile profile in profiles)
    {
      foreach (CharacterPreparedSpell entry in this.PreparedLoadout(profile))
      {
        Add(this.Catalog.Spells[entry.SpellId], this.Catalog.Classes[profile.ClassId].Name, profile, prepared: true);
      }
    }

    // always prepared spells granted by effects (domain spells, species spells ...)
    foreach (AppliedEffect effect in this.Effects.Where(entry => entry.Target.StartsWith("spell.", StringComparison.Ordinal)).ToList())
    {
      string key = effect.Target["spell.".Length..];
      if (!this.Catalog.SpellsByKey.TryGetValue(key, out Spell? spell) || this.Stat(effect.Target) <= 0)
      {
        continue;
      }

      CharacterClass? @class = this.OwningClass(effect.Owner);
      Add(spell, effect.Label, profiles.FirstOrDefault(entry => entry.ClassId == @class?.Id), alwaysPrepared: true, prepared: true);
    }

    return spells.Values.OrderBy(entry => entry.Spell.Level).ThenBy(entry => entry.Spell.Name).ToList();
  }

  #endregion
}
