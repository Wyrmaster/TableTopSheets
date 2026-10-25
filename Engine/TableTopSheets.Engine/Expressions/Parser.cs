namespace TableTopSheets.Engine.Expressions;

/// <summary>
///   Precedence climbing parser for the rule expression language
/// </summary>
internal sealed class Parser
{
  #region Fields

  private static readonly Dictionary<string, int> Precedence = new()
  {
    ["||"] = 1,
    ["&&"] = 2,
    ["=="] = 3,
    ["!="] = 3,
    ["<"] = 4,
    ["<="] = 4,
    [">"] = 4,
    [">="] = 4,
    ["+"] = 5,
    ["-"] = 5,
    ["*"] = 6,
    ["/"] = 6,
    ["%"] = 6,
  };

  private readonly string _source;
  private readonly List<Token> _tokens;
  private int _position;

  #endregion

  #region Constructor

  private Parser(string source)
  {
    this._source = source;
    this._tokens = Lexer.Tokenize(source);
  }

  #endregion

  #region Methods

  public static Node Parse(string source)
  {
    Parser parser = new(source);
    Node node = parser.ParseConditional();
    parser.Expect(TokenKind.End);
    return node;
  }

  private Token Current => this._tokens[this._position];

  private Token Next() => this._tokens[this._position++];

  private Token Expect(TokenKind kind)
  {
    Token token = this.Next();
    if (token.Kind != kind)
    {
      throw new ExpressionException($"Expected {kind} but found '{token.Text}' at {token.Position} in '{this._source}'");
    }

    return token;
  }

  private Node ParseConditional()
  {
    Node condition = this.ParseBinary(1);
    if (this.Current.Kind != TokenKind.Question)
    {
      return condition;
    }

    this.Next();
    Node whenTrue = this.ParseConditional();
    this.Expect(TokenKind.Colon);
    Node whenFalse = this.ParseConditional();
    return new ConditionalNode(condition, whenTrue, whenFalse);
  }

  private Node ParseBinary(int minimumPrecedence)
  {
    Node left = this.ParseUnary();
    while (this.Current.Kind == TokenKind.Operator
           && Precedence.TryGetValue(this.Current.Text, out int precedence)
           && precedence >= minimumPrecedence)
    {
      string op = this.Next().Text;
      Node right = this.ParseBinary(precedence + 1);
      left = new BinaryNode(op, left, right);
    }

    return left;
  }

  private Node ParseUnary()
  {
    if (this.Current.Kind == TokenKind.Operator && this.Current.Text is "!" or "-")
    {
      string op = this.Next().Text;
      return new UnaryNode(op, this.ParseUnary());
    }

    return this.ParsePrimary();
  }

  private Node ParsePrimary()
  {
    Token token = this.Next();
    switch (token.Kind)
    {
      case TokenKind.Number:
        return new LiteralNode(ExprValue.Of(Lexer.ParseNumber(token.Text)));
      case TokenKind.Dice:
        return new LiteralNode(ExprValue.Parse(token.Text));
      case TokenKind.String:
        return new LiteralNode(ExprValue.Of(token.Text));
      case TokenKind.LeftParen:
        Node inner = this.ParseConditional();
        this.Expect(TokenKind.RightParen);
        return inner;
      case TokenKind.Identifier when this.Current.Kind == TokenKind.LeftParen:
        this.Next();
        List<Node> arguments = new();
        if (this.Current.Kind != TokenKind.RightParen)
        {
          do
          {
            arguments.Add(this.ParseConditional());
          } while (this.Current.Kind == TokenKind.Comma && this.Next().Kind == TokenKind.Comma);
        }

        this.Expect(TokenKind.RightParen);
        return new CallNode(token.Text, arguments);
      case TokenKind.Identifier:
        return new IdentifierNode(token.Text);
      default:
        throw new ExpressionException($"Unexpected '{token.Text}' at {token.Position} in '{this._source}'");
    }
  }

  #endregion
}
