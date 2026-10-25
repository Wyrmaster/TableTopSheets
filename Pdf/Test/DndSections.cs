using System.Diagnostics;
using System.Reflection;
using TableTopSheets.Common.Container.Magic;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Character;
using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Magic;
using TableTopSheets.Pdf.Common;
using TableTopSheets.Pdf.Component;
using TableTopSheets.Pdf.Test.Pdf;

namespace TableTopSheets.Pdf.Test;

public class DndSections
{
  #region Field

  private string _basePath;

  #endregion

  #region Setup TearDown

  public DndSections()
  {
    this._basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    Process.GetProcessesByName("chrome").ToList().ForEach(process => process.Kill());
  }

  #endregion
  
  #region Tests

  // [Fact]
  // public void DndSectionsTest()
  // {
  //   SectionComponent component = new
  //   (
  //     [
  //       new TitleBox("Drizzt D'Urden", "Exile", "Ranger/Monk", "Drow", "Hunter/Way of the Open Hand", "15"),
  //       new AcShield("18", true),
  //       new HitPoints("130", "13D10 2D8"),
  //       new Logo(),
  //       new AbilitySection
  //       (
  //         8,
  //         new()
  //         {
  //           AbilityName = "STRENGTH",
  //           Score = 14,
  //           SavingThrow = false,
  //           Skill = new List<(SkillModifierEnum skillModifier, string skill)>
  //           (
  //             [
  //               new(SkillModifierEnum.None, "Athletics")
  //             ]
  //           )
  //         },
  //         new()
  //         {
  //           AbilityName = "DEXTERITY",
  //           Score = 20,
  //           SavingThrow = true,
  //           Skill = new List<(SkillModifierEnum skillModifier, string skill)>
  //           (
  //             [
  //               new(SkillModifierEnum.Expertise, "Acrobatics"),
  //               new(SkillModifierEnum.Proficient, "Sleight of Hand"),
  //               new(SkillModifierEnum.Expertise, "Stealth"),
  //             ]
  //           )
  //         },
  //         new()
  //         {
  //           AbilityName = "CONSTITUTION",
  //           Score = 14,
  //           SavingThrow = true,
  //           Skill = new List<(SkillModifierEnum skillModifier, string skill)>()
  //         },
  //         new()
  //         {
  //           AbilityName = "INTELLIGENCE",
  //           Score = 12,
  //           SavingThrow = false,
  //           Skill = new List<(SkillModifierEnum skillModifier, string skill)>
  //           (
  //             [
  //               new(SkillModifierEnum.Proficient, "Arcana"),
  //               new(SkillModifierEnum.None, "History"),
  //               new(SkillModifierEnum.Proficient, "Investigation"),
  //               new(SkillModifierEnum.None, "Nature"),
  //               new(SkillModifierEnum.None, "Religion"),
  //             ]
  //           )
  //         },
  //         new()
  //         {
  //           AbilityName = "WISDOM",
  //           Score = 18,
  //           SavingThrow = true,
  //           Skill = new List<(SkillModifierEnum skillModifier, string skill)>
  //           (
  //             [
  //               new(SkillModifierEnum.Expertise, "Animal Handling"),
  //               new(SkillModifierEnum.Proficient, "Insight"),
  //               new(SkillModifierEnum.Half, "Medicine"),
  //               new(SkillModifierEnum.Proficient, "Perception"),
  //               new(SkillModifierEnum.Expertise, "Survival"),
  //             ]
  //           )
  //         },
  //         new()
  //         {
  //           AbilityName = "CHARISMA",
  //           Score = 15,
  //           SavingThrow = false,
  //           Skill = new List<(SkillModifierEnum skillModifier, string skill)>
  //           (
  //             [
  //               new(SkillModifierEnum.Proficient, "Deception"),
  //               new(SkillModifierEnum.None, "Intimidation"),
  //               new(SkillModifierEnum.None, "Performance"),
  //               new(SkillModifierEnum.None, "Persuasion"),
  //             ]
  //           )
  //         }
  //       ),
  //       new EquipmentBox(),
  //       new OtherContainer("+5", "60", "MEDIUM", "15"),
  //       new WeaponDamageBox(),
  //       new ClassFeature(),
  //       new SpeciesTraits(),
  //       new Feats()
  //     ],
  //     [
  //       new SecondPageLogo(),
  //       
  //       new SpellCasting("Wisdom","+4", "14", "15"),
  //       new SpellSlots(),
  //       new Spells(),
  //       
  //       new Appearance(),
  //       new Backstory(),
  //       new Languages(),
  //       new Equipment(),
  //       new Coins(),
  //       
  //     ]
  //   );
  //   string path = Path.Combine(this._basePath, "DndSections.pdf");
  //   component.GenerateCharacterSheetFile(path);
  //   
  //   Process.Start(new ProcessStartInfo()
  //   {
  //     FileName = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
  //     Arguments = path
  //   });
  // }
  
