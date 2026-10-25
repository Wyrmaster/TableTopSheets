using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Sheet;
using TableTopSheets.Pdf;
using TableTopSheets.Persistence.Entity.Characters;

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
  }
}
