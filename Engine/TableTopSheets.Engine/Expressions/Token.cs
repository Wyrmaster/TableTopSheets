using System.Globalization;

namespace TableTopSheets.Engine.Expressions;

internal readonly record struct Token(TokenKind Kind, string Text, int Position);
