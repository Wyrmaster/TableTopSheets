using PdfSharp.Drawing;
using TableTopSheets.Pdf;
using TableTopSheets.Pdf.Text;

namespace TableTopSheets.Engine.Tests;

public class TextFitTests
{
  #region Fields

  private static readonly XRect Box = new(0, 0, 150, 100);

  private const string Sentence = "Arcane Recovery. You can regain some of your magical energy by studying your spellbook. ";

  #endregion

  #region Constructor

  static TextFitTests()
  {
    // the document sets the global Roboto font resolver
    System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Document).TypeHandle);
  }

  #endregion

  #region Tests

  [Fact]
  public void ShortTextKeepsMaximumSize()
  {
    using XGraphics gfx = MeasureContext();

    TextFitResult result = TextFit.Fit(gfx, "Darkvision.", TextFit.Family, XFontStyleEx.Regular, [Box], TextFit.BodyMaxSize, TextFit.BodyMinSize);

    Assert.Equal(TextFit.BodyMaxSize, result.Font.Size);
    Assert.Null(result.Overflow);
    Assert.Equal(["Darkvision."], result.AllLines);
  }

  [Fact]
  public void LongTextShrinksButNotBelowMinimum()
  {
    using XGraphics gfx = MeasureContext();
    string text = string.Concat(Enumerable.Repeat(Sentence, 6));

    TextFitResult result = TextFit.Fit(gfx, text, TextFit.Family, XFontStyleEx.Regular, [Box], TextFit.BodyMaxSize, TextFit.BodyMinSize);

    Assert.Null(result.Overflow);
    Assert.True(result.Font.Size < TextFit.BodyMaxSize);
    Assert.True(result.Font.Size >= TextFit.BodyMinSize);
    Assert.True(result.AllLines.Count() * result.LineHeight <= Box.Height);
    Assert.All(result.AllLines, line => Assert.True(gfx.MeasureString(line, result.Font).Width <= Box.Width));
  }

  [Fact]
  public void VeryLongTextOverflowsWithoutLosingWords()
  {
    using XGraphics gfx = MeasureContext();
    string text = string.Join("\n\n", Enumerable.Range(1, 40).Select(index => $"Feature{index}. {Sentence}"));
    XRect[] columns = [Box, Box with { X = 160 }];

    TextFitResult result = TextFit.Fit(gfx, text, TextFit.Family, XFontStyleEx.Regular, columns, TextFit.BodyMaxSize, TextFit.BodyMinSize);

    Assert.NotNull(result.Overflow);
    Assert.Equal(TextFit.BodyMinSize, result.Font.Size);
    Assert.All(result.Lines, lines => Assert.NotEmpty(lines));
    string last = result.AllLines.Last();
    Assert.EndsWith(TextFit.Ellipsis, last);
    Assert.True(gfx.MeasureString(last, result.Font).Width <= Box.Width);

    string drawn = string.Join(" ", result.AllLines).Replace(TextFit.Ellipsis, string.Empty);
    Assert.Equal(Words(text), Words(drawn).Concat(Words(result.Overflow!)));
  }

  [Fact]
  public void SingleLineIsTruncatedWithEllipsis()
  {
    using XGraphics gfx = MeasureContext();
    const string text = "Versatile (1d10), Melee, Sap mastery, Finesse, Light, Thrown (20/60 feet)";

    (XFont font, string line, bool truncated) = TextFit.FitSingleLine(gfx, text, TextFit.Family, XFontStyleEx.Bold, TextFit.BodyMaxSize, TextFit.CellMinSize, 80);

    Assert.True(truncated);
    Assert.Equal(TextFit.CellMinSize, font.Size);
    Assert.EndsWith(TextFit.Ellipsis, line);
    Assert.StartsWith(line.TrimEnd('…').TrimEnd(), text);
    Assert.True(gfx.MeasureString(line, font).Width <= 80);
  }

  [Fact]
  public void SingleLineShrinksBeforeTruncating()
  {
    using XGraphics gfx = MeasureContext();
    const string text = "Wizard 3 / Fighter 2";
    double width = gfx.MeasureString(text, new XFont(TextFit.Family, 7)).Width;

    (XFont font, string line, bool truncated) = TextFit.FitSingleLine(gfx, text, TextFit.Family, XFontStyleEx.Regular, TextFit.BodyMaxSize, TextFit.CellMinSize, width);

    Assert.False(truncated);
    Assert.Equal(text, line);
    Assert.True(font.Size <= 7 && font.Size > TextFit.CellMinSize);
  }

  #endregion

  #region Helpers

  private static XGraphics MeasureContext() => XGraphics.CreateMeasureContext(new XSize(595, 842), XGraphicsUnit.Point, XPageDirection.Downwards);

  private static string[] Words(string text) => text.Split([' ', '\n'], StringSplitOptions.RemoveEmptyEntries);

  #endregion
}
