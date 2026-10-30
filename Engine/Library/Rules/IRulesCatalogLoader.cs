using Microsoft.EntityFrameworkCore;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Engine.Choices;
using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Persistence;
using TableTopSheets.Persistence.Entity.Characters;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Engine.Rules;

/// <summary>
///   Contract of <see cref="RulesCatalogLoader"/>
/// </summary>
public interface IRulesCatalogLoader
{
  /// <summary>
  ///   Rules the character reaches: its species, background, classes and items, every feature granted by them or by its
  ///   selections, and the options of the choices of those features
  /// </summary>
  Task<RulesCatalog> LoadAsync(Character character, CancellationToken cancellationToken = default);

  /// <summary>
  ///   Read the descriptions of options the catalog doesn't know yet (option html is not part of the option lists)
  /// </summary>
  Task DescribeAsync(RulesCatalog catalog, IEnumerable<(Choice Choice, string Key)> options, CancellationToken cancellationToken = default);

  /// <summary>
  ///   Read the descriptions of the options a pending choice of the evaluation embeds (see <see cref="ChoiceService.EmbeddedOptionLimit"/>)
  /// </summary>
  Task DescribePendingAsync(CharacterEvaluation evaluation, CancellationToken cancellationToken = default);
}
