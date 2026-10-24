using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TestModeSkeleton.CabinetDef;
using TestModeSkeleton.Price;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

[RequireComponent(typeof(VirtualCoinContentsPrice))]
public class Pattern : VirtualCoinBase
{
	private enum TimeCursor
	{
		Default,
		HourStart,
		MinStart,
		HourEnd,
		MinEnd
	}

	private static VirtualCoinDef.Patterns currentPattern;

	private TimeCursor timeCursor;

	private Dictionary<string, TestModeValue> options;

	private static string pricePatternPrefix = "料金パターン";

	private readonly Converter<int, string> ToCopyFromPatternString = delegate(int n)
	{
		List<string> list = new List<string> { "1", "2", "3" };
		list.RemoveAt((int)CurrentPattern);
		return pricePatternPrefix + list.ElementAtOrDefault(n);
	};

	[SerializeField]
	[Tooltip("BASIC RATE")]
	private Text basicRateText;

	[SerializeField]
	[Tooltip("BALANCE OF CREDIT")]
	private Text balanceOfCreditText;

	[SerializeField]
	[Tooltip("TIME SERVICE")]
	private Text timeServiceText;

	[SerializeField]
	[Tooltip("SERVICE VALUE")]
	private Text serviceValueText;

	[SerializeField]
	[Tooltip("SERVICE LIMIT")]
	private Text serviceLimitText;

	[SerializeField]
	[Tooltip("SERVICE REMAIN")]
	private Text serviceRemainText;

	[SerializeField]
	[Tooltip("SERVICE TIME")]
	private Text serviceTimeText;

	[SerializeField]
	[Tooltip("COPY FROM PATTERN")]
	private Text copyFromPatternText;

	[SerializeField]
	[Tooltip("価格表示")]
	private VirtualCoinContentsPrice contentsPrice;

	public static VirtualCoinDef.Patterns CurrentPattern
	{
		get
		{
			return currentPattern;
		}
		set
		{
			VirtualCoinDef.Patterns patterns = (VirtualCoinDef.Patterns)CabinetUtility.EnumLength<VirtualCoinDef.Patterns>();
			currentPattern = ((VirtualCoinDef.Patterns.P1 <= value && value < patterns) ? value : VirtualCoinDef.Patterns.P1);
		}
	}

	protected override string SubTitleName => "<l>OPERATION SETTINGS</l>";

	protected override string SubSubTitleName => "<l>PATTERN</l> " + $"{(int)(CurrentPattern + 1)}";

	protected override bool IsMenuLocked => timeCursor != TimeCursor.Default;

	private void Start()
	{
		options = new Dictionary<string, TestModeValue>
		{
			["BasicRate"] = new TestModeValue(VirtualCoinDef.BasicRateSetting, isLoop: true, TestModeValue.TextColorType.Default, VirtualCoinBase.ToBasicRateString),
			["BalanceOfCredit"] = new TestModeValue(VirtualCoinDef.BalanceOfCreditSetting, isLoop: true, TestModeValue.TextColorType.Default, VirtualCoinBase.ToBalanceOfCreditString),
			["TimeService"] = new TestModeValue(VirtualCoinDef.TimeServiceSetting, isLoop: true, TestModeValue.TextColorType.Default, TestModeValue.ToOffOnString),
			["ServiceValue"] = new TestModeValue(VirtualCoinDef.ServiceValueSetting),
			["ServiceLimit"] = new TestModeValue(VirtualCoinDef.ServiceLimitSetting, isLoop: true, TestModeValue.TextColorType.Default, VirtualCoinBase.ToServiceLimitString),
			["ServiceRemain"] = new TestModeValue(0, GetServiceLimit(CabinetManager.Clock.GetTime()), CabinetManager.VirtualCoin.Data.ServiceRemain, 1, isLoop: true, TestModeValue.TextColorType.OnlyYellow, (int n) => $"{n} <l>PLAY</l>"),
			["ServiceTimeStart"] = new TestModeValue(VirtualCoinDef.ServiceTimeStartSetting),
			["ServiceTimeEnd"] = new TestModeValue(VirtualCoinDef.ServiceTimeEndSetting),
			["CopyFromPattern"] = new TestModeValue(VirtualCoinDef.CopyFromPatternSetting, isLoop: true, TestModeValue.TextColorType.OnlyWhite, ToCopyFromPatternString)
		};
		options["BasicRate"].Set(NowPattern().BasicRate, basicRateText);
		options["BalanceOfCredit"].Set((int)NowPattern().BalanceOfCredit, balanceOfCreditText);
		options["TimeService"].Set((int)NowPattern().TimeService, timeServiceText);
		options["ServiceValue"].Set(NowPattern().ServiceValue, serviceValueText);
		options["ServiceLimit"].Set(NowPattern().ServiceLimit, serviceLimitText);
		options["ServiceRemain"].Set(CabinetManager.VirtualCoin.Data.ServiceRemain, serviceRemainText);
		options["ServiceTimeStart"].Set(NowPattern().ServiceTimeStart);
		options["ServiceTimeEnd"].Set(NowPattern().ServiceTimeEnd);
		options["CopyFromPattern"].Set(0, copyFromPatternText);
		SetContetnsPrice();
		TimeServiceElementsOnOff();
		StartOrResumeMainCoroutine();
		StartCoroutine(CoServiceTimeText());
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
		ArrowkeyUp.SetActive(value: true);
		ArrowkeyDown.SetActive(value: true);
		ArrowkeyLeft.SetActive(value: false);
		ArrowkeyRight.SetActive(value: false);
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
	}

