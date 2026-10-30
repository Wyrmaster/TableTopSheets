namespace TableTopSheets.Engine.Expressions;

internal sealed record IdentifierNode(string Name): Node
{
  public override ExprValue Evaluate(IExpressionContext context) => this.Name switch
  {
    "true" => ExprValue.True,
    "false" => ExprValue.False,
    _ => context.Resolve(this.Name),
  };
}
