using PdfSharp.Drawing;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Sheet;
using TableTopSheets.Pdf;
using TableTopSheets.Pdf.Text;

namespace TableTopSheets.Engine.Tests;

public class RichTextLayoutTests
{
  #region Fields

  private const double Width = 200;

  private const double Size = 7;

  private const string Sentence = "You can regain some of your magical energy by studying your spellbook. ";

  #endregion

  #region Constructor

  static RichTextLayoutTests()
  {
    // the document sets the global Roboto font resolver
    System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Document).TypeHandle);
  }

  #endregion

  #region Tests

  [Fact]
  public void SpansKeepTheirFontsOnOneLine()
  {
    using XGraphics gfx = MeasureContext();
    TextBlock paragraph = Paragraph(new TextSpan("Breath.", true, true), new TextSpan(" You exhale "), new TextSpan("fire", Italic: true),
      new TextSpan("."));

    RichTextLayout layout = RichTextLayout.Create(gfx, [paragraph], Width, Size);

    RichTextLine line = Assert.Single(layout.Lines);
    Assert.Equal("Breath. You exhale fire.", line.Text);
    Assert.Equal(["Breath.", " You exhale", " fire", "."], line.Runs.Select(run => run.Text));
    Assert.True(line.Runs[0].Font is { Bold: true, Italic: true });
    Assert.True(line.Runs[1].Font is { Bold: false, Italic: false });
    Assert.True(line.Runs[2].Font is { Bold: false, Italic: true });
    for (int index = 1; index < line.Runs.Count; index++)
    {
      // each run starts where the previous one ends, measured with its own font
      RichTextRun previous = line.Runs[index - 1];
      Assert.Equal(previous.X + previous.Width, line.Runs[index].X, 3);
      Assert.Equal(gfx.MeasureString(line.Runs[index].Text, line.Runs[index].Font).Width, line.Runs[index].Width, 3);
    }

    Assert.All(line.Runs, run => Assert.Equal(line.Runs[0].Baseline, run.Baseline));
  }

  [Fact]
  public void LongParagraphWrapsInsideTheWidthWithoutLosingWords()
  {
    using XGraphics gfx = MeasureContext();
    string text = string.Concat(Enumerable.Repeat(Sentence, 8));

    RichTextLayout layout = RichTextLayout.Create(gfx, [Paragraph(new TextSpan(text))], Width, Size);

    Assert.True(layout.Lines.Count > 5);
    Assert.All(layout.Lines, line => Assert.True(line.Runs[^1].X + line.Runs[^1].Width <= Width + 0.001));
    Assert.Equal(CardSamples.Split(text), layout.Lines.SelectMany(line => CardSamples.Split(line.Text)));
    double height = layout.Lines.Sum(line => line.Height);
    Assert.Equal(height, layout.Height, 3);
  }

  [Fact]
  public void LineBreakInsideASpanStartsANewLine()
  {
    using XGraphics gfx = MeasureContext();

    RichTextLayout layout = RichTextLayout.Create(gfx, [Paragraph(new TextSpan("first\nsecond"))], Width, Size);

    Assert.Equal(["first", "second"], layout.Lines.Select(line => line.Text));
  }

  [Fact]
  public void ListEntriesGetBulletsAndAnIndent()
  {
    using XGraphics gfx = MeasureContext();
    TextBlock list = new(TextBlockKindEnum.List)
    {
      Items =
      [
        [new TextSpan("Short entry.")],
        [new TextSpan("Long entry. ", Bold: true), new TextSpan(string.Concat(Enumerable.Repeat(Sentence, 2)))],
      ],
    };

    RichTextLayout layout = RichTextLayout.Create(gfx, [Paragraph(new TextSpan("Intro.")), list], Width, Size);

    List<RichTextLine> entries = layout.Lines
      .Where(line => line.Block == 1)
      .ToList();
    Assert.True(entries.Count >= 3);
    Assert.Equal(2, entries.Count(line => line.Bullets.Count == 1));
    Assert.Single(entries[0].Bullets);
    Assert.Single(entries[1].Bullets);
    Assert.Empty(entries[2].Bullets);
    double indent = entries[0].Runs[0].X;
    Assert.True(indent > entries[0].Bullets[0].Right);
    Assert.All(entries, line => Assert.Equal(indent, line.Runs[0].X, 3));
    Assert.Equal(0, layout.Lines[0].Runs[0].X);
    Assert.True(entries[0].SpaceBefore > entries[1].SpaceBefore);
  }

  [Fact]
  public void TableHasAHeaderGridAndWrappedCells()
  {
    using XGraphics gfx = MeasureContext();
    TextBlock table = Table(1,
      ["d6", "Effect", "Notes"],
      ["1", "Fire", string.Concat(Enumerable.Repeat(Sentence, 2))],
      ["2", "Cold", "None"]);

    RichTextLayout layout = RichTextLayout.Create(gfx, [table], Width, Size);

    Assert.Equal(3, layout.Lines.Count);
    RichTextLine header = layout.Lines[0];
    Assert.True(header.KeepWithNext);
    Assert.NotEmpty(header.Shades);
    Assert.All(header.Runs, run => Assert.True(run.Font.Bold));
    Assert.All(layout.Lines.Skip(1), row => Assert.Same(header, Assert.Single(row.Header)));
    Assert.All(layout.Lines.Skip(1), row => Assert.Empty(row.Shades));

    // the long cell wraps, so its row is taller than the others
    Assert.True(layout.Lines[1].Height > layout.Lines[2].Height * 2);

    // four vertical lines (border and between three columns) plus top and bottom, inside the width
    RichTextLine row = layout.Lines[2];
    double[] verticals = row.Rules
      .Where(rule => rule.X1 == rule.X2)
      .Select(rule => rule.X1)
      .ToArray();
    Assert.Equal(4, verticals.Length);
    Assert.Equal(Width, verticals.Max(), 3);
    Assert.All(row.Rules, rule => Assert.True(rule.X2 <= Width + 0.001));

    // the short columns stay narrow, the long one takes the rest; every run stays in its column
    double[] columns = verticals
      .Order()
      .ToArray();
    Assert.True(columns[3] - columns[2] > (columns[1] - columns[0]) * 3);
    RichTextLine wrapped = layout.Lines[1];
    Assert.All(wrapped.Runs.Where(run => run.X > columns[2]), run => Assert.True(run.X + run.Width <= columns[3] + 0.001));
    Assert.Equal(CardSamples.Split(string.Concat(Enumerable.Repeat(Sentence, 2))), wrapped.Runs
      .Where(run => run.X > columns[2])
      .SelectMany(run => CardSamples.Split(run.Text)));
  }

  [Fact]
  public void FitSplitsAtRowsAndRepeatsTheTableHeader()
  {
    using XGraphics gfx = MeasureContext();
    string[][] rows = Enumerable
      .Range(1, 30)
      .Select(index => new[] { $"{index}", $"Card{index}" })
      .Prepend(["Roll", "Card"])
      .ToArray();
    RichTextLayout layout = RichTextLayout.Create(gfx, [Paragraph(new TextSpan(Sentence)), Table(1, rows)], Width, Size);

    List<RichTextSlice> slices = [];
    int start = 0;
    while (start < layout.Lines.Count)
    {
      RichTextSlice slice = layout.Fit(start, 80);
      slices.Add(slice);
      start = slice.End;
    }

    Assert.True(slices.Count >= 3);
    Assert.All(slices, slice => Assert.True(slice.Height <= 80 + 0.001));
    Assert.All(slices.Skip(1), slice => Assert.True(slice.Lines[0].Repeated));
    Assert.All(slices.Skip(1), slice => Assert.Equal("Roll Card", slice.Lines[0].Line.Text));
    Assert.All(slices, slice => Assert.Equal(0, slice.Lines[0].Y));

    List<string> drawn = slices
      .SelectMany(slice => slice.Lines)
      .Where(line => !line.Repeated)
      .SelectMany(line => CardSamples.Split(line.Line.Text))
      .ToList();
    List<string> expected = CardSamples
      .Split(Sentence)
      .Concat(rows.SelectMany(row => row))
      .ToList();
    Assert.Equal(expected, drawn);
  }

  [Fact]
  public void HeadingIsNotLeftAtTheBottomOfAColumn()
  {
    using XGraphics gfx = MeasureContext();
    TextBlock heading = new(TextBlockKindEnum.Heading) { Spans = [new TextSpan("Heading")] };
    RichTextLayout layout = RichTextLayout.Create(gfx, [Paragraph(new TextSpan("One.")), heading, Paragraph(new TextSpan("Two."))], Width, Size);
    double withHeading = layout.Lines[0].Height + layout.Lines[1].SpaceBefore + layout.Lines[1].Height;

    RichTextSlice slice = layout.Fit(0, withHeading + 0.5);

    Assert.Equal(1, slice.Count);
    Assert.True(layout.Lines[1].Runs[0].Font.Bold);
    Assert.True(layout.Lines[1].Runs[0].Font.Size > Size);
  }

  [Fact]
  public void FitAlwaysTakesOneLine()
  {
    using XGraphics gfx = MeasureContext();
    RichTextLayout layout = RichTextLayout.Create(gfx, [Paragraph(new TextSpan(Sentence))], Width, Size);

    RichTextSlice slice = layout.Fit(0, 1);

    Assert.Equal(1, slice.Count);
  }

  #endregion

  #region Helpers

  private static XGraphics MeasureContext() => XGraphics.CreateMeasureContext(new XSize(595, 842), XGraphicsUnit.Point, XPageDirection.Downwards);

  private static TextBlock Paragraph(params TextSpan[] spans) => new(TextBlockKindEnum.Paragraph) { Spans = spans };

  private static TextBlock Table(int headerRows, params string[][] rows) => new(TextBlockKindEnum.Table)
  {
    HeaderRows = headerRows,
    Rows = rows
      .Select(row => (IReadOnlyList<IReadOnlyList<TextSpan>>)row
        .Select(cell => (IReadOnlyList<TextSpan>)[new TextSpan(cell)])
        .ToList())
      .ToList(),
  };

  #endregion
}
