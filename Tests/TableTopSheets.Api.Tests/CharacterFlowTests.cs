using System.Net.Http.Json;
using System.Net;
using TableTopSheets.Api.Contracts;

namespace TableTopSheets.Api.Tests;

public sealed class CharacterFlowTests(ApiFactory factory): IClassFixture<ApiFactory>
{
  #region Fields

  private readonly ApiClient _api = new(factory.CreateClient());

  #endregion

  #region Tests

  [Fact]
  public async Task DraftCreation_ListsOriginChoices_UntilAnswered()
  {
    CharacterStateDto draft = await this._api.CreateDraftAsync();

    Assert.False(draft.Complete);
    Assert.Equal(0, draft.Level);
    Assert.Contains(draft.PendingChoices, entry => entry.ChoiceKey == "elf.elven-lineage");
    Assert.Contains(draft.PendingChoices, entry => entry.ChoiceKey == "sage.ability-scores");

    CharacterStateDto complete = await this._api.AnswerAllAsync(draft);
    Assert.True(complete.Complete);
    Assert.Empty(complete.PendingChoices);

    CharacterStateDto loaded = await this._api.GetAsync<CharacterStateDto>($"/api/characters/{draft.Id}");
    Assert.True(loaded.Complete);
  }

  [Fact]
  public async Task CreateWithEverything_InOneRequest_WhenIncompleteIsNotAllowed()
  {
    CreateCharacterDto request = new()
    {
      Name = "Strict",
      SpeciesKey = "elf",
      BackgroundKey = "sage",
      Abilities = ApiClient.Scores,
      AllowIncomplete = false,
      Selections =
      [
        new("elf.elven-lineage-ability", "int"),
        new("elf.elven-lineage", "high-elf"),
        new("elf.keen-senses", "perception"),
        new("sage.ability-scores", "int"),
        new("sage.ability-scores", "int"),
        new("sage.ability-scores", "con"),
        new("sage.languages", "draconic"),
        new("sage.languages", "dwarvish"),
        new("magic-initiate.list", "wizard", Ref: "initiate"),
        new("magic-initiate.wizard.ability", "int", ParentRef: "initiate"),
        new("magic-initiate.wizard.spell", "sleep", ParentRef: "initiate"),
        new("magic-initiate.wizard.cantrips", "light", ParentRef: "initiate"),
        new("magic-initiate.wizard.cantrips", "mage-hand", ParentRef: "initiate"),
      ],
    };

    CharacterStateDto state = await this._api.PostAsync<CharacterStateDto>("/api/characters", request);
    Assert.True(state.Complete);
    Assert.Equal(17, state.Abilities.Single(entry => entry.Key == "int").Score);

    // without the nested selections the strict request is rejected
    HttpResponseMessage rejected = await this._api.PostRawAsync("/api/characters", request with { Selections = request.Selections.Take(9).ToList() });
    Assert.Equal(HttpStatusCode.UnprocessableEntity, rejected.StatusCode);
  }

  [Fact]
  public async Task AbilityScoreImprovement_OpensFollowUpChoice()
  {
    CharacterStateDto state = await this.WizardAsync(3, "evoker");
    state = await this._api.LevelUpAsync(state.Id, "wizard");

    PendingChoiceDto feat = state.PendingChoices.Single(entry => entry.ChoiceKey == "wizard.ability-score-improvement-4");
    Assert.Equal(ChoiceInputKind.SingleSelect, feat.Input);
    Assert.Null(feat.Options);

    state = await this._api.SelectAsync(state.Id, new SelectionDto(feat.ChoiceKey, "ability-score-improvement"));
    PendingChoiceDto abilities = state.PendingChoices.Single(entry => entry.ChoiceKey == "ability-score-improvement.abilities");
    Assert.Equal(ChoiceInputKind.Distribution, abilities.Input);
    Assert.NotNull(abilities.ParentRef);
    Assert.Equal([[2], [1, 1]], abilities.Constraints.AllowedPatterns!.Select(entry => entry.ToArray()).ToArray());

    int before = state.Abilities.Single(entry => entry.Key == "int").Score;
    state = await this._api.SelectAsync
    (
      state.Id,
      new SelectionDto(abilities.ChoiceKey, "int", ParentRef: abilities.ParentRef),
      new SelectionDto(abilities.ChoiceKey, "dex", ParentRef: abilities.ParentRef)
    );
    Assert.Equal(before + 1, state.Abilities.Single(entry => entry.Key == "int").Score);
    Assert.DoesNotContain(state.PendingChoices, entry => entry.ChoiceKey == "ability-score-improvement.abilities");
  }

