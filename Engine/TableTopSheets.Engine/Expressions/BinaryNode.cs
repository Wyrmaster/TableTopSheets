namespace TableTopSheets.Engine.Expressions;

internal sealed record BinaryNode(string Operator, Node Left, Node Right): Node
{
  public override ExprValue Evaluate(IExpressionContext context)
  {
    switch (this.Operator)
    {
      case "&&":
        return ExprValue.Of(this.Left.Evaluate(context).AsBoolean() && this.Right.Evaluate(context).AsBoolean());
      case "||":
        return ExprValue.Of(this.Left.Evaluate(context).AsBoolean() || this.Right.Evaluate(context).AsBoolean());
    }

    ExprValue left = this.Left.Evaluate(context);
    ExprValue right = this.Right.Evaluate(context);
    return this.Operator switch
    {
      "==" => ExprValue.Of(Equal(left, right)),
      "!=" => ExprValue.Of(!Equal(left, right)),
      "<" => ExprValue.Of(left.AsNumber() < right.AsNumber()),
      "<=" => ExprValue.Of(left.AsNumber() <= right.AsNumber()),
      ">" => ExprValue.Of(left.AsNumber() > right.AsNumber()),
      ">=" => ExprValue.Of(left.AsNumber() >= right.AsNumber()),
      "+" => ExprValue.Of(left.AsNumber() + right.AsNumber()),
      "-" => ExprValue.Of(left.AsNumber() - right.AsNumber()),
      "*" => ExprValue.Of(left.AsNumber() * right.AsNumber()),
      "/" => ExprValue.Of(left.AsNumber() / right.AsNumber()),
      "%" => ExprValue.Of(left.AsNumber() % right.AsNumber()),
      _ => throw new ExpressionException($"Unknown operator {this.Operator}"),
    };
  }

  public override IEnumerable<Node> Descendants() => new Node[] { this }.Concat(this.Left.Descendants()).Concat(this.Right.Descendants());

  private static bool Equal(ExprValue left, ExprValue right)
    => left.Kind == ExprValueKind.String || right.Kind == ExprValueKind.String
      ? left.AsString() == right.AsString()
      : Math.Abs(left.AsNumber() - right.AsNumber()) < 1e-9;
}
