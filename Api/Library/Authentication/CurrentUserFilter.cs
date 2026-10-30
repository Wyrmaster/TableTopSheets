using TableTopSheets.Persistence;
using TableTopSheets.Persistence.Entity.Users;

namespace TableTopSheets.Api.Authentication;

/// <summary>
///   Maps the authenticated principal to its <see cref="User"/> row and stores it in <see cref="CurrentUser"/>
/// </summary>
public sealed class CurrentUserFilter(IAuthenticationProvider provider, TableTopSheetsContext context, CurrentUser currentUser) : IEndpointFilter
{
  #region IEndpointFilter

  public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext invocation, EndpointFilterDelegate next)
  {
    HttpContext http = invocation.HttpContext;
    User? user = await provider.ResolveUserAsync(http.User, context, http.RequestAborted);
    if (user == null)
    {
      return TypedResults.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Unknown user", detail: "The token doesn't belong to a known user");
    }

    currentUser.UserId = user.Id;
    return await next(invocation);
  }

  #endregion
}
