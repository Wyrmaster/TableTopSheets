using Microsoft.EntityFrameworkCore;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Key;
using TableTopSheets.Common.Sheet;
using TableTopSheets.Engine.Choices;
using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Engine.Rules;
using TableTopSheets.Engine.Sheet;
using TableTopSheets.Persistence;
using TableTopSheets.Persistence.Entity.Characters;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Engine.Services;

/// <summary>
///   All changes to a character go through this service. Every change is validated against a fresh evaluation and either
///   applied completely or not at all
/// </summary>
public sealed class CharacterService(TableTopSheetsContext context, RulesCatalog catalog)
{
  #region Constants

  public const int MaxLevel = 20;
  public const int MulticlassMinimumScore = 13;

  #endregion

  #region Fields

  private DateTime _lastTimestamp = DateTime.MinValue;

  #endregion

  #region Loading

  public async Task<Character> LoadAsync(Guid characterId, CancellationToken cancellationToken = default)
    => await context.Characters
         .AsSplitQuery()
         .Include(entry => entry.Levels)
         .Include(entry => entry.Choices)
         .Include(entry => entry.Items)
         .Include(entry => entry.PreparedSpells)
         .Include(entry => entry.SpellbookEntries)
         .Include(entry => entry.ResourceUsages)
         .SingleOrDefaultAsync(entry => entry.Id == characterId, cancellationToken)
       ?? throw new RuleViolationException($"Character {characterId} doesn't exist");

  public CharacterEvaluation Evaluate(Character character) => new(catalog, character);

  public async Task<CharacterSheet> BuildSheetAsync(Guid characterId, CancellationToken cancellationToken = default)
    => CharacterSheetBuilder.Build(this.Evaluate(await this.LoadAsync(characterId, cancellationToken)));

  #endregion

  #region Creation

  public async Task<Character> CreateAsync(CreateCharacterRequest request, CancellationToken cancellationToken = default)
  {
    Species species = catalog.Species.Values.SingleOrDefault(entry => entry.Key == request.SpeciesKey)
                      ?? throw new RuleViolationException($"Unknown species '{request.SpeciesKey}'");
    Background background = catalog.Backgrounds.Values.SingleOrDefault(entry => entry.Key == request.BackgroundKey)
                            ?? throw new RuleViolationException($"Unknown background '{request.BackgroundKey}'");

    byte Score(string ability) => request.Abilities.TryGetValue(ability, out byte score)
      ? score
      : throw new RuleViolationException($"Missing ability score '{ability}'");

    Character character = new()
    {
      Id = Guid.NewGuid(),
      Name = request.Name,
      SpeciesId = species.Id,
      BackgroundId = background.Id,
      Alignment = request.Alignment,
      AbilityMethod = request.AbilityMethod,
      Strength = Score("str"),
      Dexterity = Score("dex"),
      Constitution = Score("con"),
      Intelligence = Score("int"),
      Wisdom = Score("wis"),
      Charisma = Score("cha"),
    };

    this.ApplySelections(character, null, request.Selections);
    if (!request.AllowIncomplete)
    {
      EnsureComplete(this.Evaluate(character));
    }

    context.Characters.Add(character);
    await context.SaveChangesAsync(cancellationToken);
    return character;
  }

  /// <summary>
  ///   Answer open choices. The selections belong to the latest level (origin when the character has no level yet),
  ///   so undoing that level removes them as well
  /// </summary>
  public async Task<CharacterEvaluation> SelectAsync(Guid characterId, IReadOnlyList<SelectionRequest> selections, CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    this.Select(character, selections);
    await context.SaveChangesAsync(cancellationToken);
    return this.Evaluate(character);
  }

  /// <summary>
  ///   Answer open choices of a loaded character (in memory, saved by the caller)
  /// </summary>
  public void Select(Character character, IReadOnlyList<SelectionRequest> selections)
  {
    Guid? levelId = character.Levels.MaxBy(entry => entry.Number)?.Id;
    this.Transaction(character, () => this.ApplySelections(character, levelId, selections));
  }

  /// <summary>
  ///   Throw when required choices are still open
  /// </summary>
  public void EnsureComplete(Character character) => EnsureComplete(this.Evaluate(character));

  #endregion

  #region Level Up

