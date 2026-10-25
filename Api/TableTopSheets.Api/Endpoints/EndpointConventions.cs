namespace TableTopSheets.Api.Endpoints;

public static class EndpointConventions
{
  #region Methods

  /// <summary>
  ///   Document the problem responses every endpoint of a group can answer with (400 invalid query or body, 422 rule violation)
  /// </summary>
  public static RouteGroupBuilder WithRuleProblems(this RouteGroupBuilder group)
  {
    group.ProducesValidationProblem(StatusCodes.Status400BadRequest);
    group.ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
    return group;
  }

  /// <summary>
  ///   Additionally document 404 for endpoints addressing a character
  /// </summary>
  public static RouteGroupBuilder WithCharacterProblems(this RouteGroupBuilder group)
  {
    group.WithRuleProblems();
    group.ProducesProblem(StatusCodes.Status404NotFound);
    return group;
  }

  #endregion
}
