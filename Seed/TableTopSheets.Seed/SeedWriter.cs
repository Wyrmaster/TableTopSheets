using Microsoft.EntityFrameworkCore;
using TableTopSheets.Engine.Rules;
using TableTopSheets.Persistence;
using TableTopSheets.Persistence.Entity;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Seed;

/// <summary>
///   Writes seed data into the database. Rows are matched by their deterministic id, so seeding can be repeated:
///   new rows are inserted, existing rows updated and stale child rows (effects, options, scales ...) removed
/// </summary>
public static class SeedWriter
{
  #region Methods

  /// <summary>
  ///   Load the seed files, validate them and write them into the database
  /// </summary>
  public static async Task<SeedResult> SeedAsync(TableTopSheetsContext context, string? directory = null, CancellationToken cancellationToken = default)
  {
    SeedResult result = SeedBuilder.Build(SeedLoader.Load(directory));
    IReadOnlyList<string> errors = CatalogValidator.Validate(result.ToCatalog());
    if (errors.Count > 0)
    {
      throw new InvalidDataException("Seed data is invalid:" + Environment.NewLine + string.Join(Environment.NewLine, errors));
    }

    await WriteAsync(context, result, cancellationToken);
    return result;
  }

  public static async Task WriteAsync(TableTopSheetsContext context, SeedResult result, CancellationToken cancellationToken = default)
  {
    List<object> roots =
    [
      result.Source,
      .. result.Languages,
      .. result.Masteries,
      .. result.Spells,
      .. result.Items,
      .. result.Feats,
      .. result.Species,
      .. result.Backgrounds,
      .. result.Classes,
      .. result.Features,
      .. result.ScaleValues,
      .. result.SpellSlotRows,
    ];

    HashSet<Guid> seeded = SeededIds(result);
    context.ChangeTracker.Clear();

    await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

    // child rows that are owned completely by the seed data
    await context.Effects
      .Where(entry => !seeded.Contains(entry.Id))
      .ExecuteDeleteAsync(cancellationToken);
    await context.ChoiceOptions
      .Where(entry => !seeded.Contains(entry.Id))
      .ExecuteDeleteAsync(cancellationToken);
    await context.ScaleValues
      .Where(entry => !seeded.Contains(entry.Id))
      .ExecuteDeleteAsync(cancellationToken);
    await context.SpellSlotRows
      .Where(entry => !seeded.Contains(entry.Id))
      .ExecuteDeleteAsync(cancellationToken);
    await context.Resources
      .Where(entry => !seeded.Contains(entry.Id))
      .ExecuteDeleteAsync(cancellationToken);
    await context.SpellcastingProfiles
      .Where(entry => !seeded.Contains(entry.Id))
      .ExecuteDeleteAsync(cancellationToken);

    HashSet<Guid> existing = (await ExistingIds(context, cancellationToken)).ToHashSet();
    existing.UnionWith(await context.Sources
      .Select(entry => entry.Id)
      .ToListAsync(cancellationToken));

    foreach (object root in roots)
    {
      context.ChangeTracker.TrackGraph(root, node =>
      {
        if (node.Entry.State != EntityState.Detached)
        {
          return;
        }

        Guid id = node.Entry.Entity is BaseEntity entity ? entity.Id : Guid.Empty;
        node.Entry.State = existing.Contains(id) ? EntityState.Modified : EntityState.Added;
      });
    }

    await context.SaveChangesAsync(cancellationToken);
    await transaction.CommitAsync(cancellationToken);
    context.ChangeTracker.Clear();
  }

  private static HashSet<Guid> SeededIds(SeedResult result)
  {
    IEnumerable<BaseEntity> entities = result.Features
      .SelectMany(feature => new BaseEntity?[] { feature, feature.Spellcasting }
        .Concat(feature.Effects)
        .Concat(feature.Resources)
        .Concat(feature.Choices)
        .Concat(feature.Choices.SelectMany(choice => choice.Options))
        .Concat(feature.Choices.SelectMany(choice => choice.Effects)))
      .OfType<BaseEntity>()
      .Concat(result.Classes)
      .Concat(result.Classes.SelectMany(entry => entry.Subclasses))
      .Concat(result.Species)
      .Concat(result.Backgrounds)
      .Concat(result.Feats)
      .Concat(result.Spells)
      .Concat(result.Items)
      .Concat(result.Languages)
      .Concat(result.Masteries)
      .Concat(result.ScaleValues)
      .Concat(result.SpellSlotRows);
    return entities
      .Select(entry => entry.Id)
      .Append(result.Source.Id)
      .ToHashSet();
  }

  private static async Task<List<Guid>> ExistingIds(TableTopSheetsContext context, CancellationToken cancellationToken)
  {
    List<Guid> ids = new();
    ids.AddRange(await context.Languages
      .Select(entry => entry.Id)
      .ToListAsync(cancellationToken));
    ids.AddRange(await context.WeaponMasteries
      .Select(entry => entry.Id)
      .ToListAsync(cancellationToken));
    ids.AddRange(await context.Spells
      .Select(entry => entry.Id)
      .ToListAsync(cancellationToken));
    ids.AddRange(await context.Items
      .Select(entry => entry.Id)
      .ToListAsync(cancellationToken));
    ids.AddRange(await context.Feats
      .Select(entry => entry.Id)
      .ToListAsync(cancellationToken));
    ids.AddRange(await context.Species
      .Select(entry => entry.Id)
      .ToListAsync(cancellationToken));
    ids.AddRange(await context.Backgrounds
      .Select(entry => entry.Id)
      .ToListAsync(cancellationToken));
    ids.AddRange(await context.Classes
      .Select(entry => entry.Id)
      .ToListAsync(cancellationToken));
    ids.AddRange(await context.Subclasses
      .Select(entry => entry.Id)
      .ToListAsync(cancellationToken));
    ids.AddRange(await context.Features
      .Select(entry => entry.Id)
      .ToListAsync(cancellationToken));
    ids.AddRange(await context.Effects
      .Select(entry => entry.Id)
      .ToListAsync(cancellationToken));
    ids.AddRange(await context.Choices
      .Select(entry => entry.Id)
      .ToListAsync(cancellationToken));
    ids.AddRange(await context.ChoiceOptions
      .Select(entry => entry.Id)
      .ToListAsync(cancellationToken));
    ids.AddRange(await context.Resources
      .Select(entry => entry.Id)
      .ToListAsync(cancellationToken));
    ids.AddRange(await context.SpellcastingProfiles
      .Select(entry => entry.Id)
      .ToListAsync(cancellationToken));
    ids.AddRange(await context.ScaleValues
      .Select(entry => entry.Id)
      .ToListAsync(cancellationToken));
    ids.AddRange(await context.SpellSlotRows
      .Select(entry => entry.Id)
      .ToListAsync(cancellationToken));
    return ids;
  }

  #endregion
}