	protected override void OnUpdate(TestModeMenuElement element)
	{
		if (element.Event == "ServiceTime")
		{
			if (SingletonBehaviour<TestModeInput>.Instance.IsClockAddTimeCursor() || SingletonBehaviour<TestModeInput>.Instance.IsClockSubTimeCursor())
			{
				timeCursor = GetNextCursor(timeCursor, SingletonBehaviour<TestModeInput>.Instance.IsClockAddTimeCursor());
				TestModeMenuElement testModeMenuElement = menuElements.Find((TestModeMenuElement e) => e.Event == "ServiceTime");
				if (testModeMenuElement != null)
				{
					testModeMenuElement.State = ((timeCursor == TimeCursor.Default) ? TestModeMenuElement.TextState.Selecting : TestModeMenuElement.TextState.Default);
				}
				help.Set((timeCursor == TimeCursor.Default) ? TestModeHelp.HelpType.TimeChange : TestModeHelp.HelpType.ClockSetTime);
			}
			else if (SingletonBehaviour<TestModeInput>.Instance.IsClockAddOffset() || SingletonBehaviour<TestModeInput>.Instance.IsClockSubOffset())
			{
				TestModeValue testModeValue = options["ServiceTimeStart"];
				TestModeValue testModeValue2 = options["ServiceTimeEnd"];
				int delta = (int)TimeSpan.FromHours(1.0).TotalMinutes;
				int delta2 = (int)TimeSpan.FromMinutes(15.0).TotalMinutes;
				bool isAdd = SingletonBehaviour<TestModeInput>.Instance.IsClockAddOffset();
				switch (timeCursor)
				{
				case TimeCursor.HourStart:
					testModeValue.Delta = delta;
					testModeValue.Change(isAdd);
					break;
				case TimeCursor.MinStart:
					testModeValue.Delta = delta2;
					testModeValue.Change(isAdd);
					break;
				case TimeCursor.HourEnd:
					testModeValue2.Delta = delta;
					testModeValue2.Change(isAdd);
					break;
				case TimeCursor.MinEnd:
					testModeValue2.Delta = delta2;
					testModeValue2.Change(isAdd);
					break;
				}
			}
		}
		else
		{
			if (element.Event == "BasicRate")
			{
				options[element.Event].Delta = TestModeUtility.GetDeltaInButtonHolding(new TestModeInput.Key[2]
				{
					TestModeInput.Key.Right,
					TestModeInput.Key.Left
				});
			}
			if ((!(element.Event == "CopyFromPattern") || !SingletonBehaviour<TestModeInput>.Instance.IsMenuExecute()) && (SingletonBehaviour<TestModeInput>.Instance.IsAddValue() || SingletonBehaviour<TestModeInput>.Instance.IsSubValue()) && options.ContainsKey(element.Event))
			{
				options[element.Event].Change(SingletonBehaviour<TestModeInput>.Instance.IsAddValue());
				switch (element.Event)
				{
				case "BasicRate":
				case "BalanceOfCredit":
				case "TimeService":
				case "ServiceValue":
					SetContetnsPrice();
					if (element.Event == "TimeService")
					{
						TimeServiceElementsOnOff();
					}
					break;
				}
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

	protected override IEnumerator OnCoDecide(TestModeMenuElement element)
	{
		if (element.Event != "CopyFromPattern")
		{
			yield break;
		}
		Func<VirtualCoinSettings.SaveData.Pattern> func = delegate
		{
			VirtualCoinDef.Patterns patterns = VirtualCoinDef.Patterns.P1;
			switch (options["CopyFromPattern"].CurrentString.Replace(pricePatternPrefix, ""))
			{
			case "1":
				patterns = VirtualCoinDef.Patterns.P1;
				break;
			case "2":
				patterns = VirtualCoinDef.Patterns.P2;
				break;
			case "3":
				patterns = VirtualCoinDef.Patterns.P3;
				break;
			}
			return CabinetManager.VirtualCoin.Data.patterns[(int)patterns];
		};
		options["BasicRate"].ForcedChange(func().BasicRate);
		options["BalanceOfCredit"].ForcedChange((int)func().BalanceOfCredit);
		options["TimeService"].ForcedChange((int)func().TimeService);
		options["ServiceValue"].ForcedChange(func().ServiceValue);
		options["ServiceLimit"].ForcedChange(func().ServiceLimit);
		options["ServiceTimeStart"].ForcedChange(func().ServiceTimeStart);
		options["ServiceTimeEnd"].ForcedChange(func().ServiceTimeEnd);
		SetContetnsPrice();
		TimeServiceElementsOnOff();
	}

	protected override void OnChangeMenuOn(TestModeMenuElement element)
	{
		switch (element.Event)
		{
		case "BasicRate":
		case "BalanceOfCredit":
		case "TimeService":
		case "ServiceValue":
		case "ServiceLimit":
		case "ServiceRemain":
			help.Set(TestModeHelp.HelpType.SelectChange);
			ArrowkeyEnter.SetActive(value: false);
			ArrowkeyUp.SetActive(value: true);
			ArrowkeyDown.SetActive(value: true);
			ArrowkeyLeft.SetActive(value: true);
			ArrowkeyRight.SetActive(value: true);
			break;
		case "ServiceTime":
			help.Set(TestModeHelp.HelpType.TimeChange);
			ArrowkeyEnter.SetActive(value: false);
			ArrowkeyUp.SetActive(value: true);
			ArrowkeyDown.SetActive(value: true);
			ArrowkeyLeft.SetActive(value: true);
			ArrowkeyRight.SetActive(value: true);
			break;
		case "CopyFromPattern":
			help.Set(TestModeHelp.HelpType.SelectChangeExecute);
			ArrowkeyEnter.SetActive(value: true);
			ArrowkeyUp.SetActive(value: true);
			ArrowkeyDown.SetActive(value: true);
			ArrowkeyLeft.SetActive(value: true);
			ArrowkeyRight.SetActive(value: true);
			break;
		default:
			help.Set(TestModeHelp.HelpType.SelectExecute);
			ArrowkeyEnter.SetActive(value: true);
			ArrowkeyUp.SetActive(value: true);
			ArrowkeyDown.SetActive(value: true);
			ArrowkeyLeft.SetActive(value: false);
			ArrowkeyRight.SetActive(value: false);
			SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
			break;
		}
	}

	protected override void OnFactorySettings(TestModeMenuElement element)
	{
		options.Keys.Where((string k) => k != "CopyFromPattern").ToList().ForEach(delegate(string k)
		{
			options[k].ToDefault();
		});
		SetContetnsPrice();
		TimeServiceElementsOnOff();
	}

	protected override void OnSave()
	{
		DateTime time = CabinetManager.Clock.GetTime();
		NowPattern().BasicRate = options["BasicRate"].Current;
		NowPattern().BalanceOfCredit = (VirtualCoinDef.BalanceOfCredit)options["BalanceOfCredit"].Current;
		NowPattern().TimeService = (VirtualCoinDef.TimeService)options["TimeService"].Current;
		NowPattern().ServiceValue = options["ServiceValue"].Current;
		NowPattern().ServiceLimit = options["ServiceLimit"].Current;
		NowPattern().ServiceTimeStart = options["ServiceTimeStart"].Current;
		NowPattern().ServiceTimeEnd = options["ServiceTimeEnd"].Current;
		CabinetManager.VirtualCoin.Save(time, CabinetManager.Coin.Data);
		if (GetServiceLimit(time) != options["ServiceRemain"].Max)
		{
			CabinetManager.VirtualCoin.Data.ServiceRemain = GetServiceLimit(time);
		}
		else
		{
			CabinetManager.VirtualCoin.Data.ServiceRemain = options["ServiceRemain"].Current;
		}
	}

	protected override bool IsModification()
	{
		return options.Keys.Where((string k) => k != "CopyFromPattern").Any((string k) => options[k].IsModification);
	}

	private void TimeServiceElementsOnOff()
	{
		bool isActive = options["TimeService"].CurrentString == "ON";
		List<string> timeServiceGroup = new List<string> { "ServiceValue", "ServiceLimit", "ServiceRemain", "ServiceTime" };
		menuElements.FindAll((TestModeMenuElement e) => timeServiceGroup.Contains(e.Event)).ForEach(delegate(TestModeMenuElement e)
		{
			e.State = ((!isActive) ? TestModeMenuElement.TextState.UnSelctable : TestModeMenuElement.TextState.Default);
		});
		options.Keys.Where((string k) => timeServiceGroup.Contains(k)).ToList().ForEach(delegate(string k)
		{
			if (isActive)
			{
				options[k].FromUnSelectable();
			}
			else
			{
				options[k].ToUnSelectable();
			}
		});
	}

	private IEnumerator CoServiceTimeText()
	{
		float timer = 0f;
		TimeCursor cursor = timeCursor;
		while (true)
		{
			if (cursor != timeCursor)
			{
				timer = 0f;
				cursor = timeCursor;
			}
			else
			{
				timer += Time.deltaTime;
			}
			Color baseColor = Color.gray;
			if (options["TimeService"].CurrentString == "ON")
			{
				bool flag = options["ServiceTimeStart"].IsDefault && options["ServiceTimeEnd"].IsDefault;
				baseColor = (flag ? Color.green : Color.red);
			}
			Func<TimeCursor, Color> func = delegate(TimeCursor c)
			{
				Color result = baseColor;
				if (c == timeCursor)
				{
					result.a = ((timer % 1f < 0.5f) ? 0.6f : 1f);
				}
				return result;
			};
			TimeSpan start = TimeSpan.FromMinutes(options["ServiceTimeStart"].Current);
			TimeSpan end = TimeSpan.FromMinutes(options["ServiceTimeEnd"].Current);
			serviceTimeText.text = TestModeUtility.ToTimeRangeString(start, end, func(TimeCursor.HourStart), func(TimeCursor.MinStart), func(TimeCursor.HourEnd), func(TimeCursor.MinEnd));
			serviceTimeText.color = baseColor;
			yield return null;
		}
	}

	private TimeCursor GetNextCursor(TimeCursor current, bool isAdd)
	{
		current = (TimeCursor)Mathf.Repeat((float)(timeCursor + (isAdd ? 1 : (-1))), CabinetUtility.EnumLength<TimeCursor>());
		return current;
	}

	private void SetContetnsPrice()
	{
		Action<GameContentsInfo.GameContents> obj = delegate(GameContentsInfo.GameContents contents)
		{
			int current = options["BasicRate"].Current;
			VirtualCoinDef.BalanceOfCredit current2 = (VirtualCoinDef.BalanceOfCredit)options["BalanceOfCredit"].Current;
			var (price, isLimitOrReferCredit) = PriceUtility.CalcPaseliPrice(contents, CabinetManager.Coin.Data, current, current2, isTimeServiceActive: false, 0);
			contentsPrice.SetBasicPrice(contents, price, isLimitOrReferCredit);
			bool isEnableServiceTime = options["TimeService"].CurrentString == "ON";
			int current3 = options["ServiceValue"].Current;
			var (price2, isLimitOrReferCredit2) = PriceUtility.CalcPaseliPrice(contents, CabinetManager.Coin.Data, current, current2, isTimeServiceActive: true, current3);
			contentsPrice.SetTimeServicePrice(contents, price2, isLimitOrReferCredit2, isEnableServiceTime);
		};
		obj(GameContentsInfo.GameContents.Start);
		obj(GameContentsInfo.GameContents.Gacha);
		obj(GameContentsInfo.GameContents.Gacha5);
		obj(GameContentsInfo.GameContents.PremiumStart);
	}

	private VirtualCoinSettings.SaveData.Pattern NowPattern()
	{
		return CabinetManager.VirtualCoin.Data.patterns[(int)CurrentPattern];
	}

	private int GetServiceLimit(DateTime time)
	{
		return CabinetManager.VirtualCoin.GetPatternDataInTime(time).ServiceLimit;
	}

	private bool IsPayable()
	{
		return new List<int>
		{
			contentsPrice.GetBasicPrice(GameContentsInfo.GameContents.Start),
			contentsPrice.GetBasicPrice(GameContentsInfo.GameContents.Gacha),
			contentsPrice.GetBasicPrice(GameContentsInfo.GameContents.Gacha5),
			contentsPrice.GetBasicPrice(GameContentsInfo.GameContents.PremiumStart)
		}.All((int p) => PriceUtility.IsPayablePrice(p));
	}
}
