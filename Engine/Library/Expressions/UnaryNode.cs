namespace TableTopSheets.Engine.Expressions;

internal sealed record UnaryNode(string Operator, Node Operand): Node
{
  public override ExprValue Evaluate(IExpressionContext context)
  {
    ExprValue value = this.Operand.Evaluate(context);
    return this.Operator switch
    {
      "!" => ExprValue.Of(!value.AsBoolean()),
      "-" => ExprValue.Of(-value.AsNumber()),
      _ => throw new ExpressionException($"Unknown unary operator {this.Operator}"),
    };
  }

  public override IEnumerable<Node> Descendants() => new Node[] { this }.Concat(this.Operand.Descendants());
}