  /// <summary>
  ///   Choices the character will have to answer when gaining a level in the class (nothing is saved)
  /// </summary>
  public async Task<IReadOnlyList<PendingChoice>> PreviewLevelUpAsync(Guid characterId, string classKey, string? subclassKey = null, CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    CharacterLevel level = this.CreateLevel(character, new LevelUpRequest { ClassKey = classKey, SubclassKey = subclassKey });
    character.Levels.Add(level);
    try
    {
      return ChoiceService.Pending(this.Evaluate(character)).ToList();
    }
    finally
    {
      character.Levels.Remove(level);
    }
  }

  public async Task<CharacterEvaluation> LevelUpAsync(Guid characterId, LevelUpRequest request, CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    this.LevelUp(character, request);
    await context.SaveChangesAsync(cancellationToken);
    return this.Evaluate(character);
  }

  /// <summary>
  ///   Apply a level up to a loaded character (in memory, saved by the caller)
  /// </summary>
  public void LevelUp(Character character, LevelUpRequest request)
  {
    CharacterLevel level = this.CreateLevel(character, request);
    this.Transaction(character, () =>
    {
      if (request.Replacements
          .GroupBy(entry => character.Choices.SingleOrDefault(choice => choice.Id == entry.SelectionId)?.ChoiceId)
          .Any(group => group.Count() > 1))
      {
        throw new RuleViolationException("Only one selection per choice can be replaced on a level up");
      }

      character.Levels.Add(level);
      CharacterEvaluation evaluation = this.Evaluate(character);

      foreach (ReplacementRequest replacement in request.Replacements)
      {
        this.Replace(character, level, replacement, evaluation);
        evaluation = this.Evaluate(character);
      }

      this.ApplySelections(character, level.Id, request.Selections);
      if (!request.AllowIncomplete)
      {
        EnsureComplete(this.Evaluate(character));
      }
    });
  }

  private CharacterLevel CreateLevel(Character character, LevelUpRequest request)
  {
    CharacterClass @class = catalog.ClassesByKey.GetValueOrDefault(request.ClassKey)
                            ?? throw new RuleViolationException($"Unknown class '{request.ClassKey}'");
    if (character.Levels.Count >= MaxLevel)
    {
      throw new RuleViolationException($"A character can't exceed level {MaxLevel}");
    }

    CharacterEvaluation current = this.Evaluate(character);
    int classLevel = current.ClassLevel(@class.Id) + 1;

    // multiclassing: 13+ in the primary abilities of every current class and of the new class
    if (classLevel == 1 && current.TotalLevel > 0)
    {
      List<string> errors = current.ClassLevels.Keys
        .Select(id => catalog.Classes[id])
        .Append(@class)
        .Where(entry => !MeetsPrimaryAbilities(current, entry))
        .Select(entry => $"Multiclassing requires {string.Join(entry.PrimaryAbilityMode == PrimaryAbilityModeEnum.Any ? " or " : " and ", entry.PrimaryAbilities.Select(Naming.Title))} {MulticlassMinimumScore}+ ({entry.Name})")
        .ToList();
      if (errors.Count > 0)
      {
        throw new RuleViolationException(errors);
      }
    }

    Subclass? subclass = null;
    if (classLevel == @class.SubclassLevel)
    {
      subclass = request.SubclassKey == null
        ? throw new RuleViolationException($"{@class.Name} level {classLevel} requires a subclass")
        : catalog.SubclassesByKey.GetValueOrDefault(request.SubclassKey);
      if (subclass == null || subclass.ClassId != @class.Id)
      {
        throw new RuleViolationException($"'{request.SubclassKey}' is not a {@class.Name} subclass");
      }
    }
    else if (request.SubclassKey != null)
    {
      throw new RuleViolationException($"The {@class.Name} subclass is chosen at level {@class.SubclassLevel}");
    }

    if (request.HitPointRoll is { } roll && (roll < 1 || roll > (int)@class.HitDie))
    {
      throw new RuleViolationException($"Hit point roll {roll} is not possible with a {@class.HitDie}");
    }

    return new CharacterLevel
    {
      Id = Guid.NewGuid(),
      CharacterId = character.Id,
      Number = (byte)(character.Levels.Count + 1),
      ClassId = @class.Id,
      SubclassId = subclass?.Id,
      HitPointRoll = request.HitPointRoll,
      CreatedAt = this.Timestamp(),
    };
  }

