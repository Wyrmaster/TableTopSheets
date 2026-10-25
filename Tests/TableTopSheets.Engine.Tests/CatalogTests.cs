using TableTopSheets.Engine.Rules;
using TableTopSheets.Seed;

namespace TableTopSheets.Engine.Tests;

public class CatalogTests
{
  [Fact]
  public void SeedDataIsValid()
  {
    SeedResult result = SeedBuilder.Build(SeedLoader.Load());
    IReadOnlyList<string> errors = CatalogValidator.Validate(result.ToCatalog());

    Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
  }
}
