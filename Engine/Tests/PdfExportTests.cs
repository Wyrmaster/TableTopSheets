using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Sheet;
using TableTopSheets.Pdf;
using TableTopSheets.Persistence.Entity.Characters;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace TableTopSheets.Engine.Tests;

public class PdfExportTests
{
  [Fact]
  public async Task MulticlassCharacterRendersToPdf()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(14, 13, 14, 15, 10, 8), "sage", "int", "int", "con");
    await world.LevelUpManyAsync(id, "wizard", 3, "evoker");
    await world.LevelUpAsync(id, "fighter", selections: [new("fighter.fighting-style", "defense"), new("fighter.weapon-mastery", "longsword")]);
    await world.LevelUpAsync(id, "fighter");

    CharacterItem armor = await world.Service.AddItemAsync(id, "chain-shirt");
    CharacterItem sword = await world.Service.AddItemAsync(id, "longsword");
    await world.Service.AddItemAsync(id, "dagger", 2);
    await world.Service.EquipAsync(id, armor.Id, EquipSlotEnum.Armor);
    await world.Service.EquipAsync(id, sword.Id, EquipSlotEnum.MainHand);

    CharacterSheet sheet = await world.SheetAsync(id);
    string path = Path.Combine(AppContext.BaseDirectory, "multiclass-sheet.pdf");
    new CharacterSheetDocument(sheet).GenerateCharacterSheetFile(path);

    Assert.True(new FileInfo(path).Length > 10_000);
    Assert.Equal("Wizard 3 / Fighter 2", sheet.Classes);
    // chain shirt 13 + Dexterity (+1), Defense +1
    Assert.Equal(15, sheet.ArmorClass);
    Assert.DoesNotContain(sheet.Violations, entry => entry.Severity == ViolationSeverityEnum.Error);
    // the class features of five levels don't fit the box even at the minimum size, they continue on page 3
    Assert.Equal(3, PageCount(path));
  }

  [Fact]
  public async Task ShortCharacterStaysOnTwoPages()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.StandardArray, "soldier", "str", "str", "con");
    await world.LevelUpAsync(id, "fighter", selections: [new("fighter.fighting-style", "defense"), new("fighter.weapon-mastery", "longsword")]);

    CharacterSheet sheet = await world.SheetAsync(id);
    string path = Path.Combine(AppContext.BaseDirectory, "short-sheet.pdf");
    new CharacterSheetDocument(sheet).GenerateCharacterSheetFile(path);

    Assert.Equal(2, PageCount(path));
  }

  [Fact]
  public async Task Level20CharacterContinuesOnExtraPage()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(8, 14, 14, 15, 12, 10), "sage", "int", "int", "con");
    await world.LevelUpManyAsync(id, "wizard", 20, "evoker");

    CharacterSheet sheet = await world.SheetAsync(id);
    string path = Path.Combine(AppContext.BaseDirectory, "level20-sheet.pdf");
    new CharacterSheetDocument(sheet).GenerateCharacterSheetFile(path);

    Assert.Equal("Wizard 20", sheet.Classes);
    Assert.True(PageCount(path) >= 3);
  }

  private static int PageCount(string path)
  {
    using PdfDocument document = PdfReader.Open(path, PdfDocumentOpenMode.Import);
    return document.PageCount;
  }
}
