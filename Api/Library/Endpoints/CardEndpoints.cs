using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TableTopSheets.Api.Authentication;
using TableTopSheets.Api.Contracts;
using TableTopSheets.Api.Mapping;
using TableTopSheets.Common.Card;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Engine.Services;
using TableTopSheets.Pdf;
using TableTopSheets.Persistence;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Api.Endpoints;

/// <summary>
///   Printable cards of spells and items
/// </summary>
public static class CardEndpoints
{
  #region Constants

  /// <summary>
  ///   Most cards of one request; a PDF of more gets slow to build and print
  /// </summary>
  public const int MaxCards = 200;

  #endregion

  #region Methods

  public static RouteGroupBuilder MapCardEndpoints(this IEndpointRouteBuilder routes)
  {
    RouteGroupBuilder group = routes
      .MapGroup("/api")
      .WithTags("Cards")
      .WithCharacterProblems()
      .RequireUser();

    group
      .MapPost("/cards.pdf", Pdf)
      .WithName("CreateCardsPdf")
      .WithSummary("Printable cards of spells and items")
      .WithDescription($"Each card shows the picture, the key facts and the full text. Every card gets the smallest of three sizes "
                       + $"its text fits (poker, A6, A5), and very long texts continue on further cards. At most {MaxCards} cards; "
                       + "unknown keys answer 422.")
      .Produces(StatusCodes.Status200OK, contentType: "application/pdf");
    return group;
  }

  private static async Task<FileContentHttpResult> Pdf
  (
    TableTopSheetsContext context,
    CardsRequestDto request,
    CancellationToken cancellationToken
  )
  {
    List<string> spellKeys = (request.Spells ?? [])
      .Distinct()
      .ToList();
    List<string> itemKeys = (request.Items ?? [])
      .Distinct()
      .ToList();
    if (spellKeys.Count + itemKeys.Count == 0)
    {
      throw new RuleViolationException("Select at least one spell or item");
    }

    if (spellKeys.Count + itemKeys.Count > MaxCards)
    {
      throw new RuleViolationException($"At most {MaxCards} cards can be printed at once");
    }

    Dictionary<string, Spell> spells = await context.Spells
      .AsNoTracking()
      .Where(entry => spellKeys.Contains(entry.Key))
      .ToDictionaryAsync(entry => entry.Key, cancellationToken);
    Dictionary<string, Item> items = await context.Items
      .AsNoTracking()
      .Include(entry => ((Weapon)entry).Mastery)
      .Where(entry => itemKeys.Contains(entry.Key))
      .ToDictionaryAsync(entry => entry.Key, cancellationToken);
    List<string> unknown = spellKeys
      .Where(key => !spells.ContainsKey(key))
      .Select(key => $"Spell '{key}' doesn't exist")
      .Concat(itemKeys
        .Where(key => !items.ContainsKey(key))
        .Select(key => $"Item '{key}' doesn't exist"))
      .ToList();
    if (unknown.Count > 0)
    {
      throw new RuleViolationException(unknown);
    }

    // the artwork is JPEG, which the PDF embeds as it is
    Dictionary<(ImageCategoryEnum, string), byte[]> images = await Images(context, spellKeys, itemKeys, cancellationToken);
    byte[]? Picture(ImageCategoryEnum category, string key) => images.GetValueOrDefault((category, key));

    List<CardData> cards = spellKeys
      .Select(key => CardMapper.Card(spells[key], Picture(ImageCategoryEnum.Spell, key)))
      .Concat(itemKeys.Select(key => CardMapper.Card(items[key], Picture(ImageCategoryEnum.Item, key))))
      .ToList();

    using MemoryStream stream = new();
    await using (Stream pdf = new CardDocument(cards).GenerateStream())
    {
      await pdf.CopyToAsync(stream, cancellationToken);
    }

    return TypedResults.File(stream.ToArray(), "application/pdf", "cards.pdf");
  }

  private static async Task<Dictionary<(ImageCategoryEnum, string), byte[]>> Images
  (
    TableTopSheetsContext context,
    List<string> spellKeys,
    List<string> itemKeys,
    CancellationToken cancellationToken
  )
  {
    var rows = await context.Images
      .AsNoTracking()
      .Where(entry => (entry.Category == ImageCategoryEnum.Spell && spellKeys.Contains(entry.OwnerKey))
                      || (entry.Category == ImageCategoryEnum.Item && itemKeys.Contains(entry.OwnerKey)))
      .Select(entry => new { entry.Category, entry.OwnerKey, entry.Data })
      .ToListAsync(cancellationToken);
    return rows
      .GroupBy(entry => (entry.Category, entry.OwnerKey))
      .ToDictionary(group => group.Key, group => group.First().Data);
  }

  #endregion
}
