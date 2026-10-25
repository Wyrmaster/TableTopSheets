using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace TableTopSheets.Api.Tests;

/// <summary>
///   API host on a seeded in-memory SQLite database shared by the tests of a class
/// </summary>
public sealed class ApiFactory: WebApplicationFactory<Program>
{
  #region Fields

  private readonly string _connectionString = $"Data Source=file:api-tests-{Guid.NewGuid():N}?mode=memory&cache=shared";

  private readonly SqliteConnection _keepAlive;

  #endregion

  #region Constructor

  public ApiFactory()
  {
    // the in-memory database lives as long as one connection is open
    this._keepAlive = new SqliteConnection(this._connectionString);
    this._keepAlive.Open();
  }

  #endregion

  #region Overrides

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.UseEnvironment("Development");
    builder.UseSetting("Database:Provider", "Sqlite");
    builder.UseSetting("ConnectionStrings:TableTopSheets", this._connectionString);
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    if (disposing)
    {
      this._keepAlive.Dispose();
    }
  }

  #endregion
}
