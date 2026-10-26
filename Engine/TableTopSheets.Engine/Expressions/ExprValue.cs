using System.Globalization;

namespace TableTopSheets.Engine.Expressions;

/// <summary>
///   Result of an expression: a number, boolean, string or dice (<c>2d6</c>)
/// </summary>
public readonly record struct ExprValue
{
  #region Properties

  public ExprValueKind Kind { get; private init; }

  public double Number { get; private init; }

  public string? Text { get; private init; }

  /// <summary>
  ///   Number of dice for <see cref="ExprValueKind.Dice"/>
  /// </summary>
  public int DiceCount { get; private init; }

  /// <summary>
  ///   Sides of the dice for <see cref="ExprValueKind.Dice"/>
  /// </summary>
  public int DiceSides { get; private init; }

  #endregion

  #region Factory Methods

  public static ExprValue Of(double number) => new() { Kind = ExprValueKind.Number, Number = number };

  public static ExprValue Of(bool value) => new() { Kind = ExprValueKind.Boolean, Number = value ? 1 : 0 };

  public static ExprValue Of(string text) => new() { Kind = ExprValueKind.String, Text = text };

  public static ExprValue OfDice(int count, int sides) => new() { Kind = ExprValueKind.Dice, DiceCount = count, DiceSides = sides };

  public static readonly ExprValue Zero = Of(0);

  public static readonly ExprValue True = Of(true);

  public static readonly ExprValue False = Of(false);

  /// <summary>
  ///   Parses a literal stored in data (<c>3</c>, <c>1d6</c>, <c>-1</c>, otherwise a string)
  /// </summary>
  public static ExprValue Parse(string literal)
  {
    literal = literal.Trim();
    if (double.TryParse(literal, NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
    {
      return Of(number);
    }

    int index = literal.IndexOf('d');
    if (index >= 0
        && int.TryParse(index == 0 ? "1" : literal[..index], out int count)
        && int.TryParse(literal[(index + 1)..], out int sides))
    {
      return OfDice(count, sides);
    }

    return Of(literal);
  }

  #endregion

  #region Conversion

  /// <summary>
  ///   Numeric representation; dice convert to their number of sides so a die size can be stored in a numeric stat
  /// </summary>
  public double AsNumber() => this.Kind switch
  {
    ExprValueKind.Number or ExprValueKind.Boolean => this.Number,
    ExprValueKind.Dice => this.DiceSides,
    _ => throw new ExpressionException($"'{this.Text}' is not a number"),
  };

  public bool AsBoolean() => this.Kind switch
  {
    ExprValueKind.String => !string.IsNullOrEmpty(this.Text),
    ExprValueKind.Dice => this.DiceCount > 0,
    _ => this.Number != 0,
  };

  public string AsString() => this.Kind switch
  {
    ExprValueKind.String => this.Text!,
    ExprValueKind.Boolean => this.Number != 0 ? "true" : "false",
    ExprValueKind.Dice => $"{this.DiceCount}d{this.DiceSides}",
    _ => this.Number.ToString(CultureInfo.InvariantCulture),
  };

  public override string ToString() => this.AsString();

  #endregion
}
