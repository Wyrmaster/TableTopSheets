using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Sheet;
using TableTopSheets.Engine.Choices;
using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Engine.Expressions;
using TableTopSheets.Persistence.Entity.Characters;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Engine.Sheet;

/// <summary>
///   Finds rule problems of a character. Problems are reported, never fixed silently (e.g. after undoing a level up)
/// </summary>
public static class ViolationChecker
{
  #region Constants

  public const int AttunementLimit = 3;

  #endregion

  #region Methods

  public static IReadOnlyList<SheetViolation> Check(CharacterEvaluation evaluation)
  {
    List<SheetViolation> violations = new();

    foreach (PendingChoice pending in ChoiceService.Pending(evaluation).Where(entry => !entry.Choice.Optional))
    {
      violations.Add(new("choice.pending", $"{pending.Owner.Source}: '{pending.Choice.Name}' needs {pending.Remaining} more selection(s)"));
    }

    foreach (CharacterChoice orphan in evaluation.OrphanedSelections)
    {
      string name = evaluation.Catalog.Choices.TryGetValue(orphan.ChoiceId, out Choice? choice) ? choice.Name : orphan.ChoiceId.ToString();
      violations.Add(new("choice.orphaned", $"Selection '{orphan.OptionKey}' of '{name}' belongs to a feature the character no longer has", ViolationSeverityEnum.Info));
    }

    foreach (SelectionInstance selection in evaluation.Selections.Where(entry => entry.Owner.Active))
    {
      OptionDescriptor? option = ChoiceService.AllOptions(evaluation.Catalog, selection.Choice).FirstOrDefault(entry => entry.Key == selection.Selection.OptionKey);
      if (option?.PrerequisiteExpr != null && !Expression.Parse(option.PrerequisiteExpr).EvaluateBoolean(new OptionContext(evaluation, option)))
      {
        violations.Add(new("prerequisite.unmet", $"'{option.Name}' ({selection.Choice.Name}) no longer meets its prerequisite"));
      }
    }

    CheckPreparedSpells(evaluation, violations);
    CheckEquipment(evaluation, violations);

    return violations;
  }

  private static void CheckPreparedSpells(CharacterEvaluation evaluation, List<SheetViolation> violations)
  {
    List<SpellcastingProfile> profiles = evaluation.SpellcastingProfiles.ToList();
    foreach (IGrouping<Guid, CharacterPreparedSpell> group in evaluation.Character.PreparedSpells.GroupBy(entry => entry.ClassId))
    {
      string className = evaluation.Catalog.Classes[group.Key].Name;
      SpellcastingProfile? profile = profiles.FirstOrDefault(entry => entry.ClassId == group.Key && entry.PrepareChange == PrepareChangeEnum.LongRest);
      if (profile == null)
      {
        violations.Add(new("prepared.no-spellcasting", $"Spells are prepared for {className} but the character can't prepare {className} spells"));
        continue;
      }

      int limit = profile.PreparedCountKey == null ? 0 : (int)evaluation.Scale(profile.PreparedCountKey).AsNumber();
      if (group.Count() > limit)
      {
        violations.Add(new("prepared.over-limit", $"{group.Count()} {className} spells are prepared but only {limit} are allowed"));
      }

      int maxLevel = evaluation.MaxSpellLevel(profile);
      IReadOnlySet<Guid> spellbook = evaluation.SpellbookSpellIds();
      foreach (CharacterPreparedSpell prepared in group)
      {
        Spell spell = evaluation.Catalog.Spells[prepared.SpellId];
        if (spell.Level == 0 || spell.Level > maxLevel)
        {
          violations.Add(new("prepared.level", $"{spell.Name} (level {spell.Level}) can't be prepared as {className} (max level {maxLevel})"));
        }

        bool inPool = profile.Pool == SpellPoolEnum.Spellbook ? spellbook.Contains(spell.Id) : spell.Lists.Contains(profile.SpellListKey);
        if (!inPool)
        {
          violations.Add(new("prepared.pool", $"{spell.Name} is not available to prepare as {className}"));
        }
      }
    }
  }

  private static void CheckEquipment(CharacterEvaluation evaluation, List<SheetViolation> violations)
  {
    int attuned = evaluation.Character.Items.Count(entry => entry.Attuned);
    if (attuned > AttunementLimit)
    {
      violations.Add(new("attunement.limit", $"{attuned} items are attuned but only {AttunementLimit} are allowed", ViolationSeverityEnum.Error));
    }

    foreach (CharacterItem item in evaluation.Character.Items.Where(entry => entry.Attuned && !evaluation.Catalog.Items[entry.ItemId].RequiresAttunement))
    {
      violations.Add(new("attunement.unneeded", $"{evaluation.Catalog.Items[item.ItemId].Name} doesn't require attunement", ViolationSeverityEnum.Info));
    }

    if (evaluation.WornArmor is { } armor)
    {
      if (!evaluation.HasArmorTraining(armor.Category))
      {
        violations.Add(new
        (
          "armor.untrained",
          $"Not trained with {armor.Category} armor ({armor.Name}): Disadvantage on Strength and Dexterity D20 Tests and you can't cast spells",
          ViolationSeverityEnum.Error
        ));
      }

      if (armor.StrengthRequirement is { } strength && evaluation.Score("str") < strength)
      {
        violations.Add(new("armor.strength", $"{armor.Name} requires Strength {strength}: Speed is reduced by 10 feet", ViolationSeverityEnum.Info));
      }
    }

    if (evaluation.WieldedShield is { } shield && !evaluation.HasArmorTraining(ArmorEnum.Shield))
    {
      violations.Add(new("shield.untrained", $"Not trained with shields: {shield.Name} grants no Armor Class bonus", ViolationSeverityEnum.Error));
    }

    int hands = evaluation.Character.Items
      .Where(entry => entry.EquipSlot is EquipSlotEnum.MainHand or EquipSlotEnum.OffHand)
      .Sum(entry => evaluation.Catalog.Items[entry.ItemId] is Weapon weapon && weapon.Properties.HasFlag(WeaponPropertyEnum.TwoHanded) ? 2 : 1);
    if (hands > 2)
    {
      violations.Add(new("equipment.hands", "More items are held than the character has hands", ViolationSeverityEnum.Error));
    }
  }

  #endregion
}
