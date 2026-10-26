using System.Linq.Expressions;

namespace TableTopSheets.Api.Querying;

/// <summary>
///   Whitelist of the fields a database query can be sorted by
/// </summary>
public sealed class QuerySortMap<T>
{
  #region Fields

  private readonly Dictionary<string, Sorter> _fields = new(StringComparer.OrdinalIgnoreCase);

  #endregion

  #region Methods

  public QuerySortMap<T> Add<TKey>(string field, Expression<Func<T, TKey>> selector)
  {
    this._fields[field] = (source, first, descending) => (first, descending) switch
    {
      (true, false) => source.OrderBy(selector),
      (true, true) => source.OrderByDescending(selector),
      (false, false) => ((IOrderedQueryable<T>)source).ThenBy(selector),
      (false, true) => ((IOrderedQueryable<T>)source).ThenByDescending(selector),
    };
    return this;
  }

  /// <summary>
  ///   Sort by the key of an enum value (e.g. <c>fighting-style</c>) instead of its number, so the order matches the keys
  ///   clients see
  /// </summary>
  public QuerySortMap<T> AddKey<TEnum>(string field, Expression<Func<T, TEnum>> member, Func<TEnum, string> key) where TEnum : struct, Enum
  {
    Type underlying = Enum.GetUnderlyingType(typeof(TEnum));
    Expression number = Expression.Convert(member.Body, underlying);
    Expression body = Expression.Constant(null, typeof(string));
    foreach (TEnum value in Enum.GetValues<TEnum>().Reverse())
    {
      body = Expression.Condition
      (
        Expression.Equal(number, Expression.Constant(Convert.ChangeType(value, underlying), underlying)),
        Expression.Constant(key(value), typeof(string)),
        body
      );
    }

    return this.Add(field, Expression.Lambda<Func<T, string>>(body, member.Parameters));
  }

  public IEnumerable<string> Fields => this._fields.Keys;

  /// <summary>
  ///   Sort by a comma separated list of fields (<c>-</c> prefix = descending), then by a unique field so pages are stable
  /// </summary>
  public IQueryable<T> Apply<TKey>(IQueryable<T> source, string sort, Expression<Func<T, TKey>> tieBreaker)
  {
    bool first = true;
    foreach (string part in sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    {
      bool descending = part.StartsWith('-');
      string field = part.TrimStart('-', '+');
      if (!this._fields.TryGetValue(field, out Sorter? sorter))
      {
        throw new QueryException("sort", $"Unknown sort field '{field}' (allowed: {string.Join(", ", this.Fields)})");
      }

      source = sorter(source, first, descending);
      first = false;
    }

    return first ? source.OrderBy(tieBreaker) : ((IOrderedQueryable<T>)source).ThenBy(tieBreaker);
  }

  #endregion

  #region Nested Types

  private delegate IOrderedQueryable<T> Sorter(IQueryable<T> source, bool first, bool descending);

  #endregion
}
