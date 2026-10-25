namespace TableTopSheets.Api.Contracts;

/// <summary>
///   Paging, search and sort parameters shared by every list endpoint
/// </summary>
public sealed record ListQuery
{
  public const int DefaultPageSize = 50;
  public const int MaxPageSize = 200;

  /// <summary>1-based page number</summary>
  public int? Page { get; init; }

  /// <summary>Entries per page (1 - 200, default 50)</summary>
  public int? PageSize { get; init; }

  /// <summary>Case-insensitive search in name and key</summary>
  public string? Search { get; init; }

  /// <summary>Comma separated sort fields; a leading <c>-</c> sorts descending (e.g. <c>level,-name</c>)</summary>
  public string? Sort { get; init; }
}

/// <summary>
///   One page of a list
/// </summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
  public int TotalPages => this.TotalCount == 0 ? 0 : (this.TotalCount + this.PageSize - 1) / this.PageSize;
}
