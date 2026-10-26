using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TableTopSheets.Api.Contracts;

namespace TableTopSheets.Api.Tests;

/// <summary>
///   Typed calls of the API; <see cref="AnswerAllAsync"/> answers choices only from the metadata the API returns, the way a
///   generic GUI would
/// </summary>
public sealed class ApiClient(HttpClient http)
{
  #region Fields

  public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

  #endregion

  #region Properties

  public HttpClient Http { get; } = http;

  #endregion

  #region Requests

  public async Task<T> GetAsync<T>(string url)
  {
    HttpResponseMessage response = await this.Http.GetAsync(url);
    return await Read<T>(response);
  }

  public async Task<T> PostAsync<T>(string url, object body)
  {
    HttpResponseMessage response = await this.Http.PostAsJsonAsync(url, body, Json);
    return await Read<T>(response);
  }

  public async Task<T> DeleteAsync<T>(string url)
  {
    HttpResponseMessage response = await this.Http.DeleteAsync(url);
    return await Read<T>(response);
  }

  public async Task<HttpResponseMessage> PostRawAsync(string url, object body) => await this.Http.PostAsJsonAsync(url, body, Json);

  private static async Task<T> Read<T>(HttpResponseMessage response)
  {
    if (!response.IsSuccessStatusCode)
    {
      throw new HttpRequestException($"{(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}", null, response.StatusCode);
    }

    return (await response.Content.ReadFromJsonAsync<T>(Json))!;
  }

  #endregion

  #region Authentication

  public const string Password = "correct horse battery staple";

  /// <summary>
  ///   Register a user (unique name by default) and send its token with every following request
  /// </summary>
  public async Task<TokenDto> SignInAsync(string? userName = null)
  {
    TokenDto token = await this.PostAsync<TokenDto>("/api/auth/register", new RegisterDto(userName ?? $"user-{Guid.NewGuid():N}", Password));
    this.UseToken(token.AccessToken);
    return token;
  }

  public void UseToken(string? accessToken)
    => this.Http.DefaultRequestHeaders.Authorization = accessToken == null ? null : new AuthenticationHeaderValue("Bearer", accessToken);

  #endregion

  #region Character Helpers

  public static readonly IReadOnlyDictionary<string, byte> Scores = new Dictionary<string, byte>
  {
    ["str"] = 8, ["dex"] = 14, ["con"] = 13, ["int"] = 15, ["wis"] = 12, ["cha"] = 10,
  };

  public Task<CharacterStateDto> CreateDraftAsync(string name = "Elara", string species = "elf", string background = "sage")
    => this.PostAsync<CharacterStateDto>("/api/characters", new CreateCharacterDto
    {
      Name = name,
      SpeciesKey = species,
      BackgroundKey = background,
      Abilities = Scores,
    });

  public Task<CharacterStateDto> SelectAsync(Guid id, params SelectionDto[] selections)
    => this.PostAsync<CharacterStateDto>($"/api/characters/{id}/choices", new SelectionsDto(selections));

  public Task<CharacterStateDto> LevelUpAsync(Guid id, string classKey, string? subclassKey = null)
    => this.PostAsync<CharacterStateDto>($"/api/characters/{id}/levels", new LevelUpDto { ClassKey = classKey, SubclassKey = subclassKey });

  /// <summary>
  ///   Answer every pending choice with the first available options until nothing is open; feat choices take the
  ///   Ability Score Improvement so the follow-up choice is exercised
  /// </summary>
  public async Task<CharacterStateDto> AnswerAllAsync(CharacterStateDto state)
  {
    for (int round = 0; round < 20 && state.PendingChoices.Count > 0; round++)
    {
      PendingChoiceDto choice = state.PendingChoices[0];
      IReadOnlyList<OptionDto> options = choice.Options
                                         ?? (await this.GetAsync<PagedResult<OptionDto>>($"{choice.OptionsUrl}{(choice.OptionsUrl.Contains('?') ? '&' : '?')}pageSize=200")).Items;
      if (choice.OptionSource == Common.Enum.Rules.OptionSourceEnum.Feat && options.Any(entry => entry.Key == "ability-score-improvement"))
      {
        options = options.Where(entry => entry.Key == "ability-score-improvement").ToList();
      }

      List<SelectionDto> selections = [];
      int perOption = choice.Input == ChoiceInputKind.Distribution ? choice.Constraints.MaxPerOption : 1;
      foreach (OptionDto option in options)
      {
        for (int pick = 0; pick < perOption && selections.Count < choice.Remaining; pick++)
        {
          selections.Add(new SelectionDto(choice.ChoiceKey, option.Key, ParentRef: choice.ParentRef));
        }
      }

      state = await this.SelectAsync(state.Id, selections.ToArray());
    }

    return state;
  }

  public static async Task<HttpStatusCode> StatusOf(Func<Task> call)
  {
    try
    {
      await call();
      return HttpStatusCode.OK;
    }
    catch (HttpRequestException exception)
    {
      return exception.StatusCode ?? HttpStatusCode.InternalServerError;
    }
  }

  #endregion
}
