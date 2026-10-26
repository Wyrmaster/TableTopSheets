using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TableTopSheets.Common.Sheet;
using TableTopSheets.Engine.Choices;
using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Engine.Expressions;
using TableTopSheets.Engine.Rules;
using TableTopSheets.Engine.Services;
using TableTopSheets.Engine.Sheet;
using TableTopSheets.Persistence;
using TableTopSheets.Persistence.Entity.Characters;
using TableTopSheets.Seed;

namespace TableTopSheets.Engine.Tests;

/// <summary>
///   Seeded in-memory SQLite database with a character service. Every <see cref="Service"/> call gets a new service (and rule
///   loader), like every API request does
/// </summary>
public sealed class TestWorld: IAsyncDisposable
{
  #region Fields

  private readonly SqliteConnection _connection;

  #endregion

  #region Constructor

  private TestWorld(SqliteConnection connection, TableTopSheetsContext context, RulesCatalog catalog)
  {
    this._connection = connection;
    this.Context = context;
    this.Catalog = catalog;
  }

  #endregion

  #region Properties

  public TableTopSheetsContext Context { get; }

  /// <summary>
  ///   Every rule, for looking up keys in tests (evaluations read their rules from the database)
  /// </summary>
  public RulesCatalog Catalog { get; }

  public CharacterService Service => new(this.Context, new RulesCatalogLoader(this.Context));

  #endregion

  #region Factory Methods

  public static async Task<TestWorld> CreateAsync()
  {
    SqliteConnection connection = new("DataSource=:memory:");
    await connection.OpenAsync();
    TableTopSheetsContext context = CreateContext(connection);
    await context.Database.EnsureCreatedAsync();
    await SeedWriter.WriteAsync(context, SeedBuilder.Build(SeedLoader.Load()));
    RulesCatalog catalog = await RulesCatalog.LoadAsync(context);
    return new TestWorld(connection, context, catalog);
  }

  public static TableTopSheetsContext CreateContext(SqliteConnection connection)
    => new(new DbContextOptionsBuilder<TableTopSheetsContext>().UseSqlite(connection).Options);

  #endregion

  #region Helpers

  public static readonly IReadOnlyDictionary<string, byte> StandardArray = new Dictionary<string, byte>
  {
    ["str"] = 15, ["dex"] = 14, ["con"] = 13, ["int"] = 12, ["wis"] = 10, ["cha"] = 8,
  };

  public static IReadOnlyDictionary<string, byte> Scores(byte str, byte dex, byte con, byte @int, byte wis, byte cha) => new Dictionary<string, byte>
  {
    ["str"] = str, ["dex"] = dex, ["con"] = con, ["int"] = @int, ["wis"] = wis, ["cha"] = cha,
  };

  /// <summary>
  ///   Dwarf with a background; the given background increases are used, every other origin choice gets the first option
  /// </summary>
  public Task<Guid> CreateCharacterAsync(IReadOnlyDictionary<string, byte>? abilities = null, string background = "farmer", params string[] backgroundIncreases)
    => this.CreateCharacterAsync("dwarf", abilities, background, [], backgroundIncreases);

  /// <summary>
  ///   Character of any species; <paramref name="originSelections"/> answer species choices, everything else is auto completed
  /// </summary>
  public async Task<Guid> CreateCharacterAsync
  (
    string species,
    IReadOnlyDictionary<string, byte>? abilities,
    string background,
    IReadOnlyList<SelectionRequest> originSelections,
    params string[] backgroundIncreases
  )
  {
    CharacterService service = this.Service;
    Character character = await service.CreateAsync(new CreateCharacterRequest
    {
      Name = "Test",
      SpeciesKey = species,
      BackgroundKey = background,
      Abilities = abilities ?? StandardArray,
      Selections = backgroundIncreases.Select(entry => new SelectionRequest($"{background}.ability-scores", entry)).Concat(originSelections).ToList(),
      AllowIncomplete = true,
    });
    await this.AutoCompleteAsync(service, character);
    await service.EnsureCompleteAsync(character);
    await this.Context.SaveChangesAsync();
    return character.Id;
  }

  /// <summary>
  ///   Level up with the given selections; every other open choice is answered with the first available option
  /// </summary>
  public async Task<CharacterEvaluation> LevelUpAsync
  (
    Guid characterId,
    string classKey,
    string? subclassKey = null,
    IReadOnlyList<SelectionRequest>? selections = null,
    IReadOnlyList<ReplacementRequest>? replacements = null
  )
  {
    CharacterService service = this.Service;
    Character character = await service.LoadAsync(characterId);
    await service.LevelUpAsync(character, new LevelUpRequest
    {
      ClassKey = classKey,
      SubclassKey = subclassKey,
      Selections = selections ?? [],
      Replacements = replacements ?? [],
      AllowIncomplete = true,
    });
    await this.AutoCompleteAsync(service, character);
    await service.EnsureCompleteAsync(character);
    await this.Context.SaveChangesAsync();
    return await service.EvaluateAsync(character);
  }

  public async Task<CharacterEvaluation> LevelUpManyAsync(Guid characterId, string classKey, int levels, string? subclassKey = null)
  {
    CharacterEvaluation evaluation = await this.EvaluateAsync(characterId);
    for (int index = 0; index < levels; index++)
    {
      int next = evaluation.ClassLevel(classKey) + 1;
      string? subclass = next == this.Catalog.ClassesByKey[classKey].SubclassLevel ? subclassKey : null;
      evaluation = await this.LevelUpAsync(characterId, classKey, subclass);
    }

    return evaluation;
  }

  public async Task AutoCompleteAsync(CharacterService service, Character character)
  {
    while (true)
    {
      CharacterEvaluation evaluation = await service.EvaluateAsync(character);
      PendingChoice? pending = ChoiceService.Pending(evaluation).FirstOrDefault(entry => !entry.Choice.Optional);
      if (pending == null)
      {
        return;
      }

      OptionDescriptor option = ChoiceService.AvailableOptions(evaluation, pending.Choice, pending.Owner).FirstOrDefault()
                                ?? throw new InvalidOperationException($"No option left for '{pending.Choice.Key}'");
      await service.SelectAsync(character, [new SelectionRequest(pending.Choice.Key, option.Key, ParentRef: pending.ParentSelectionId?.ToString())]);
    }
  }

  public async Task<CharacterSheet> SheetAsync(Guid characterId) => await this.Service.BuildSheetAsync(characterId);

  public async Task<CharacterEvaluation> EvaluateAsync(Guid characterId)
  {
    CharacterService service = this.Service;
    return await service.EvaluateAsync(await service.LoadAsync(characterId));
  }

  #endregion

  #region IAsyncDisposable

  public async ValueTask DisposeAsync()
  {
    await this.Context.DisposeAsync();
    await this._connection.DisposeAsync();
  }

  #endregion
}
