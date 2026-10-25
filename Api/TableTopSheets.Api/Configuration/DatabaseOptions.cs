using TableTopSheets.Persistence.Postgresql;
using TableTopSheets.Persistence.Sqlite;

namespace TableTopSheets.Api.Configuration;

public enum DatabaseProvider
{
  Sqlite,
  Postgresql,
}

/// <summary>
///   <c>Database</c> section of the configuration
/// </summary>
public sealed class DatabaseOptions
{
  #region Constants

  public const string Section = "Database";
  public const string ConnectionStringName = "TableTopSheets";

  #endregion

  #region Properties

  public DatabaseProvider Provider { get; set; } = DatabaseProvider.Sqlite;

  /// <summary>
  ///   Apply pending migrations on startup
  /// </summary>
  public bool MigrateOnStartup { get; set; } = true;

  /// <summary>
  ///   Write the rules seed on startup (an idempotent upsert)
  /// </summary>
  public bool SeedOnStartup { get; set; } = true;

  #endregion

  #region Methods

  /// <summary>
  ///   <c>ConnectionStrings:TableTopSheets</c>, otherwise the environment variable of the provider, otherwise a local default
  /// </summary>
  public string ConnectionString(IConfiguration configuration)
    => configuration.GetConnectionString(ConnectionStringName)
       ?? this.Provider switch
       {
         DatabaseProvider.Postgresql => Environment.GetEnvironmentVariable(PostgresqlContextFactory.ConnectionVariable)
                                        ?? "Host=localhost;Database=tabletopsheets;Username=postgres",
         _ => Environment.GetEnvironmentVariable(SqliteContextFactory.ConnectionVariable) ?? "Data Source=tabletopsheets.db",
       };

  #endregion
}
