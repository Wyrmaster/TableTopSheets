using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TableTopSheets.Api.Configuration;
using TableTopSheets.Persistence;
using TableTopSheets.Seed;

namespace TableTopSheets.Api.Startup;

/// <summary>
///   Migrates and seeds the database on startup
/// </summary>
public static class DatabaseInitializer
{
  #region Methods

  public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
  {
    await using AsyncServiceScope scope = services.CreateAsyncScope();
    DatabaseOptions options = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    TableTopSheetsContext context = scope.ServiceProvider.GetRequiredService<TableTopSheetsContext>();
    ILogger logger = scope.ServiceProvider
      .GetRequiredService<ILoggerFactory>()
      .CreateLogger(typeof(DatabaseInitializer));

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

    logger.LogInformation
    (
      "Rules in the database: {Classes} classes, {Spells} spells, {Feats} feats",
      await context.Classes.CountAsync(cancellationToken),
      await context.Spells.CountAsync(cancellationToken),
      await context.Feats.CountAsync(cancellationToken)
    );
  }

  #endregion
}