  [Fact]
  public void Sheet()
  {
    SectionComponent component = new
    (
      [
        new TitleBox("Erybar Wagglefall", "Wayfarer", "Ranger", "Gnome", "Gloom Stalker", "5"),
        new AcShield("17", false),
        new HitPoints("55", "5D10"),
        new Logo(),
        new AbilitySection
        (
          3,
          new()
          {
            AbilityName = "STRENGTH",
            Score = 10,
            SavingThrow = true,
            Skill = new List<(SkillModifierEnum skillModifier, string skill)>
            (
              [
                new(SkillModifierEnum.None, "Athletics")
              ]
            )
          },
          new()
          {
            AbilityName = "DEXTERITY",
            Score = 18,
            SavingThrow = true,
            Skill = new List<(SkillModifierEnum skillModifier, string skill)>
            (
              [
                new(SkillModifierEnum.None, "Acrobatics"),
                new(SkillModifierEnum.None, "Sleight of Hand"),
                new(SkillModifierEnum.Expertise, "Stealth"),
              ]
            )
          },
          new()
          {
            AbilityName = "CONSTITUTION",
            Score = 12,
            SavingThrow = false,
            Skill = new List<(SkillModifierEnum skillModifier, string skill)>()
          },
          new()
          {
            AbilityName = "INTELLIGENCE",
            Score = 10,
            SavingThrow = false,
            Skill = new List<(SkillModifierEnum skillModifier, string skill)>
            (
              [
                new(SkillModifierEnum.None, "Arcana"),
                new(SkillModifierEnum.None, "History"),
                new(SkillModifierEnum.Proficient, "Investigation"),
                new(SkillModifierEnum.Proficient, "Nature"),
                new(SkillModifierEnum.None, "Religion"),
              ]
            )
          },
          new()
          {
            AbilityName = "WISDOM",
            Score = 16,
            SavingThrow = false,
            Skill = new List<(SkillModifierEnum skillModifier, string skill)>
            (
              [
                new(SkillModifierEnum.None, "Animal Handling"),
                new(SkillModifierEnum.Proficient, "Insight"),
                new(SkillModifierEnum.None, "Medicine"),
                new(SkillModifierEnum.Proficient, "Perception"),
                new(SkillModifierEnum.None, "Survival"),
              ]
            )
          },
          new()
          {
            AbilityName = "CHARISMA",
            Score = 10,
            SavingThrow = false,
            Skill = new List<(SkillModifierEnum skillModifier, string skill)>
            (
              [
                new(SkillModifierEnum.None, "Deception"),
                new(SkillModifierEnum.None, "Intimidation"),
                new(SkillModifierEnum.None, "Performance"),
                new(SkillModifierEnum.None, "Persuasion"),
              ]
            )
          }
        ),
        new EquipmentBox(ArmorEnum.Light|ArmorEnum.Medium|ArmorEnum.Shield, "Simple,Martial", "Thieves' Tools"),
        new OtherContainer("+7", "25", "SMALL", "15"),
        new WeaponDamageBox(
          new() { Name = "Sylvan Talon", AttackRollModifier = 7, DamageRoll = [DiceEnum.D6,DiceEnum.D1,DiceEnum.D1,DiceEnum.D1,DiceEnum.D1], DamageType = DamageTypeEnum.Slashing},
          new() { Name = "Longbow", AttackRollModifier = 7, DamageRoll = [DiceEnum.D8,DiceEnum.D1,DiceEnum.D1,DiceEnum.D1,DiceEnum.D1], DamageType = DamageTypeEnum.Piercing, Notes = "150/600"},
          new() { Name = "Dagger", AttackRollModifier = 7, DamageRoll = [DiceEnum.D4,DiceEnum.D1,DiceEnum.D1,DiceEnum.D1,DiceEnum.D1], DamageType = DamageTypeEnum.Piercing, }
        ),
        new ClassFeature
        (
          "Hunter's Mark [ ] [ ] [ ]\n\n" +
          "Fighting Style: Defense (While you are wearing armor, you gain a +1 bonus to AC)\n\n" +
          "Extra Attack\n\n" +
          "Dread Ambusher\nYou can give yourself a bonus to your initiative rolls equal to your Wisdom modifier.\n\nAt the start of your first turn of each combat, your walking speed increases by 10 feet, which lasts until the end of that turn. If you take the Attack action on that turn, you can make one additional weapon attack as part of that action. If that attack hits, the target takes an extra 1d8 damage of the weapon's damage type.",
          "Umbral Sight\nYou gain Darkvision with a range of 60 feet. If you already have Darkvision when you gain this feature, its range increases by 60 feet.\n\nYou are also adept at evading creatures that rely on Darkvision. While entirely in Darkness, you have the Invisible condition to any creature that relies on Darkvision to see you in that Darkness.\n\n" +
          ""
        ),
        new SpeciesTraits
        (
          "Darkvision. You have Darkvision with a range of 60 feet.\n\n" +
          "Gnomish Cunning. You have Advantage on Intelligence, Wisdom, and Charisma saving throws.\n\n" +
          "Rock Gnome. You can spend 10 minutes casting Prestidigitation to create a Tiny clockwork device (AC 5, 1 HP), such as a toy, fire starter, or music box. When you create the device, you determine its function by choosing one effect from Prestidigitation; the device produces that effect whenever you or another creature takes a Bonus Action to activate it with a touch. If the chosen effect has options within it, you choose one of those options for the device when you create it. For example, if you choose the spell’s ignite-extinguish effect, you determine whether the device ignites or extinguishes fire; the device doesn’t do both. You can have three such devices in existence at a time, and each falls apart 8 hours after its creation or when you dismantle it with a touch as a Utilize action."
        ),
        new Feats
        (
          "Lucky: [ ] [ ] [ ]\n" +
          "Luck Points.You have a number of Luck Points equal to your Proficiency Bonus and can spend the points on the benefits below. You regain your expended Luck Points when you finish a Long Rest.\n" +
          "Advantage. When you roll a d20 for a D20 Test, you can spend 1 Luck Point to give yourself Advantage on the roll.\n" +
          "Disadvantage. When a creature rolls a d20 for an attack roll against you, you can spend 1 Luck Point to impose Disadvantage on that roll."
        )
      ],
      [
        new SecondPageLogo(),
        
        new SpellCasting("Wisdom","+3", "14", "+6"),
        new SpellSlots([
          new SpellSlot{Level = 1, RefreshPeriod = PeriodEnum.LongRest},
          new SpellSlot{Level = 1, RefreshPeriod = PeriodEnum.LongRest},
          new SpellSlot{Level = 1, RefreshPeriod = PeriodEnum.LongRest},
          new SpellSlot{Level = 1, RefreshPeriod = PeriodEnum.LongRest},
          new SpellSlot{Level = 2, RefreshPeriod = PeriodEnum.LongRest},
          new SpellSlot{Level = 2, RefreshPeriod = PeriodEnum.LongRest},
        ]),
        new Spells
        (
          new Spell {Level = "C", Name = "Prestidigitation", CastingTime = "A", Range = "10 ft.", Notes = "Gnome"},
          new Spell {Level = "C", Name = "Mending", CastingTime = "1min", Range = "Touch.", Notes = "Gnome"},
          new Spell {Level = "1", Name = "Disguise Self", CastingTime = "A", Range = "Self", Notes = "Gloom Stalker"},
          new Spell {Level = "1", Name = "Hunter's Mark", CastingTime = "BA", Concentration = true, Range = "90 ft.", Notes = "Ranger Always"},
          new Spell {Level = "1", Name = "Goodberry", CastingTime = "A", Range = "Self", Notes = "Ranger"},
          new Spell {Level = "1", Name = "Longstrider", CastingTime = "A", Range = "Touch", Notes = "Ranger"},
          new Spell {Level = "1", Name = "Speak with Animals", CastingTime = "A", Range = "Self", Notes = "Ranger"},
          new Spell {Level = "1", Name = "Cure Wounds", CastingTime = "A", Range = "Touch", Notes = "Ranger"},
          new Spell {Level = "1", Name = "Hail of Thorns ", CastingTime = "BA", Range = "Self", Notes = "Ranger"},
          new Spell {Level = "2", Name = "Rope Trick", CastingTime = "A", Range = "Touch", Notes = "Gloom Stalker"},
          new Spell {Level = "2", Name = "Silence", CastingTime = "A", Range = "120ft. (20 ft.)", Notes = "Ranger"}
        ),
        
        new Appearance(),
        new Backstory(),
        new Languages("Common, Gnomish"),
        new Equipment
        (
          "Sylvan Talon, Quiver of Ehlonna, Thieves' Tools, Bedroll, 2 Pouches, Traveler's Clothes,Longbow,2 Daggers, Breastplate, " + 
          "60 Arrows, 3 D4 Healing Potions, Three-Dragon Ante, Mistletoe (Druidic Focus), Explorer's Pack,", 
          "Sylvan Talon"
        ),
        new Coins(),
        
      ]
    );
    string path = Path.Combine(this._basePath, "DndSections.pdf");
    component.GenerateCharacterSheetFile(path);
    
    Process.Start(new ProcessStartInfo()
    {
      FileName = @"xdg-open",
      Arguments = path
    });
  }

