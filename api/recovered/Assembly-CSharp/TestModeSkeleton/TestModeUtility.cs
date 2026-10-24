using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using KAMUNITY;
using TestModeSkeleton.CabinetDef;
using TestModeSkeleton.Price;
using TestModeSkeleton.TestModeExtensions;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

public static class TestModeUtility
{
	public static readonly Converter<int, string> BookKeepingTextConverter = (int v) => (v >= 0) ? $"{v}" : "<color=red>-</color>";

	public static string ToTimeString(DateTime dateTime, Color hour, Color min, Color sec)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"<color=#{ColorUtility.ToHtmlStringRGBA(hour)}>{dateTime:HH}</color>");
		stringBuilder.Append(":");
		stringBuilder.Append($"<color=#{ColorUtility.ToHtmlStringRGBA(min)}>{dateTime:mm}</color>");
		stringBuilder.Append(":");
		stringBuilder.Append($"<color=#{ColorUtility.ToHtmlStringRGBA(sec)}>{dateTime:ss}</color>");
		return stringBuilder.ToString();
	}

	public static string ToTimeString(DateTime dateTime)
	{
		return ToTimeString(dateTime, Color.white, Color.white, Color.white);
	}

	public static string ToDateString(DateTime dateTime)
	{
		CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture((CabinetManager.Lang.Data.TestModeLanguage == LanguageOptionsDef.TestMode.Japanese) ? "ja-JP" : "en-US");
		return string.Format("{0:yyyy}-{1:MM}-{2:dd} ({3})", dateTime, dateTime, dateTime, dateTime.ToString("ddd", cultureInfo));
	}

	public static string ToTimeRangeString(TimeSpan start, TimeSpan end, Color hourStart, Color minStart, Color hourEnd, Color minEnd)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"<color=#{ColorUtility.ToHtmlStringRGBA(hourStart)}>{start:hh}</color>");
		stringBuilder.Append(":");
		stringBuilder.Append($"<color=#{ColorUtility.ToHtmlStringRGBA(minStart)}>{start:mm}</color>");
		stringBuilder.Append("～" + ((start >= end) ? "翌" : ""));
		stringBuilder.Append($"<color=#{ColorUtility.ToHtmlStringRGBA(hourEnd)}>{end:hh}</color>");
		stringBuilder.Append(":");
		stringBuilder.Append($"<color=#{ColorUtility.ToHtmlStringRGBA(minEnd)}>{end:mm}</color>");
		return stringBuilder.ToString();
	}

	public static string ToTimeRangeString(TimeSpan start, TimeSpan end)
	{
		return ToTimeRangeString(start, end, Color.white, Color.white, Color.white, Color.white);
	}

	public static int GetDeltaInButtonHolding(IEnumerable<TestModeInput.Key> checkKeys, float rate = 50f, int min = 1, int max = 500)
	{
		float num = checkKeys.Select((TestModeInput.Key k) => SingletonBehaviour<TestModeInput>.Instance.GetSecAfterRepeat(k)).Max();
		return Mathf.Clamp((int)(rate * num), min, max);
	}

	public static void SetBookKeepingText(IEnumerable<string> input, GameObject gameObject, string objectName = "Value")
	{
		int index = 0;
		gameObject.FindChildComponentsRecursively<Text>(objectName).ForEach(delegate(Text t)
		{
			t.SetLocalizedText(input.ElementAtOrDefault(index++) ?? "<color=red>?</color>");
		});
		if (index != input.Count())
		{
			KAMUNITY.Logger.Log(string.Format("Textコンポーネントの数({0})と{1}の要素数({2})が一致していません", index, "input", input.Count()));
		}
	}

	public static void SetBookKeepingText(IEnumerable<int> input, GameObject gameObject, string objectName = "Value")
	{
		SetBookKeepingText(input.Select((int n) => BookKeepingTextConverter(n)), gameObject, objectName);
	}

	public static bool IsPayableAtCoinSettings()
	{
		return IsPayableAtCoinSettings(CabinetManager.Coin.Data.CoinSlot, CabinetManager.Coin.Data.Start, CabinetManager.Coin.Data.PremiumStart, CabinetManager.Coin.Data.Gacha, CabinetManager.Coin.Data.Gacha5);
	}

	public static bool IsPayableAtCoinSettings(CoinOptionsDef.CoinSlot coinSlot, CoinOptionsDef.Start startCredit, CoinOptionsDef.PremiumStart premiumStartCredit, CoinOptionsDef.Gacha gachaCredit, CoinOptionsDef.Gacha5 gacha5Credit)
	{
		int num = CabinetUtility.ExtractCoinNum(coinSlot);
		int num2 = CabinetUtility.ExtractCreditNum(coinSlot);
		return new List<int>
		{
			PriceUtility.PricePerCoin * (int)(startCredit + 1) * num / num2,
			PriceUtility.PricePerCoin * ((int)startCredit + (int)premiumStartCredit + 2) * num / num2,
			PriceUtility.PricePerCoin * (int)(gachaCredit + 1) * num / num2,
			PriceUtility.PricePerCoin * (int)(gacha5Credit + 1) * num / num2
		}.All((int p) => PriceUtility.IsPayablePrice(p));
	}

	public static bool IsPayableAtVirtualCoinSettingsWithActiveSchedule()
	{
		bool result = true;
		VirtualCoinDef.Patterns[] schedules = CabinetManager.VirtualCoin.Data.schedules;
		foreach (VirtualCoinDef.Patterns patterns in schedules)
		{
			if (!CabinetManager.VirtualCoin.Data.patterns[(int)patterns].IsPayable())
			{
				result = false;
				break;
			}
		}
		return result;
	}
}
