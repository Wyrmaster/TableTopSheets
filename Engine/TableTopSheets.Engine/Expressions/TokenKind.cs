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
