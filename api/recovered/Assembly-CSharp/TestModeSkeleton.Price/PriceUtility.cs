using System;
using KAMUNITY;
using MFG.Sytem.Utilities;
using TestModeSkeleton.CabinetDef;
using UnityEngine;

namespace TestModeSkeleton.Price;

public static class PriceUtility
{
	public static class SmartContinue
	{
		public static (int currentPrice, int factoryPrice, int lowerScorePrice, int upperScorePrice) CalcContinuePrice(CoinSettings.SaveData coin, int basicRate, VirtualCoinDef.BalanceOfCredit balanceOfCredit, Fraction priceRate, bool isTimeServiceActive, int serviceTimeValue)
		{
			int item = CalcPaseliPrice(GameContentsInfo.GameContents.Continue, coin, basicRate, balanceOfCredit, isTimeServiceActive: false, 0).price;
			int basePrice = coin.CalcCoinPrice(GameContentsInfo.GameContents.Continue, isUseFactorySettingValue: true);
			(int currentPrice, int lowerScorePrice, int upperScorePrice) tuple = CalcContinuePrice(item, priceRate, isTimeServiceActive, serviceTimeValue);
			int item2 = tuple.currentPrice;
			int item3 = tuple.lowerScorePrice;
			int item4 = tuple.upperScorePrice;
			int item5 = CalcContinuePrice(basePrice, priceRate, isTimeServiceActive: false, 0).currentPrice;
			return (currentPrice: item2, factoryPrice: item5, lowerScorePrice: item3, upperScorePrice: item4);
		}

		public static (int currentPrice, int lowerScorePrice, int upperScorePrice) CalcContinuePrice(int basePrice, Fraction priceRate, bool isTimeServiceActive, int serviceTimeValue)
		{
			(int lowerScorePrice, int upperScorePrice) boundaryPrice = GetBoundaryPrice(basePrice);
			int item = boundaryPrice.lowerScorePrice;
			int item2 = boundaryPrice.upperScorePrice;
			int num = Math.Sign(priceRate.Numerator) * Math.Sign(priceRate.Denominator);
			int num2 = basePrice;
			num2 = ((Math.Abs(priceRate.Numerator) - Math.Abs(priceRate.Denominator) > 0 && num > 0) ? basePrice : ((priceRate.Numerator != 0 && num >= 0) ? (item2 + (item - item2) * priceRate.Numerator / priceRate.Denominator) : 0));
			if (isTimeServiceActive && num2 > 0)
			{
				int item3 = CalcLimitedServiceTimePrice(num2, serviceTimeValue).price;
				int item4 = CalcLimitedServiceTimePrice(item, serviceTimeValue).price;
				int item5 = CalcLimitedServiceTimePrice(item2, serviceTimeValue).price;
				return (currentPrice: item3, lowerScorePrice: item4, upperScorePrice: item5);
			}
			return (currentPrice: num2, lowerScorePrice: item, upperScorePrice: item2);
		}

		public static (int currentPrice, int factoryPrice, int lowerScorePrice, int upperScorePrice) CalcPremiumContinuePrice(CoinSettings.SaveData coin, int basicRate, VirtualCoinDef.BalanceOfCredit balanceOfCredit, Fraction priceRate, bool isTimeServiceActive, int serviceTimeValue)
		{
			int item = CalcPaseliPrice(GameContentsInfo.GameContents.PremiumContinue, coin, basicRate, balanceOfCredit, isTimeServiceActive: false, 0).price;
			int basePrice = coin.CalcCoinPrice(GameContentsInfo.GameContents.PremiumContinue, isUseFactorySettingValue: true);
			(int currentPrice, int lowerScorePrice, int upperScorePrice) tuple = CalcPremiumContinuePrice(item, priceRate, isTimeServiceActive, serviceTimeValue);
			int item2 = tuple.currentPrice;
			int item3 = tuple.lowerScorePrice;
			int item4 = tuple.upperScorePrice;
			int item5 = CalcPremiumContinuePrice(basePrice, priceRate, isTimeServiceActive: false, 0).currentPrice;
			return (currentPrice: item2, factoryPrice: item5, lowerScorePrice: item3, upperScorePrice: item4);
		}

