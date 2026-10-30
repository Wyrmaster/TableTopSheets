namespace TableTopSheets.Api.Querying;

/// <summary>
///   Invalid paging, sorting or filter parameters (answered with 400)
/// </summary>
public sealed class QueryException(string parameter, string message) : Exception(message)
{
  public string Parameter { get; } = parameter;
}
