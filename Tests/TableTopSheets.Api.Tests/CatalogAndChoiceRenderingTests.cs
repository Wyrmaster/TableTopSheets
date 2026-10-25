using System.Net;
using System.Text.Json;
using TableTopSheets.Api.Contracts;

namespace TableTopSheets.Api.Tests;

public sealed class CatalogAndChoiceRenderingTests(ApiFactory factory): IClassFixture<ApiFactory>
{
  #region Fields

  private readonly ApiClient _api = new(factory.CreateClient());

  #endregion

  #region Tests

  [Fact]
  public async Task Spells_ArePagedFilteredAndSorted()
  {
    PagedResult<SpellSummaryDto> all = await this._api.GetAsync<PagedResult<SpellSummaryDto>>("/api/rules/spells?class=wizard&level=1&pageSize=200");
    PagedResult<SpellSummaryDto> page = await this._api.GetAsync<PagedResult<SpellSummaryDto>>("/api/rules/spells?class=wizard&level=1&pageSize=10&page=2");

    Assert.Equal(all.TotalCount, page.TotalCount);
    Assert.Equal(all.Items.Skip(10).Take(10).Select(entry => entry.Key), page.Items.Select(entry => entry.Key));
    Assert.All(all.Items, entry => Assert.True(entry.Level == 1 && entry.Lists.Contains("wizard")));
    Assert.Equal(all.Items.Select(entry => entry.Name).Order(StringComparer.CurrentCulture), all.Items.Select(entry => entry.Name));

    PagedResult<SpellSummaryDto> byLevel = await this._api.GetAsync<PagedResult<SpellSummaryDto>>("/api/rules/spells?sort=-level,name&pageSize=5");
    Assert.All(byLevel.Items, entry => Assert.Equal(9, entry.Level));

    PagedResult<SpellSummaryDto> search = await this._api.GetAsync<PagedResult<SpellSummaryDto>>("/api/rules/spells?search=fire%20bolt");
    Assert.Contains(search.Items, entry => entry.Key == "fire-bolt");
  }

  [Theory]
  [InlineData("/api/rules/spells?sort=unknown")]
  [InlineData("/api/rules/spells?pageSize=0")]
  [InlineData("/api/rules/spells?ritual=maybe")]
  public async Task InvalidQuery_Is400(string url)
    => Assert.Equal(HttpStatusCode.BadRequest, (await this._api.Http.GetAsync(url)).StatusCode);

  [Fact]
  public async Task Details_AreAvailable()
  {
    ClassDetailDto wizard = await this._api.GetAsync<ClassDetailDto>("/api/rules/classes/wizard");
    Assert.Contains(wizard.Features, entry => entry.Choices.Any(choice => choice.Key == "wizard.cantrips"));
    Assert.Contains(wizard.Subclasses, entry => entry.Key == "evoker");

    SpellDetailDto fireball = await this._api.GetAsync<SpellDetailDto>("/api/rules/spells/fireball");
    Assert.Contains("explosion", fireball.Description, StringComparison.OrdinalIgnoreCase);

    Assert.Equal(HttpStatusCode.NotFound, (await this._api.Http.GetAsync("/api/rules/spells/unknown")).StatusCode);
  }

  [Fact]
  public async Task Choices_DescribeHowToRenderThem()
  {
    CharacterStateDto draft = await this._api.CreateDraftAsync();

    PendingChoiceDto abilities = draft.PendingChoices.Single(entry => entry.ChoiceKey == "sage.ability-scores");
    Assert.Equal(ChoiceInputKind.Distribution, abilities.Input);
    Assert.Equal(2, abilities.Constraints.MaxPerOption);
    Assert.Equal(3, abilities.Constraints.Min);
    Assert.Equal([[2, 1], [1, 1, 1]], abilities.Constraints.AllowedPatterns!.Select(entry => entry.ToArray()).ToArray());
    Assert.Equal(["con", "int", "wis"], abilities.Options!.Select(entry => entry.Key).Order());

    PendingChoiceDto senses = draft.PendingChoices.Single(entry => entry.ChoiceKey == "elf.keen-senses");
    Assert.Equal(ChoiceInputKind.SingleSelect, senses.Input);
    Assert.Equal(3, senses.Options!.Count);

    PendingChoiceDto lineage = draft.PendingChoices.Single(entry => entry.ChoiceKey == "elf.elven-lineage");
    Assert.All(lineage.Options!, entry => Assert.NotEmpty(entry.Grants));

    PendingChoiceDto languages = draft.PendingChoices.Single(entry => entry.ChoiceKey == "sage.languages");
    Assert.Equal(ChoiceInputKind.MultiSelect, languages.Input);
    Assert.Contains("category", languages.OptionFilters);
  }

