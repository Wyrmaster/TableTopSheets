namespace TableTopSheets.Api.Querying;

/// <summary>
///   Whitelist of the fields a list can be sorted by
/// </summary>
public sealed class SortMap<T>
{
  #region Fields

  private readonly Dictionary<string, Func<T, IComparable?>> _fields = new(StringComparer.OrdinalIgnoreCase);

  #endregion

  #region Methods

  public SortMap<T> Add(string field, Func<T, IComparable?> selector)
  {
    this._fields[field] = selector;
    return this;
  }

  public IEnumerable<string> Fields => this._fields.Keys;

  /// <summary>
  ///   Sort by a comma separated list of fields (<c>-</c> prefix = descending); null keeps the order
  /// </summary>
  public IEnumerable<T> Apply(IEnumerable<T> source, string? sort)
  {
    if (string.IsNullOrWhiteSpace(sort))
    {
      return source;
    }

    IOrderedEnumerable<T>? ordered = null;
    foreach (string part in sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    {
      bool descending = part.StartsWith('-');
      string field = part.TrimStart('-', '+');
      if (!this._fields.TryGetValue(field, out Func<T, IComparable?>? selector))
      {
        throw new QueryException("sort", $"Unknown sort field '{field}' (allowed: {string.Join(", ", this.Fields)})");
      }

      ordered = (ordered, descending) switch
      {
        (null, false) => source.OrderBy(selector),
        (null, true) => source.OrderByDescending(selector),
        (_, false) => ordered.ThenBy(selector),
        (_, true) => ordered.ThenByDescending(selector),
      };
    }

    return ordered ?? source;
  }

  #endregion
}
