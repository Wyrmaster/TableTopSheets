using TableTopSheets.Persistence.Entity.Characters;

namespace TableTopSheets.Engine.Services;

/// <summary>
///   Coin arithmetic on the purse of a character (copper, silver, electrum, gold and platinum pieces)
/// </summary>
public static class Purse
{
  #region Constants

  /// <summary>
  ///   Value in copper pieces of each coin, lowest first
  /// </summary>
  private static readonly int[] Values = [1, 10, 50, 100, 1000];

  #endregion

  #region Methods

  /// <summary>
  ///   Value of all coins in copper pieces
  /// </summary>
  public static long Value(CharacterPlayState purse) => Counts(purse)
    .Select((count, index) => (long)count * Values[index])
    .Sum();

  /// <summary>
  ///   Pays an amount: coins are spent from the lowest value up; when no exact payment is possible, one larger coin is
  ///   spent and the change comes back in gold, silver and copper
  /// </summary>
  /// <exception cref="RuleViolationException">the coins are worth less than the amount</exception>
  public static void Pay(CharacterPlayState purse, long amountCp)
  {
    if (amountCp < 0)
    {
      throw new RuleViolationException("An amount to pay can't be negative");
    }

    long value = Value(purse);
    if (value < amountCp)
    {
      throw new RuleViolationException($"Not enough coins: {Format(amountCp)} needed, {Format(value)} available");
    }

    // 1. spend whole coins from the lowest value up, as long as they fit into what is still open
    int[] counts = Counts(purse);
    long remaining = amountCp;
    for (int index = 0; index < Values.Length; index++)
    {
      long take = Math.Min(counts[index], remaining / Values[index]);
      counts[index] -= (int)take;
      remaining -= take * Values[index];
    }

    // 2. still open: every coin left is worth more than the rest, so the smallest one pays it and returns change
    if (remaining > 0)
    {
      int index = Array.FindIndex(counts, count => count > 0);
      counts[index] -= 1;
      long change = Values[index] - remaining;
      Store(purse, counts);
      Add(purse, change);
      return;
    }

    Store(purse, counts);
  }

  /// <summary>
  ///   Adds an amount as few coins as possible in gold, silver and copper (selling, change)
  /// </summary>
  public static void Add(CharacterPlayState purse, long amountCp)
  {
    if (amountCp < 0)
    {
      throw new RuleViolationException("An amount to add can't be negative");
    }

    purse.GoldPieces = checked(purse.GoldPieces + (int)(amountCp / 100));
    purse.SilverPieces += (int)(amountCp % 100 / 10);
    purse.CopperPieces += (int)(amountCp % 10);
  }

  /// <summary>
  ///   Adds or removes coins of each kind (found treasure, an exact payment)
  /// </summary>
  /// <exception cref="RuleViolationException">a kind of coin would become negative</exception>
  public static void Change(CharacterPlayState purse, int copper, int silver, int electrum, int gold, int platinum)
  {
    int[] counts = Counts(purse);
    int[] deltas = [copper, silver, electrum, gold, platinum];
    string[] names = ["copper", "silver", "electrum", "gold", "platinum"];
    List<string> errors = new();
    for (int index = 0; index < counts.Length; index++)
    {
      counts[index] = checked(counts[index] + deltas[index]);
      if (counts[index] < 0)
      {
        errors.Add($"Not enough {names[index]} pieces: {-deltas[index]} removed, {counts[index] - deltas[index]} available");
      }
    }

    if (errors.Count > 0)
    {
      throw new RuleViolationException(errors);
    }

    Store(purse, counts);
  }

  /// <summary>
  ///   Replaces all coins
  /// </summary>
  /// <exception cref="RuleViolationException">a count is negative</exception>
  public static void Set(CharacterPlayState purse, int copper, int silver, int electrum, int gold, int platinum)
  {
    int[] counts = [copper, silver, electrum, gold, platinum];
    if (counts.Any(count => count < 0))
    {
      throw new RuleViolationException("Coin counts can't be negative");
    }

    Store(purse, counts);
  }

  /// <summary>
  ///   Amount for messages, e.g. <c>15 gp 4 sp</c>
  /// </summary>
  public static string Format(long amountCp)
  {
    List<string> parts = new();
    if (amountCp >= 100)
    {
      parts.Add($"{amountCp / 100} gp");
    }

    if (amountCp % 100 >= 10)
    {
      parts.Add($"{amountCp % 100 / 10} sp");
    }

    if (amountCp % 10 > 0 || parts.Count == 0)
    {
      parts.Add($"{amountCp % 10} cp");
    }

    return string.Join(' ', parts);
  }

  private static int[] Counts(CharacterPlayState purse) =>
    [purse.CopperPieces, purse.SilverPieces, purse.ElectrumPieces, purse.GoldPieces, purse.PlatinumPieces];

  private static void Store(CharacterPlayState purse, int[] counts)
  {
    purse.CopperPieces = counts[0];
    purse.SilverPieces = counts[1];
    purse.ElectrumPieces = counts[2];
    purse.GoldPieces = counts[3];
    purse.PlatinumPieces = counts[4];
  }

  #endregion
}