  [Fact]
  public async Task LargeChoices_LinkTheirOptions_WithFiltersAndDisabledReasons()
  {
    CharacterStateDto state = await this._api.AnswerAllAsync(await this._api.CreateDraftAsync());
    state = await this._api.LevelUpAsync(state.Id, "wizard");

    PendingChoiceDto spellbook = state.PendingChoices.Single(entry => entry.ChoiceKey == "wizard.spellbook-initial");
    Assert.Null(spellbook.Options);
    Assert.True(spellbook.OptionCount > ChoiceMapperLimit);
    Assert.Contains("school", spellbook.OptionFilters);

    PagedResult<OptionDto> evocation = await this._api.GetAsync<PagedResult<OptionDto>>($"{spellbook.OptionsUrl}?school=evocation&pageSize=5");
    Assert.NotEmpty(evocation.Items);
    Assert.All(evocation.Items, entry => Assert.Equal("evocation", entry.Meta.School));
    Assert.All(evocation.Items, entry => Assert.Null(entry.Description));

    PagedResult<OptionDto> described = await this._api.GetAsync<PagedResult<OptionDto>>($"{spellbook.OptionsUrl}?search=magic%20missile&includeDescriptions=true");
    Assert.NotNull(described.Items.Single().Description);

    // after selecting a spell it is listed as disabled with a reason
    state = await this._api.SelectAsync(state.Id, new SelectionDto(spellbook.ChoiceKey, "magic-missile"));
    PagedResult<OptionDto> withDisabled = await this._api.GetAsync<PagedResult<OptionDto>>($"{spellbook.OptionsUrl}?search=magic%20missile&includeDisabled=true");
    OptionDto missile = withDisabled.Items.Single();
    Assert.True(missile.Disabled);
    Assert.NotNull(missile.DisabledReason);
    Assert.Empty((await this._api.GetAsync<PagedResult<OptionDto>>($"{spellbook.OptionsUrl}?search=magic%20missile")).Items);
  }

  [Fact]
  public async Task OpenApi_DescribesTheOperations()
  {
    using JsonDocument document = JsonDocument.Parse(await this._api.Http.GetStringAsync("/openapi/v1.json"));
    List<string> operations = document.RootElement.GetProperty("paths").EnumerateObject()
      .SelectMany(path => path.Value.EnumerateObject())
      .Where(operation => operation.Value.ValueKind == JsonValueKind.Object && operation.Value.TryGetProperty("operationId", out _))
      .Select(operation => operation.Value.GetProperty("operationId").GetString()!)
      .ToList();

    Assert.Contains("CreateCharacter", operations);
    Assert.Contains("GetPendingChoices", operations);
    Assert.Contains("SelectOptions", operations);
    Assert.Contains("GetChoiceOptions", operations);
    Assert.Contains("PreviewLevelUp", operations);
    Assert.Contains("LevelUp", operations);
    Assert.Contains("UndoLevel", operations);
    Assert.True(document.RootElement.GetProperty("components").GetProperty("schemas").TryGetProperty("ChoiceInputKind", out _));
  }

  #endregion

  #region Helpers

  private const int ChoiceMapperLimit = Mapping.ChoiceMapper.EmbeddedOptionLimit;

  #endregion
}
