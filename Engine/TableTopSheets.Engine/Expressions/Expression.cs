using System.Collections.Concurrent;

namespace TableTopSheets.Engine.Expressions;

/// <summary>
///   Parsed rule expression. Parsing is cached, so the same string is only parsed once
/// </summary>
public sealed class Expression
{
  #region Fields

  private static readonly ConcurrentDictionary<string, Expression> Cache = new();

  private readonly Node _root;

  #endregion

  #region Constructor

  private Expression(string source, Node root)
  {
    this.Source = source;
    this._root = root;
  }

  #endregion

  #region Properties

  public string Source { get; }

  /// <summary>
  ///   Functions called by the expression (excluding built in math helpers)
  /// </summary>
  public IEnumerable<(string Function, IReadOnlyList<string?> LiteralArguments)> Calls => this._root
    .Descendants()
    .OfType<CallNode>()
    .Select(call => (call.Function, (IReadOnlyList<string?>)call.Arguments
      .Select(argument => (argument as LiteralNode)?.Value.AsString())
      .ToList()));

  /// <summary>
  ///   Identifiers referenced by the expression
  /// </summary>
  public IEnumerable<string> Identifiers => this._root
    .Descendants()
    .OfType<IdentifierNode>()
    .Select(node => node.Name);

  #endregion

  #region Methods

  public static Expression Parse(string source) => Cache.GetOrAdd(source, key => new Expression(key, Parser.Parse(key)));

  public ExprValue Evaluate(IExpressionContext context)
  {
    try
    {
      return this._root.Evaluate(context);
    }
    catch (ExpressionException exception) when (!exception.Message.Contains(this.Source))
    {
      throw new ExpressionException($"{exception.Message} (in '{this.Source}')", exception);
    }
  }

  public double EvaluateNumber(IExpressionContext context) => this
    .Evaluate(context)
    .AsNumber();

  public bool EvaluateBoolean(IExpressionContext context) => this
    .Evaluate(context)
    .AsBoolean();

  public override string ToString() => this.Source;

  #endregion
}
