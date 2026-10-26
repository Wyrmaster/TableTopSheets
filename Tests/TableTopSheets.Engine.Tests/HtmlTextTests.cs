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
}
