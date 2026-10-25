namespace TableTopSheets.Engine.Expressions;

/// <summary>
///   Node of a parsed expression
/// </summary>
internal abstract record Node
{
  public abstract ExprValue Evaluate(IExpressionContext context);

  /// <summary>
  ///   Visit this node and all children (used for static validation)
  /// </summary>
  public virtual IEnumerable<Node> Descendants() => [this];
}

internal sealed record LiteralNode(ExprValue Value): Node
{
  public override ExprValue Evaluate(IExpressionContext context) => this.Value;
}

internal sealed record IdentifierNode(string Name): Node
{
  public override ExprValue Evaluate(IExpressionContext context) => this.Name switch
  {
    "true" => ExprValue.True,
    "false" => ExprValue.False,
    _ => context.Resolve(this.Name),
  };
}

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

internal sealed record ConditionalNode(Node Condition, Node WhenTrue, Node WhenFalse): Node
{
  public override ExprValue Evaluate(IExpressionContext context)
    => this.Condition.Evaluate(context).AsBoolean() ? this.WhenTrue.Evaluate(context) : this.WhenFalse.Evaluate(context);

  public override IEnumerable<Node> Descendants()
    => new Node[] { this }.Concat(this.Condition.Descendants()).Concat(this.WhenTrue.Descendants()).Concat(this.WhenFalse.Descendants());
}
