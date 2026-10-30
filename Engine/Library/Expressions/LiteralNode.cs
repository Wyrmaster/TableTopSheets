namespace TableTopSheets.Engine.Expressions;

internal sealed record LiteralNode(ExprValue Value): Node
{
  public override ExprValue Evaluate(IExpressionContext context) => this.Value;
}