  private void Replace(Character character, CharacterLevel level, ReplacementRequest replacement, CharacterEvaluation evaluation)
  {
    SelectionInstance selection = evaluation.Selections.FirstOrDefault(entry => entry.Selection.Id == replacement.SelectionId)
                                  ?? throw new RuleViolationException($"Selection {replacement.SelectionId} is not active");
    if (selection.Choice.ReplacePolicy != ReplacePolicyEnum.OneOnLevelUp)
    {
      throw new RuleViolationException($"'{selection.Choice.Name}' can't be changed on a level up");
    }

    if (evaluation.OwningClass(selection.Owner)?.Id != level.ClassId)
    {
      throw new RuleViolationException($"'{selection.Choice.Name}' can only be changed when gaining a level in its class");
    }

    selection.Selection.SupersededByLevelId = level.Id;
    CharacterEvaluation withoutOld = this.Evaluate(character);
    FeatureInstance owner = ChoiceService.FindOwner(withoutOld, selection.Choice, selection.Selection.ParentId)
                            ?? throw new RuleViolationException($"'{selection.Choice.Name}' is not available");
    if (ChoiceService.Validate(withoutOld, selection.Choice, owner, replacement.OptionKey) is { } error)
    {
      throw new RuleViolationException(error);
    }

    character.Choices.Add(new CharacterChoice
    {
      Id = Guid.NewGuid(),
      CharacterId = character.Id,
      ChoiceId = selection.Choice.Id,
      OptionKey = replacement.OptionKey,
      LevelId = level.Id,
      ParentId = selection.Selection.ParentId,
      CreatedAt = this.Timestamp(),
    });
  }

  #endregion

  #region Undo

  /// <summary>
  ///   Remove the highest level: its selections are deleted and selections it replaced become active again.
  ///   Loadouts that became invalid are reported as violations of the returned evaluation
  /// </summary>
  public async Task<CharacterEvaluation> UndoLastLevelAsync(Guid characterId, CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    CharacterLevel level = character.Levels.MaxBy(entry => entry.Number)
                           ?? throw new RuleViolationException("The character has no level to undo");

    foreach (CharacterChoice choice in character.Choices.Where(entry => entry.LevelId == level.Id).ToList())
    {
      RemoveSelection(character, choice);
    }

    foreach (CharacterChoice choice in character.Choices.Where(entry => entry.SupersededByLevelId == level.Id))
    {
      choice.SupersededByLevelId = null;
    }

    character.Levels.Remove(level);
    context.CharacterLevels.Remove(level);
    await context.SaveChangesAsync(cancellationToken);
    return this.Evaluate(character);
  }

  #endregion

  #region Rest Changes

  /// <summary>
  ///   Change a selection in place when its choice allows changes on a rest (Wizard cantrips ...)
  /// </summary>
  public async Task<CharacterEvaluation> ReplaceOnRestAsync(Guid characterId, Guid selectionId, string optionKey, CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    CharacterEvaluation evaluation = this.Evaluate(character);
    SelectionInstance selection = evaluation.Selections.FirstOrDefault(entry => entry.Selection.Id == selectionId)
                                  ?? throw new RuleViolationException($"Selection {selectionId} is not active");
    if (selection.Choice.ReplacePolicy is not (ReplacePolicyEnum.OnLongRest or ReplacePolicyEnum.OnShortRest))
    {
      throw new RuleViolationException($"'{selection.Choice.Name}' can't be changed on a rest");
    }

    string previous = selection.Selection.OptionKey;

    // validate as if the old selection was gone; the marker never reaches the database
    selection.Selection.SupersededByLevelId = Guid.Empty;
    try
    {
      CharacterEvaluation withoutOld = this.Evaluate(character);
      FeatureInstance owner = ChoiceService.FindOwner(withoutOld, selection.Choice, selection.Selection.ParentId)!;
      if (ChoiceService.Validate(withoutOld, selection.Choice, owner, optionKey) is { } error)
      {
        throw new RuleViolationException(error);
      }
    }
    finally
    {
      selection.Selection.SupersededByLevelId = null;
    }

    selection.Selection.OptionKey = optionKey;
    if (previous != optionKey)
    {
      // features granted by the previous option (and their choices) are gone
      foreach (CharacterChoice child in selection.Selection.Children.ToList())
      {
        RemoveSelection(character, child);
      }
    }

    await context.SaveChangesAsync(cancellationToken);
    return this.Evaluate(character);
  }

