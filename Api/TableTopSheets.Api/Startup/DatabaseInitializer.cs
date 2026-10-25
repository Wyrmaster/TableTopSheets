using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TableTopSheets.Api.Configuration;
using TableTopSheets.Engine.Rules;
using TableTopSheets.Persistence;
using TableTopSheets.Seed;

namespace TableTopSheets.Api.Startup;

/// <summary>
///   Migrates and seeds the database and loads the rules catalog once on startup
/// </summary>
public static class DatabaseInitializer
{
  #region Methods

  public static async Task<RulesCatalog> InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
  {
    await using AsyncServiceScope scope = services.CreateAsyncScope();
    DatabaseOptions options = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    TableTopSheetsContext context = scope.ServiceProvider.GetRequiredService<TableTopSheetsContext>();
    ILogger logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DatabaseInitializer));

    if (options.MigrateOnStartup)
    {
      logger.LogInformation("Applying migrations ({Provider})", options.Provider);
      await context.Database.MigrateAsync(cancellationToken);
    }

    if (options.SeedOnStartup)
    {
      logger.LogInformation("Writing the rules seed");
      await SeedWriter.SeedAsync(context, cancellationToken: cancellationToken);
    }

    RulesCatalog catalog = await RulesCatalog.LoadAsync(context, cancellationToken);
    logger.LogInformation("Rules catalog loaded: {Classes} classes, {Spells} spells, {Feats} feats", catalog.Classes.Count, catalog.Spells.Count, catalog.Feats.Count);
    return catalog;
  }

  #endregion
}
