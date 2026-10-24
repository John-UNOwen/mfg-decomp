using System;
using KAMUNITY;
using TestModeSkeleton.CabinetDef;

namespace TestModeSkeleton;

public static class CabinetUtility
{
	public static int EnumLength<T>() where T : struct
	{
		return Enum.GetValues(typeof(T)).Length;
	}

	public static int SafeMod(int n, int m)
	{
		if (m == 0)
		{
			return 0;
		}
		return n % m;
	}

	public static int Loop(int val, int min, int max)
	{
		max -= min;
		val -= min;
		int num = SafeMod(val, max) + max;
		return min + SafeMod((val >= 0) ? val : num, max);
	}

	public static char GetDestCode()
	{
		return Avs.GetSoftIdCode()[4];
	}

	public static char GetSpecCode()
	{
		return Avs.GetSoftIdCode()[6];
	}

	public static int ExtractCoinNum(CoinOptionsDef.CoinSlot coinSlot)
	{
		switch (coinSlot)
		{
		case CoinOptionsDef.CoinSlot.Coin01Credit1:
		case CoinOptionsDef.CoinSlot.Coin01Credit2:
		case CoinOptionsDef.CoinSlot.Coin01Credit3:
			return 1;
		case CoinOptionsDef.CoinSlot.Coin02Credit1:
		case CoinOptionsDef.CoinSlot.Coin02Credit3:
			return 2;
		case CoinOptionsDef.CoinSlot.Coin03Credit1:
		case CoinOptionsDef.CoinSlot.Coin03Credit2:
			return 3;
		case CoinOptionsDef.CoinSlot.Coin04Credit1:
		case CoinOptionsDef.CoinSlot.Coin04Credit3:
			return 4;
		case CoinOptionsDef.CoinSlot.Coin05Credit1:
		case CoinOptionsDef.CoinSlot.Coin05Credit2:
			return 5;
		case CoinOptionsDef.CoinSlot.Coin06Credit1:
			return 6;
		case CoinOptionsDef.CoinSlot.Coin07Credit1:
			return 7;
		case CoinOptionsDef.CoinSlot.Coin08Credit1:
			return 8;
		case CoinOptionsDef.CoinSlot.Coin09Credit1:
			return 9;
		case CoinOptionsDef.CoinSlot.Coin10Credit1:
			return 10;
		default:
			Logger.LogError("不正な値");
			return 1;
		}
	}

	public static int ExtractCreditNum(CoinOptionsDef.CoinSlot coinSlot)
	{
		switch (coinSlot)
		{
		case CoinOptionsDef.CoinSlot.Coin01Credit1:
		case CoinOptionsDef.CoinSlot.Coin02Credit1:
		case CoinOptionsDef.CoinSlot.Coin03Credit1:
		case CoinOptionsDef.CoinSlot.Coin04Credit1:
		case CoinOptionsDef.CoinSlot.Coin05Credit1:
		case CoinOptionsDef.CoinSlot.Coin06Credit1:
		case CoinOptionsDef.CoinSlot.Coin07Credit1:
		case CoinOptionsDef.CoinSlot.Coin08Credit1:
		case CoinOptionsDef.CoinSlot.Coin09Credit1:
		case CoinOptionsDef.CoinSlot.Coin10Credit1:
			return 1;
		case CoinOptionsDef.CoinSlot.Coin01Credit2:
		case CoinOptionsDef.CoinSlot.Coin03Credit2:
		case CoinOptionsDef.CoinSlot.Coin05Credit2:
			return 2;
		case CoinOptionsDef.CoinSlot.Coin01Credit3:
		case CoinOptionsDef.CoinSlot.Coin02Credit3:
		case CoinOptionsDef.CoinSlot.Coin04Credit3:
			return 3;
		default:
			Logger.LogError("不正な値");
			return 1;
		}
	}
}
