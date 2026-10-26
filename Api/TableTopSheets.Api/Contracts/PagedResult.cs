namespace TableTopSheets.Api.Contracts;

/// <summary>
///   One page of a list
/// </summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
  public int TotalPages => this.TotalCount == 0 ? 0 : (this.TotalCount + this.PageSize - 1) / this.PageSize;
}
