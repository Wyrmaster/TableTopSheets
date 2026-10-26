using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Key;
using TableTopSheets.Engine.Expressions;

namespace TableTopSheets.Engine.Evaluation;

public sealed partial class CharacterEvaluation
{
  #region Fields

  /// <summary>
  ///   Value of <c>ac.dexcap</c> when the worn armor doesn't limit the Dexterity modifier
  /// </summary>
  public const int NoDexCap = 99;

  private readonly Dictionary<string, double> _statCache = new();
  private readonly Dictionary<string, List<StatSource>> _statSources = new();
  private readonly HashSet<string> _inProgress = new();

  #endregion

  #region Stat Methods

  /// <summary>
  ///   Value of a stat. Stats are resolved lazily and memoized; expressions may reference other stats
  /// </summary>
  public double Stat(string key)
  {
    if (this._statCache.TryGetValue(key, out double cached))
    {
      return cached;
    }

    if (!this._inProgress.Add(key))
    {
      throw new ExpressionException($"Circular stat reference: {string.Join(" → ", this._inProgress)} → {key}");
    }

    try
    {
      double value = this.ComputeStat(key);
      this._statCache[key] = value;
      return value;
    }
    finally
    {
      this._inProgress.Remove(key);
    }
  }

  /// <summary>
  ///   Effects that contributed to a stat
  /// </summary>
  public IReadOnlyList<StatSource> Sources(string key)
  {
    this.Stat(key);
    return this._statSources.TryGetValue(key, out List<StatSource>? sources) ? sources : [];
  }

  /// <summary>
  ///   All stat keys targeted by an effect that start with the prefix and are non zero
  /// </summary>
  public IEnumerable<string> StatKeysWithPrefix(string prefix)
    => this._effects.Keys.Where(key => key.StartsWith(prefix, StringComparison.Ordinal) && this.Stat(key) != 0).Order();

  /// <summary>
  ///   Applicable <see cref="EffectOperationEnum.Candidate"/> formulas of a stat
  /// </summary>
  public IEnumerable<(string Label, double Value)> Candidates(string key)
    => this.ApplicableEffects(key)
      .Where(entry => entry.Effect.Operation == EffectOperationEnum.Candidate)
      .Select(entry => (entry.Label, Expression.Parse(entry.ValueExpr).EvaluateNumber(this)));

  public int Score(string ability) => (int)this.Stat($"ability.{ability}.score");

  public int Modifier(string ability) => (int)Math.Floor((this.Score(ability) - 10) / 2.0);

  #endregion

  #region Private Methods

  private IEnumerable<AppliedEffect> ApplicableEffects(string key)
    => this._effects.TryGetValue(key, out List<AppliedEffect>? effects)
      ? effects.Where(entry => entry.Effect.ConditionExpr == null || Expression.Parse(entry.Effect.ConditionExpr).EvaluateBoolean(this))
      : [];

  private double DefaultValue(string key)
  {
    string[] parts = key.Split('.');
    return parts switch
    {
      ["ability", var ability, "score"] => ability switch
      {
        "str" => this.Character.Strength,
        "dex" => this.Character.Dexterity,
        "con" => this.Character.Constitution,
        "int" => this.Character.Intelligence,
        "wis" => this.Character.Wisdom,
        "cha" => this.Character.Charisma,
        _ => 10,
      },
      ["ability", _, "max"] => 20,
      ["ac", "dexcap"] => this.WornArmor?.DexCap ?? NoDexCap,
      ["size"] => (double)Common.Enum.SizeEnum.Medium,
      _ => 0,
    };
  }

