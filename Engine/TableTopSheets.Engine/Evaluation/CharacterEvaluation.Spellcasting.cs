using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Engine.Evaluation;

public sealed partial class CharacterEvaluation
{
  #region Constants

  public const string MulticlassTable = "multiclass";
  public const string PactTable = "pact";

  #endregion

  #region Properties

  /// <summary>
  ///   Spellcasting profiles of active features
  /// </summary>
  public IReadOnlyList<SpellcastingProfile> SpellcastingProfiles => this._features
    .Where(entry => entry.Active && entry.Feature.Spellcasting != null)
    .Select(entry => entry.Feature.Spellcasting!)
    .DistinctBy(entry => entry.Id)
    .ToList();

  #endregion

  #region Methods

  /// <summary>
  ///   Spell slots per spell level (index 1..9) from the Spellcasting features, without Pact Magic
  /// </summary>
  public int[] SpellSlots()
  {
    List<SpellcastingProfile> profiles = this.SpellcastingProfiles
      .Where(entry => entry.Progression is not (CasterProgressionEnum.None or CasterProgressionEnum.Pact))
      .ToList();

    if (profiles.Count == 0)
    {
      return new int[10];
    }

    // "If you multiclass but have the Spellcasting feature from only one class, follow the rules for that class"
    if (profiles.Count == 1)
    {
      return this.SingleClassSlots(profiles[0]);
    }

    int casterLevel = profiles.Sum(this.CasterLevel);
    return this.Slots(MulticlassTable, casterLevel);
  }

  /// <summary>
  ///   Pact Magic slots: count and slot level
  /// </summary>
  public (int Count, int Level) PactSlots()
  {
    SpellcastingProfile? profile = this.SpellcastingProfiles.FirstOrDefault(entry => entry.Progression == CasterProgressionEnum.Pact);
    if (profile == null)
    {
      return (0, 0);
    }

    int[] slots = this.Slots(profile.SlotTableKey ?? PactTable, this.ClassLevel(profile.ClassId));
    int level = Array.FindLastIndex(slots, count => count > 0);
    return level <= 0 ? (0, 0) : (slots[level], level);
  }

  /// <summary>
  ///   Highest spell level a class can prepare, determined as if single classed
  /// </summary>
  public int MaxSpellLevel(string classKey)
  {
    SpellcastingProfile? profile = this.SpellcastingProfiles.FirstOrDefault(entry => this.Catalog.Classes[entry.ClassId].Key == classKey);
    return profile == null ? 0 : this.MaxSpellLevel(profile);
  }

  public int MaxSpellLevel(SpellcastingProfile profile)
  {
    int[] slots = profile.Progression == CasterProgressionEnum.Pact
      ? this.Slots(profile.SlotTableKey ?? PactTable, this.ClassLevel(profile.ClassId))
      : this.SingleClassSlots(profile);
    return Math.Max(0, Array.FindLastIndex(slots, count => count > 0));
  }

  /// <summary>
  ///   Contribution of a profile to the multiclass spellcaster level
  /// </summary>
  public int CasterLevel(SpellcastingProfile profile)
  {
    int level = this.ClassLevel(profile.ClassId);
    return profile.Progression switch
    {
      CasterProgressionEnum.Full => level,
      CasterProgressionEnum.Half => (level + 1) / 2,
      CasterProgressionEnum.Third => level / 3,
      _ => 0,
    };
  }

  private int[] SingleClassSlots(SpellcastingProfile profile)
    => profile.SlotTableKey != null
      ? this.Slots(profile.SlotTableKey, this.ClassLevel(profile.ClassId))
      : this.Slots(MulticlassTable, this.CasterLevel(profile));

  private int[] Slots(string table, int casterLevel)
    => this.Catalog.SpellSlotTables.TryGetValue(table, out Dictionary<int, int[]>? rows) && rows.TryGetValue(casterLevel, out int[]? slots)
      ? slots
      : new int[10];

  #endregion
}
