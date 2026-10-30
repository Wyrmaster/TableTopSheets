using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using TableTopSheets.Api.Contracts;
using TableTopSheets.Api.Mapping;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence;

namespace TableTopSheets.Api.Endpoints;

/// <summary>
///   Artwork of spells, classes, species and backgrounds, and scenes. Anonymous like the rules: an <c>img</c> element
///   can't send a bearer token
/// </summary>
public static class ImageEndpoints
{
  #region Constants

  /// <summary>
  ///   Images only change with a new seed; the ETag lets browsers check cheaply after a day
  /// </summary>
  private const string CacheControl = "public, max-age=86400";

  #endregion

  #region Methods

  public static RouteGroupBuilder MapImageEndpoints(this IEndpointRouteBuilder routes)
  {
    RouteGroupBuilder group = routes
      .MapGroup("/api/images")
      .WithTags("Images");

    group
      .MapGet("/scenes", ListScenes)
      .WithName("ListScenes")
      .WithSummary("List the scenes (mood pictures shown at random)");
    group
      .MapGet("/{category}/{key}", GetImage)
      .WithName("GetImage")
      .WithSummary("Image of a spell, class, species, background, item, coin or scene")
      .WithDescription("category: spell, class, species, background, item, coin or scene. thumb=true answers the small version. "
                       + "Answers 304 when If-None-Match carries the current ETag.")
      .Produces(StatusCodes.Status200OK, contentType: "image/webp")
      .Produces(StatusCodes.Status304NotModified)
      .ProducesProblem(StatusCodes.Status404NotFound);
    return group;
  }

  /// <summary>
  ///   Keys of the rule entities of a category that have an image
  /// </summary>
  public static async Task<IReadOnlySet<string>> ImageOwnersAsync(this TableTopSheetsContext context, ImageCategoryEnum category, CancellationToken cancellationToken)
    => await context.Images
      .AsNoTracking()
      .Where(entry => entry.Category == category)
      .Select(entry => entry.OwnerKey)
      .ToHashSetAsync(cancellationToken);

  private static async Task<List<SceneDto>> ListScenes(TableTopSheetsContext context, CancellationToken cancellationToken)
  {
    var scenes = await context.Images
      .AsNoTracking()
      .Where(entry => entry.Category == ImageCategoryEnum.Scene)
      .OrderBy(entry => entry.OwnerKey)
      .Select(entry => new
      {
        entry.OwnerKey,
        entry.Title,
        entry.Width,
        entry.Height,
      })
      .ToListAsync(cancellationToken);
    return scenes
      .Select(entry => new SceneDto
      (
        entry.OwnerKey,
        entry.Title,
        entry.Width,
        entry.Height,
        ImageLinks.Url(ImageCategoryEnum.Scene, entry.OwnerKey),
        ImageLinks.Url(ImageCategoryEnum.Scene, entry.OwnerKey, true)
      ))
      .ToList();
  }

  private static async Task<Results<FileContentHttpResult, StatusCodeHttpResult, NotFound>> GetImage
  (
    HttpContext httpContext,
    TableTopSheetsContext context,
    string category,
    string key,
    bool? thumb,
    CancellationToken cancellationToken
  )
  {
    if (!Enum.TryParse(category, true, out ImageCategoryEnum parsed) || int.TryParse(category, out _))
    {
      return TypedResults.NotFound();
    }

    bool thumbnail = thumb == true;
    var header = await context.Images
      .AsNoTracking()
      .Where(entry => entry.Category == parsed && entry.OwnerKey == key)
      .Select(entry => new
      {
        entry.Id,
        entry.ContentHash,
        entry.ContentType,
      })
      .FirstOrDefaultAsync(cancellationToken);
    if (header == null)
    {
      return TypedResults.NotFound();
    }

    // image and thumbnail share the row, so their tags differ by a suffix
    EntityTagHeaderValue tag = new($"\"{header.ContentHash}{(thumbnail ? "-thumb" : string.Empty)}\"");
    httpContext.Response.Headers.CacheControl = CacheControl;
    if (httpContext.Request
      .GetTypedHeaders()
      .IfNoneMatch
      .Any(entry => entry.Compare(tag, false) || entry.Equals(EntityTagHeaderValue.Any)))
    {
      httpContext.Response.Headers.ETag = tag.ToString();
      return TypedResults.StatusCode(StatusCodes.Status304NotModified);
    }

    byte[] data = await context.Images
      .AsNoTracking()
      .Where(entry => entry.Id == header.Id)
      .Select(entry => thumbnail ? entry.ThumbnailData : entry.Data)
      .FirstAsync(cancellationToken);
    return TypedResults.File(data, header.ContentType, entityTag: tag);
  }

  #endregion
}