  private double ComputeStat(string key)
  {
    List<StatSource> sources = new();
    this._statSources[key] = sources;

    List<(AppliedEffect Effect, double Value)> effects = this.ApplicableEffects(key)
      .Where(entry => entry.Effect.Operation != EffectOperationEnum.Candidate)
      .Select(entry => (entry, Expression.Parse(entry.ValueExpr).EvaluateNumber(this)))
      .ToList();

    double value = this.DefaultValue(key);

    List<(AppliedEffect Effect, double Value)> sets = effects.Where(entry => entry.Effect.Effect.Operation == EffectOperationEnum.Set).ToList();
    if (sets.Count > 0)
    {
      (AppliedEffect effect, double setValue) = sets.MaxBy(entry => entry.Value);
      value = setValue;
      sources.Add(new(effect.Label, "Set", setValue));
    }

    // additions stack, except effects of the same stack group: only the highest of those counts
    List<(AppliedEffect Effect, double Value)> additions = effects.Where(entry => entry.Effect.Effect.Operation == EffectOperationEnum.Add).ToList();
    IEnumerable<(AppliedEffect Effect, double Value)> counted = additions
      .Where(entry => entry.Effect.Effect.StackGroup == null)
      .Concat(additions
        .Where(entry => entry.Effect.Effect.StackGroup != null)
        .GroupBy(entry => entry.Effect.Effect.StackGroup)
        .Select(group => group.MaxBy(entry => entry.Value)));
    foreach ((AppliedEffect effect, double add) in counted)
    {
      value += add;
      sources.Add(new(effect.Label, "Add", add));
    }

    // ability scores are capped by their maximum before effects that set a minimum apply
    if (key.StartsWith("ability.", StringComparison.Ordinal) && key.EndsWith(".score", StringComparison.Ordinal))
    {
      double maximum = this.Stat(key.Replace(".score", ".max"));
      value = Math.Min(value, maximum);
    }

    foreach ((AppliedEffect effect, double max) in effects.Where(entry => entry.Effect.Effect.Operation == EffectOperationEnum.Max))
    {
      value = Math.Max(value, max);
      sources.Add(new(effect.Label, "Max", max));
    }

    foreach ((AppliedEffect effect, double min) in effects.Where(entry => entry.Effect.Effect.Operation == EffectOperationEnum.Min))
    {
      value = Math.Min(value, min);
      sources.Add(new(effect.Label, "Min", min));
    }

    foreach ((AppliedEffect effect, double proficiency) in effects.Where(entry => entry.Effect.Effect.Operation == EffectOperationEnum.Proficiency))
    {
      value = Math.Max(value, proficiency);
      sources.Add(new(effect.Label, "Proficiency", proficiency));
    }

    foreach ((AppliedEffect effect, _) in effects.Where(entry => entry.Effect.Effect.Operation == EffectOperationEnum.Grant))
    {
      value = 1;
      sources.Add(new(effect.Label, "Grant", 1));
    }

    return value;
  }

  #endregion

  #region Derived Stats

  /// <summary>
  ///   Proficiency level of a saving throw (<see cref="Common.Enum.Character.SkillModifierEnum"/> value)
  /// </summary>
  public int SaveProficiency(string ability) => (int)Math.Max(this.Stat($"save.{ability}.prof"), this.Stat("save.all.prof"));

  public int SaveBonus(string ability)
    => this.Modifier(ability)
       + ProficiencyPart(this.SaveProficiency(ability), this.ProficiencyBonus)
       + (int)this.Stat($"save.{ability}.bonus")
       + (int)this.Stat("save.all.bonus");

  public int SkillProficiency(string skill) => (int)Math.Max(this.Stat($"skill.{skill}.prof"), this.Stat("skill.all.prof"));

  public int SkillBonus(string skill)
  {
    string ability = RuleKeys.SkillAbilityKey(skill);
    return this.Modifier(ability)
           + ProficiencyPart(this.SkillProficiency(skill), this.ProficiencyBonus)
           + (int)this.Stat($"skill.{skill}.bonus")
           + (int)this.Stat("check.all.bonus")
           + (int)this.Stat($"check.{ability}.bonus");
  }

  /// <summary>
  ///   Bonus from a proficiency level: half (5), proficient (10), expertise (20)
  /// </summary>
  public static int ProficiencyPart(int proficiency, int proficiencyBonus) => (int)Math.Floor(proficiencyBonus * proficiency / 10.0);

  #endregion
}
