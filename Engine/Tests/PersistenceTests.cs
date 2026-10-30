using Microsoft.EntityFrameworkCore;
using TableTopSheets.Engine.Rules;
using TableTopSheets.Persistence;
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
}
