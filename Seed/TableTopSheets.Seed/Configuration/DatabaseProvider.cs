namespace TableTopSheets.Seed.Configuration;

/// <summary>
///   Database the migrator writes to (<c>Database:Provider</c>, the same setting as the API)
/// </summary>
public enum DatabaseProvider
{
  Sqlite,
  Postgresql,
}
