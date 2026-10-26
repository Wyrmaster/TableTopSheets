using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Characters;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Engine.Evaluation;

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

  /// <summary>
  ///   Character levels at which every cantrip upgrades ("Cantrip Upgrade"); the damage per tier is stored on the spell
  /// </summary>
  public static readonly IReadOnlyList<int> CantripUpgradeLevels = [5, 11, 17];

  #endregion

  #region Properties

  /// <summary>
  ///   Cantrip tier 0..3; cantrips scale with the character level, not with a class level
  /// </summary>
  public int CantripTier => CantripUpgradeLevels.Count(level => this.TotalLevel >= level);

  #endregion

  #region Methods

  /// <summary>
  ///   Damage of a cantrip at the current character level (null for cantrips without damage)
  /// </summary>
  public string? CantripDamage(Spell spell)
    => spell.CantripDamage.Count > this.CantripTier ? spell.CantripDamage[this.CantripTier] : spell.Damage;

  /// <summary>
  ///   Spellcasting ability for spells granted outside a class (species, feats): the ability chosen with the granting
  ///   feature or one of the features above it (Elven Lineage, Magic Initiate); otherwise the best mental ability
  /// </summary>
  public string ChosenSpellcastingAbility(FeatureInstance instance)
  {
    for (FeatureInstance? current = instance; current != null;)
    {
      SelectionInstance? ability = this._selections.FirstOrDefault(entry => entry.Owner == current && entry.Choice.OptionSource == OptionSourceEnum.Ability);
      if (ability != null)
      {
        return ability.Selection.OptionKey;
      }

      CharacterChoice? grantedBy = current.GrantedBy;
      current = grantedBy == null ? null : this._selections.FirstOrDefault(entry => entry.Selection.Id == grantedBy.Id)?.Owner;
    }

    return new[] { "int", "wis", "cha" }.MaxBy(this.Score)!;
  }

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

    void Add(Spell spell, string source, SpellcastingProfile? profile, FeatureInstance? owner, bool alwaysPrepared = false, bool prepared = false, bool inSpellbook = false)
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

      string ability = profile?.Ability ?? (owner == null ? "int" : this.ChosenSpellcastingAbility(owner));
      spells[(spell.Id, profile?.Id)] = new(spell, source, profile, alwaysPrepared, prepared, inSpellbook, ability);
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
          Add(spell, source, profile, selection.Owner, prepared: true);
          break;
        case SpellbookCategory:
          Add(spell, source, profile, selection.Owner, inSpellbook: true);
          break;
        default:
          Add(spell, selection.Owner.Feature.Name, profile, selection.Owner, alwaysPrepared: true, prepared: true);
          break;
      }
    }

    foreach (CharacterSpellbookEntry entry in this.Character.SpellbookEntries)
    {
      SpellcastingProfile? profile = profiles.FirstOrDefault(profile => profile.Pool == SpellPoolEnum.Spellbook);
      Add(this.Catalog.Spells[entry.SpellId], "Spellbook", profile, null, inSpellbook: true);
    }

    foreach (SpellcastingProfile profile in profiles)
    {
      foreach (CharacterPreparedSpell entry in this.PreparedLoadout(profile))
      {
        Add(this.Catalog.Spells[entry.SpellId], this.Catalog.Classes[profile.ClassId].Name, profile, null, prepared: true);
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
      Add(spell, effect.Label, profiles.FirstOrDefault(entry => entry.ClassId == @class?.Id), effect.Owner, alwaysPrepared: true, prepared: true);
    }

    return spells.Values.OrderBy(entry => entry.Spell.Level).ThenBy(entry => entry.Spell.Name).ToList();
  }

  #endregion
}