		public static (int currentPrice, int lowerScorePrice, int upperScorePrice) CalcPremiumContinuePrice(int basePrice, Fraction priceRate, bool isTimeServiceActive, int serviceTimeValue)
		{
			int num = basePrice / 2;
			(int lowerScorePrice, int upperScorePrice) boundaryPrice = GetBoundaryPrice(num);
			int item = boundaryPrice.lowerScorePrice;
			int item2 = boundaryPrice.upperScorePrice;
			int num2 = Math.Sign(priceRate.Numerator) * Math.Sign(priceRate.Denominator);
			int num3 = basePrice;
			num3 = ((Math.Abs(priceRate.Numerator) - Math.Abs(priceRate.Denominator) > 0 && num2 > 0) ? basePrice : ((priceRate.Numerator != 0 && num2 >= 0) ? (num + item2 + (item - item2) * priceRate.Numerator / priceRate.Denominator) : num));
			if (isTimeServiceActive && num3 > 0)
			{
				int item3 = CalcLimitedServiceTimePrice(num3, serviceTimeValue).price;
				int item4 = CalcLimitedServiceTimePrice(num + item, serviceTimeValue).price;
				int item5 = CalcLimitedServiceTimePrice(num + item2, serviceTimeValue).price;
				return (currentPrice: item3, lowerScorePrice: item4, upperScorePrice: item5);
			}
			return (currentPrice: num3, lowerScorePrice: num + item, upperScorePrice: num + item2);
		}

		private static (int lowerScorePrice, int upperScorePrice) GetBoundaryPrice(int normalPrice)
		{
			int item = normalPrice * 210 / 1000;
			return (lowerScorePrice: normalPrice, upperScorePrice: item);
		}
	}

	public static readonly int PayablePrice = 500;

	public static readonly int PricePerCoin = 100;

	private static readonly int MinPrice = 10;

	private static readonly int PlayMaxPrice = 500;

	private static readonly int GachaMaxPrice = 1000;

	private static readonly int Permil1000 = 1000;

	public static bool IsPayablePrice(int price)
	{
		if (GameUtility.IsKamunityBootEnable() && CabinetUtility.GetDestCode() != 'J')
		{
			return true;
		}
		return price <= PayablePrice;
	}

	public static PriceInfo CalcPaseliPrice(CoinSettings.SaveData coinSettings, int basicRate, VirtualCoinDef.BalanceOfCredit balanceOfCredit, Fraction continuePriceRate, bool isTimeServiceActive, int serviceTimeValue)
	{
		return CalcPaseliPrice(coinSettings, basicRate, basicRate, basicRate, balanceOfCredit, continuePriceRate, isTimeServiceActive, serviceTimeValue);
	}

	public static PriceInfo CalcPaseliPrice(CoinSettings.SaveData coinSettings, int playPriceBasicRate, int gachaPriceBasicRate, int musicGachaPriceBasicRate, VirtualCoinDef.BalanceOfCredit balanceOfCredit, Fraction continuePriceRate, bool isTimeServiceActive, int serviceTimeValue)
	{
		PriceInfo priceInfo = new PriceInfo();
		priceInfo.StartPrice = CalcPaseliPrice(GameContentsInfo.GameContents.Start, coinSettings, playPriceBasicRate, balanceOfCredit, isTimeServiceActive, serviceTimeValue).price;
		priceInfo.PremiumPrice = CalcPaseliPrice(GameContentsInfo.GameContents.PremiumStart, coinSettings, playPriceBasicRate, balanceOfCredit, isTimeServiceActive, serviceTimeValue).price;
		priceInfo.PremiumStartPrice = priceInfo.StartPrice + priceInfo.PremiumPrice;
		(int currentPrice, int factoryPrice, int lowerScorePrice, int upperScorePrice) tuple = SmartContinue.CalcContinuePrice(coinSettings, playPriceBasicRate, balanceOfCredit, continuePriceRate, isTimeServiceActive, serviceTimeValue);
		int item = tuple.currentPrice;
		int item2 = tuple.factoryPrice;
		int item3 = tuple.lowerScorePrice;
		int item4 = tuple.upperScorePrice;
		priceInfo.ContinuePrice = item;
		priceInfo.ContinueFactoryPrice = item2;
		priceInfo.ContinueLowerPrice = item3;
		priceInfo.ContinueUpperPrice = item4;
		priceInfo.PremiumContinuePrice = priceInfo.PremiumPrice + item;
		priceInfo.PremiumContinueFactoryPrice = 100 + item2;
		priceInfo.GachaPrice = CalcPaseliPrice(GameContentsInfo.GameContents.Gacha, coinSettings, gachaPriceBasicRate, balanceOfCredit, isTimeServiceActive, serviceTimeValue).price;
		priceInfo.Gacha2Price = CalcPaseliPrice(GameContentsInfo.GameContents.Gacha5, coinSettings, gachaPriceBasicRate, balanceOfCredit, isTimeServiceActive, serviceTimeValue).price;
		priceInfo.MusicGachaPrice = 300 * musicGachaPriceBasicRate / Permil1000;
		return priceInfo;
	}

