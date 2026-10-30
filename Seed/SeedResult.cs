using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Engine.Rules;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;
using TableTopSheets.Seed.Model;

namespace TableTopSheets.Seed;

/// <summary>
///   Entities built from seed data. Navigation properties and foreign keys are both set
/// </summary>
public sealed class SeedResult
{
  public required Source Source { get; init; }

  public List<CharacterClass> Classes { get; } = [];

  public List<Species> Species { get; } = [];

  public List<Background> Backgrounds { get; } = [];

  public List<Feat> Feats { get; } = [];

  /// <summary>
  ///   Every feature, including those also reachable through their owner
  /// </summary>
  public List<Feature> Features { get; } = [];

  public List<ScaleValue> ScaleValues { get; } = [];

  public List<SpellSlotRow> SpellSlotRows { get; } = [];

  public List<Spell> Spells { get; } = [];

  public List<Item> Items { get; } = [];

  public List<Language> Languages { get; } = [];

  public List<WeaponMastery> Masteries { get; } = [];

  public RulesCatalog ToCatalog() => new
  (
    this.Classes,
    this.Species,
    this.Backgrounds,
    this.Feats,
    this.Features,
    this.ScaleValues,
    this.SpellSlotRows,
    this.Spells,
    this.Items,
    this.Languages,
    this.Masteries
  );
}
