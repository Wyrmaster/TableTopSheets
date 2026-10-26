using TableTopSheets.Persistence.Postgresql;
using TableTopSheets.Persistence.Sqlite;

namespace TableTopSheets.Api.Configuration;

public enum DatabaseProvider
{
  Sqlite,
  Postgresql,
}
