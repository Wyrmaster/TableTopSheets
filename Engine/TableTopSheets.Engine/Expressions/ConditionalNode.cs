namespace TableTopSheets.Engine.Expressions;

internal sealed record ConditionalNode(Node Condition, Node WhenTrue, Node WhenFalse): Node
{
  public override ExprValue Evaluate(IExpressionContext context)
    => this.Condition
      .Evaluate(context)
      .AsBoolean() ? this.WhenTrue.Evaluate(context) : this.WhenFalse.Evaluate(context);

  public override IEnumerable<Node> Descendants()
    => new Node[] { this }
      .Concat(this.Condition.Descendants())
      .Concat(this.WhenTrue.Descendants())
      .Concat(this.WhenFalse.Descendants());
}
