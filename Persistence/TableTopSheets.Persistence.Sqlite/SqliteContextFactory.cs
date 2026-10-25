using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TableTopSheets.Persistence.Sqlite;

/// <summary>
///   Creates a <see cref="TableTopSheetsContext"/> for SQLite (local development). The connection string is read from
///   <c>TABLETOPSHEETS_SQLITE</c> and defaults to <c>Data Source=tabletopsheets.db</c>
/// </summary>
public class SqliteContextFactory: IDesignTimeDbContextFactory<TableTopSheetsContext>
{
  #region Constants

  public const string ConnectionVariable = "TABLETOPSHEETS_SQLITE";

  #endregion

  #region Methods

  public TableTopSheetsContext CreateDbContext(string[] args)
    => Create(Environment.GetEnvironmentVariable(ConnectionVariable) ?? "Data Source=tabletopsheets.db");

  public static TableTopSheetsContext Create(string connectionString)
    => new(new DbContextOptionsBuilder<TableTopSheetsContext>()
      .UseSqlite(connectionString, options => options.MigrationsAssembly(typeof(SqliteContextFactory).Assembly.FullName))
      .Options);

  #endregion
}
