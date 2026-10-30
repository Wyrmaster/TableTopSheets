using TableTopSheets.Persistence.Postgresql;
using TableTopSheets.Persistence.Sqlite;

namespace TableTopSheets.Api.Configuration;

/// <summary>
///   <c>Database</c> section of the configuration. The API neither migrates nor seeds: the ruleset migrator
///   (<c>Seed</c>) does both in its own process, with the same settings
/// </summary>
public sealed class DatabaseOptions
{
  #region Constants

  public const string Section = "Database";
  public const string ConnectionStringName = "TableTopSheets";

  #endregion

  #region Properties

  public DatabaseProvider Provider { get; set; } = DatabaseProvider.Sqlite;

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
