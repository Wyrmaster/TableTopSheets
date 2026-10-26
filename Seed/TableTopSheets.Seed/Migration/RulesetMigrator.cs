using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TableTopSheets.Engine.Rules;
using TableTopSheets.Persistence;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Seed.Migration;

/// <inheritdoc />
public sealed class RulesetMigrator(TableTopSheetsContext context, IOptions<RulesetMigratorOptions> options, ILogger<RulesetMigrator> logger): IRulesetMigrator
{
  #region Constants

  /// <summary>
  ///   EF migration that creates the history table; before it exists there is no ruleset history to read
  /// </summary>
  private const string HistoryMigration = "_AddRulesetMigrations";

  #endregion

  #region Properties

  /// <summary>
  ///   Version of the migrator, written into the history
  /// </summary>
  public static string ToolVersion { get; } = typeof(RulesetMigrator).Assembly
    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
    ?.InformationalVersion
    .Split('+')[0] ?? "0.0.0";

  /// <summary>
  ///   Seed folder; a folder without <c>Extracted</c> or <c>Rules</c> is refused, it would be an empty ruleset
  /// </summary>
  private string DataDirectory
  {
    get
    {
      string directory = options.Value.DataDirectory ?? SeedLoader.DefaultDirectory;
      if (!Directory.Exists(Path.Combine(directory, "Extracted")) && !Directory.Exists(Path.Combine(directory, "Rules")))
      {
        throw new DirectoryNotFoundException($"'{Path.GetFullPath(directory)}' contains no seed data (Extracted/ or Rules/)");
      }

      return directory;
    }
  }

  #endregion

  #region IRulesetMigrator

  /// <inheritdoc />
  public async Task<RulesetMigration?> MigrateAsync(bool force = false, CancellationToken cancellationToken = default)
  {
    List<string> pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
    if (pending.Count > 0)
    {
      logger.LogInformation("Applying {Count} schema migration(s): {Migrations}", pending.Count, string.Join(", ", pending));
      await context.Database.MigrateAsync(cancellationToken);
    }
    else
    {
      logger.LogInformation("Schema is up to date");
    }

    string hash = this.ContentHash();
    RulesetMigration? last = await this.LastAppliedAsync(cancellationToken);
    if (!force && last?.ContentHash == hash)
    {
      logger.LogInformation("Rules are up to date (written {AppliedAt:u} by version {Version})", last.AppliedAt, last.ToolVersion);
      return null;
    }

    SeedResult result = SeedBuilder.Build(SeedLoader.Load(this.DataDirectory));
    IReadOnlyList<string> errors = CatalogValidator.Validate(result.ToCatalog());
    if (errors.Count > 0)
    {
      throw new InvalidDataException("Seed data is invalid:" + Environment.NewLine + string.Join(Environment.NewLine, errors));
    }

    if (last == null)
    {
      logger.LogInformation("Writing the rules");
    }
    else
    {
      logger.LogInformation("Writing the rules (previous hash {Previous}, force: {Force})", last.ContentHash, force);
    }

    SeedWriteReport report = await SeedWriter.WriteAsync(context, result, cancellationToken);

    RulesetMigration migration = new()
    {
      Id = Guid.NewGuid(),
      ContentHash = hash,
      ToolVersion = ToolVersion,
      AppliedAt = DateTime.UtcNow,
      Summary = $"{result.Classes.Count} classes, {result.Species.Count} species, {result.Backgrounds.Count} backgrounds, "
                + $"{result.Feats.Count} feats, {result.Spells.Count} spells, {result.Items.Count} items; "
                + $"{report.Inserted} rows inserted, {report.Updated} updated, {report.Deleted} stale rows removed",
    };
    context.RulesetMigrations.Add(migration);
    await context.SaveChangesAsync(cancellationToken);
    context.ChangeTracker.Clear();

    logger.LogInformation("Rules written: {Summary}", migration.Summary);
    return migration;
  }

  /// <inheritdoc />
  public IReadOnlyList<string> Validate()
  {
    try
    {
      return CatalogValidator.Validate(SeedBuilder
        .Build(SeedLoader.Load(this.DataDirectory))
        .ToCatalog());
    }
    catch (Exception exception) when (exception is InvalidDataException or DirectoryNotFoundException)
    {
      return [exception.Message];
    }
  }

  /// <inheritdoc />
  public async Task<RulesetStatus> StatusAsync(CancellationToken cancellationToken = default)
  {
    List<string> pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
    RulesetMigration? last = pending.Any(entry => entry.EndsWith(HistoryMigration, StringComparison.Ordinal))
      ? null
      : await this.LastAppliedAsync(cancellationToken);
    return new RulesetStatus(pending, last, this.ContentHash());
  }

  #endregion

  #region Methods

  /// <summary>
  ///   Hash of the seed files and of the code that turns them into rows: a new migrator version writes the rules again
  ///   even when the files are unchanged
  /// </summary>
  private string ContentHash()
  {
    StringBuilder builder = new(SeedLoader.Hash(this.DataDirectory));
    foreach (Assembly assembly in new[] { typeof(SeedBuilder).Assembly, typeof(CatalogValidator).Assembly, typeof(TableTopSheetsContext).Assembly })
    {
      builder
        .Append('|')
        .Append(assembly.ManifestModule.ModuleVersionId);
    }

    return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString())));
  }

  private async Task<RulesetMigration?> LastAppliedAsync(CancellationToken cancellationToken)
    => await context.RulesetMigrations
      .AsNoTracking()
      .OrderByDescending(entry => entry.AppliedAt)
      .FirstOrDefaultAsync(cancellationToken);

  #endregion
}