	public static (int price, bool isLimitOrReferCredit) CalcPaseliPrice(GameContentsInfo.GameContents contents, CoinSettings.SaveData coinSettings, int basicRate, VirtualCoinDef.BalanceOfCredit balanceOfCredit, bool isTimeServiceActive, int serviceTimeValue)
	{
		return CalcPaseliPrice(contents, coinSettings.CalcCoinPrice(contents), basicRate, balanceOfCredit, isTimeServiceActive, serviceTimeValue);
	}

	public static (int price, bool isLimitOrReferCredit) CalcPaseliPrice(GameContentsInfo.GameContents contents, int coinPrice, int basicRate, VirtualCoinDef.BalanceOfCredit balanceOfCredit, bool isTimeServiceActive, int serviceTimeValue)
	{
		if (coinPrice == 0)
		{
			return (price: 0, isLimitOrReferCredit: false);
		}
		int num = 0;
		bool item = false;
		int num2 = ((contents == GameContentsInfo.GameContents.Gacha || contents == GameContentsInfo.GameContents.Gacha5) ? GachaMaxPrice : PlayMaxPrice);
		switch (balanceOfCredit)
		{
		case VirtualCoinDef.BalanceOfCredit.Normal:
			item = true;
			num = coinPrice;
			break;
		case VirtualCoinDef.BalanceOfCredit.Imbalance:
			if (IsStandardContents(contents))
			{
				item = true;
				num = coinPrice;
			}
			else
			{
				num = coinPrice * basicRate / Permil1000;
			}
			break;
		case VirtualCoinDef.BalanceOfCredit.MoreImbalance:
			IsStandardContents(contents);
			num = coinPrice * basicRate / Permil1000;
			break;
		}
		if (num < MinPrice || num > num2)
		{
			item = true;
		}
		num = Mathf.Clamp(num, MinPrice, num2);
		if (isTimeServiceActive)
		{
			(int price, bool isLimited) tuple = CalcLimitedServiceTimePrice(num, serviceTimeValue);
			_ = tuple.isLimited;
			return (price: tuple.price, isLimitOrReferCredit: item);
		}
		return (price: num, isLimitOrReferCredit: item);
	}

	public static (int price, bool isLimited) CalcLimitedServiceTimePrice(int normalPrice, int serviceValue)
	{
		bool item = false;
		int num = normalPrice / 2;
		int num2 = normalPrice - serviceValue;
		if (num2 < num || num2 < MinPrice)
		{
			item = true;
		}
		num2 = Mathf.Max(num2, num);
		num2 = Mathf.Max(num2, MinPrice);
		return (price: num2, isLimited: item);
	}

	private static bool IsStandardContents(GameContentsInfo.GameContents contents)
	{
		switch (contents)
		{
		case GameContentsInfo.GameContents.Start:
		case GameContentsInfo.GameContents.Continue:
		case GameContentsInfo.GameContents.Gacha:
		case GameContentsInfo.GameContents.Gacha5:
		case GameContentsInfo.GameContents.PremiumStart:
		case GameContentsInfo.GameContents.PremiumContinue:
			return true;
		case GameContentsInfo.GameContents.MusicGacha:
			return false;
		default:
			KAMUNITY.Logger.LogError("不正な値");
			return true;
		}
	}
}
