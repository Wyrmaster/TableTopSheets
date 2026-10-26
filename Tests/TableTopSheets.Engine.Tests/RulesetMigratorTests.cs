using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Sqlite;
using TableTopSheets.Seed;
using TableTopSheets.Seed.Migration;

namespace TableTopSheets.Engine.Tests;

public class RulesetMigratorTests
{
  [Fact]
  public async Task MigratorWritesChangedRulesOnlyAndKeepsAHistory()
  {
    string path = Path.Combine(AppContext.BaseDirectory, $"migrator-{Guid.NewGuid():N}.db");
    try
    {
      await using (TableTopSheetsContext context = SqliteContextFactory.Create($"Data Source={path}"))
      {
        RulesetStatus fresh = await Migrator(context).StatusAsync();
        Assert.NotEmpty(fresh.PendingSchemaMigrations);
        Assert.Null(fresh.LastApplied);

        RulesetMigration? first = await Migrator(context).MigrateAsync();
        Assert.NotNull(first);
        Assert.Contains("classes", first.Summary);
      }

      await using (TableTopSheetsContext context = SqliteContextFactory.Create($"Data Source={path}"))
      {
        // unchanged rules: nothing is written
        Assert.Null(await Migrator(context).MigrateAsync());
        Assert.True((await Migrator(context).StatusAsync()).UpToDate);

        // --force writes them again
        Assert.NotNull(await Migrator(context).MigrateAsync(force: true));
        Assert.Equal(2, await context.RulesetMigrations.CountAsync());
        Assert.Contains("barbarian", await context.Classes
          .Select(entry => entry.Key)
          .ToListAsync());
      }
    }
    finally
    {
      Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
      File.Delete(path);
    }
  }

  [Fact]
  public void ValidateReportsNoProblemsForTheBundledRules()
  {
    using TableTopSheetsContext context = SqliteContextFactory.Create("Data Source=:memory:");
    Assert.Empty(Migrator(context).Validate());
  }

  [Fact]
  public async Task ReseedingKeepsRowsOfOtherRulesAndSelectedOptions()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync("dragonborn", null, "farmer", [], "con", "con", "wis");
    Guid choiceId = SeedKeys.Id("choice", "dragonborn.draconic-ancestry");
    string selected = await world.Context.CharacterChoices
      .Where(entry => entry.CharacterId == id && entry.ChoiceId == choiceId)
      .Select(entry => entry.OptionKey)
      .SingleAsync();

    // a homebrew feature the seed doesn't know
    Feature homebrew = new()
    {
      Id = Guid.NewGuid(),
      Key = "homebrew.swift",
      Name = "Swift",
      OwnerType = OwnerTypeEnum.Option,
      Effects =
      {
        new Effect { Id = Guid.NewGuid(), Target = "speed.walk", Operation = EffectOperationEnum.Add, ValueExpr = "5" },
      },
    };
    world.Context.Features.Add(homebrew);
    await world.Context.SaveChangesAsync();

    // a seed that dropped the selected ancestry and one other ancestry
    SeedResult result = SeedBuilder.Build(SeedLoader.Load());
    Choice ancestry = result.Features
      .SelectMany(entry => entry.Choices)
      .Single(entry => entry.Id == choiceId);
    ChoiceOption other = ancestry.Options.First(entry => entry.Key != selected);
    ancestry.Options = ancestry.Options
      .Where(entry => entry.Key != selected && entry.Key != other.Key)
      .ToList();
    SeedWriteReport report = await SeedWriter.WriteAsync(world.Context, result);

    Assert.True(await world.Context.Features.AnyAsync(entry => entry.Id == homebrew.Id));
    Assert.True(await world.Context.Effects.AnyAsync(entry => entry.FeatureId == homebrew.Id));
    Assert.True(await world.Context.ChoiceOptions.AnyAsync(entry => entry.ChoiceId == choiceId && entry.Key == selected));
    Assert.False(await world.Context.ChoiceOptions.AnyAsync(entry => entry.Id == other.Id));
    Assert.Equal(1, report.Deleted);

    // the character still evaluates with its selection
    Assert.Contains((await world.EvaluateAsync(id)).Selections, entry => entry.Selection.OptionKey == selected);
  }

  private static RulesetMigrator Migrator(TableTopSheetsContext context)
    => new(context, Options.Create(new RulesetMigratorOptions()), NullLogger<RulesetMigrator>.Instance);
}
