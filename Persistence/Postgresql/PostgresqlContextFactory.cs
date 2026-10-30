using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TableTopSheets.Persistence.Postgresql;

/// <summary>
///   Creates a <see cref="TableTopSheetsContext"/> for PostgreSQL. The connection string is read from
///   <c>TABLETOPSHEETS_POSTGRESQL</c> (e.g. <c>Host=localhost;Database=tabletopsheets;Username=postgres;Password=...</c>)
/// </summary>
public class PostgresqlContextFactory: IDesignTimeDbContextFactory<TableTopSheetsContext>
{
  #region Constants

  public const string ConnectionVariable = "TABLETOPSHEETS_POSTGRESQL";

  #endregion

  #region Methods

  public TableTopSheetsContext CreateDbContext(string[] args)
    => Create(Environment.GetEnvironmentVariable(ConnectionVariable) ?? "Host=localhost;Database=tabletopsheets;Username=postgres");

  public static TableTopSheetsContext Create(string connectionString)
    => new(new DbContextOptionsBuilder<TableTopSheetsContext>()
      .UseNpgsql(connectionString, options => options.MigrationsAssembly(typeof(PostgresqlContextFactory).Assembly.FullName))
      .Options);

  #endregion
}
