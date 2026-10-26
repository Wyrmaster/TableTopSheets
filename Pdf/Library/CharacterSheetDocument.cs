using System.Globalization;
using TableTopSheets.Common.Container;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Character;
using TableTopSheets.Common.Sheet;
using TableTopSheets.Pdf.Component;
using TableTopSheets.Pdf.Interface;
using TableTopSheets.Pdf.Page;
using PdfSharp;
using PdfSharp.Drawing;
using PdfAttack = TableTopSheets.Pdf.Common.Attack;
using PdfSpell = TableTopSheets.Pdf.Common.Spell;

namespace TableTopSheets.Pdf;

/// <summary>
///   Two page character sheet rendered from a computed <see cref="CharacterSheet"/>.
///   Text that doesn't fit its box continues in sections on extra pages.
/// </summary>
public class CharacterSheetDocument: Document
{
  #region Constants

  private const double ContinuationX = 13;
  private const double ContinuationTop = 12;
  private const double ContinuationBottom = 830;
  private const double ContinuationGap = 8;

  #endregion

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
    AbilityContainer Ability(string key) => ToContainer(sheet.Abilities.Single(entry => entry.Key == key));

    ClassFeature classFeature = new(ClassFeatureText(sheet));
    SpeciesTraits speciesTraits = new(Text(sheet.SpeciesTraits));
    Feats feats = new(Text(sheet.Feats.Where(Printed)));
    EquipmentBox equipmentBox = new(sheet.ArmorTraining, string.Join(", ", sheet.WeaponProficiencies), string.Join(", ", sheet.ToolProficiencies));
    WeaponDamageBox weaponDamage = new(sheet.Attacks
      .Select(ToAttack)
      .ToArray());

    new ComponentPage
    (
      this.PdfDocument,
      new TitleBox(sheet.Name, sheet.Background, sheet.Classes, sheet.Species, sheet.Subclasses, sheet.Level.ToString(CultureInfo.InvariantCulture)),
      new AcShield(sheet.ArmorClass.ToString(CultureInfo.InvariantCulture), sheet.ShieldEquipped),
      new HitPoints(sheet.HitPointMaximum.ToString(CultureInfo.InvariantCulture), string.Join(" ", sheet.HitDice.Select(entry => $"{entry.Count}{entry.Die}"))),
      new Logo(),
      new AbilitySection(sheet.ProficiencyBonus, Ability("str"), Ability("dex"), Ability("con"), Ability("int"), Ability("wis"), Ability("cha")),
      equipmentBox,
      new OtherContainer(Signed(sheet.Initiative), sheet.Speed.ToString(CultureInfo.InvariantCulture), sheet.Size
        .ToString()
        .ToUpperInvariant(), sheet.PassivePerception.ToString(CultureInfo.InvariantCulture)),
      weaponDamage,
      classFeature,
      speciesTraits,
      feats
    ).Draw();

    SpellcastingSheet? casting = sheet.Spellcasting.FirstOrDefault();
    List<string> attuned = sheet.AttunedItems.ToList();
    Spells spells = new(sheet.Spells
      .Select(ToSpell)
      .ToArray());
    Languages languages = new(string.Join(", ", sheet.Languages));
    Equipment equipment = new(string.Join(Environment.NewLine, sheet.Equipment), attuned.ElementAtOrDefault(0), attuned.ElementAtOrDefault(1), attuned.ElementAtOrDefault(2));
    new ComponentPage
    (
      this.PdfDocument,
      new SecondPageLogo(),
      new SpellCasting
      (
        string.Join(" / ", sheet.Spellcasting
          .Select(entry => entry.Ability)
          .Distinct()),
        casting == null ? string.Empty : string.Join(" / ", sheet.Spellcasting
          .Select(entry => Signed(entry.Modifier))
          .Distinct()),
        casting == null ? string.Empty : string.Join(" / ", sheet.Spellcasting
          .Select(entry => entry.SaveDc.ToString(CultureInfo.InvariantCulture))
          .Distinct()),
        casting == null ? string.Empty : string.Join(" / ", sheet.Spellcasting
          .Select(entry => Signed(entry.AttackBonus))
          .Distinct())
      ),
      new SpellSlots(sheet.SpellSlots.ToArray()),
      spells,
      new Appearance(),
      new Backstory(),
      languages,
      equipment,
      new Component.Coins()
    ).Draw();

