using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TableTopSheets.Persistence;
using TableTopSheets.Persistence.Postgresql;
using TableTopSheets.Persistence.Sqlite;

namespace TableTopSheets.Seed.Configuration;

/// <summary>
///   Registers the <see cref="TableTopSheetsContext"/> of the configured provider
/// </summary>
public static class DatabaseServiceCollectionExtensions
{
  #region Methods

  public static IServiceCollection AddTableTopSheetsDatabase(this IServiceCollection services, IConfiguration configuration)
  {
    MigratorDatabaseOptions database = configuration
      .GetSection(MigratorDatabaseOptions.Section)
      .Get<MigratorDatabaseOptions>() ?? new MigratorDatabaseOptions();
    string connectionString = database.ConnectionString(configuration);

    return services.AddDbContext<TableTopSheetsContext>(options =>
    {
      switch (database.Provider)
      {
        case DatabaseProvider.Postgresql:
          options.UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(typeof(PostgresqlContextFactory).Assembly.FullName));
          break;
        default:
          options.UseSqlite(connectionString, sqlite => sqlite.MigrationsAssembly(typeof(SqliteContextFactory).Assembly.FullName));
          break;
      }
    });
  }

  #endregion
}
