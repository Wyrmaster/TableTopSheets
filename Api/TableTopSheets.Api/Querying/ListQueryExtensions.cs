using TableTopSheets.Api.Contracts;

namespace TableTopSheets.Api.Querying;

public static class ListQueryExtensions
{
  #region Methods

  /// <summary>
  ///   Search, sort and page an in memory list
  /// </summary>
  /// <param name="source">filtered entries</param>
  /// <param name="query">paging, search and sort parameters</param>
  /// <param name="sorts">allowed sort fields</param>
  /// <param name="searchText">texts the search matches (name, key ...)</param>
  /// <param name="defaultSort">sort used when the query has none (null keeps the order of the source)</param>
  public static PagedResult<T> ToPaged<T>(this IEnumerable<T> source, ListQuery query, SortMap<T> sorts, Func<T, IEnumerable<string?>> searchText, string? defaultSort = "name")
  {
    int page = query.Page ?? 1;
    int pageSize = query.PageSize ?? ListQuery.DefaultPageSize;
    if (page < 1)
    {
      throw new QueryException("page", "The page has to be 1 or higher");
    }

    if (pageSize is < 1 or > ListQuery.MaxPageSize)
    {
      throw new QueryException("pageSize", $"The page size has to be between 1 and {ListQuery.MaxPageSize}");
    }

    if (!string.IsNullOrWhiteSpace(query.Search))
    {
      string search = query.Search.Trim();
      source = source.Where(entry => searchText(entry).Any(text => text?.Contains(search, StringComparison.OrdinalIgnoreCase) == true));
    }

    List<T> sorted = sorts.Apply(source, query.Sort ?? defaultSort).ToList();
    return new PagedResult<T>(sorted.Skip((page - 1) * pageSize).Take(pageSize).ToList(), page, pageSize, sorted.Count);
  }

  /// <summary>
  ///   Map the entries of a page (e.g. to load descriptions only for the visible entries)
  /// </summary>
  public static PagedResult<TResult> Select<T, TResult>(this PagedResult<T> page, Func<T, TResult> map)
    => new(page.Items.Select(map).ToList(), page.Page, page.PageSize, page.TotalCount);

  /// <summary>
  ///   Apply a filter only when the parameter is given
  /// </summary>
  public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, bool condition, Func<T, bool> predicate)
    => condition ? source.Where(predicate) : source;

  #endregion
}
