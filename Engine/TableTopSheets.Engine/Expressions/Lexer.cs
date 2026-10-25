using System.Globalization;

namespace TableTopSheets.Engine.Expressions;

internal enum TokenKind
{
  Number,
  Dice,
  String,
  Identifier,
  Operator,
  LeftParen,
  RightParen,
  Comma,
  Question,
  Colon,
  End,
}

internal readonly record struct Token(TokenKind Kind, string Text, int Position);

/// <summary>
///   Splits an expression into tokens
/// </summary>
internal static class Lexer
{
  #region Fields

  private static readonly string[] Operators = ["&&", "||", "==", "!=", "<=", ">=", "<", ">", "+", "-", "*", "/", "%", "!"];

  #endregion

  #region Methods

  public static List<Token> Tokenize(string source)
  {
    List<Token> tokens = new();
    int position = 0;

    while (position < source.Length)
    {
      char current = source[position];
      if (char.IsWhiteSpace(current))
      {
        position++;
        continue;
      }

      int start = position;
      if (char.IsDigit(current))
      {
        while (position < source.Length && (char.IsDigit(source[position]) || source[position] == '.'))
        {
          position++;
        }

        // dice literal such as 2d6
        if (position + 1 < source.Length && source[position] == 'd' && char.IsDigit(source[position + 1]))
        {
          position++;
          while (position < source.Length && char.IsDigit(source[position]))
          {
            position++;
          }

          tokens.Add(new(TokenKind.Dice, source[start..position], start));
          continue;
        }

        tokens.Add(new(TokenKind.Number, source[start..position], start));
        continue;
      }

      if (current is '\'' or '"')
      {
        position++;
        while (position < source.Length && source[position] != current)
        {
          position++;
        }

        if (position >= source.Length)
        {
          throw new ExpressionException($"Unterminated string at {start} in '{source}'");
        }

        tokens.Add(new(TokenKind.String, source[(start + 1)..position], start));
        position++;
        continue;
      }

      if (char.IsLetter(current) || current is '_' or '$')
      {
        while (position < source.Length && (char.IsLetterOrDigit(source[position]) || source[position] is '_' or '.' or '$'))
        {
          position++;
        }

        tokens.Add(new(TokenKind.Identifier, source[start..position], start));
        continue;
      }

      switch (current)
      {
        case '(':
          tokens.Add(new(TokenKind.LeftParen, "(", start));
          position++;
          continue;
        case ')':
          tokens.Add(new(TokenKind.RightParen, ")", start));
          position++;
          continue;
        case ',':
          tokens.Add(new(TokenKind.Comma, ",", start));
          position++;
          continue;
        case '?':
          tokens.Add(new(TokenKind.Question, "?", start));
          position++;
          continue;
        case ':':
          tokens.Add(new(TokenKind.Colon, ":", start));
          position++;
          continue;
      }

      string? op = Operators.FirstOrDefault(entry => string.CompareOrdinal(source, position, entry, 0, entry.Length) == 0);
      if (op == null)
      {
        throw new ExpressionException($"Unexpected character '{current}' at {position} in '{source}'");
      }

      tokens.Add(new(TokenKind.Operator, op, start));
      position += op.Length;
    }

    tokens.Add(new(TokenKind.End, string.Empty, source.Length));
    return tokens;
  }

  public static double ParseNumber(string text) => double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);

  #endregion
}