  [Fact]
  public void GenerateItemCards()
  {
    Cards cards = new Cards([
      ("Chime of opening", "This hollow metal tube measures about 1 foot long and weights 1 pound. As a Magic action, you can strike the chime to cast Knock. The spell's customary knocking sound is replaced by the clear, ringing tone of the chime, which is audible out to 300 feet.\nThe chime can be used 10 times. After the tenth time, it cracks and becomes useless."),
      ("Boots of Speed", "While you wear these boots, you can take a Bonus Action to click the boots' heels together. If you do, the boots double your Speed, and any creature that makes an Opportunity Attack against you has Disadvantage on the attack roll. If you click your heels together again, you end the effect.\nWhen you've used the boots' property for a total of 10 minutes, the magic ceases to function for you until you finish a Long Rest."),
      ("Dwarven Thrower", "You gain a +3 bonus to attack rolls and damage rolls made with this magic weapon. It has the Thrown property with a normal range of 20 feet and a long range of 60 feet. When you hit with a ranged attack using this weapon, it deals an extra 1d8 Force damage, or an extra 2d8 Force Damage if the target is a Giant. Immediately after hitting or missing, the weapon flies back to your hand. "),
      ("Frost Brand Longsword", "When you hit with an attack roll using this magic weapon, the target takes an extra 1d6 Cold damage. In addition, while you hold the weapon, you have Resistance to Fire damage.\nIn freezing temperatures, the weapon sheds Bright Light in a 10-foot radius and Dim Light for an additional 10 feet.\nWhen you draw this weapon, you extinguish all nonmagical flames within 30 feet of yourself. Once used, this property can't be used again for 1 hour."),
      ("Belt of Hill Giant Strength", "While wearing this belt, your Strength changes to 21. The item has no effect on you if your Strength without the belt is equal to or greater than the belt's score."),
      ("Helm of Teleportation", "The helm has 3 charges. While wearing it, you can expend 1 charge to cast Teleport from it. The helm regains 1d3 expends charges daily at dawn."),
      ("Oil of Slipperiness", "One vial of this oil can cover one Medium or smaller creature, along with the equipment it's wearing and carrying (one additional vial is required for each size category above Medium). Applying the oil takes 10 minutes. The affected creature then gains the effect of the Freedom of Movement spell for 8 hours."),
      ("Robe of Stars", "This black or dark-blue robe is embroidered with small white or silver stars. You gain a +1 bonus to saving throws while you wear it.\nSix stars, located on the robe's upper-front portion are particularly large. While wearing this robe, you can take a Magic action to remove on of the stars and expend it to cast the level 5 version of Magic Missiles. Daily at dusk, 1d6 removed stars reappear on the robe."),
      ("Wraps of Unarmed Power", "While wearing these wraps, you have a bonus to attack rolls and damage rolls made with your Unarmed Strikes (+2). Those strikes deal either Force or their normal damage Type."),
      ("Silver Dagger", "An alchemical process has bonded silver to this magic weapon. When you score a Critical Hit with it against a creature that is a shape-shifted, the weapon deals an additional die of damage."),
    ]);
    
    string path = Path.Combine(this._basePath, "Items.pdf");
    cards.GenerateCharacterSheetFile(path);
    
    Process.Start(new ProcessStartInfo()
    {
      FileName = @"xdg-open",
      Arguments = path
    });
  }
  
  #endregion
}