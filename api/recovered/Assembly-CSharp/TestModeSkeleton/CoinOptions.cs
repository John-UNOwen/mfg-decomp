using System;
using System.Collections.Generic;
using System.Linq;
using MFG.Sytem.Utilities;
using TestModeSkeleton.CabinetDef;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class CoinOptions : TestModeBase
{
	[SerializeField]
	[Tooltip("フリープレー設定")]
	private Text freePlayText;

	[SerializeField]
	[Tooltip("コインスロット")]
	private Text coinSlotText;

	[SerializeField]
	[Tooltip("ノーマルプレー料金")]
	private Text startText;

	[SerializeField]
	[Tooltip("プレミアムプレー料金")]
	private Text premiumStartText;

	[SerializeField]
	[Tooltip("ガチャ料金")]
	private Text GachaText;

	[SerializeField]
	[Tooltip("ガチャ2料金")]
	private Text Gacha2Text;

	private Dictionary<string, TestModeValue> options = new Dictionary<string, TestModeValue>
	{
		["FreePlay"] = new TestModeValue(CoinOptionsDef.FreePlaySetting, isLoop: true, TestModeValue.TextColorType.Default, TestModeValue.ToOffOnString),
		["CoinSlot"] = new TestModeValue(CoinOptionsDef.CoinSlotSetting, isLoop: true, TestModeValue.TextColorType.Default, ToCoinSlotString),
		["Start"] = new TestModeValue(CoinOptionsDef.StartSetting, isLoop: true, TestModeValue.TextColorType.Default, ToStartPriceString),
		["PremiumStart"] = new TestModeValue(CoinOptionsDef.PremiumStartSetting, isLoop: true, TestModeValue.TextColorType.Default, ToStartPriceString),
		["Gacha"] = new TestModeValue(CoinOptionsDef.GachaSetting, isLoop: true, TestModeValue.TextColorType.Default, ToGachaPriceString),
		["Gacha2"] = new TestModeValue(CoinOptionsDef.Gacha5Setting, isLoop: true, TestModeValue.TextColorType.Default, ToGachaPriceString)
	};

	private static readonly Converter<int, string> ToCoinSlotString = (int n) => new string[16]
	{
		"1 COIN 1 CREDIT", "1 COIN 2 CREDITS", "1 COIN 3 CREDITS", "2 COINS 1 CREDIT", "2 COINS 3 CREDITS", "3 COINS 1 CREDIT", "3 COINS 2 CREDITS", "4 COINS 1 CREDIT", "4 COINS 3 CREDITS", "5 COINS 1 CREDIT",
		"5 COINS 2 CREDITS", "6 COINS 1 CREDIT", "7 COINS 1 CREDIT", "8 COINS 1 CREDIT", "9 COINS 1 CREDIT", "10 COINS 1 CREDIT"
	}.ElementAtOrDefault(n) ?? n.ToString();

	private static readonly Converter<int, string> ToStartPriceString = (int n) => new string[10] { "1 CREDIT <l>TO START</l>", "2 CREDITS <l>TO START</l>", "3 CREDITS <l>TO START</l>", "4 CREDITS <l>TO START</l>", "5 CREDITS <l>TO START</l>", "6 CREDITS <l>TO START</l>", "7 CREDITS <l>TO START</l>", "8 CREDITS <l>TO START</l>", "9 CREDITS <l>TO START</l>", "10 CREDITS <l>TO START</l>" }.ElementAtOrDefault(n) ?? n.ToString();

	private static readonly Converter<int, string> ToExBoostStartPriceString = (int n) => new string[10] { "1 CREDIT <l>TO START</l>", "2 CREDITS <l>TO START</l>", "3 CREDITS <l>TO START</l>", "4 CREDITS <l>TO START</l>", "5 CREDITS <l>TO START</l>", "6 CREDITS <l>TO START</l>", "7 CREDITS <l>TO START</l>", "8 CREDITS <l>TO START</l>", "9 CREDITS <l>TO START</l>", "10 CREDITS <l>TO START</l>" }.ElementAtOrDefault(n) ?? n.ToString();

	private static readonly Converter<int, string> ToContinuePriceString = (int n) => new string[10] { "1 CREDIT <l>TO CONTINUE</l>", "2 CREDITS <l>TO CONTINUE</l>", "3 CREDITS <l>TO CONTINUE</l>", "4 CREDITS <l>TO CONTINUE</l>", "5 CREDITS <l>TO CONTINUE</l>", "6 CREDITS <l>TO CONTINUE</l>", "7 CREDITS <l>TO CONTINUE</l>", "8 CREDITS <l>TO CONTINUE</l>", "9 CREDITS <l>TO CONTINUE</l>", "10 CREDITS <l>TO CONTINUE</l>" }.ElementAtOrDefault(n) ?? n.ToString();

	private static readonly Converter<int, string> ToGachaPriceString = (int n) => new string[10] { "1 CREDIT <l>TO GACHA</l>", "2 CREDITS <l>TO GACHA</l>", "3 CREDITS <l>TO GACHA</l>", "4 CREDITS <l>TO GACHA</l>", "5 CREDITS <l>TO GACHA</l>", "6 CREDITS <l>TO GACHA</l>", "7 CREDITS <l>TO GACHA</l>", "8 CREDITS <l>TO GACHA</l>", "9 CREDITS <l>TO GACHA</l>", "10 CREDITS <l>TO GACHA</l>" }.ElementAtOrDefault(n) ?? n.ToString();

	private static readonly Converter<int, string> ToFightModePriceString = (int n) => new string[10] { "1 CREDIT <l>TO START</l>", "2 CREDITS <l>TO START</l>", "3 CREDITS <l>TO START</l>", "4 CREDITS <l>TO START</l>", "5 CREDITS <l>TO START</l>", "6 CREDITS <l>TO START</l>", "7 CREDITS <l>TO START</l>", "8 CREDITS <l>TO START</l>", "9 CREDITS <l>TO START</l>", "10 CREDITS <l>TO START</l>" }.ElementAtOrDefault(n) ?? n.ToString();

	private static readonly Converter<int, string> ToStoryStartPriceString = (int n) => new string[10] { "1 CREDIT <l>TO START</l>", "2 CREDITS <l>TO START</l>", "3 CREDITS <l>TO START</l>", "4 CREDITS <l>TO START</l>", "5 CREDITS <l>TO START</l>", "6 CREDITS <l>TO START</l>", "7 CREDITS <l>TO START</l>", "8 CREDITS <l>TO START</l>", "9 CREDITS <l>TO START</l>", "10 CREDITS <l>TO START</l>" }.ElementAtOrDefault(n) ?? n.ToString();

	private static readonly Converter<int, string> ToFullSetStartPriceString = (int n) => new string[10] { "1 CREDIT <l>TO START</l>", "2 CREDITS <l>TO START</l>", "3 CREDITS <l>TO START</l>", "4 CREDITS <l>TO START</l>", "5 CREDITS <l>TO START</l>", "6 CREDITS <l>TO START</l>", "7 CREDITS <l>TO START</l>", "8 CREDITS <l>TO START</l>", "9 CREDITS <l>TO START</l>", "10 CREDITS <l>TO START</l>" }.ElementAtOrDefault(n) ?? n.ToString();

	protected override string TitleName => "<l>COIN OPTIONS</l>";

	private void Start()
	{
		options["FreePlay"].Set((int)CabinetManager.Coin.Data.FreePlay, freePlayText);
		options["CoinSlot"].Set((int)CabinetManager.Coin.Data.CoinSlot, coinSlotText);
		options["Start"].Set((int)CabinetManager.Coin.Data.Start, startText);
		options["PremiumStart"].Set((int)CabinetManager.Coin.Data.PremiumStart, premiumStartText);
		options["Gacha"].Set((int)CabinetManager.Coin.Data.Gacha, GachaText);
		options["Gacha2"].Set((int)CabinetManager.Coin.Data.Gacha5, Gacha2Text);
		SomeElementsOnOff();
		StartOrResumeMainCoroutine();
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyUp.SetActive(value: true);
		ArrowkeyDown.SetActive(value: true);
		ArrowkeyLeft.SetActive(value: false);
		ArrowkeyRight.SetActive(value: false);
		ArrowkeyEnter.SetActive(value: true);
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
	}

	protected override void OnUpdate(TestModeMenuElement element)
	{
		if ((SingletonBehaviour<TestModeInput>.Instance.IsAddValue() || SingletonBehaviour<TestModeInput>.Instance.IsSubValue()) && options.ContainsKey(element.Event))
		{
			options[element.Event].Change(SingletonBehaviour<TestModeInput>.Instance.IsAddValue());
			if (element.Event == "FreePlay")
			{
				SomeElementsOnOff();
			}
		}
		if (IsPayable())
		{
			notice.Stop();
		}
		else
		{
			notice.StartPaymentWarning();
		}
	}

	protected override void OnChangeMenuOn(TestModeMenuElement element)
	{
		switch (element.Event)
		{
		case "FreePlay":
		case "CoinSlot":
		case "Start":
		case "PremiumStart":
		case "Gacha":
		case "Gacha2":
			help.Set(TestModeHelp.HelpType.SelectChange);
			ArrowkeyUp.SetActive(value: true);
			ArrowkeyDown.SetActive(value: true);
			ArrowkeyLeft.SetActive(value: true);
			ArrowkeyRight.SetActive(value: true);
			ArrowkeyEnter.SetActive(value: false);
			break;
		default:
			help.Set(TestModeHelp.HelpType.SelectExecute);
			ArrowkeyUp.SetActive(value: true);
			ArrowkeyDown.SetActive(value: true);
			ArrowkeyLeft.SetActive(value: false);
			ArrowkeyRight.SetActive(value: false);
			ArrowkeyEnter.SetActive(value: true);
			SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
			break;
		}
	}

	protected override void OnFactorySettings(TestModeMenuElement element)
	{
		options.Values.ToList().ForEach(delegate(TestModeValue v)
		{
			v.ToDefault();
		});
		SomeElementsOnOff();
	}

	protected override void OnSave()
	{
		CabinetManager.Coin.Data.FreePlay = (CoinOptionsDef.FreePlay)options["FreePlay"].Current;
		CabinetManager.Coin.Data.CoinSlot = (CoinOptionsDef.CoinSlot)options["CoinSlot"].Current;
		CabinetManager.Coin.Data.Start = (CoinOptionsDef.Start)options["Start"].Current;
		CabinetManager.Coin.Data.PremiumStart = (CoinOptionsDef.PremiumStart)options["PremiumStart"].Current;
		CabinetManager.Coin.Data.Gacha = (CoinOptionsDef.Gacha)options["Gacha"].Current;
		CabinetManager.Coin.Data.Gacha5 = (CoinOptionsDef.Gacha5)options["Gacha2"].Current;
		CabinetManager.Coin.Save();
		CabinetManager.VirtualCoin.RefleshPrice(CabinetManager.Clock.GetTime(), CabinetManager.Coin.Data, Fraction.One);
	}

	protected override bool IsModification()
	{
		return options.Values.Any((TestModeValue v) => v.IsModification);
	}

	private void SomeElementsOnOff()
	{
		bool isActive = options["FreePlay"].CurrentString == "OFF";
		menuElements.FindAll((TestModeMenuElement e) => e.Mode == TestModeMenuElement.EventMode.Event && e.Event != "FreePlay").ForEach(delegate(TestModeMenuElement e)
		{
			e.State = ((!isActive) ? TestModeMenuElement.TextState.NotActive : TestModeMenuElement.TextState.Default);
		});
	}

	private bool IsPayable()
	{
		if (options["FreePlay"].Current != 1)
		{
			return TestModeUtility.IsPayableAtCoinSettings((CoinOptionsDef.CoinSlot)options["CoinSlot"].Current, (CoinOptionsDef.Start)options["Start"].Current, (CoinOptionsDef.PremiumStart)options["PremiumStart"].Current, (CoinOptionsDef.Gacha)options["Gacha"].Current, (CoinOptionsDef.Gacha5)options["Gacha2"].Current);
		}
		return true;
	}
}