  [Fact]
  public async Task FeatAndAbilities_InOneRequest_WithRef()
  {
    CharacterStateDto state = await this.WizardAsync(3, "evoker");
    state = await this._api.LevelUpAsync(state.Id, "wizard");

    state = await this._api.SelectAsync
    (
      state.Id,
      new SelectionDto("wizard.ability-score-improvement-4", "ability-score-improvement", Ref: "asi"),
      new SelectionDto("ability-score-improvement.abilities", "int", ParentRef: "asi"),
      new SelectionDto("ability-score-improvement.abilities", "int", ParentRef: "asi")
    );

    Assert.DoesNotContain(state.PendingChoices, entry => entry.ChoiceKey.StartsWith("ability-score-improvement", StringComparison.Ordinal));
  }

  [Fact]
  public async Task InvalidSelection_Is422_AndUnknownCharacter_Is404()
  {
    CharacterStateDto draft = await this._api.CreateDraftAsync();

    HttpResponseMessage invalid = await this._api.PostRawAsync($"/api/characters/{draft.Id}/choices", new SelectionsDto([new("elf.keen-senses", "arcana")]));
    Assert.Equal(HttpStatusCode.UnprocessableEntity, invalid.StatusCode);
    Assert.Contains("not an available option", await invalid.Content.ReadAsStringAsync());

    HttpResponseMessage missing = await this._api.Http.GetAsync($"/api/characters/{Guid.NewGuid()}");
    Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
  }

  [Fact]
  public async Task Preview_AsksForSubclass_AndSavesNothing()
  {
    CharacterStateDto state = await this.WizardAsync(2);

    LevelUpPreviewDto withoutSubclass = await this._api.GetAsync<LevelUpPreviewDto>($"/api/characters/{state.Id}/level-up/preview?class=wizard");
    Assert.True(withoutSubclass.RequiresSubclass);
    Assert.Contains(withoutSubclass.Subclasses, entry => entry.Key == "evoker");

    LevelUpPreviewDto preview = await this._api.GetAsync<LevelUpPreviewDto>($"/api/characters/{state.Id}/level-up/preview?class=wizard&subclass=evoker");
    Assert.Equal(3, preview.CharacterLevel);
    PendingChoiceDto savant = preview.PendingChoices.Single(entry => entry.ChoiceKey == "evoker.evocation-savant");
    Assert.Contains("preview=wizard%3Aevoker", savant.OptionsUrl);

    PagedResult<OptionDto> options = await this._api.GetAsync<PagedResult<OptionDto>>(savant.OptionsUrl);
    Assert.NotEmpty(options.Items);
    Assert.All(options.Items, entry => Assert.Equal("evocation", entry.Meta.School));

    CharacterStateDto after = await this._api.GetAsync<CharacterStateDto>($"/api/characters/{state.Id}");
    Assert.Equal(2, after.Level);
    Assert.DoesNotContain(after.PendingChoices, entry => entry.ChoiceKey == "evoker.evocation-savant");
  }

  [Fact]
  public async Task Undo_RemovesTheLevelAndItsSelections()
  {
    CharacterStateDto state = await this.WizardAsync(2);
    int selections = (await this._api.GetAsync<IReadOnlyList<SelectionInfoDto>>($"/api/characters/{state.Id}/selections")).Count;

    state = await this._api.AnswerAllAsync(await this._api.LevelUpAsync(state.Id, "wizard", "evoker"));
    Assert.Equal(3, state.Level);

    CharacterStateDto undone = await this._api.DeleteAsync<CharacterStateDto>($"/api/characters/{state.Id}/levels/last");
    Assert.Equal(2, undone.Level);
    Assert.Equal(selections, (await this._api.GetAsync<IReadOnlyList<SelectionInfoDto>>($"/api/characters/{state.Id}/selections")).Count);
  }

