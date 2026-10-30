using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TableTopSheets.Engine.Rules;
using TableTopSheets.Persistence;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Seed.Images;

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
        throw new DirectoryNotFoundException(
          $"Seed folder '{Path.GetFullPath(directory)}' contains no seed data (Extracted/ or Rules/): mount the seed data "
          + "(dnddata.zip from the GitHub releases) and point Migrator__DataDirectory or --data to it");
      }

      return directory;
    }
  }

  /// <summary>
  ///   Artwork below the seed folder (<c>Images</c>); may be missing, then the images in the database stay as they are
  /// </summary>
  private string ImagesDirectory => Path.Combine(this.DataDirectory, "Images");

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

    RulesetMigration? migration = await this.WriteRulesAsync(force, cancellationToken);
    await this.WriteImagesAsync(cancellationToken);
    return migration;
  }

  /// <inheritdoc />
  public IReadOnlyList<string> Validate()
  {
    try
    {
      SeedResult result = SeedBuilder.Build(SeedLoader.Load(this.DataDirectory));
      return CatalogValidator
        .Validate(result.ToCatalog())
        .Concat(UnknownImageOwners(result, ImageLoader.Load(this.ImagesDirectory)))
        .ToList();
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
  ///   Write the rules unless the same ruleset was written last
  /// </summary>
  private async Task<RulesetMigration?> WriteRulesAsync(bool force, CancellationToken cancellationToken)
  {
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

  /// <summary>
  ///   Bring the images in line with <c>Images</c> of the seed folder; independent from the rules hash, unchanged
  ///   images are skipped. A seed folder without <c>Images</c> leaves the images alone (no artwork mounted)
  /// </summary>
  private async Task WriteImagesAsync(CancellationToken cancellationToken)
  {
    if (!Directory.Exists(this.ImagesDirectory))
    {
      logger.LogWarning("No Images folder in the seed folder, the images are left as they are");
      return;
    }

    List<RuleImage> images = ImageLoader.Load(this.ImagesDirectory);
    ImageWriteReport report = await ImageWriter.WriteAsync(context, images, cancellationToken);
    logger.LogInformation(
      "Images: {Inserted} inserted, {Updated} updated, {Deleted} removed, {Unchanged} unchanged",
      report.Inserted,
      report.Updated,
      report.Deleted,
      report.Unchanged);
  }

  /// <summary>
  ///   Images whose owner is not a spell, class, species, background or item of the seed or no coin (scenes have no
  ///   owner)
  /// </summary>
  private static IEnumerable<string> UnknownImageOwners(SeedResult result, IReadOnlyList<RuleImage> images)
  {
    Dictionary<ImageCategoryEnum, HashSet<string>> owners = new()
    {
      [ImageCategoryEnum.Spell] = result.Spells
        .Select(entry => entry.Key)
        .ToHashSet(),
      [ImageCategoryEnum.Class] = result.Classes
        .Select(entry => entry.Key)
        .ToHashSet(),
      [ImageCategoryEnum.Species] = result.Species
        .Select(entry => entry.Key)
        .ToHashSet(),
      [ImageCategoryEnum.Background] = result.Backgrounds
        .Select(entry => entry.Key)
        .ToHashSet(),
      [ImageCategoryEnum.Item] = result.Items
        .Select(entry => entry.Key)
        .ToHashSet(),
      [ImageCategoryEnum.Coin] = ["copper", "silver", "electrum", "gold", "platinum"],
    };

    return images
      .Where(image => owners.TryGetValue(image.Category, out HashSet<string>? keys) && !keys.Contains(image.OwnerKey))
      .Select(image => $"Image '{image.Category}/{image.OwnerKey}' belongs to no {image.Category.ToString().ToLowerInvariant()} of the seed");
  }

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
