using TableTopSheets.Engine.Expressions;

namespace TableTopSheets.Engine.Tests;

public class ExpressionTests
{
  private sealed class Context: IExpressionContext
  {
    public ExprValue Resolve(string identifier) => identifier switch
    {
      "level" => ExprValue.Of(5),
      "option.key" => ExprValue.Of("wis"),
      _ => throw new ExpressionException($"Unknown identifier '{identifier}'"),
    };

    public ExprValue Call(string function, IReadOnlyList<ExprValue> arguments) => function switch
    {
      "mod" => ExprValue.Of(arguments[0].AsString() == "dex" ? 3 : 1),
      _ => throw new ExpressionException($"Unknown function '{function}'"),
    };
  }

  [Theory]
  [InlineData("1 + 2 * 3", 7)]
  [InlineData("(1 + 2) * 3", 9)]
  [InlineData("10 + mod('dex') + mod('wis')", 14)]
  [InlineData("floor(level / 2)", 2)]
  [InlineData("ceil(level / 2)", 3)]
  [InlineData("max(1, mod('cha'), -4)", 1)]
  [InlineData("level >= 5 ? 2 : 1", 2)]
  [InlineData("-level + 1", -4)]
  [InlineData("7 % 4", 3)]
  public void EvaluatesNumbers(string source, double expected)
    => Assert.Equal(expected, Expression
      .Parse(source)
      .EvaluateNumber(new Context()));

  [Theory]
  [InlineData("level >= 5 && !(level > 6)", true)]
  [InlineData("level < 5 || false", false)]
  [InlineData("oneOf(option.key, 'int', 'wis', 'cha')", true)]
  [InlineData("option.key == 'str'", false)]
  [InlineData("option.key != \"str\"", true)]
  public void EvaluatesBooleans(string source, bool expected)
    => Assert.Equal(expected, Expression
      .Parse(source)
      .EvaluateBoolean(new Context()));

  [Fact]
  public void DiceLiteralsKeepCountAndSides()
  {
    ExprValue value = Expression
      .Parse("2d6")
      .Evaluate(new Context());

    Assert.Equal(ExprValueKind.Dice, value.Kind);
    Assert.Equal(2, value.DiceCount);
    Assert.Equal(6, value.DiceSides);
    Assert.Equal(6, value.AsNumber());
  }

  [Theory]
  [InlineData("1 +")]
  [InlineData("mod('dex'")]
  [InlineData("'open")]
  [InlineData("1 # 2")]
  public void RejectsInvalidSyntax(string source) => Assert.Throws<ExpressionException>(() => Expression.Parse(source));

  [Fact]
  public void UnknownIdentifierFails()
    => Assert.Throws<ExpressionException>(() => Expression
      .Parse("unknown + 1")
      .Evaluate(new Context()));
}
