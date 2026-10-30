using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Sheet;
using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Engine.Services;
using TableTopSheets.Persistence.Entity.Characters;

namespace TableTopSheets.Engine.Tests;

public class BarbarianTests
{
  // farmer increases Constitution twice and Wisdom once: Str 15, Dex 14 (+2), Con 18 (+4), Wis 14 (+2)
  private static readonly IReadOnlyDictionary<string, byte> Scores = TestWorld.Scores(15, 14, 16, 8, 13, 8);

  [Fact]
  public async Task UnarmoredDefenseAllowsAShieldAndNeverStacksWithTheMonks()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(Scores, "farmer", "con", "con", "wis");
    await world.LevelUpAsync(id, "barbarian");

    // 10 + Dexterity (+2) + Constitution (+4)
    CharacterSheet unarmored = await world.SheetAsync(id);
    Assert.Equal(16, unarmored.ArmorClass);
    Assert.Equal([new SourceValue("Unarmored Defense (Barbarian)", 16)], unarmored.ArmorClassSources);

    // unlike the Monk's, a shield in hand keeps Unarmored Defense and adds on top
    CharacterItem shield = await world.Service.AddItemAsync(id, "shield");
    await world.Service.EquipAsync(id, shield.Id, EquipSlotEnum.OffHand);
    Assert.Equal(16 + 2, (await world.SheetAsync(id)).ArmorClass);

    // a shield carried on the back isn't wielded
    await world.Service.EquipAsync(id, shield.Id, EquipSlotEnum.Worn);
    Assert.Equal(16, (await world.SheetAsync(id)).ArmorClass);

    CharacterItem leather = await world.Service.AddItemAsync(id, "leather-armor");
    await world.Service.EquipAsync(id, leather.Id, EquipSlotEnum.Armor);
    Assert.Equal(11 + 2, (await world.SheetAsync(id)).ArmorClass);
    await world.Service.EquipAsync(id, leather.Id, EquipSlotEnum.None);

