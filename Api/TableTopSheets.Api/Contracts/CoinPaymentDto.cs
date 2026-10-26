using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>An amount to pay in copper pieces (1 gp = 100 cp); coins are spent and change is returned</summary>
public sealed record CoinPaymentDto(long AmountCp);
