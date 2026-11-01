using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using TableTopSheets.Common.Card;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Sheet;
using TableTopSheets.Pdf;
using TableTopSheets.Pdf.Card;
using TableTopSheets.Pdf.Enum;
using TableTopSheets.Pdf.Text;

namespace TableTopSheets.Engine.Tests;

public class CardLayoutTests
{
  #region Fields

  private const string Sentence = "The flames spread around corners and ignite flammable objects in the area that aren't being worn or carried. ";

  private const double Tolerance = 0.001;

  #endregion

  #region Constructor

  static CardLayoutTests()
  {
    // the document sets the global Roboto font resolver
    System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Document).TypeHandle);
  }

  #endregion

  #region Tests

  [Fact]
  public void ShortTextGetsASmallCard()
  {
    using XGraphics gfx = MeasureContext();
    CardData card = CardSamples.Spell("fireball", TestImages.Png(64, 64));

    CardPart part = Assert.Single(CardLayout.Plan(gfx, card));

    Assert.Equal(CardSizeEnum.Small, part.Format.Size);
    Assert.InRange(part.Geometry.BodySize, CardLayout.MinBodySize, CardFormat.Small.MaxBodySize);
    Assert.NotNull(part.Image);
    Assert.NotNull(part.Geometry.ImageRect);
    Assert.Equal(string.Empty, part.PartLabel);
    Assert.Single(part.Geometry.Columns);
    AssertInside(part);
  }

  [Fact]
  public void ShortTextGrowsToFillTheCard()
  {
    using XGraphics gfx = MeasureContext();
    CardData card = Card(Words(20));

    CardPart part = Assert.Single(CardLayout.Plan(gfx, card));

    Assert.Equal(CardSizeEnum.Small, part.Format.Size);
    Assert.True(part.Geometry.BodySize > 7.5, $"body {part.Geometry.BodySize}");
    Assert.InRange(part.Geometry.BodySize, CardLayout.MinBodySize, CardFormat.Small.MaxBodySize);
    AssertInside(part);
  }

  [Fact]
  public void GrownBodyIsTheLargestThatFits()
  {
    using XGraphics gfx = MeasureContext();
    CardData card = Card(Words(150));

    CardPart part = Assert.Single(CardLayout.Plan(gfx, card));

    // one step more doesn't fit the same card any more, unless the body is at its largest
    double larger = part.Geometry.BodySize + CardLayout.Step;
    if (larger <= part.Format.MaxBodySize)
    {
      CardGeometry geometry = CardGeometry.Create(gfx, card, part.Format, larger, false, false);
      RichTextLayout layout = RichTextLayout.Create(gfx, card.Description, geometry.ColumnWidth, larger);
      IReadOnlyList<RichTextSlice> columns = layout.Flow(0, geometry.Columns
        .Select(column => column.Height)
        .ToList());
      Assert.True(columns[^1].End < layout.Lines.Count);
    }
  }

  [Fact]
  public void MediumAndLargeAreLandscapeWithColumns()
  {
    Assert.False(CardFormat.Small.Landscape);
    Assert.Equal(1, CardFormat.Small.TextColumns);
    Assert.True(CardFormat.Medium.Landscape);
    Assert.Equal(2, CardFormat.Medium.TextColumns);
    Assert.True(CardFormat.Large.Landscape);
    Assert.Equal(3, CardFormat.Large.TextColumns);
    Assert.Equal(XUnit.FromMillimeter(138).Point, CardFormat.Medium.Width, 3);
    Assert.Equal(XUnit.FromMillimeter(98).Point, CardFormat.Medium.Height, 3);
    Assert.Equal(XUnit.FromMillimeter(200).Point, CardFormat.Large.Width, 3);
    Assert.Equal(XUnit.FromMillimeter(140).Point, CardFormat.Large.Height, 3);

    using XGraphics gfx = MeasureContext();
    CardData card = CardSamples.Item("bag-of-holding", TestImages.Png(64, 64, 4));
    foreach (CardFormat format in new[] { CardFormat.Medium, CardFormat.Large })
    {
      CardGeometry geometry = CardGeometry.Create(gfx, card, format, 8, true, false);

      Assert.Equal(format.TextColumns, geometry.Columns.Count);
      Assert.All(geometry.Columns, column => Assert.Equal(geometry.ColumnWidth, column.Width, 3));

      // the title spans the card, the picture and the table start the first column, the others start at the top
      Assert.True(geometry.TitleRect.Width > 2 * geometry.ColumnWidth);
      XRect image = Assert.NotNull(geometry.ImageRect);
      Assert.Equal(geometry.Columns[0].X, image.X, 3);
      Assert.Equal(geometry.ColumnWidth, image.Width, 3);
      RichTextSlice? facts = geometry.Facts;
      Assert.NotNull(facts);
      Assert.True(geometry.FactsOrigin.Y >= image.Bottom);
      Assert.True(geometry.FactsOrigin.Y + facts.Height <= geometry.Columns[0].Top);
      Assert.All(geometry.Columns.Skip(1), column => Assert.Equal(image.Top, column.Top, 3));
      for (int index = 1; index < geometry.Columns.Count; index++)
      {
        Assert.True(geometry.Columns[index].Left > geometry.Columns[index - 1].Right);
      }

      Assert.True(geometry.Columns[^1].Right <= geometry.Frame.Right);
    }
  }

  [Fact]
  public void LongTextFlowsThroughTheColumnsOfALandscapeCard()
  {
    using XGraphics gfx = MeasureContext();
    CardData card = Card(Words(300));

    CardPart part = Assert.Single(CardLayout.Plan(gfx, card));

    Assert.NotEqual(CardSizeEnum.Small, part.Format.Size);
    Assert.Equal(part.Format.TextColumns, part.Columns.Count);
    Assert.All(part.Columns, column => Assert.NotEmpty(column.Lines));
    for (int index = 1; index < part.Columns.Count; index++)
    {
      Assert.Equal(part.Columns[index - 1].End, part.Columns[index].Start);
    }

    AssertInside(part);
    AssertEveryWordOnce(card, [part]);
  }

  [Fact]
  public void DeckOfManyThingsTakesAtMostTwoLargeCards()
  {
    using XGraphics gfx = MeasureContext();
    CardData card = CardSamples.Item("deck-of-many-things", TestImages.Jpeg());
    TextBlock table = Assert.Single(card.Description, block => block.Kind == TextBlockKindEnum.Table);
    Assert.Equal(23, table.Rows.Count);
    Assert.InRange(CardSamples.Words(card.Description).Count, 1200, 1500);

    IReadOnlyList<CardPart> parts = CardLayout.Plan(gfx, card);

    Assert.InRange(parts.Count, 1, 2);
    Assert.All(parts, part => Assert.Equal(CardSizeEnum.Large, part.Format.Size));
    Assert.Equal(Enumerable.Range(1, parts.Count), parts.Select(part => part.Part));
    Assert.All(parts, part => Assert.Equal(parts.Count, part.Parts));
    Assert.NotNull(parts[0].Geometry.ImageRect);
    if (parts.Count > 1)
    {
      Assert.Equal("continued 2/2", parts[1].PartLabel);
      Assert.Null(parts[1].Geometry.ImageRect);
      Assert.Null(parts[1].Geometry.Facts);
    }

    Assert.All(parts, AssertInside);
    AssertEveryWordOnce(card, parts);
  }

  [Fact]
  public void TableRowsStayWholeAndRepeatTheHeaderInTheNextColumn()
  {
    using XGraphics gfx = MeasureContext();
    CardData card = CardSamples.Item("deck-of-many-things", TestImages.Jpeg());

    List<RichTextSlice> columns = CardLayout
      .Plan(gfx, card)
      .SelectMany(part => part.Columns)
      .ToList();

    // a column that continues the table starts with its header again
    RichTextSlice? continued = columns.FirstOrDefault(column => column.Lines.Count > 0 && column.Lines[0].Repeated);
    if (continued != null)
    {
      Assert.True(continued.Lines[0].Line.KeepWithNext);
      Assert.Contains(continued.Lines, line => !line.Repeated);
    }

    // every layout line is in exactly one column
    List<int> starts = columns
      .Where(column => column.Count > 0)
      .Select(column => column.Start)
      .ToList();
    Assert.Equal(starts.Order(), starts);
  }

  [Fact]
  public void EveryWordIsDrawnExactlyOnce()
  {
    using XGraphics gfx = MeasureContext();
    CardData[] cards = [.. Samples(), Card(Words(20)), Card(Words(300))];

    foreach (CardData card in cards)
    {
      IReadOnlyList<CardPart> parts = CardLayout.Plan(gfx, card);
      AssertEveryWordOnce(card, parts);
      Assert.All(parts, AssertInside);
    }
  }

  [Fact]
  public void WithoutImageTheTableSpansTheRow()
  {
    using XGraphics gfx = MeasureContext();
    CardData card = CardSamples.Spell("fireball");

    CardPart part = Assert.Single(CardLayout.Plan(gfx, card));

    Assert.Null(part.Image);
    Assert.Null(part.Geometry.ImageRect);
    Assert.Equal(part.Geometry.Columns[0].X, part.Geometry.FactsOrigin.X, 3);
  }

  [Fact]
  public void UnreadableImageIsLeftOut()
  {
    using XGraphics gfx = MeasureContext();
    CardData card = CardSamples.Spell("fireball", [1, 2, 3, 4]);

    CardPart part = Assert.Single(CardLayout.Plan(gfx, card));

    Assert.Null(part.Image);
    Assert.Null(part.Geometry.ImageRect);
  }

  [Fact]
  public void NineSmallCardsMakeOnePage()
  {
    IReadOnlyList<IReadOnlyList<CardPlacement>> pages = CardPacker.Pack(Parts((CardFormat.Small, 9)));

    IReadOnlyList<CardPlacement> page = Assert.Single(pages);
    Assert.Equal(9, page.Count);
    Assert.All(page, placement => Assert.False(placement.Rotated));
    AssertPacked(pages);
  }

  [Fact]
  public void FourMediumCardsMakeOnePageTurned()
  {
    IReadOnlyList<IReadOnlyList<CardPlacement>> pages = CardPacker.Pack(Parts((CardFormat.Medium, 4)));

    IReadOnlyList<CardPlacement> page = Assert.Single(pages);
    Assert.All(page, placement => Assert.True(placement.Rotated));
    AssertPacked(pages);
  }

  [Fact]
  public void TwoLargeCardsMakeOnePage()
  {
    IReadOnlyList<IReadOnlyList<CardPlacement>> pages = CardPacker.Pack(Parts((CardFormat.Large, 2)));

    Assert.Equal(2, Assert.Single(pages).Count);
    AssertPacked(pages);
  }

  [Fact]
  public void OneLargeAndThreeSmallCardsShareAPage()
  {
    IReadOnlyList<IReadOnlyList<CardPlacement>> pages = CardPacker.Pack(Parts((CardFormat.Small, 3), (CardFormat.Large, 1)));

    Assert.Equal(4, Assert.Single(pages).Count);
    AssertPacked(pages);
  }

  [Fact]
  public void OneLargeAndTwoMediumCardsShareAPage()
  {
    IReadOnlyList<IReadOnlyList<CardPlacement>> pages = CardPacker.Pack(Parts((CardFormat.Medium, 2), (CardFormat.Large, 1)));

    Assert.Equal(3, Assert.Single(pages).Count);
    AssertPacked(pages);
  }

  [Fact]
  public void TwoLargeAndThreeSmallCardsNeedTwoPages()
  {
    // two large cards take 283 of the 287 mm, the small cards start the next page
    IReadOnlyList<IReadOnlyList<CardPlacement>> pages = CardPacker.Pack(Parts((CardFormat.Small, 3), (CardFormat.Large, 2)));

    Assert.Equal(2, pages.Count);
    AssertPacked(pages);
  }

  [Fact]
  public void LargestCardsGoFirstAndANewPageOnlyWhenNothingFits()
  {
    IReadOnlyList<IReadOnlyList<CardPlacement>> pages = CardPacker.Pack(Parts((CardFormat.Small, 10), (CardFormat.Large, 3), (CardFormat.Medium, 3)));

    Assert.Equal(CardSizeEnum.Large, pages[0][0].Part.Format.Size);
    AssertPacked(pages);

    // no card of a later page would have fitted an earlier one: the last page holds what's left over
    Assert.True(pages.Count <= 4, $"{pages.Count} pages");
  }

  [Fact]
  public void SampleSetSharesPages()
  {
    using XGraphics gfx = MeasureContext();
    IReadOnlyList<CardPart> parts = CardLayout.Plan(gfx, Samples());

    IReadOnlyList<IReadOnlyList<CardPlacement>> pages = CardPacker.Pack(parts);

    Assert.Equal(parts.Count, pages.Sum(page => page.Count));
    AssertPacked(pages);
    Assert.True(pages.Count < PagesApart(parts), $"{pages.Count} pages, {PagesApart(parts)} with the sizes apart");
    Assert.Contains(pages, page => page
      .Select(placement => placement.Part.Format)
      .Distinct()
      .Count() > 1);
  }

  [Fact]
  public void DocumentPacksTheCardsOntoA4Pages()
  {
    CardData[] cards = Samples(artwork: true);
    using XGraphics gfx = MeasureContext();
    IReadOnlyList<CardPart> parts = CardLayout.Plan(gfx, cards);
    int expectedPages = CardPacker
      .Pack(parts)
      .Count;

    using Stream stream = new CardDocument(cards).GenerateStream();
    using PdfDocument pdf = PdfReader.Open(stream, PdfDocumentOpenMode.Import);

    Assert.Equal(expectedPages, pdf.PageCount);
    Assert.All(pdf.Pages.Cast<PdfPage>(), page => Assert.True(page.Width.Point < page.Height.Point));

    // a sample to look at
    string path = Path.Combine(AppContext.BaseDirectory, "cards-sample.pdf");
    new CardDocument(cards).GenerateFile(path);
    Assert.True(new FileInfo(path).Length > 1000);
  }

  [Fact]
  public void EmptyDocumentStillHasAPage()
  {
    using Stream stream = new CardDocument([]).GenerateStream();
    using PdfDocument pdf = PdfReader.Open(stream, PdfDocumentOpenMode.Import);

    Assert.Equal(1, pdf.PageCount);
  }

  #endregion

  #region Helpers

  private static XGraphics MeasureContext() => XGraphics.CreateMeasureContext(new XSize(842, 842), XGraphicsUnit.Point, XPageDirection.Downwards);

  /// <summary>
  ///   The selection of the card plan: spells and items of every size, one long enough to need two large cards; with
  ///   <paramref name="artwork"/> the pictures of the seed data where there are some
  /// </summary>
  private static CardData[] Samples(bool artwork = false)
  {
    byte[] Picture(string folder, string key, byte[] fallback) => artwork ? CardSamples.Artwork(folder, key, fallback) : fallback;

    return
    [
      CardSamples.Spell("fireball", Picture("spells", "fireball", TestImages.Png(96, 96, 1))),
      CardSamples.Spell("wish", Picture("spells", "wish", TestImages.Png(120, 80, 2))),
      CardSamples.Spell("mage-hand", Picture("spells", "mage-hand", TestImages.Jpeg())),
      CardSamples.Spell("counterspell", Picture("spells", "counterspell", TestImages.Png(64, 64, 6))),
      CardSamples.Item("plus1-weapon", Picture("magic-items", "plus1-longsword", TestImages.Png(80, 120, 3)), "+1 Longsword",
        new CardFact("Damage", "1d8 Slashing (Versatile 1d10)"),
        new CardFact("Properties", "Versatile"),
        new CardFact("Mastery", "Sap")),
      CardSamples.Item("bag-of-holding", Picture("magic-items", "bag-of-holding", TestImages.Png(64, 64, 4))),
      CardSamples.Item("deck-of-many-things", Picture("magic-items", "deck-of-many-things", TestImages.Png(96, 96, 5))),
      CardSamples.Item("robe-of-the-archmagi"),
      CardSamples.Item("plus1-armor", Picture("magic-items", "plus1-chain-mail", TestImages.Png(64, 64, 7)), "+1 Chain Mail",
        new CardFact("Armor Class", "16 + 1"),
        new CardFact("Strength", "13"),
        new CardFact("Stealth", "Disadvantage")),
      CardSamples.Item("cloak-of-protection", Picture("magic-items", "cloak-of-protection", TestImages.Png(64, 64, 10))),
      CardSamples.Item("potion-of-healing", Picture("magic-items", "potion-of-healing", TestImages.Png(64, 64, 8))),
      CardSamples.Item("sun-blade", Picture("magic-items", "sun-blade", TestImages.Png(64, 64, 9))),
    ];
  }

  private static CardData Card(string text) => new()
  {
    Kind = CardKindEnum.Item,
    Name = "Test Item",
    Subtitle = "Wondrous Item, Rare",
    Facts = [new("Type", "Wondrous Item"), new("Rarity", "Rare")],
    Description = text
      .Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
      .Select(paragraph => new TextBlock(TextBlockKindEnum.Paragraph) { Spans = [new TextSpan(paragraph)] })
      .ToList(),
    Footer = "Item · Rare",
  };

  /// <summary>
  ///   About <paramref name="count"/> words in paragraphs of about 60
  /// </summary>
  private static string Words(int count)
  {
    string[] words = CardSamples
      .Split(string.Concat(Enumerable.Repeat(Sentence, count / 15 + 1)))
      .Take(count)
      .ToArray();
    return string.Join("\n\n", words
      .Chunk(60)
      .Select(chunk => string.Join(" ", chunk)));
  }

  /// <summary>
  ///   Empty cards of the given formats, for packing
  /// </summary>
  private static List<CardPart> Parts(params (CardFormat Format, int Count)[] sets)
  {
    using XGraphics gfx = MeasureContext();
    CardData card = Card(string.Empty);
    return sets
      .SelectMany(set => Enumerable.Repeat(set.Format, set.Count))
      .Select(format => new CardPart(card, CardGeometry.Create(gfx, card, format, CardLayout.MinBodySize, false, false), [], null, 1, 1))
      .ToList();
  }

  /// <summary>
  ///   Pages each size needs on its own
  /// </summary>
  private static int PagesApart(IReadOnlyList<CardPart> parts) => parts
    .GroupBy(part => part.Format)
    .Sum(group => CardPacker
      .Pack(group.ToList())
      .Count);

  /// <summary>
  ///   Every card has its size (turned or not), stays inside the margin and keeps the gap to every other card
  /// </summary>
  private static void AssertPacked(IReadOnlyList<IReadOnlyList<CardPlacement>> pages)
  {
    foreach (IReadOnlyList<CardPlacement> page in pages)
    {
      Assert.NotEmpty(page);
      foreach (CardPlacement placement in page)
      {
        XRect bounds = placement.Bounds;
        CardFormat format = placement.Part.Format;
        Assert.Equal(placement.Rotated ? format.Height : format.Width, bounds.Width, 3);
        Assert.Equal(placement.Rotated ? format.Width : format.Height, bounds.Height, 3);
        Assert.True(bounds.Left >= CardPacker.Margin - Tolerance);
        Assert.True(bounds.Top >= CardPacker.Margin - Tolerance);
        Assert.True(bounds.Right <= CardPacker.PageWidth - CardPacker.Margin + Tolerance);
        Assert.True(bounds.Bottom <= CardPacker.PageHeight - CardPacker.Margin + Tolerance);
      }

      for (int first = 0; first < page.Count; first++)
      {
        for (int second = first + 1; second < page.Count; second++)
        {
          XRect a = page[first].Bounds;
          XRect b = page[second].Bounds;
          bool apartX = a.Right + CardPacker.Gap <= b.Left + Tolerance || b.Right + CardPacker.Gap <= a.Left + Tolerance;
          bool apartY = a.Bottom + CardPacker.Gap <= b.Top + Tolerance || b.Bottom + CardPacker.Gap <= a.Top + Tolerance;
          Assert.True(apartX || apartY, $"cards {first} and {second} are closer than the gap: {a} {b}");
        }
      }
    }
  }

  private static void AssertEveryWordOnce(CardData card, IReadOnlyList<CardPart> parts)
  {
    List<string> drawn = parts
      .SelectMany(part => part.Lines)
      .Where(line => !line.Repeated)
      .SelectMany(line => CardSamples.Split(line.Line.Text))
      .ToList();
    Assert.Equal(CardSamples.Words(card.Description), drawn);
  }

  /// <summary>
  ///   Every column of the description stays inside its rectangle and the rectangles inside the frame, above the footer
  /// </summary>
  private static void AssertInside(CardPart part)
  {
    CardGeometry geometry = part.Geometry;
    Assert.Equal(geometry.Columns.Count, part.Columns.Count);
    for (int index = 0; index < part.Columns.Count; index++)
    {
      RichTextSlice slice = part.Columns[index];
      XRect column = geometry.Columns[index];
      Assert.True(slice.Height <= column.Height + Tolerance, $"{part.Data.Name}: column {index} text {slice.Height} > {column.Height}");
      Assert.True(column.Bottom <= geometry.FooterRect.Top);
      Assert.True(column.Right <= geometry.Frame.Right);
      Assert.All(slice.Lines, line => Assert.All(line.Line.Runs, run => Assert.True(run.X + run.Width <= column.Width + Tolerance)));
    }

    Assert.True(geometry.FooterRect.Bottom <= geometry.Frame.Bottom);
    if (geometry.Facts is { } facts)
    {
      Assert.True(geometry.FactsOrigin.Y + facts.Height <= geometry.Columns[0].Top);
    }
  }

  #endregion
}