  /// <summary>
  ///   Replace the long rest prepared spell list of a class
  /// </summary>
  public async Task<CharacterEvaluation> SetPreparedSpellsAsync(Guid characterId, string classKey, IReadOnlyList<string> spellKeys, CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    CharacterEvaluation evaluation = this.Evaluate(character);
    CharacterClass @class = catalog.ClassesByKey.GetValueOrDefault(classKey) ?? throw new RuleViolationException($"Unknown class '{classKey}'");
    SpellcastingProfile profile = evaluation.SpellcastingProfiles.FirstOrDefault(entry => entry.ClassId == @class.Id && entry.PrepareChange == PrepareChangeEnum.LongRest)
                                  ?? throw new RuleViolationException($"The character doesn't prepare {@class.Name} spells on a long rest");

    List<string> errors = new();
    int limit = profile.PreparedCountKey == null ? 0 : (int)evaluation.Scale(profile.PreparedCountKey).AsNumber();
    if (spellKeys.Count > limit)
    {
      errors.Add($"Only {limit} {@class.Name} spells can be prepared");
    }

    if (spellKeys.Distinct().Count() != spellKeys.Count)
    {
      errors.Add("A spell can only be prepared once");
    }

    int maxLevel = evaluation.MaxSpellLevel(profile);
    IReadOnlySet<Guid> spellbook = evaluation.SpellbookSpellIds();
    List<Spell> spells = new();
    foreach (string key in spellKeys)
    {
      if (!catalog.SpellsByKey.TryGetValue(key, out Spell? spell))
      {
        errors.Add($"Unknown spell '{key}'");
        continue;
      }

      if (spell.Level == 0 || spell.Level > maxLevel)
      {
        errors.Add($"{spell.Name} is level {spell.Level}; {@class.Name} can prepare spells of level 1 to {maxLevel}");
      }

      if (profile.Pool == SpellPoolEnum.Spellbook ? !spellbook.Contains(spell.Id) : !spell.Lists.Contains(profile.SpellListKey))
      {
        errors.Add(profile.Pool == SpellPoolEnum.Spellbook ? $"{spell.Name} is not in the spellbook" : $"{spell.Name} is not on the {@class.Name} spell list");
      }

      spells.Add(spell);
    }

    if (errors.Count > 0)
    {
      throw new RuleViolationException(errors);
    }

    foreach (CharacterPreparedSpell prepared in character.PreparedSpells.Where(entry => entry.ClassId == @class.Id).ToList())
    {
      character.PreparedSpells.Remove(prepared);
      context.CharacterPreparedSpells.Remove(prepared);
    }

    foreach (Spell spell in spells)
    {
      character.PreparedSpells.Add(new CharacterPreparedSpell { Id = Guid.NewGuid(), CharacterId = character.Id, ClassId = @class.Id, SpellId = spell.Id });
    }

    await context.SaveChangesAsync(cancellationToken);
    return this.Evaluate(character);
  }

  /// <summary>
  ///   Copy a spell into the spellbook (scroll or another book)
  /// </summary>
  public async Task<CharacterEvaluation> CopyToSpellbookAsync(Guid characterId, string spellKey, string? note = null, CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    CharacterEvaluation evaluation = this.Evaluate(character);
    SpellcastingProfile profile = evaluation.SpellcastingProfiles.FirstOrDefault(entry => entry.Pool == SpellPoolEnum.Spellbook)
                                  ?? throw new RuleViolationException("The character has no spellbook");
    Spell spell = catalog.SpellsByKey.GetValueOrDefault(spellKey) ?? throw new RuleViolationException($"Unknown spell '{spellKey}'");
    if (spell.Level == 0 || spell.Level > evaluation.MaxSpellLevel(profile) || !spell.Lists.Contains(profile.SpellListKey))
    {
      throw new RuleViolationException($"{spell.Name} can't be copied into the spellbook");
    }

    if (evaluation.SpellbookSpellIds().Contains(spell.Id))
    {
      throw new RuleViolationException($"{spell.Name} is already in the spellbook");
    }

    character.SpellbookEntries.Add(new CharacterSpellbookEntry { Id = Guid.NewGuid(), CharacterId = character.Id, SpellId = spell.Id, Note = note });
    await context.SaveChangesAsync(cancellationToken);
    return this.Evaluate(character);
  }

  #endregion

  #region Equipment and Toggles

