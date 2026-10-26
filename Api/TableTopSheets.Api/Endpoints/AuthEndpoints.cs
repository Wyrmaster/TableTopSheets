using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TableTopSheets.Api.Authentication;
using TableTopSheets.Api.Contracts;
using TableTopSheets.Persistence;
using TableTopSheets.Persistence.Entity.Users;

namespace TableTopSheets.Api.Endpoints;

/// <summary>
///   Sign in (endpoints of the configured <see cref="IAuthenticationProvider"/>) and the signed in user
/// </summary>
public static class AuthEndpoints
{
  #region Methods

  public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
  {
    RouteGroupBuilder auth = routes.MapGroup("/api/auth").WithTags("Authentication").AllowAnonymous();
    routes.ServiceProvider.GetRequiredService<IAuthenticationProvider>().MapEndpoints(auth);

    RouteGroupBuilder users = routes.MapGroup("/api/users").WithTags("Users").RequireUser();
    users.MapGet("/me", Me).WithName("GetCurrentUser").WithSummary("The signed in user");
    return routes;
  }

  private static async Task<Ok<CurrentUserDto>> Me(TableTopSheetsContext context, CurrentUser currentUser, CancellationToken cancellationToken)
  {
    User user = await context.Users.AsNoTracking().SingleAsync(entry => entry.Id == currentUser.UserId, cancellationToken);
    int characters = await context.Characters.CountAsync(entry => entry.OwnerId == user.Id, cancellationToken);
    return TypedResults.Ok(new CurrentUserDto(user.Id, user.UserName, user.Email, characters));
  }

  #endregion
}
