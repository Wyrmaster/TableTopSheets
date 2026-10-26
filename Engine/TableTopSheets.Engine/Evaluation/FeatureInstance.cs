using TableTopSheets.Persistence.Entity.Characters;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Engine.Evaluation;

/// <summary>
///   A feature the character currently has
/// </summary>
/// <param name="Feature">feature of the rules catalog</param>
/// <param name="GrantedBy">selection that granted the feature (null for class, species, background, item features)</param>
/// <param name="Active">false for toggleable features that are switched off; their effects don't apply</param>
/// <param name="Source">human readable origin (e.g. <c>Wizard 1</c>, <c>Elf</c>, <c>Feat</c>)</param>
public sealed record FeatureInstance(Feature Feature, CharacterChoice? GrantedBy, bool Active, string Source)
{
  /// <summary>
  ///   Parent id the selections of the choices of this instance carry
  /// </summary>
  public Guid? SelectionScope => this.GrantedBy?.Id;
}
