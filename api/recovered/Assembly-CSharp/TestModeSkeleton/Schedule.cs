using System;
using System.Collections.Generic;
using System.Linq;
using TestModeSkeleton.CabinetDef;
using TestModeSkeleton.Price;
using TestModeSkeleton.TestModeExtensions;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

[RequireComponent(typeof(VirtualCoinContentsPrice))]
public class Schedule : VirtualCoinBase
{
	[Serializable]
	private class TableObject
	{
		[Tooltip("識別名")]
		public string name;

		[Tooltip("'週'行の背景オブジェクト")]
		public GameObject weekBackGround;

		[Tooltip("'パターン'行の背景オブジェクト")]
		public GameObject patternBackGround;

		[Tooltip("パターン値のテキスト")]
		public Text valueText;

		public void SetBackGroundActive(bool isEnable)
		{
			weekBackGround.SetActive(isEnable);
			patternBackGround.SetActive(isEnable);
		}
	}

	private Dictionary<string, TestModeValue> options;

	private Dictionary<string, TableObject> tableObjectDictionary;

	private readonly List<string> dayOfWeekList = new List<string> { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Holiday" };

	[SerializeField]
	[Tooltip("表関連のオブジェクトリスト")]
	private TableObject[] tableObjects;

	[Header("表以外のオブジェクト")]
	[SerializeField]
	[Tooltip("日付 or 選択中の曜日")]
	private Text dateText;

	[SerializeField]
	[Tooltip("選択中のパターン")]
	private Text patternText;

	[SerializeField]
	[Tooltip("BASIC RATEの値")]
	private Text basicRateText;

	[SerializeField]
	[Tooltip("BALANCE OF CREDITの値")]
	private Text balanceOfCreditText;

	[SerializeField]
	[Tooltip("TIME SERVICEの値")]
	private Text timeServiceText;

	[SerializeField]
	[Tooltip("SERVICE VALUEの値")]
	private Text serviceValueText;

	[SerializeField]
	[Tooltip("SERVICE LIMITの値")]
	private Text serviceLimitText;

	[SerializeField]
	[Tooltip("SERVICE TIMEの値")]
	private Text serviceTimeText;

	[SerializeField]
	[Tooltip("価格表示")]
	private VirtualCoinContentsPrice contentsPrice;

	protected override string SubTitleName => "<l>OPERATION SETTINGS</l>";

	protected override string SubSubTitleName => "<l>SCHEDULE</l>";

	private void Start()
	{
		tableObjects.ToList().ForEach(delegate(TableObject v)
		{
			v.SetBackGroundActive(isEnable: false);
		});
		tableObjectDictionary = tableObjects.ToDictionary((TableObject k) => k.name);
		Converter<int, string> converter = (int n) => $"{n + 1}";
		options = new Dictionary<string, TestModeValue>
		{
			[dayOfWeekList[0]] = new TestModeValue(VirtualCoinDef.ScheduleSetting, isLoop: true, TestModeValue.TextColorType.Default, converter),
			[dayOfWeekList[1]] = new TestModeValue(VirtualCoinDef.ScheduleSetting, isLoop: true, TestModeValue.TextColorType.Default, converter),
			[dayOfWeekList[2]] = new TestModeValue(VirtualCoinDef.ScheduleSetting, isLoop: true, TestModeValue.TextColorType.Default, converter),
			[dayOfWeekList[3]] = new TestModeValue(VirtualCoinDef.ScheduleSetting, isLoop: true, TestModeValue.TextColorType.Default, converter),
			[dayOfWeekList[4]] = new TestModeValue(VirtualCoinDef.ScheduleSetting, isLoop: true, TestModeValue.TextColorType.Default, converter),
			[dayOfWeekList[5]] = new TestModeValue(VirtualCoinDef.ScheduleSetting, isLoop: true, TestModeValue.TextColorType.Default, converter),
			[dayOfWeekList[6]] = new TestModeValue(VirtualCoinDef.ScheduleSetting, isLoop: true, TestModeValue.TextColorType.Default, converter),
			[dayOfWeekList[7]] = new TestModeValue(VirtualCoinDef.ScheduleSetting, isLoop: true, TestModeValue.TextColorType.Default, converter)
		};
		int index = 0;
		dayOfWeekList.ForEach(delegate(string s)
		{
			options[s].Set((int)CabinetManager.VirtualCoin.Data.schedules[index++], tableObjectDictionary[s].valueText);
		});
		StartOrResumeMainCoroutine();
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
		if ((SingletonBehaviour<TestModeInput>.Instance.IsAddValue() || SingletonBehaviour<TestModeInput>.Instance.IsSubValue()) && options.ContainsKey(element.Event))
		{
			options[element.Event].Change(SingletonBehaviour<TestModeInput>.Instance.IsAddValue());
			SetPattern(GetDisplayPattern(element));
		}
		if (element.Mode == TestModeMenuElement.EventMode.Event)
		{
			dateText.text = element.TextString + "\n";
		}
		else
		{
			DateTime time = CabinetManager.Clock.GetTime();
			dateText.text = "現在 " + TestModeUtility.ToDateString(time) + "  " + TestModeUtility.ToTimeString(time) + "\n" + (CabinetManager.Clock.IsHoliday(time) ? "（祝日）" : "");
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
		if (tableObjectDictionary.ContainsKey(element.Event))
		{
			tableObjectDictionary[element.Event].SetBackGroundActive(isEnable: true);
		}
		SetPattern(GetDisplayPattern(element));
		help.Set((element.Mode == TestModeMenuElement.EventMode.Event) ? TestModeHelp.HelpType.SelectChange : TestModeHelp.HelpType.SelectExecute);
		if (element.Mode == TestModeMenuElement.EventMode.Event)
		{
			ArrowkeyEnter.SetActive(value: false);
			ArrowkeyUp.SetActive(value: true);
			ArrowkeyDown.SetActive(value: true);
			ArrowkeyLeft.SetActive(value: true);
			ArrowkeyRight.SetActive(value: true);
		}
		else
		{
			ArrowkeyEnter.SetActive(value: true);
			ArrowkeyUp.SetActive(value: true);
			ArrowkeyDown.SetActive(value: true);
			ArrowkeyLeft.SetActive(value: false);
			ArrowkeyRight.SetActive(value: false);
			SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
		}
	}

	protected override void OnChangeMenuOff(TestModeMenuElement element)
	{
		if (tableObjectDictionary.ContainsKey(element.Event))
		{
			tableObjectDictionary[element.Event].SetBackGroundActive(isEnable: false);
		}
	}

	protected override void OnFactorySettings(TestModeMenuElement element)
	{
		options.Values.ToList().ForEach(delegate(TestModeValue v)
		{
			v.ToDefault();
		});
	}

	protected override void OnSave()
	{
		int index = 0;
		dayOfWeekList.ForEach(delegate(string s)
		{
			CabinetManager.VirtualCoin.Data.schedules[index++] = (VirtualCoinDef.Patterns)options[s].Current;
		});
		CabinetManager.VirtualCoin.Save(CabinetManager.Clock.GetTime(), CabinetManager.Coin.Data);
	}

	protected override bool IsModification()
	{
		return options.Values.Any((TestModeValue v) => v.IsModification);
	}

	private void SetPattern(VirtualCoinDef.Patterns pattern)
	{
		patternText.SetLocalizedText($"<l>PATTERN</l> {(int)(pattern + 1)}");
		VirtualCoinSettings.SaveData.Pattern pattern2 = CabinetManager.VirtualCoin.Data.patterns[(int)pattern];
		basicRateText.text = VirtualCoinBase.ToBasicRateString(pattern2.BasicRate);
		balanceOfCreditText.text = VirtualCoinBase.ToBalanceOfCreditString((int)pattern2.BalanceOfCredit);
		timeServiceText.text = TestModeValue.ToOffOnString((int)pattern2.TimeService);
		serviceValueText.text = $"{pattern2.ServiceValue}";
		serviceLimitText.SetLocalizedText(VirtualCoinBase.ToServiceLimitString(pattern2.ServiceLimit));
		serviceTimeText.text = TestModeUtility.ToTimeRangeString(TimeSpan.FromMinutes(pattern2.ServiceTimeStart), TimeSpan.FromMinutes(pattern2.ServiceTimeEnd));
		Action<GameContentsInfo.GameContents> obj = delegate(GameContentsInfo.GameContents contents)
		{
			VirtualCoinSettings.SaveData.Pattern pattern3 = CabinetManager.VirtualCoin.Data.patterns[(int)pattern];
			var (price, isLimitOrReferCredit) = pattern3.CalcPaseliPrice(contents, CabinetManager.Coin.Data, isTimeServiceActive: false);
			contentsPrice.SetBasicPrice(contents, price, isLimitOrReferCredit);
			var (price2, isLimitOrReferCredit2) = pattern3.CalcPaseliPrice(contents, CabinetManager.Coin.Data, isTimeServiceActive: true);
			contentsPrice.SetTimeServicePrice(contents, price2, isLimitOrReferCredit2, pattern3.TimeService == VirtualCoinDef.TimeService.On);
		};
		obj(GameContentsInfo.GameContents.Start);
		obj(GameContentsInfo.GameContents.Gacha);
		obj(GameContentsInfo.GameContents.Gacha5);
		obj(GameContentsInfo.GameContents.PremiumStart);
	}

	private VirtualCoinDef.Patterns GetDisplayPattern(TestModeMenuElement element)
	{
		if (element.Mode == TestModeMenuElement.EventMode.Event)
		{
			return (VirtualCoinDef.Patterns)options[element.Event].Current;
		}
		return CabinetManager.VirtualCoin.GetPatternInTime(CabinetManager.Clock.GetTime());
	}

	private bool IsPayable()
	{
		bool result = true;
		foreach (string dayOfWeek in dayOfWeekList)
		{
			VirtualCoinDef.Patterns current2 = (VirtualCoinDef.Patterns)options[dayOfWeek].Current;
			if (!CabinetManager.VirtualCoin.Data.patterns[(int)current2].IsPayable())
			{
				result = false;
				break;
			}
		}
		return result;
	}
}
