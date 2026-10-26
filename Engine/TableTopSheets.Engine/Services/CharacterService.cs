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
///   applied completely or not at all. Only the characters of the current user are visible
/// </summary>
public sealed class CharacterService(TableTopSheetsContext context, IRulesCatalogLoader rules, ICurrentUser user): ICharacterService
{
  #region Constants

  public const int MaxLevel = 20;
  public const int MulticlassMinimumScore = 13;

  #endregion

  #region Fields

  private DateTime _lastTimestamp = DateTime.MinValue;

  #endregion

  #region Loading

  /// <inheritdoc />
  public async Task<Character> LoadAsync(Guid characterId, CancellationToken cancellationToken = default)
    => await context.Characters
         .AsSplitQuery()
         .Include(entry => entry.Levels)
         .Include(entry => entry.Choices)
         .Include(entry => entry.Items)
         .Include(entry => entry.PreparedSpells)
         .Include(entry => entry.SpellbookEntries)
         .Include(entry => entry.ResourceUsages)
         .SingleOrDefaultAsync(entry => entry.Id == characterId && entry.OwnerId == user.UserId, cancellationToken)
       ?? throw new CharacterNotFoundException(characterId);

  /// <inheritdoc />
  public async Task<CharacterEvaluation> EvaluateAsync(Character character, CancellationToken cancellationToken = default)
  {
    CharacterEvaluation evaluation = await this.EvaluateRulesAsync(character, cancellationToken);
    await rules.DescribePendingAsync(evaluation, cancellationToken);
    return evaluation;
  }

  /// <inheritdoc />
  public async Task<CharacterSheet> BuildSheetAsync(Guid characterId, CancellationToken cancellationToken = default)
    => CharacterSheetBuilder.Build(await this.EvaluateRulesAsync(await this.LoadAsync(characterId, cancellationToken), cancellationToken));

  #endregion

  #region Creation

