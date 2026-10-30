using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TableTopSheets.Api.Configuration;
using TableTopSheets.Persistence;

namespace TableTopSheets.Api.Startup;

/// <summary>
///   Reports on startup whether the ruleset migrator has prepared the database. It only reads: migrating and seeding is
///   the job of the migrator (<c>Seed</c>, the <c>migrator</c> service in docker compose)
/// </summary>
public static class DatabaseStartupCheck
{
  #region Methods

  public static async Task RunAsync(IServiceProvider services, CancellationToken cancellationToken = default)
  {
    await using AsyncServiceScope scope = services.CreateAsyncScope();
    DatabaseOptions options = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    TableTopSheetsContext context = scope.ServiceProvider.GetRequiredService<TableTopSheetsContext>();
    ILogger logger = scope.ServiceProvider
      .GetRequiredService<ILoggerFactory>()
      .CreateLogger(typeof(DatabaseStartupCheck));

    try
    {
      List<string> pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
      if (pending.Count > 0)
      {
        logger.LogWarning
        (
          "The database ({Provider}) misses {Count} schema migration(s): {Migrations}. Run the ruleset migrator (Seed) first",
          options.Provider,
          pending.Count,
          string.Join(", ", pending)
        );
        return;
      }

      logger.LogInformation
      (
        "Rules in the database: {Classes} classes, {Spells} spells, {Feats} feats",
        await context.Classes.CountAsync(cancellationToken),
        await context.Spells.CountAsync(cancellationToken),
        await context.Feats.CountAsync(cancellationToken)
      );
    }
    catch (Exception exception) when (exception is not OperationCanceledException)
    {
      logger.LogWarning(exception, "The database ({Provider}) couldn't be checked", options.Provider);
    }
  }

  #endregion
}
