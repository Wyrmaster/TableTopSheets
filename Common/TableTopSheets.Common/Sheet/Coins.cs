using TableTopSheets.Common.Container.Magic;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;

namespace TableTopSheets.Common.Sheet;

public sealed record Coins
{
  public int Copper { get; init; }

  public int Silver { get; init; }

  public int Electrum { get; init; }

  public int Gold { get; init; }

  public int Platinum { get; init; }
}
