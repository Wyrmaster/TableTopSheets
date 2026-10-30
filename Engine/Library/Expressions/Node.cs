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