    // a Barbarian 1 / Monk 1 uses the higher formula (Monk: 10 + 2 + 2 = 14), the two never stack
    await world.LevelUpAsync(id, "monk");
    CharacterSheet multiclass = await world.SheetAsync(id);
    Assert.Equal(16, multiclass.ArmorClass);
    Assert.Equal([new SourceValue("Unarmored Defense (Barbarian)", 16)], multiclass.ArmorClassSources);
  }

  [Fact]
  public async Task StartingEquipmentIsAGreataxeFourHandaxesAndAnExplorersPack()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(Scores, "farmer", "con", "con", "wis");
    int gold = (await world.EvaluateAsync(id)).Character.PlayState.GoldPieces;

    CharacterEvaluation evaluation = await world.Service.LevelUpAsync(id, new LevelUpRequest
    {
      ClassKey = "barbarian",
      StartingEquipment = new StartingEquipmentRequest("a"),
      AllowIncomplete = true,
    });

    Dictionary<string, int> items = evaluation.Character.Items
      .GroupBy(entry => world.Catalog.Items[entry.ItemId].Key)
      .ToDictionary(group => group.Key, group => group.Sum(entry => entry.Quantity));
    Assert.Equal(1, items["greataxe"]);
    Assert.Equal(4, items["handaxe"]);
    Assert.Equal(1, items["explorers-pack"]);
    Assert.Equal(gold + 15, evaluation.Character.PlayState.GoldPieces);
    Assert.Equal(["a", "b"], world.Catalog.ClassesByKey["barbarian"].StartingEquipment.Select(entry => entry.Key));
  }

  [Fact]
  public async Task RageIsAToggleWithUsesLikeSecondWind()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(Scores, "farmer", "con", "con", "wis");
    CharacterEvaluation level1 = await world.LevelUpAsync(id, "barbarian", selections:
    [
      new("barbarian.weapon-mastery", "greataxe"),
      new("barbarian.weapon-mastery", "handaxe"),
    ]);

    // Weapon Mastery only marks the chosen weapons
    Assert.True(level1.Stat("mastery.greataxe") > 0);
    Assert.True(level1.Stat("mastery.handaxe") > 0);
    Assert.Equal(0, level1.Stat("mastery.longsword"));

    ResourceSheet rage = (await world.SheetAsync(id)).Resources.Single(entry => entry.Key == "rage");
    Assert.Equal(2, rage.Maximum);
    Assert.Equal("ShortRest", rage.Recharge);

    Assert.Equal(0, level1.Stat("damage.resist.slashing"));
    CharacterEvaluation raging = await world.Service.ToggleAsync(id, "barbarian.rage", true);
    Assert.True(raging.Stat("damage.resist.bludgeoning") > 0);
    Assert.True(raging.Stat("damage.resist.piercing") > 0);
    Assert.True(raging.Stat("damage.resist.slashing") > 0);
    Assert.True(raging.Stat("save.str.advantage") > 0);
    Assert.True(raging.Stat("check.str.advantage") > 0);

    // three rages from level 3
    await world.LevelUpManyAsync(id, "barbarian", 2, "path-of-the-berserker");
    Assert.Equal(3, (await world.SheetAsync(id)).Resources.Single(entry => entry.Key == "rage").Maximum);
  }

  [Fact]
  public async Task ExtraAttackAndFastMovementAtLevelFive()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(Scores, "farmer", "con", "con", "wis");
    CharacterEvaluation level5 = await world.LevelUpManyAsync(id, "barbarian", 5, "path-of-the-zealot");
    CharacterSheet sheet = await world.SheetAsync(id);

    Assert.Equal(2, sheet.AttacksPerAction);
    Assert.Equal(30 + 10, sheet.Speed);
    Assert.True(level5.Stat("save.dex.advantage") > 0);

    // Warrior of the Gods: four d12s from level 3
    ResourceSheet dice = sheet.Resources.Single(entry => entry.Key == "warrior-of-the-gods");
    Assert.Equal(4, dice.Maximum);
    Assert.Equal("1d12", dice.Die);

    // Fast Movement ends in heavy armor
    CharacterItem chainMail = await world.Service.AddItemAsync(id, "chain-mail");
    await world.Service.EquipAsync(id, chainMail.Id, EquipSlotEnum.Armor);
    Assert.Equal(30, (await world.SheetAsync(id)).Speed);
  }

  [Fact]
  public async Task AspectOfTheWildsOwlExtendsDarkvision()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(Scores, "farmer", "con", "con", "wis");
    await world.LevelUpManyAsync(id, "barbarian", 5, "path-of-the-wild-heart");
    CharacterEvaluation level6 = await world.LevelUpAsync(id, "barbarian", selections:
    [
      new("path-of-the-wild-heart.aspect-of-the-wilds", "owl"),
    ]);

    // the Dwarf's 120 feet increase by 60 feet
    Assert.Equal(120 + 60, level6.Stat("sense.darkvision"));
    Assert.True(level6.Stat("spell.speak-with-animals") > 0);
  }

  [Fact]
  public async Task PrimalChampionRaisesStrengthAndConstitution()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(Scores, "farmer", "con", "con", "wis");
    CharacterEvaluation level19 = await world.LevelUpManyAsync(id, "barbarian", 19, "path-of-the-world-tree");
    CharacterEvaluation level20 = await world.LevelUpAsync(id, "barbarian");

    // the maximum rises to 25; increases clipped at 20 before count again once the maximum is higher (engine behaviour)
    Assert.Equal(25, level20.Stat("ability.str.max"));
    Assert.Equal(25, level20.Stat("ability.con.max"));
    Assert.InRange(level20.Score("str"), Math.Min(level19.Score("str") + 4, 25), 25);
    Assert.InRange(level20.Score("con"), Math.Min(level19.Score("con") + 4, 25), 25);
  }
}