  public async Task<CharacterItem> AddItemAsync(Guid characterId, string itemKey, int quantity = 1, CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    Item item = catalog.ItemsByKey.GetValueOrDefault(itemKey) ?? throw new RuleViolationException($"Unknown item '{itemKey}'");
    CharacterItem entry = new() { Id = Guid.NewGuid(), CharacterId = character.Id, ItemId = item.Id, Quantity = quantity };
    character.Items.Add(entry);
    await context.SaveChangesAsync(cancellationToken);
    return entry;
  }

  /// <summary>
  ///   Don, doff, wield or stow an item. Equipping into an occupied slot stows the previous item
  /// </summary>
  public async Task<CharacterEvaluation> EquipAsync(Guid characterId, Guid characterItemId, EquipSlotEnum slot, CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    CharacterItem entry = character.Items.SingleOrDefault(item => item.Id == characterItemId)
                          ?? throw new RuleViolationException($"Item {characterItemId} doesn't belong to the character");
    Item item = catalog.Items[entry.ItemId];

    bool allowed = slot switch
    {
      EquipSlotEnum.None or EquipSlotEnum.Worn => true,
      EquipSlotEnum.Armor => item is Armor { Category: not Common.Enum.Item.ArmorEnum.Shield },
      EquipSlotEnum.MainHand => item is not Armor,
      EquipSlotEnum.OffHand => item is not Armor { Category: not Common.Enum.Item.ArmorEnum.Shield },
      _ => false,
    };
    if (!allowed)
    {
      throw new RuleViolationException($"{item.Name} can't be equipped as {slot}");
    }

    if (slot is EquipSlotEnum.Armor or EquipSlotEnum.MainHand or EquipSlotEnum.OffHand)
    {
      foreach (CharacterItem other in character.Items.Where(other => other.EquipSlot == slot && other.Id != entry.Id))
      {
        other.EquipSlot = EquipSlotEnum.None;
      }
    }

    entry.EquipSlot = slot;
    await context.SaveChangesAsync(cancellationToken);
    return this.Evaluate(character);
  }

  public async Task<CharacterEvaluation> AttuneAsync(Guid characterId, Guid characterItemId, bool attuned, CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    CharacterItem entry = character.Items.SingleOrDefault(item => item.Id == characterItemId)
                          ?? throw new RuleViolationException($"Item {characterItemId} doesn't belong to the character");
    if (attuned && !catalog.Items[entry.ItemId].RequiresAttunement)
    {
      throw new RuleViolationException($"{catalog.Items[entry.ItemId].Name} doesn't require attunement");
    }

    if (attuned && character.Items.Count(item => item.Attuned && item.Id != entry.Id) >= ViolationChecker.AttunementLimit)
    {
      throw new RuleViolationException($"A character can be attuned to at most {ViolationChecker.AttunementLimit} items");
    }

    entry.Attuned = attuned;
    await context.SaveChangesAsync(cancellationToken);
    return this.Evaluate(character);
  }

  /// <summary>
  ///   Switch a toggleable feature (Rage, Mage Armor ...) on or off
  /// </summary>
  public async Task<CharacterEvaluation> ToggleAsync(Guid characterId, string featureKey, bool active, CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    Feature feature = catalog.FeaturesByKey.GetValueOrDefault(featureKey) ?? throw new RuleViolationException($"Unknown feature '{featureKey}'");
    if (!feature.Toggleable)
    {
      throw new RuleViolationException($"{feature.Name} can't be toggled");
    }

    // spell effects can be toggled by anyone (cast on the character), other features only when the character has them
    if (feature.OwnerType != OwnerTypeEnum.Spell && this.Evaluate(character).Features.All(entry => entry.Feature.Id != feature.Id))
    {
      throw new RuleViolationException($"The character doesn't have {feature.Name}");
    }

    character.ActiveToggles = active
      ? character.ActiveToggles.Append(feature.Key).Distinct().ToList()
      : character.ActiveToggles.Where(entry => entry != feature.Key).ToList();
    await context.SaveChangesAsync(cancellationToken);
    return this.Evaluate(character);
  }

  /// <summary>
  ///   Pick the Armor Class calculation by its label (null = best)
  /// </summary>
  public async Task<CharacterEvaluation> ChooseArmorClassAsync(Guid characterId, string? label, CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    character.ArmorClassChoice = label;
    await context.SaveChangesAsync(cancellationToken);
    return this.Evaluate(character);
  }

  #endregion

  #region Private Methods

