using System.Globalization;
using TableTopSheets.Common.Container;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Character;
using TableTopSheets.Common.Sheet;
using TableTopSheets.Pdf.Component;
using TableTopSheets.Pdf.Page;
using PdfAttack = TableTopSheets.Pdf.Common.Attack;
using PdfSpell = TableTopSheets.Pdf.Common.Spell;

namespace TableTopSheets.Pdf;

/// <summary>
///   Two page character sheet rendered from a computed <see cref="CharacterSheet"/>
/// </summary>
public class CharacterSheetDocument: Document
{
  #region Field

  private readonly CharacterSheet _sheet;

  #endregion

  #region Constructor

  public CharacterSheetDocument(CharacterSheet sheet)
  {
    this._sheet = sheet;
  }

  #endregion

  #region Override

  protected override void Generate()
  {
    CharacterSheet sheet = this._sheet;
    (string column1, string column2) = Columns(ClassFeatureText(sheet));
    AbilityContainer Ability(string key) => ToContainer(sheet.Abilities.Single(entry => entry.Key == key));

    new ComponentPage
    (
      this.PdfDocument,
      new TitleBox(sheet.Name, sheet.Background, sheet.Classes, sheet.Species, sheet.Subclasses, sheet.Level.ToString(CultureInfo.InvariantCulture)),
      new AcShield(sheet.ArmorClass.ToString(CultureInfo.InvariantCulture), sheet.ShieldEquipped),
      new HitPoints(sheet.HitPointMaximum.ToString(CultureInfo.InvariantCulture), string.Join(" ", sheet.HitDice.Select(entry => $"{entry.Count}{entry.Die}"))),
      new Logo(),
      new AbilitySection(sheet.ProficiencyBonus, Ability("str"), Ability("dex"), Ability("con"), Ability("int"), Ability("wis"), Ability("cha")),
      new EquipmentBox(sheet.ArmorTraining, string.Join(", ", sheet.WeaponProficiencies), string.Join(", ", sheet.ToolProficiencies)),
      new OtherContainer(Signed(sheet.Initiative), sheet.Speed.ToString(CultureInfo.InvariantCulture), sheet.Size.ToString().ToUpperInvariant(), sheet.PassivePerception.ToString(CultureInfo.InvariantCulture)),
      new WeaponDamageBox(sheet.Attacks.Select(ToAttack).ToArray()),
      new ClassFeature(column1, column2),
      new SpeciesTraits(Text(sheet.SpeciesTraits)),
      new Feats(Text(sheet.Feats))
    ).Draw();

    SpellcastingSheet? casting = sheet.Spellcasting.FirstOrDefault();
    List<string> attuned = sheet.AttunedItems.ToList();
    new ComponentPage
    (
      this.PdfDocument,
      new SecondPageLogo(),
      new SpellCasting
      (
        string.Join(" / ", sheet.Spellcasting.Select(entry => entry.Ability).Distinct()),
        casting == null ? string.Empty : string.Join(" / ", sheet.Spellcasting.Select(entry => Signed(entry.Modifier)).Distinct()),
        casting == null ? string.Empty : string.Join(" / ", sheet.Spellcasting.Select(entry => entry.SaveDc.ToString(CultureInfo.InvariantCulture)).Distinct()),
        casting == null ? string.Empty : string.Join(" / ", sheet.Spellcasting.Select(entry => Signed(entry.AttackBonus)).Distinct())
      ),
      new SpellSlots(sheet.SpellSlots.ToArray()),
      new Spells(sheet.Spells.Select(ToSpell).ToArray()),
      new Appearance(),
      new Backstory(),
      new Languages(string.Join(", ", sheet.Languages)),
      new Equipment(string.Join(Environment.NewLine, sheet.Equipment), attuned.ElementAtOrDefault(0), attuned.ElementAtOrDefault(1), attuned.ElementAtOrDefault(2)),
      new Component.Coins()
    ).Draw();
  }

  #endregion

  #region Mapping

  private static AbilityContainer ToContainer(AbilitySheet ability) => new()
  {
    AbilityName = ability.Name.ToUpperInvariant(),
    Score = ability.Score,
    SavingThrow = ability.SaveProficiency > 0,
    Skill = ability.Skills.Select(skill => ((SkillModifierEnum)skill.Proficiency, skill.Name)).ToList(),
  };

  /// <summary>
  ///   Flat damage bonuses are drawn as d1 dice
  /// </summary>
  private static PdfAttack ToAttack(AttackSheet attack) => new()
  {
    Name = attack.Name,
    AttackRollModifier = (sbyte)attack.AttackBonus,
    DamageRoll = attack.DamageDice.Concat(Enumerable.Repeat(DiceEnum.D1, Math.Max(0, attack.DamageBonus))).ToArray(),
    DamageType = attack.DamageType,
    Notes = attack.Notes,
  };

  private static PdfSpell ToSpell(SpellSheet spell) => new()
  {
    Name = spell.Name,
    Notes = spell.Notes,
    Level = spell.Level == 0 ? "C" : spell.Level.ToString(CultureInfo.InvariantCulture),
    CastingTime = spell.CastingTime,
    Range = spell.Range,
    Concentration = spell.Concentration,
    Ritual = spell.Ritual,
    Material = spell.Material,
  };

  private static string ClassFeatureText(CharacterSheet sheet)
  {
    IEnumerable<string> resources = sheet.Resources.Select(resource =>
      $"{resource.Name}{(resource.Die == null ? string.Empty : $" ({resource.Die})")}: {string.Join(" ", Enumerable.Repeat("[ ]", Math.Max(0, resource.Maximum)))}");
    return string.Join("\n", resources) + (sheet.Resources.Count > 0 ? "\n\n" : string.Empty) + Text(sheet.ClassFeatures);
  }

  private static string Text(IEnumerable<FeatureSheet> features)
    => string.Join("\n\n", features.Select(entry => string.IsNullOrWhiteSpace(entry.Description) ? entry.Name : $"{entry.Name}. {entry.Description}"));

  /// <summary>
  ///   Split a text into two columns of similar length at a paragraph boundary
  /// </summary>
  private static (string, string) Columns(string text)
  {
    string[] paragraphs = text.Split("\n\n");
    int total = text.Length;
    int length = 0;
    int split = paragraphs.Length;
    for (int index = 0; index < paragraphs.Length; index++)
    {
      length += paragraphs[index].Length;
      if (length >= total / 2)
      {
        split = index + 1;
        break;
      }
    }

    return (string.Join("\n\n", paragraphs.Take(split)), string.Join("\n\n", paragraphs.Skip(split)));
  }

  private static string Signed(int value) => value >= 0 ? $"+{value}" : value.ToString(CultureInfo.InvariantCulture);

  #endregion
}
