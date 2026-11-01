using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Sheet;
using TableTopSheets.Engine.Sheet;

namespace TableTopSheets.Engine.Tests;

public class HtmlTextTests
{
  [Fact]
  public void TwoColumnTableBecomesLabelledLines()
  {
    const string html = "<p>Choose a dragon.</p><table><tr><th>Dragon</th><th>Damage Type</th></tr><tr><td>Black</td><td>Acid</td></tr>"
                        + "<tr><td>Red</td><td>Fire</td></tr></table>";

    Assert.Equal("Choose a dragon.\nBlack: Acid\nRed: Fire", HtmlText.ToPlain(html));
  }

  [Fact]
  public void WiderTableLabelsEveryCellWithItsHeader()
  {
    const string html = "<p>Choose a legacy.</p><h5>Fiendish Legacies</h5><table><tr><th>Legacy</th><th>Level 1</th><th>Level 3</th>"
                        + "<th>Level 5</th></tr><tr><td>Infernal</td><td>You know the <em>Fire Bolt</em> cantrip.</td>"
                        + "<td><em>Hellish Rebuke</em></td><td><em>Darkness</em></td></tr></table><p>After.</p>";

    Assert.Equal(
      "Choose a legacy.\nFiendish Legacies\nInfernal – Level 1: You know the Fire Bolt cantrip; Level 3: Hellish Rebuke; Level 5: Darkness\nAfter.",
      HtmlText.ToPlain(html));
  }

  [Fact]
  public void TableWithoutHeaderJoinsItsCells()
  {
    Assert.Equal("1 | 2 | 3", HtmlText.ToPlain("<table><tr><td>1</td><td>2</td><td>3</td></tr></table>"));
  }

  [Fact]
  public void BlocksKeepParagraphsHeadingsListsAndTables()
  {
    const string html = "<p>Choose a legacy.</p><h5>Fiendish Legacies</h5><ul><li>one</li><li>two</li></ul>"
                        + "<table><tr><th>Legacy</th><th>Level 1</th></tr><tr><td>Infernal</td><td><em>Fire Bolt</em></td></tr></table>";

    IReadOnlyList<TextBlock> blocks = HtmlText.ToBlocks(html);

    Assert.Equal([TextBlockKindEnum.Paragraph, TextBlockKindEnum.Heading, TextBlockKindEnum.List, TextBlockKindEnum.Table], blocks.Select(block => block.Kind));
    Assert.Equal("Fiendish Legacies", Assert.Single(blocks[1].Spans).Text);
    Assert.Equal(["one", "two"], blocks[2].Items.Select(item => Assert.Single(item).Text));
    TextBlock table = blocks[3];
    Assert.Equal(1, table.HeaderRows);
    Assert.Equal(2, table.Rows.Count);
    Assert.Equal(new TextSpan("Fire Bolt", Italic: true), Assert.Single(table.Rows[1][1]));
  }

  [Fact]
  public void BlocksCarryBoldAndItalicRuns()
  {
    IReadOnlyList<TextSpan> spans = Assert.Single(HtmlText.ToBlocks("<p><strong><em>Breath.</em></strong>  You exhale &amp; burn.</p>")).Spans;

    Assert.Equal([new TextSpan("Breath.", true, true), new TextSpan(" You exhale & burn.")], spans);
  }

  [Fact]
  public void TextWithoutTagsIsOneParagraph()
  {
    TextBlock block = Assert.Single(HtmlText.ToBlocks("Just text."));

    Assert.Equal(TextBlockKindEnum.Paragraph, block.Kind);
    Assert.Equal("Just text.", Assert.Single(block.Spans).Text);
    Assert.Empty(HtmlText.ToBlocks(null));
  }
}