  /// <inheritdoc />
  public async Task<Character> CreateAsync(CreateCharacterRequest request, CancellationToken cancellationToken = default)
  {
    Species species = await context.Species
      .AsNoTracking()
      .SingleOrDefaultAsync(entry => entry.Key == request.SpeciesKey, cancellationToken)
                      ?? throw new RuleViolationException($"Unknown species '{request.SpeciesKey}'");
    Background background = await context.Backgrounds
      .AsNoTracking()
      .SingleOrDefaultAsync(entry => entry.Key == request.BackgroundKey, cancellationToken)
                            ?? throw new RuleViolationException($"Unknown background '{request.BackgroundKey}'");

    byte Score(string ability) => request.Abilities.TryGetValue(ability, out byte score)
      ? score
      : throw new RuleViolationException($"Missing ability score '{ability}'");

    Character character = new()
    {
      Id = Guid.NewGuid(),
      OwnerId = user.UserId,
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

    await this.ApplySelectionsAsync(character, null, request.Selections, cancellationToken);
    if (request.StartingEquipment != null)
    {
      await this.TakeStartingEquipmentAsync(character, StartingEquipmentSourceEnum.Background, request.StartingEquipment, cancellationToken);
    }

    if (!request.AllowIncomplete)
    {
      EnsureComplete(await this.EvaluateRulesAsync(character, cancellationToken));
    }

    context.Characters.Add(character);
    await context.SaveChangesAsync(cancellationToken);
    return character;
  }

  /// <inheritdoc />
  public async Task<CharacterEvaluation> UpdateDetailsAsync(Guid characterId, CharacterDetailsUpdate update, CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    if (update.Name is { } name)
    {
      character.Name = string.IsNullOrWhiteSpace(name) ? throw new RuleViolationException("A character needs a name") : name;
    }

    if (update.Experience is < 0)
    {
      throw new RuleViolationException("Experience can't be negative");
    }

    character.Alignment = update.Alignment ?? character.Alignment;
    character.Appearance = update.Appearance ?? character.Appearance;
    character.Backstory = update.Backstory ?? character.Backstory;
    character.Experience = update.Experience ?? character.Experience;
    await context.SaveChangesAsync(cancellationToken);
    return await this.EvaluateAsync(character, cancellationToken);
  }

  /// <inheritdoc />
  public async Task DeleteAsync(Guid characterId, CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    context.Characters.Remove(character);
    await context.SaveChangesAsync(cancellationToken);
  }

  /// <inheritdoc />
  public async Task<CharacterEvaluation> SelectAsync(Guid characterId, IReadOnlyList<SelectionRequest> selections, CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    await this.SelectAsync(character, selections, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);
    return await this.EvaluateAsync(character, cancellationToken);
  }

  /// <inheritdoc />
  public Task SelectAsync(Character character, IReadOnlyList<SelectionRequest> selections, CancellationToken cancellationToken = default)
  {
    Guid? levelId = character.Levels.MaxBy(entry => entry.Number)?.Id;
    return this.TransactionAsync(character, () => this.ApplySelectionsAsync(character, levelId, selections, cancellationToken));
  }

  /// <inheritdoc />
  public async Task EnsureCompleteAsync(Character character, CancellationToken cancellationToken = default)
    => EnsureComplete(await this.EvaluateRulesAsync(character, cancellationToken));

  /// <inheritdoc />
  public Task<T> ValidateSelectionsAsync<T>(Guid characterId, IReadOnlyList<SelectionRequest> selections, Func<CharacterEvaluation, T> project, CancellationToken cancellationToken = default)
    => this.DryRunAsync(characterId, character => this.SelectAsync(character, selections, cancellationToken), project, cancellationToken);

  /// <inheritdoc />
  public async Task<CharacterEvaluation> DeselectAsync(Guid characterId, Guid selectionId, CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    CharacterChoice selection = character.Choices.SingleOrDefault(entry => entry.Id == selectionId)
                                ?? throw new RuleViolationException($"Selection {selectionId} doesn't exist");
    Guid? latest = character.Levels.MaxBy(entry => entry.Number)?.Id;
    if (selection.LevelId != latest)
    {
      throw new RuleViolationException("Only selections of the latest level can be removed; undo the level instead");
    }

    if (selection.SupersededByLevelId != null)
    {
      throw new RuleViolationException($"Selection {selectionId} was replaced and is no longer active");
    }

    // a replacement made on this level brings the replaced selection back
    if (selection.LevelId != null)
    {
      CharacterChoice? replaced = character.Choices
        .Where(entry => entry.SupersededByLevelId == selection.LevelId && entry.ChoiceId == selection.ChoiceId && entry.ParentId == selection.ParentId)
        .SingleOrDefault();
      replaced?.SupersededByLevelId = null;
    }

    RemoveSelection(character, selection);
    await context.SaveChangesAsync(cancellationToken);
    return await this.EvaluateAsync(character, cancellationToken);
  }

  #endregion

  #region Level Up

  /// <inheritdoc />
  public async Task<IReadOnlyList<PendingChoice>> PreviewLevelUpAsync(Guid characterId, string classKey, string? subclassKey = null, CancellationToken cancellationToken = default)
    => await this.PreviewLevelUpAsync(characterId, classKey, subclassKey, evaluation => ChoiceService
      .Pending(evaluation)
      .ToList(), cancellationToken);

  /// <inheritdoc />
  public Task<T> PreviewLevelUpAsync<T>(Guid characterId, string classKey, string? subclassKey, Func<CharacterEvaluation, T> project, CancellationToken cancellationToken = default)
    => this.DryRunAsync
    (
      characterId,
      async character => character.Levels.Add(await this.CreateLevelAsync(character, new LevelUpRequest { ClassKey = classKey, SubclassKey = subclassKey }, cancellationToken)),
      project,
      cancellationToken
    );

  /// <inheritdoc />
  public Task<T> ValidateLevelUpAsync<T>(Guid characterId, LevelUpRequest request, Func<CharacterEvaluation, T> project, CancellationToken cancellationToken = default)
    => this.DryRunAsync(characterId, character => this.LevelUpAsync(character, request, cancellationToken), project, cancellationToken);

  /// <inheritdoc />
  public async Task<CharacterEvaluation> LevelUpAsync(Guid characterId, LevelUpRequest request, CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    await this.LevelUpAsync(character, request, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);
    return await this.EvaluateAsync(character, cancellationToken);
  }

  /// <inheritdoc />
  public async Task LevelUpAsync(Character character, LevelUpRequest request, CancellationToken cancellationToken = default)
  {
    CharacterLevel level = await this.CreateLevelAsync(character, request, cancellationToken);
    await this.TransactionAsync(character, async () =>
    {
      if (request.Replacements
          .GroupBy(entry => character.Choices.SingleOrDefault(choice => choice.Id == entry.SelectionId)?.ChoiceId)
          .Any(group => group.Count() > 1))
      {
        throw new RuleViolationException("Only one selection per choice can be replaced on a level up");
      }

      character.Levels.Add(level);
      CharacterEvaluation evaluation = await this.EvaluateRulesAsync(character, cancellationToken);

      foreach (ReplacementRequest replacement in request.Replacements)
      {
        await this.ReplaceAsync(character, level, replacement, evaluation, cancellationToken);
        evaluation = await this.EvaluateRulesAsync(character, cancellationToken);
      }

      await this.ApplySelectionsAsync(character, level.Id, request.Selections, cancellationToken);
      if (request.StartingEquipment != null)
      {
        if (level.Number != 1)
        {
          throw new RuleViolationException("Class starting equipment comes only with the first level of a character");
        }

        await this.TakeStartingEquipmentAsync(character, StartingEquipmentSourceEnum.Class, request.StartingEquipment, cancellationToken);
      }

      if (!request.AllowIncomplete)
      {
        EnsureComplete(await this.EvaluateRulesAsync(character, cancellationToken));
      }
    });
  }

  private async Task<CharacterLevel> CreateLevelAsync(Character character, LevelUpRequest request, CancellationToken cancellationToken)
  {
    CharacterEvaluation current = await this.EvaluateRulesAsync(character, cancellationToken);
    CharacterClass @class = current.Catalog.ClassesByKey.GetValueOrDefault(request.ClassKey)
                            ?? throw new RuleViolationException($"Unknown class '{request.ClassKey}'");
    if (character.Levels.Count >= MaxLevel)
    {
      throw new RuleViolationException($"A character can't exceed level {MaxLevel}");
    }

    int classLevel = current.ClassLevel(@class.Id) + 1;

    // multiclassing: 13+ in the primary abilities of every current class and of the new class
    if (classLevel == 1 && current.TotalLevel > 0)
    {
      List<string> errors = current.ClassLevels.Keys
        .Select(id => current.Catalog.Classes[id])
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
        : current.Catalog.SubclassesByKey.GetValueOrDefault(request.SubclassKey);
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

  private async Task ReplaceAsync(Character character, CharacterLevel level, ReplacementRequest replacement, CharacterEvaluation evaluation, CancellationToken cancellationToken)
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
    CharacterEvaluation withoutOld = await this.EvaluateRulesAsync(character, cancellationToken);
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

  /// <inheritdoc />
  public async Task<CharacterEvaluation> UndoLastLevelAsync(Guid characterId, CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    CharacterLevel level = character.Levels.MaxBy(entry => entry.Number)
                           ?? throw new RuleViolationException("The character has no level to undo");

    foreach (CharacterChoice choice in character.Choices
      .Where(entry => entry.LevelId == level.Id)
      .ToList())
    {
      RemoveSelection(character, choice);
    }

    foreach (CharacterChoice choice in character.Choices.Where(entry => entry.SupersededByLevelId == level.Id))
    {
      choice.SupersededByLevelId = null;
    }

    // a new first class offers its starting equipment again (the items already taken stay in the inventory)
    if (level.Number == 1)
    {
      character.ClassEquipment = null;
    }

    character.Levels.Remove(level);
    context.CharacterLevels.Remove(level);
    await context.SaveChangesAsync(cancellationToken);
    return await this.EvaluateAsync(character, cancellationToken);
  }

  #endregion

  #region Rest Changes

  /// <inheritdoc />
  public async Task<CharacterEvaluation> ReplaceOnRestAsync(Guid characterId, Guid selectionId, string optionKey, CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    CharacterEvaluation evaluation = await this.EvaluateRulesAsync(character, cancellationToken);
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
      CharacterEvaluation withoutOld = await this.EvaluateRulesAsync(character, cancellationToken);
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
    return await this.EvaluateAsync(character, cancellationToken);
  }

  /// <inheritdoc />
  public async Task<CharacterEvaluation> SetPreparedSpellsAsync(Guid characterId, string classKey, IReadOnlyList<string> spellKeys, CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    CharacterEvaluation evaluation = await this.EvaluateRulesAsync(character, cancellationToken);
    CharacterClass @class = evaluation.Catalog.ClassesByKey.GetValueOrDefault(classKey) ?? throw new RuleViolationException($"Unknown class '{classKey}'");
    SpellcastingProfile profile = evaluation.SpellcastingProfiles.FirstOrDefault(entry => entry.ClassId == @class.Id && entry.PrepareChange == PrepareChangeEnum.LongRest)
                                  ?? throw new RuleViolationException($"The character doesn't prepare {@class.Name} spells on a long rest");

    List<string> errors = new();
    int limit = profile.PreparedCountKey == null ? 0 : (int)evaluation
      .Scale(profile.PreparedCountKey)
      .AsNumber();
    if (spellKeys.Count > limit)
    {
      errors.Add($"Only {limit} {@class.Name} spells can be prepared");
    }

    if (spellKeys
      .Distinct()
      .Count() != spellKeys.Count)
    {
      errors.Add("A spell can only be prepared once");
    }

    int maxLevel = evaluation.MaxSpellLevel(profile);
    IReadOnlySet<Guid> spellbook = evaluation.SpellbookSpellIds();
    List<string> keys = spellKeys
      .Distinct()
      .ToList();
    Dictionary<string, Spell> known = await context.Spells
      .AsNoTracking()
      .Where(entry => keys.Contains(entry.Key))
      .ToDictionaryAsync(entry => entry.Key, cancellationToken);
    List<Spell> spells = new();
    foreach (string key in spellKeys)
    {
      if (!known.TryGetValue(key, out Spell? spell))
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

    foreach (CharacterPreparedSpell prepared in character.PreparedSpells
      .Where(entry => entry.ClassId == @class.Id)
      .ToList())
    {
      character.PreparedSpells.Remove(prepared);
      context.CharacterPreparedSpells.Remove(prepared);
    }

    foreach (Spell spell in spells)
    {
      character.PreparedSpells.Add(new CharacterPreparedSpell { Id = Guid.NewGuid(), CharacterId = character.Id, ClassId = @class.Id, SpellId = spell.Id });
    }

    await context.SaveChangesAsync(cancellationToken);
    return await this.EvaluateAsync(character, cancellationToken);
  }

  /// <inheritdoc />
  public async Task<CharacterEvaluation> CopyToSpellbookAsync(Guid characterId, string spellKey, string? note = null, CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    CharacterEvaluation evaluation = await this.EvaluateRulesAsync(character, cancellationToken);
    SpellcastingProfile profile = evaluation.SpellcastingProfiles.FirstOrDefault(entry => entry.Pool == SpellPoolEnum.Spellbook)
                                  ?? throw new RuleViolationException("The character has no spellbook");
    Spell spell = await context.Spells
      .AsNoTracking()
      .SingleOrDefaultAsync(entry => entry.Key == spellKey, cancellationToken)
                  ?? throw new RuleViolationException($"Unknown spell '{spellKey}'");
    if (spell.Level == 0 || spell.Level > evaluation.MaxSpellLevel(profile) || !spell.Lists.Contains(profile.SpellListKey))
    {
      throw new RuleViolationException($"{spell.Name} can't be copied into the spellbook");
    }

    if (evaluation
      .SpellbookSpellIds()
      .Contains(spell.Id))
    {
      throw new RuleViolationException($"{spell.Name} is already in the spellbook");
    }

    character.SpellbookEntries.Add(new CharacterSpellbookEntry { Id = Guid.NewGuid(), CharacterId = character.Id, SpellId = spell.Id, Note = note });
    await context.SaveChangesAsync(cancellationToken);
    return await this.EvaluateAsync(character, cancellationToken);
  }

  #endregion

  #region Inventory

  /// <inheritdoc />
  public async Task<CharacterItem> AddItemAsync(Guid characterId, string itemKey, int quantity = 1, CancellationToken cancellationToken = default, bool purchase = false)
  {
    if (quantity < 1)
    {
      throw new RuleViolationException("The quantity has to be at least 1");
    }

    Character character = await this.LoadAsync(characterId, cancellationToken);
    Item item = await context.Items
      .AsNoTracking()
      .SingleOrDefaultAsync(entry => entry.Key == itemKey, cancellationToken)
                ?? throw new RuleViolationException($"Unknown item '{itemKey}'");
    // items sold in bundles (20 arrows) cost per started bundle; the quantity counts pieces
    if (purchase)
    {
      Purse.Pay(character.PlayState, (long)item.CostCp * ((quantity + item.Bundle - 1) / item.Bundle));
    }

    CharacterItem entry = new() { Id = Guid.NewGuid(), CharacterId = character.Id, ItemId = item.Id, Quantity = quantity };
    character.Items.Add(entry);
    await context.SaveChangesAsync(cancellationToken);
    return entry;
  }

  /// <inheritdoc />
  public async Task<CharacterEvaluation> RemoveItemAsync(Guid characterId, Guid characterItemId, int? quantity = null, bool sell = false, CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    CharacterItem entry = character.Items.SingleOrDefault(item => item.Id == characterItemId)
                          ?? throw new RuleViolationException($"Item {characterItemId} doesn't belong to the character");
    int removed = quantity ?? entry.Quantity;
    if (removed < 1 || removed > entry.Quantity)
    {
      throw new RuleViolationException($"Between 1 and {entry.Quantity} can be removed");
    }

    if (sell)
    {
      Item item = await context.Items
        .AsNoTracking()
        .SingleAsync(rule => rule.Id == entry.ItemId, cancellationToken);
      Purse.Add(character.PlayState, (long)item.CostCp * removed / item.Bundle / 2);
    }

    // the rest of a stack keeps its slot and attunement; removing all of it removes the entry
    if (removed < entry.Quantity)
    {
      entry.Quantity -= removed;
    }
    else
    {
      character.Items.Remove(entry);
      context.CharacterItems.Remove(entry);
    }

    await context.SaveChangesAsync(cancellationToken);
    return await this.EvaluateAsync(character, cancellationToken);
  }

  /// <inheritdoc />
  public async Task<CharacterEvaluation> AddToStackAsync(Guid characterId, Guid characterItemId, int quantity, CancellationToken cancellationToken = default)
  {
    if (quantity < 1)
    {
      throw new RuleViolationException("The quantity has to be at least 1");
    }

    Character character = await this.LoadAsync(characterId, cancellationToken);
    CharacterItem entry = character.Items.SingleOrDefault(item => item.Id == characterItemId)
                          ?? throw new RuleViolationException($"Item {characterItemId} doesn't belong to the character");
    entry.Quantity += quantity;
    await context.SaveChangesAsync(cancellationToken);
    return await this.EvaluateAsync(character, cancellationToken);
  }

  #endregion

  #region Starting Equipment

  /// <inheritdoc />
  public async Task<IReadOnlyList<StartingEquipmentOffer>> GetStartingEquipmentAsync(Guid characterId, CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    Dictionary<string, string> answered = Answered(await this.EvaluateRulesAsync(character, cancellationToken));
    List<StartingEquipmentOffer> offers = new();
    foreach (StartingEquipmentSourceEnum source in Enum.GetValues<StartingEquipmentSourceEnum>())
    {
      if (await this.StartingEquipmentOwnerAsync(character, source, cancellationToken) is not { } owner)
      {
        continue;
      }

      Dictionary<string, string> sameAs = owner.Options
        .SelectMany(option => option.Items)
        .Select(entry => entry.SameAs)
        .OfType<string>()
        .Distinct()
        .Where(answered.ContainsKey)
        .ToDictionary(key => key, key => answered[key]);
      offers.Add(new StartingEquipmentOffer(source, owner.Key, owner.Name, owner.Options, Taken(character, source), sameAs));
    }

    return offers;
  }

  /// <inheritdoc />
  public async Task<CharacterEvaluation> TakeStartingEquipmentAsync(
    Guid characterId,
    StartingEquipmentSourceEnum source,
    StartingEquipmentRequest request,
    CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    await this.TakeStartingEquipmentAsync(character, source, request, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);
    return await this.EvaluateAsync(character, cancellationToken);
  }

  private async Task TakeStartingEquipmentAsync(Character character, StartingEquipmentSourceEnum source, StartingEquipmentRequest request, CancellationToken cancellationToken)
  {
    StartingEquipmentOwner owner = await this.StartingEquipmentOwnerAsync(character, source, cancellationToken)
                                   ?? throw new RuleViolationException("The character has no class yet; take the first level before its starting equipment");
    if (Taken(character, source) is { } taken)
    {
      throw new RuleViolationException($"The starting equipment of {owner.Name} was already taken (option {taken.ToUpperInvariant()})");
    }

    StartingEquipmentOption option = owner.Options.FirstOrDefault(entry => string.Equals(entry.Key, request.OptionKey, StringComparison.OrdinalIgnoreCase))
                                     ?? throw new RuleViolationException($"'{request.OptionKey}' is not a starting equipment option of {owner.Name}");

    // every entry is a fixed item, the player's pick, or what its "same as above" choice already selected
    Dictionary<string, string> answered = Answered(await this.EvaluateRulesAsync(character, cancellationToken));
    List<SelectionRequest> selections = new();
    List<(string ItemKey, StartingEquipmentEntry Entry)> picked = new();
    List<string> errors = new();
    foreach (StartingEquipmentEntry entry in option.Items)
    {
      if (entry.ItemKey != null)
      {
        picked.Add((entry.ItemKey, entry));
        continue;
      }

      string name = entry.Name ?? entry.Key;
      string? pick = request.Picks?.GetValueOrDefault(entry.Key);
      string? same = entry.SameAs == null ? null : answered.GetValueOrDefault(entry.SameAs);
      if (same != null && entry.Options.Contains(same))
      {
        if (pick != null && pick != same)
        {
          errors.Add($"'{name}' has to be the one chosen for '{entry.SameAs}' ({same})");
        }

        picked.Add((same, entry));
      }
      else if (pick == null)
      {
        errors.Add($"Pick an item for '{name}'");
      }
      else if (!entry.Options.Contains(pick))
      {
        errors.Add($"'{pick}' is not an option for '{name}'");
      }
      else
      {
        picked.Add((pick, entry));

        // the pick of a "same as above" entry answers that choice too
        if (entry.SameAs != null && same == null)
        {
          selections.Add(new SelectionRequest(entry.SameAs, pick));
        }
      }
    }

    if (errors.Count > 0)
    {
      throw new RuleViolationException(errors);
    }

    await this.ApplySelectionsAsync(character, owner.LevelId, selections, cancellationToken);

    List<string> keys = picked
      .Select(entry => entry.ItemKey)
      .Distinct()
      .ToList();
    Dictionary<string, Guid> items = await context.Items
      .Where(entry => keys.Contains(entry.Key))
      .ToDictionaryAsync(entry => entry.Key, entry => entry.Id, cancellationToken);
    foreach ((string itemKey, StartingEquipmentEntry entry) in picked)
    {
      character.Items.Add(new CharacterItem
      {
        Id = Guid.NewGuid(),
        CharacterId = character.Id,
        ItemId = items.TryGetValue(itemKey, out Guid id) ? id : throw new RuleViolationException($"Unknown item '{itemKey}'"),
        Quantity = entry.Quantity,
        Notes = entry.Note,
      });
    }

    Purse.Add(character.PlayState, option.CoinsCp);
    if (source == StartingEquipmentSourceEnum.Background)
    {
      character.BackgroundEquipment = option.Key;
    }
    else
    {
      character.ClassEquipment = option.Key;
    }
  }

  /// <summary>
  ///   Background or first class with its starting equipment; null for a class before the first level
  /// </summary>
  private async Task<StartingEquipmentOwner?> StartingEquipmentOwnerAsync(Character character, StartingEquipmentSourceEnum source, CancellationToken cancellationToken)
  {
    if (source == StartingEquipmentSourceEnum.Background)
    {
      return await context.Backgrounds
        .AsNoTracking()
        .Where(entry => entry.Id == character.BackgroundId)
        .Select(entry => new StartingEquipmentOwner(entry.Key, entry.Name, entry.StartingEquipment, null))
        .SingleOrDefaultAsync(cancellationToken);
    }

    CharacterLevel? first = character.Levels.SingleOrDefault(entry => entry.Number == 1);
    if (first == null)
    {
      return null;
    }

    return await context.Classes
      .AsNoTracking()
      .Where(entry => entry.Id == first.ClassId)
      .Select(entry => new StartingEquipmentOwner(entry.Key, entry.Name, entry.StartingEquipment, first.Id))
      .SingleOrDefaultAsync(cancellationToken);
  }

  private static string? Taken(Character character, StartingEquipmentSourceEnum source)
    => source == StartingEquipmentSourceEnum.Background ? character.BackgroundEquipment : character.ClassEquipment;

  /// <summary>
  ///   First selected option per choice key (the tool of a tool proficiency)
  /// </summary>
  private static Dictionary<string, string> Answered(CharacterEvaluation evaluation)
    => evaluation.Selections
      .GroupBy(entry => entry.Choice.Key)
      .ToDictionary(group => group.Key, group => group.First().Selection.OptionKey);

  /// <param name="Key">key of the background or class</param>
  /// <param name="Name">name of the background or class</param>
  /// <param name="Options">its starting equipment</param>
  /// <param name="LevelId">level the selections of the owner belong to (the first level for a class)</param>
  private sealed record StartingEquipmentOwner(string Key, string Name, List<StartingEquipmentOption> Options, Guid? LevelId);

  #endregion

  #region Coins

  /// <inheritdoc />
  public Task<CharacterEvaluation> SetCoinsAsync(Guid characterId, int copper, int silver, int electrum, int gold, int platinum, CancellationToken cancellationToken = default)
    => this.UpdatePurseAsync(characterId, purse => Purse.Set(purse, copper, silver, electrum, gold, platinum), cancellationToken);

  /// <inheritdoc />
  public Task<CharacterEvaluation> ChangeCoinsAsync(Guid characterId, int copper, int silver, int electrum, int gold, int platinum, CancellationToken cancellationToken = default)
    => this.UpdatePurseAsync(characterId, purse => Purse.Change(purse, copper, silver, electrum, gold, platinum), cancellationToken);

  /// <inheritdoc />
  public Task<CharacterEvaluation> PayAsync(Guid characterId, long amountCp, CancellationToken cancellationToken = default)
    => this.UpdatePurseAsync(characterId, purse => Purse.Pay(purse, amountCp), cancellationToken);

  private async Task<CharacterEvaluation> UpdatePurseAsync(Guid characterId, Action<CharacterPlayState> update, CancellationToken cancellationToken)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    update(character.PlayState);
    await context.SaveChangesAsync(cancellationToken);
    return await this.EvaluateAsync(character, cancellationToken);
  }

  #endregion

  #region Equipment and Toggles

  /// <inheritdoc />
  public async Task<CharacterEvaluation> EquipAsync(Guid characterId, Guid characterItemId, EquipSlotEnum slot, CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    CharacterItem entry = character.Items.SingleOrDefault(item => item.Id == characterItemId)
                          ?? throw new RuleViolationException($"Item {characterItemId} doesn't belong to the character");
    Item item = await context.Items
      .AsNoTracking()
      .SingleAsync(rule => rule.Id == entry.ItemId, cancellationToken);

    bool allowed = slot switch
    {
      EquipSlotEnum.None or EquipSlotEnum.Worn => true,
      EquipSlotEnum.Armor => item is Armor { Category: not Common.Enum.Item.ArmorEnum.Shield },
      EquipSlotEnum.MainHand => item is not Armor,
      EquipSlotEnum.OffHand => item is not Armor { Category: not Common.Enum.Item.ArmorEnum.Shield },
      EquipSlotEnum.Ready => item is Weapon,
      _ => false,
    };
    if (!allowed)
    {
      throw new RuleViolationException($"{item.Name} can't be equipped as {slot}");
    }

    if (slot is EquipSlotEnum.Armor or EquipSlotEnum.MainHand or EquipSlotEnum.OffHand)
    {
      List<Guid> others = character.Items
        .Where(other => other.EquipSlot == slot && other.Id != entry.Id)
        .Select(other => other.ItemId)
        .ToList();
      Dictionary<Guid, Item> displaced = await context.Items
        .AsNoTracking()
        .Where(rule => others.Contains(rule.Id))
        .ToDictionaryAsync(rule => rule.Id, cancellationToken);
      foreach (CharacterItem other in character.Items.Where(other => other.EquipSlot == slot && other.Id != entry.Id))
      {
        other.EquipSlot = slot != EquipSlotEnum.Armor && displaced[other.ItemId] is Weapon ? EquipSlotEnum.Ready : EquipSlotEnum.None;
      }
    }

    entry.EquipSlot = slot;
    await context.SaveChangesAsync(cancellationToken);
    return await this.EvaluateAsync(character, cancellationToken);
  }

  /// <inheritdoc />
  public async Task<CharacterEvaluation> AttuneAsync(Guid characterId, Guid characterItemId, bool attuned, CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    CharacterItem entry = character.Items.SingleOrDefault(item => item.Id == characterItemId)
                          ?? throw new RuleViolationException($"Item {characterItemId} doesn't belong to the character");
    Item item = await context.Items
      .AsNoTracking()
      .SingleAsync(rule => rule.Id == entry.ItemId, cancellationToken);
    if (attuned && !item.RequiresAttunement)
    {
      throw new RuleViolationException($"{item.Name} doesn't require attunement");
    }

    if (attuned && character.Items.Count(item => item.Attuned && item.Id != entry.Id) >= ViolationChecker.AttunementLimit)
    {
      throw new RuleViolationException($"A character can be attuned to at most {ViolationChecker.AttunementLimit} items");
    }

    entry.Attuned = attuned;
    await context.SaveChangesAsync(cancellationToken);
    return await this.EvaluateAsync(character, cancellationToken);
  }

  /// <inheritdoc />
  public async Task<CharacterEvaluation> ToggleAsync(Guid characterId, string featureKey, bool active, CancellationToken cancellationToken = default)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    Feature feature = await context.Features
      .AsNoTracking()
      .SingleOrDefaultAsync(entry => entry.Key == featureKey, cancellationToken)
                      ?? throw new RuleViolationException($"Unknown feature '{featureKey}'");
    if (!feature.Toggleable)
    {
      throw new RuleViolationException($"{feature.Name} can't be toggled");
    }

    // spell effects can be toggled by anyone (cast on the character), other features only when the character has them
    if (feature.OwnerType != OwnerTypeEnum.Spell && (await this.EvaluateRulesAsync(character, cancellationToken)).Features.All(entry => entry.Feature.Id != feature.Id))
    {
      throw new RuleViolationException($"The character doesn't have {feature.Name}");
    }

    character.ActiveToggles = active
      ? character.ActiveToggles
        .Append(feature.Key)
        .Distinct()
        .ToList()
      : character.ActiveToggles
        .Where(entry => entry != feature.Key)
        .ToList();
    await context.SaveChangesAsync(cancellationToken);
    return await this.EvaluateAsync(character, cancellationToken);
  }

  #endregion

  #region Private Methods

  private async Task<CharacterEvaluation> EvaluateRulesAsync(Character character, CancellationToken cancellationToken)
    => new(await rules.LoadAsync(character, cancellationToken), character);

  /// <summary>
  ///   Apply a change to a loaded character, project the evaluation and forget the change (nothing reaches the database)
  /// </summary>
  private async Task<T> DryRunAsync<T>(Guid characterId, Func<Character, Task> change, Func<CharacterEvaluation, T> project, CancellationToken cancellationToken)
  {
    Character character = await this.LoadAsync(characterId, cancellationToken);
    try
    {
      await change(character);
      return project(await this.EvaluateAsync(character, cancellationToken));
    }
    finally
    {
      context.ChangeTracker.Clear();
    }
  }

  /// <summary>
  ///   Apply selections one after another; each selection is validated against an evaluation that includes the previous ones,
  ///   so nested choices (the abilities of a feat chosen in the same request) work
  /// </summary>
  private async Task ApplySelectionsAsync(Character character, Guid? levelId, IReadOnlyList<SelectionRequest> requests, CancellationToken cancellationToken)
  {
    List<SelectionRequest> pending = requests.ToList();
    Dictionary<string, Guid> references = new();

    // a choice of a feature granted by an earlier selection is only part of the rules once that selection is made
    List<string> choiceKeys = pending
      .Select(entry => entry.ChoiceKey)
      .Distinct()
      .ToList();
    HashSet<string> known = (await context.Choices
      .Where(entry => choiceKeys.Contains(entry.Key))
      .Select(entry => entry.Key)
      .ToListAsync(cancellationToken)).ToHashSet();
    if (choiceKeys.FirstOrDefault(entry => !known.Contains(entry)) is { } unknown)
    {
      throw new RuleViolationException($"Unknown choice '{unknown}'");
    }

    while (pending.Count > 0)
    {
      CharacterEvaluation evaluation = await this.EvaluateRulesAsync(character, cancellationToken);
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

        Choice? choice = evaluation.Catalog.ChoicesByKey.GetValueOrDefault(request.ChoiceKey);
        FeatureInstance? owner = choice == null ? null : ChoiceService.FindOwner(evaluation, choice, parentId);
        if (choice == null || owner == null)
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
        throw new RuleViolationException(pending
          .Select(entry => $"'{entry.ChoiceKey}' is not an available choice")
          .ToList());
      }

      pending.Remove(applied);
    }
  }

  private static void EnsureComplete(CharacterEvaluation evaluation)
  {
    List<string> errors = ChoiceService
      .Pending(evaluation)
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
    foreach (CharacterChoice child in character.Choices
      .Where(entry => entry.ParentId == selection.Id)
      .ToList())
    {
      RemoveSelection(character, child);
    }

    character.Choices.Remove(selection);
  }

  /// <summary>
  ///   Run changes on the in memory character and restore it when the rules are violated
  /// </summary>
  private async Task TransactionAsync(Character character, Func<Task> action)
  {
    List<CharacterLevel> levels = character.Levels.ToList();
    List<CharacterChoice> choices = character.Choices.ToList();
    Dictionary<Guid, Guid?> superseded = choices.ToDictionary(entry => entry.Id, entry => entry.SupersededByLevelId);
    try
    {
      await action();
    }
    catch
    {
      foreach (CharacterLevel added in character.Levels
        .Except(levels)
        .ToList())
      {
        character.Levels.Remove(added);
      }

      foreach (CharacterChoice added in character.Choices
        .Except(choices)
        .ToList())
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
