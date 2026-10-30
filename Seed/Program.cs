using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TableTopSheets.Seed.Configuration;
using TableTopSheets.Seed.Migration;

namespace TableTopSheets.Seed;

/// <summary>
///   Entry point of the ruleset migrator. Exit codes: 0 done, 1 failed (invalid rules, database error), 2 bad command line
/// </summary>
public static class Program
{
  #region Methods

  public static async Task<int> Main(string[] args)
  {
    MigratorArguments arguments = MigratorArguments.Parse(args);
    if (arguments.Command == "help")
    {
      if (arguments.Error != null)
      {
        await Console.Error.WriteLineAsync(arguments.Error + Environment.NewLine);
      }

      Console.WriteLine(MigratorArguments.Usage);
      return arguments.Error == null ? 0 : 2;
    }

    // the command line is ours; configuration comes from appsettings.json (optional) and the environment
    HostApplicationBuilder builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
    {
      Args = [],
      ContentRootPath = AppContext.BaseDirectory,
    });
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
    builder.Services.AddTableTopSheetsDatabase(builder.Configuration);
    builder.Services
      .AddOptions<RulesetMigratorOptions>()
      .Bind(builder.Configuration.GetSection(RulesetMigratorOptions.Section))
      .PostConfigure(options => options.DataDirectory = arguments.DataDirectory ?? options.DataDirectory);
    builder.Services.AddScoped<IRulesetMigrator, RulesetMigrator>();

    using IHost host = builder.Build();
    await using AsyncServiceScope scope = host.Services.CreateAsyncScope();
    IRulesetMigrator migrator = scope.ServiceProvider.GetRequiredService<IRulesetMigrator>();
    ILogger logger = scope.ServiceProvider
      .GetRequiredService<ILoggerFactory>()
      .CreateLogger("TableTopSheets.Migrator");
    using CancellationTokenSource cancellation = new();
    Console.CancelKeyPress += (_, eventArgs) =>
    {
      eventArgs.Cancel = true;
      cancellation.Cancel();
    };

    try
    {
      return arguments.Command switch
      {
        "validate" => Validate(migrator, logger),
        "status" => await StatusAsync(migrator, cancellation.Token),
        _ => await MigrateAsync(migrator, arguments.Force, logger, cancellation.Token),
      };
    }
    catch (Exception exception) when (exception is not OperationCanceledException)
    {
      logger.LogError(exception, "Migration failed: {Message}", exception.Message);
      return 1;
    }
  }

  private static async Task<int> MigrateAsync(IRulesetMigrator migrator, bool force, ILogger logger, CancellationToken cancellationToken)
  {
    logger.LogInformation("Ruleset migrator {Version}", RulesetMigrator.ToolVersion);
    await migrator.MigrateAsync(force, cancellationToken);
    return 0;
  }

  private static int Validate(IRulesetMigrator migrator, ILogger logger)
  {
    IReadOnlyList<string> errors = migrator.Validate();
    foreach (string error in errors)
    {
      logger.LogError("{Error}", error);
    }

    if (errors.Count == 0)
    {
      logger.LogInformation("Rules are valid");
    }

    return errors.Count == 0 ? 0 : 1;
  }

  private static async Task<int> StatusAsync(IRulesetMigrator migrator, CancellationToken cancellationToken)
  {
    RulesetStatus status = await migrator.StatusAsync(cancellationToken);
    Console.WriteLine($"Schema migrations pending: {(status.PendingSchemaMigrations.Count == 0 ? "none" : string.Join(", ", status.PendingSchemaMigrations))}");
    Console.WriteLine(status.LastApplied == null
      ? "Rules written: never"
      : $"Rules written: {status.LastApplied.AppliedAt:u} by version {status.LastApplied.ToolVersion} ({status.LastApplied.Summary})");
    Console.WriteLine($"Rules up to date: {(status.LastApplied?.ContentHash == status.ContentHash ? "yes" : "no")}");
    return 0;
  }

  #endregion
}