    this.DrawContinuations(classFeature, speciesTraits, feats, equipmentBox, weaponDamage, spells, languages, equipment);
  }

  #endregion

  #region Continuation

  /// <summary>
  ///   Stack the overflow of the given components (in this order) as full width sections on extra pages
  /// </summary>
  private void DrawContinuations(params IOverflowComponent[] components)
  {
    List<(string Title, string Text)> sections = components
      .Where(component => !string.IsNullOrWhiteSpace(component.Overflow))
      .Select(component => (component.ContinuationTitle, component.Overflow!))
      .ToList();
    if (sections.Count == 0)
    {
      return;
    }

    XSize pageSize = new(PageSizeConverter.ToSize(PageSize.A4).Width, PageSizeConverter.ToSize(PageSize.A4).Height);
    double width = pageSize.Width - ContinuationX * 2;
    using XGraphics gfx = XGraphics.CreateMeasureContext(pageSize, XGraphicsUnit.Point, XPageDirection.Downwards);

    List<List<IComponent>> pages = [[]];
    double y = ContinuationTop;
    foreach ((string title, string text) in sections)
    {
      string? remaining = text;
      while (remaining != null)
      {
        if (ContinuationBottom - y < ContinuationBox.MinimumHeight(gfx))
        {
          pages.Add([]);
          y = ContinuationTop;
        }

        double height = Math.Min(ContinuationBox.NeededHeight(gfx, remaining, width), ContinuationBottom - y);
        XRect rect = new(ContinuationX, y, width, height);
        pages[^1].Add(new ContinuationBox(title, remaining, rect));
        remaining = ContinuationBox.Layout(gfx, remaining, rect).Overflow;
        y += height + ContinuationGap;
      }
    }

    foreach (List<IComponent> page in pages)
    {
      new ComponentPage(this.PdfDocument, page.ToArray()).Draw();
    }
  }

  #endregion

  #region Mapping

  private static AbilityContainer ToContainer(AbilitySheet ability) => new()
  {
    AbilityName = ability.Name.ToUpperInvariant(),
    Score = ability.Score,
    SavingThrow = ability.SaveProficiency > 0,
    Skill = ability.Skills
      .Select(skill => ((SkillModifierEnum)skill.Proficiency, skill.Name))
      .ToList(),
  };

  /// <summary>
  ///   Flat damage bonuses are drawn as d1 dice
  /// </summary>
  private static PdfAttack ToAttack(AttackSheet attack) => new()
  {
    Name = attack.Name,
    AttackRollModifier = (sbyte)attack.AttackBonus,
    DamageRoll = attack.DamageDice
      .Concat(Enumerable.Repeat(DiceEnum.D1, Math.Max(0, attack.DamageBonus)))
      .ToArray(),
    DamageType = attack.DamageType ?? DamageTypeEnum.Force,
    Notes = attack.Notes,
    AttackRollText = attack.SaveDc is { } dc ? $"DC {dc}" : null,
    DamageText = attack.Damage.Contains(' ') || attack.DamageType == null ? $"{attack.Damage} {attack.DamageType}".TrimEnd() : null,
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
    return string.Join("\n", resources) + (sheet.Resources.Count > 0 ? "\n\n" : string.Empty) + Text(sheet.ClassFeatures.Where(Printed));
  }

  /// <summary>
  ///   Ability Score Improvements say nothing about the character and spellcasting is covered by the spell page
  /// </summary>
  private static bool Printed(FeatureSheet feature) => !feature.Spellcasting && !feature.Key.Contains("ability-score-improvement");

  private static string Text(IEnumerable<FeatureSheet> features)
    => string.Join("\n\n", features.Select(entry => string.IsNullOrWhiteSpace(entry.Description) ? entry.Name : $"{entry.Name}. {entry.Description}"));

  private static string Signed(int value) => value >= 0 ? $"+{value}" : value.ToString(CultureInfo.InvariantCulture);

  #endregion
}
