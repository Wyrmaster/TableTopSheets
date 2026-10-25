using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Persistence.Entity.Rules;

/// <summary>
///   Universal carrier of mechanics. Every class feature, species trait, feat benefit, invocation, magic item property ...
///   is a feature with <see cref="Effects"/>, <see cref="Choices"/>, <see cref="Resources"/> and an optional <see cref="Spellcasting"/>
/// </summary>
public class Feature: BaseEntity
{
  #region Properties

  public required string Key { get; set; }

  public required string Name { get; set; }

  public string DescriptionHtml { get; set; } = string.Empty;

  public OwnerTypeEnum OwnerType { get; set; }

  /// <summary>
  ///   Class level the feature is gained at (class and subclass features)
  /// </summary>
  public byte? Level { get; set; }

  public GrantModeEnum GrantMode { get; set; }

  /// <summary>
  ///   Option pool the feature belongs to (<c>invocation</c>, <c>metamagic</c>, <c>fighting-style</c> ...)
  /// </summary>
  public string? Category { get; set; }

  public SheetSectionEnum SheetSection { get; set; }

  /// <summary>
  ///   Expression that has to be true to select the feature as an option
  /// </summary>
  public string? PrerequisiteExpr { get; set; }

  /// <summary>
  ///   Feature only applies while the character toggles it on (Rage, Mage Armor ...)
  /// </summary>
  public bool Toggleable { get; set; }

  public int Order { get; set; }

  #endregion

  #region Owner

  public Guid? ClassId { get; set; }

  public CharacterClass? Class { get; set; }

  public Guid? SubclassId { get; set; }

  public Subclass? Subclass { get; set; }

  public Guid? SpeciesId { get; set; }

  public Species? Species { get; set; }

  public Guid? BackgroundId { get; set; }

  public Background? Background { get; set; }

  public Guid? FeatId { get; set; }

  public Feat? Feat { get; set; }

  public Guid? ItemId { get; set; }

  public Items.Item? Item { get; set; }

  public Guid? SpellId { get; set; }

  public Spell? Spell { get; set; }

  #endregion

  #region Navigation

  public ICollection<Effect> Effects { get; set; } = new List<Effect>();

  public ICollection<Choice> Choices { get; set; } = new List<Choice>();

  public ICollection<ResourceDefinition> Resources { get; set; } = new List<ResourceDefinition>();

  public SpellcastingProfile? Spellcasting { get; set; }

  #endregion
}
