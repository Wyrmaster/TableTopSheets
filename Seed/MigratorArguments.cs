namespace TableTopSheets.Seed;

/// <summary>
///   Command line of the migrator: <c>[migrate|validate|status|help] [--force] [--data &lt;directory&gt;]</c>
/// </summary>
/// <param name="Command">command to run (default <c>migrate</c>)</param>
/// <param name="Force">write the rules even when they are unchanged</param>
/// <param name="DataDirectory">seed folder instead of <c>Migrator:DataDirectory</c> or the repository's <c>dnddata</c></param>
/// <param name="Error">why the command line couldn't be read; null when it could</param>
public sealed record MigratorArguments(string Command, bool Force, string? DataDirectory, string? Error)
{
  #region Constants

  public const string Usage = """
    TableTopSheets ruleset migrator

    Usage: TableTopSheets.Seed [command] [options]

    Commands:
      migrate    apply pending schema migrations and write the rules if they changed (default)
      validate   build and validate the rules without a database
      status     show pending schema migrations and the last ruleset written
      help       show this text

    Options:
      --force           write the rules even when they are unchanged
      --data <folder>   seed folder with Extracted/, Rules/ and Images/ (default: Migrator__DataDirectory,
                        otherwise the dnddata folder of the repository)

    Seed folder (the container mounts it, e.g. -v ./dnddata:/seed:ro):
      Migrator__DataDirectory             folder with Extracted/, Rules/ and Images/

    Database (same settings as the API, e.g. as environment variables):
      Database__Provider                  Sqlite (default) or Postgresql
      ConnectionStrings__TableTopSheets   connection string
    """;

  private static readonly string[] Commands = ["migrate", "validate", "status", "help"];

  #endregion

  #region Methods

  public static MigratorArguments Parse(IReadOnlyList<string> args)
  {
    string? command = null;
    bool force = false;
    string? data = null;
    for (int index = 0; index < args.Count; index++)
    {
      string argument = args[index];
      switch (argument)
      {
        case "--force":
          force = true;
          break;
        case "--data" when index + 1 < args.Count:
          data = args[++index];
          break;
        case "--data":
          return new MigratorArguments("help", force, data, "--data needs a folder");
        case "-h" or "--help":
          command = "help";
          break;
        default:
          if (command != null || !Commands.Contains(argument))
          {
            return new MigratorArguments("help", force, data, $"Unknown argument '{argument}'");
          }

          command = argument;
          break;
      }
    }

    return new MigratorArguments(command ?? "migrate", force, data, null);
  }

  #endregion
}
