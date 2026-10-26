using Microsoft.Extensions.Configuration;
using TableTopSheets.Persistence.Postgresql;
using TableTopSheets.Persistence.Sqlite;

namespace TableTopSheets.Seed.Configuration;

/// <summary>
///   <c>Database</c> section of the configuration; the keys are the same as the API's, so both processes can share one
///   environment (<c>Database__Provider</c>, <c>ConnectionStrings__TableTopSheets</c>)
/// </summary>
public sealed class MigratorDatabaseOptions
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
