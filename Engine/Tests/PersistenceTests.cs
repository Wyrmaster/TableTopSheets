using Microsoft.EntityFrameworkCore;
using TableTopSheets.Engine.Rules;
using TableTopSheets.Persistence;
using TableTopSheets.Persistence.Entity.Rules.Items;
using TableTopSheets.Persistence.Sqlite;
using TableTopSheets.Seed;

namespace TableTopSheets.Engine.Tests;

public class PersistenceTests
{
  [Fact]
  public async Task MigrationCreatesSchemaAndSeedingCanBeRepeated()
  {
    string path = Path.Combine(AppContext.BaseDirectory, $"migration-{Guid.NewGuid():N}.db");
    try
    {
      await using (TableTopSheetsContext context = SqliteContextFactory.Create($"Data Source={path}"))
      {
        await context.Database.MigrateAsync();
        await SeedWriter.SeedAsync(context);
      }

      int features;
      await using (TableTopSheetsContext context = SqliteContextFactory.Create($"Data Source={path}"))
      {
        features = await context.Features.CountAsync();
        await SeedWriter.SeedAsync(context);
      }

      await using (TableTopSheetsContext context = SqliteContextFactory.Create($"Data Source={path}"))
      {
        Assert.Equal(features, await context.Features.CountAsync());
        Assert.Empty(await context.Database.GetPendingMigrationsAsync());
        RulesCatalog catalog = await RulesCatalog.LoadAsync(context);
        Assert.Empty(CatalogValidator.Validate(catalog));
        Assert.Contains("wizard", catalog.ClassesByKey.Keys);
      }
    }
    finally
    {
      Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
      File.Delete(path);
    }
  }

  [Fact]
  public async Task SeedingRemovesItemsTheSeedNoLongerHas()
  {
    string path = Path.Combine(AppContext.BaseDirectory, $"stale-{Guid.NewGuid():N}.db");
    try
    {
      Guid stale = Guid.NewGuid();
      await using (TableTopSheetsContext context = SqliteContextFactory.Create($"Data Source={path}"))
      {
        await context.Database.MigrateAsync();
        await SeedWriter.SeedAsync(context);

        // an item of the seed's source that a later seed dropped (a variant that was renamed or split)
        Guid source = await context.Items
          .Select(entry => entry.SourceId)
          .FirstAsync() ?? Guid.Empty;
        context.Items.Add(new Item
        {
          Id = stale,
          Key = "armor-of-vulnerability-bludgeoning",
          Name = "Armor of Vulnerability (Bludgeoning)",
          SourceId = source,
        });
        await context.SaveChangesAsync();
      }

      await using (TableTopSheetsContext context = SqliteContextFactory.Create($"Data Source={path}"))
      {
        SeedWriteReport report = await SeedWriter.WriteAsync(context, SeedBuilder.Build(SeedLoader.Load()));

        Assert.False(await context.Items.AnyAsync(entry => entry.Id == stale));
        Assert.True(report.Deleted >= 1);
        Assert.True(await context.Items.AnyAsync(entry => entry.Key == "plate-armor-of-vulnerability-bludgeoning"));
      }
    }
    finally
    {
      Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
      File.Delete(path);
    }
  }
}
