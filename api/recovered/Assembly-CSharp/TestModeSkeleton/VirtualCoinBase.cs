using System;
using System.Linq;

namespace TestModeSkeleton;

public class VirtualCoinBase : TestModeBase
{
	protected static readonly Converter<int, string> ToBasicRateString = (int n) => $"{(float)n / 10f:0.0}%";

	protected static readonly Converter<int, string> ToBalanceOfCreditString = (int n) => new string[3] { "NORMAL", "IMBALANCE", "MORE IMBALANCE" }.ElementAtOrDefault(n) ?? n.ToString();

	protected static readonly Converter<int, string> ToServiceLimitString = (int n) => $"{n} <l>PLAY FOR EACH DAY</l>";

	protected sealed override string TitleName => "<l>VIRTUAL COIN</l>";
}
