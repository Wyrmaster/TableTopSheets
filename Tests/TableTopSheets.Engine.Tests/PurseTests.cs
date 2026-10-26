using TableTopSheets.Engine.Services;
using TableTopSheets.Persistence.Entity.Characters;

namespace TableTopSheets.Engine.Tests;

public class PurseTests
{
  private static CharacterPlayState Coins(int copper = 0, int silver = 0, int electrum = 0, int gold = 0, int platinum = 0) => new()
  {
    CopperPieces = copper,
    SilverPieces = silver,
    ElectrumPieces = electrum,
    GoldPieces = gold,
    PlatinumPieces = platinum,
  };

  private static (int Copper, int Silver, int Electrum, int Gold, int Platinum) Counts(CharacterPlayState purse)
    => (purse.CopperPieces, purse.SilverPieces, purse.ElectrumPieces, purse.GoldPieces, purse.PlatinumPieces);

  [Fact]
  public void Value_CountsEveryCoin()
    => Assert.Equal(1 + 20 + 50 + 300 + 1000, Purse.Value(Coins(copper: 1, silver: 2, electrum: 1, gold: 3, platinum: 1)));

  [Fact]
  public void Pay_SpendsExactCoins_FromTheLowestValueUp()
  {
    CharacterPlayState purse = Coins(copper: 5, silver: 3, gold: 2);

    Purse.Pay(purse, 135);

    Assert.Equal((0, 0, 0, 1, 0), Counts(purse));
  }

  [Fact]
  public void Pay_ReturnsChange_WhenNoExactPaymentIsPossible()
  {
    CharacterPlayState purse = Coins(gold: 1);

    Purse.Pay(purse, 30);

    // one gold piece spent, 70 cp back as silver
    Assert.Equal((0, 7, 0, 0, 0), Counts(purse));
  }

  [Fact]
  public void Pay_BreaksAPlatinumPiece()
  {
    CharacterPlayState purse = Coins(silver: 2, platinum: 1);

    Purse.Pay(purse, 150);

    // the two silver pieces pay 20 cp, the platinum piece the other 130 cp: 870 cp come back as 8 gp 7 sp
    Assert.Equal((0, 7, 0, 8, 0), Counts(purse));
    Assert.Equal(1020 - 150, Purse.Value(purse));
  }

  [Fact]
  public void Pay_FailsWithoutEnoughCoins_AndChangesNothing()
  {
    CharacterPlayState purse = Coins(silver: 9);

    RuleViolationException error = Assert.Throws<RuleViolationException>(() => Purse.Pay(purse, 100));

    Assert.Contains("Not enough coins", error.Errors.Single());
    Assert.Equal((0, 9, 0, 0, 0), Counts(purse));
  }

  [Fact]
  public void Add_UsesGoldSilverAndCopper()
  {
    CharacterPlayState purse = Coins(gold: 1);

    Purse.Add(purse, 1234);

    Assert.Equal((4, 3, 0, 13, 0), Counts(purse));
  }

  [Fact]
  public void Change_AddsAndRemoves_ButNeverBelowZero()
  {
    CharacterPlayState purse = Coins(silver: 5, gold: 2);

    Purse.Change(purse, 0, -5, 0, 3, 1);
    Assert.Equal((0, 0, 0, 5, 1), Counts(purse));

    Assert.Throws<RuleViolationException>(() => Purse.Change(purse, -1, 0, 0, 0, 0));
    Assert.Equal((0, 0, 0, 5, 1), Counts(purse));
  }

  [Theory]
  [InlineData(0, "0 cp")]
  [InlineData(1500, "15 gp")]
  [InlineData(1543, "15 gp 4 sp 3 cp")]
  [InlineData(40, "4 sp")]
  public void Format_GoldSilverCopper(long amount, string expected) => Assert.Equal(expected, Purse.Format(amount));
}
