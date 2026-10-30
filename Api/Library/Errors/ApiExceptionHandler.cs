using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TableTopSheets.Api.Querying;
using TableTopSheets.Engine.Services;

namespace TableTopSheets.Api.Errors;

/// <summary>
///   Turns rule violations into 422, unknown characters into 404 and invalid query parameters or bodies into 400 problem details
/// </summary>
public sealed class ApiExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
  #region Methods

  public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
  {
    ProblemDetails? problem = exception switch
    {
      CharacterNotFoundException notFound => new ProblemDetails
      {
        Status = StatusCodes.Status404NotFound,
        Title = "Character not found",
        Detail = notFound.Message,
      },
      RuleViolationException violation => Problem(StatusCodes.Status422UnprocessableEntity, "The request breaks the rules", "rules", violation.Errors),
      BadHttpRequestException badRequest => new ProblemDetails
      {
        Status = badRequest.StatusCode,
        Title = "Bad request",
        Detail = badRequest.Message,
      },
      QueryException query => Problem(StatusCodes.Status400BadRequest, "Invalid query", query.Parameter, [query.Message]),
      _ => null,
    };
    if (problem == null)
    {
      return false;
    }

    httpContext.Response.StatusCode = problem.Status!.Value;
    return await problemDetails.TryWriteAsync(new ProblemDetailsContext
    {
      HttpContext = httpContext,
      ProblemDetails = problem,
      Exception = exception,
    });
  }

  private static HttpValidationProblemDetails Problem(int status, string title, string key, IReadOnlyList<string> errors) => new(new Dictionary<string, string[]> { [key] = errors.ToArray() })
  {
    Status = status,
    Title = title,
    Detail = string.Join(Environment.NewLine, errors),
  };

  #endregion
}