  /// <summary>
  ///   Apply selections one after another; each selection is validated against an evaluation that includes the previous ones,
  ///   so nested choices (the abilities of a feat chosen in the same request) work
  /// </summary>
  private void ApplySelections(Character character, Guid? levelId, IReadOnlyList<SelectionRequest> requests)
  {
    List<SelectionRequest> pending = requests.ToList();
    Dictionary<string, Guid> references = new();

    while (pending.Count > 0)
    {
      CharacterEvaluation evaluation = this.Evaluate(character);
      SelectionRequest? applied = null;

      foreach (SelectionRequest request in pending)
      {
        Guid? parentId = null;
        if (request.ParentRef != null)
        {
          if (references.TryGetValue(request.ParentRef, out Guid reference))
          {
            parentId = reference;
          }
          else if (Guid.TryParse(request.ParentRef, out Guid existing))
          {
            parentId = existing;
          }
          else
          {
            continue;
          }
        }

        Choice choice = catalog.ChoicesByKey.GetValueOrDefault(request.ChoiceKey)
                        ?? throw new RuleViolationException($"Unknown choice '{request.ChoiceKey}'");
        FeatureInstance? owner = ChoiceService.FindOwner(evaluation, choice, parentId);
        if (owner == null)
        {
          continue;
        }

        if (ChoiceService.Validate(evaluation, choice, owner, request.OptionKey) is { } error)
        {
          throw new RuleViolationException(error);
        }

        CharacterChoice selection = new()
        {
          Id = Guid.NewGuid(),
          CharacterId = character.Id,
          ChoiceId = choice.Id,
          OptionKey = request.OptionKey,
          LevelId = levelId,
          ParentId = parentId,
          CreatedAt = this.Timestamp(),
        };
        character.Choices.Add(selection);
        if (request.Ref != null)
        {
          references[request.Ref] = selection.Id;
        }

        applied = request;
        break;
      }

      if (applied == null)
      {
        throw new RuleViolationException(pending.Select(entry => $"'{entry.ChoiceKey}' is not an available choice").ToList());
      }

      pending.Remove(applied);
    }
  }

  private static void EnsureComplete(CharacterEvaluation evaluation)
  {
    List<string> errors = ChoiceService.Pending(evaluation)
      .Where(entry => !entry.Choice.Optional)
      .Select(entry => $"'{entry.Choice.Key}' ({entry.Owner.Source}) needs {entry.Remaining} more selection(s)")
      .ToList();
    if (errors.Count > 0)
    {
      throw new RuleViolationException(errors);
    }
  }

  private static bool MeetsPrimaryAbilities(CharacterEvaluation evaluation, CharacterClass @class)
  {
    IEnumerable<bool> checks = @class.PrimaryAbilities.Select(ability => evaluation.Score(ability) >= MulticlassMinimumScore);
    return @class.PrimaryAbilityMode == PrimaryAbilityModeEnum.Any ? checks.Any(entry => entry) : checks.All(entry => entry);
  }

  private static void RemoveSelection(Character character, CharacterChoice selection)
  {
    foreach (CharacterChoice child in character.Choices.Where(entry => entry.ParentId == selection.Id).ToList())
    {
      RemoveSelection(character, child);
    }

    character.Choices.Remove(selection);
  }

  /// <summary>
  ///   Run changes on the in memory character and restore it when the rules are violated
  /// </summary>
  private void Transaction(Character character, Action action)
  {
    List<CharacterLevel> levels = character.Levels.ToList();
    List<CharacterChoice> choices = character.Choices.ToList();
    Dictionary<Guid, Guid?> superseded = choices.ToDictionary(entry => entry.Id, entry => entry.SupersededByLevelId);
    try
    {
      action();
    }
    catch
    {
      foreach (CharacterLevel added in character.Levels.Except(levels).ToList())
      {
        character.Levels.Remove(added);
      }

      foreach (CharacterChoice added in character.Choices.Except(choices).ToList())
      {
        character.Choices.Remove(added);
      }

      foreach (CharacterChoice choice in choices)
      {
        choice.SupersededByLevelId = superseded[choice.Id];
      }

      throw;
    }
  }

  /// <summary>
  ///   Strictly increasing timestamps keep the order of selections made within the same request
  /// </summary>
  private DateTime Timestamp()
  {
    DateTime now = DateTime.UtcNow;
    this._lastTimestamp = now > this._lastTimestamp ? now : this._lastTimestamp.AddTicks(1);
    return this._lastTimestamp;
  }

  #endregion
}