  [Fact]
  public async Task RemoveSelection_OnlyOnTheLatestLevel()
  {
    CharacterStateDto state = await this.WizardAsync(2);
    IReadOnlyList<SelectionInfoDto> selections = await this._api.GetAsync<IReadOnlyList<SelectionInfoDto>>($"/api/characters/{state.Id}/selections");

    SelectionInfoDto origin = selections.First(entry => entry.Level == null);
    Assert.False(origin.Removable);
    Assert.Equal(HttpStatusCode.UnprocessableEntity, await ApiClient.StatusOf(() => this._api.DeleteAsync<CharacterStateDto>($"/api/characters/{state.Id}/selections/{origin.Id}")));

    SelectionInfoDto latest = selections.First(entry => entry.Removable);
    CharacterStateDto after = await this._api.DeleteAsync<CharacterStateDto>($"/api/characters/{state.Id}/selections/{latest.Id}");
    Assert.Contains(after.PendingChoices, entry => entry.ChoiceKey == latest.ChoiceKey);
  }

  [Fact]
  public async Task Validate_ReportsErrorsAndState_WithoutSaving()
  {
    CharacterStateDto draft = await this._api.CreateDraftAsync();

    ValidationResultDto invalid = await this._api.PostAsync<ValidationResultDto>($"/api/characters/{draft.Id}/choices/validate", new SelectionsDto([new("elf.keen-senses", "arcana")]));
    Assert.False(invalid.Valid);
    Assert.NotEmpty(invalid.Errors);

    ValidationResultDto valid = await this._api.PostAsync<ValidationResultDto>($"/api/characters/{draft.Id}/choices/validate", new SelectionsDto([new("magic-initiate.list", "wizard")]));
    Assert.True(valid.Valid);
    Assert.Contains(valid.State!.PendingChoices, entry => entry.ChoiceKey == "magic-initiate.wizard.spell");

    CharacterStateDto after = await this._api.GetAsync<CharacterStateDto>($"/api/characters/{draft.Id}");
    Assert.Contains(after.PendingChoices, entry => entry.ChoiceKey == "magic-initiate.list");
    Assert.DoesNotContain(after.PendingChoices, entry => entry.ChoiceKey == "magic-initiate.wizard.spell");

    ValidationResultDto level = await this._api.PostAsync<ValidationResultDto>($"/api/characters/{draft.Id}/levels/validate", new LevelUpDto { ClassKey = "wizard", AllowIncomplete = false });
    Assert.False(level.Valid);
  }

  [Fact]
  public async Task UpdateAndDelete()
  {
    CharacterStateDto draft = await this._api.CreateDraftAsync();

    HttpResponseMessage patched = await this._api.Http.PatchAsJsonAsync($"/api/characters/{draft.Id}", new UpdateCharacterDto { Name = "Renamed", Backstory = "Raised by owls" }, ApiClient.Json);
    patched.EnsureSuccessStatusCode();
    Assert.Equal("Renamed", (await this._api.GetAsync<CharacterStateDto>($"/api/characters/{draft.Id}")).Name);

    HttpResponseMessage deleted = await this._api.Http.DeleteAsync($"/api/characters/{draft.Id}");
    Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
    Assert.Equal(HttpStatusCode.NotFound, (await this._api.Http.GetAsync($"/api/characters/{draft.Id}")).StatusCode);
  }

  [Fact]
  public async Task Pdf_IsReturned()
  {
    CharacterStateDto state = await this.WizardAsync(1);

    HttpResponseMessage response = await this._api.Http.GetAsync($"/api/characters/{state.Id}/sheet.pdf");
    response.EnsureSuccessStatusCode();
    Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
    byte[] content = await response.Content.ReadAsByteArrayAsync();
    Assert.True(content.Length > 1000);
    Assert.Equal("%PDF"u8.ToArray(), content.Take(4).ToArray());
  }

  #endregion

  #region Helpers

  /// <summary>
  ///   Complete elf sage wizard of the level; every choice answered through the API
  /// </summary>
  private async Task<CharacterStateDto> WizardAsync(int level, string? subclass = null)
  {
    CharacterStateDto state = await this._api.AnswerAllAsync(await this._api.CreateDraftAsync());
    for (int current = 1; current <= level; current++)
    {
      state = await this._api.AnswerAllAsync(await this._api.LevelUpAsync(state.Id, "wizard", current == 3 ? subclass : null));
    }

    Assert.True(state.Complete);
    return state;
  }

  #endregion
}
