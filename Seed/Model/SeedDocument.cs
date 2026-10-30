using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Magic;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Seed.Model;

// Seed files are authored by hand (dnddata/Rules) or generated from the rule book (dnddata/Extracted).
// Records with the same key are merged: authored values win, extracted values fill the gaps (mostly descriptions).

/// <summary>
///   Content of one seed file; every section is optional
/// </summary>
public sealed class SeedDocument
{
  public List<SeedClass> Classes { get; set; } = [];

  public List<SeedSpecies> Species { get; set; } = [];

  public List<SeedBackground> Backgrounds { get; set; } = [];

  public List<SeedFeat> Feats { get; set; } = [];

  /// <summary>
  ///   Selectable option features (invocations, metamagic ...) and toggleable spell effects
  /// </summary>
  public List<SeedFeature> Features { get; set; } = [];

  /// <summary>
  ///   Descriptions of class features from the rule book
  /// </summary>
  public List<SeedClassFeatureText> ClassFeatureTexts { get; set; } = [];

  public List<SeedSpell> Spells { get; set; } = [];

  public List<SeedItem> Items { get; set; } = [];

  public List<SeedNamed> Masteries { get; set; } = [];

  public List<SeedLanguage> Languages { get; set; } = [];

  /// <summary>
  ///   Spell slot tables: table key → rows per caster level (index 0 = level 1) → slots per spell level (index 0 = level 1)
  /// </summary>
  public Dictionary<string, List<List<int>>> SlotTables { get; set; } = [];

  /// <summary>
  ///   Tables that follow the character level instead of a class level (species traits, feats):
  ///   key → 20 comma separated values for level 1..20
  /// </summary>
  public Dictionary<string, string> Scales { get; set; } = [];
}
