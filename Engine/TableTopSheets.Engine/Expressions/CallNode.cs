namespace TableTopSheets.Engine.Expressions;

internal sealed record CallNode(string Function, IReadOnlyList<Node> Arguments): Node
{
  public override ExprValue Evaluate(IExpressionContext context)
  {
    // short circuit helpers that don't need the context
    List<ExprValue> arguments = this.Arguments.Select(argument => argument.Evaluate(context)).ToList();
    return this.Function switch
    {
      "floor" => ExprValue.Of(Math.Floor(Single(arguments).AsNumber())),
      "ceil" => ExprValue.Of(Math.Ceiling(Single(arguments).AsNumber())),
      "min" => ExprValue.Of(arguments.Min(argument => argument.AsNumber())),
      "max" => ExprValue.Of(arguments.Max(argument => argument.AsNumber())),
      "oneOf" => ExprValue.Of(arguments.Skip(1).Any(argument => argument.AsString() == arguments[0].AsString())),
      _ => context.Call(this.Function, arguments),
    };
  }

  public override IEnumerable<Node> Descendants() => new Node[] { this }.Concat(this.Arguments.SelectMany(argument => argument.Descendants()));

  private ExprValue Single(List<ExprValue> arguments) => arguments.Count == 1
    ? arguments[0]
    : throw new ExpressionException($"{this.Function} expects one argument");
}
