using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Background in a list</summary>
public sealed record BackgroundSummaryDto(string Key, string Name, IReadOnlyList<string> AbilityOptions, string? OriginFeat)
{
  /// <summary>Picture (<c>/api/images/background/{key}</c>); null when there is none</summary>
  public string? ImageUrl { get; init; }

  /// <summary>Small version of the picture for lists and cards; null when there is none</summary>
  public string? ThumbnailUrl { get; init; }
}
