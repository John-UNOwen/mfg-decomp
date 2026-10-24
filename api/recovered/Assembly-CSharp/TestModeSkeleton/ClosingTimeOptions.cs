using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TestModeSkeleton.CabinetDef;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class ClosingTimeOptions : TestModeBase
{
	private enum RequestSateInStateMenu
	{
		Waiting,
		ClockError
	}

	[SerializeField]
	private List<Text> shopCloseTexts;

	[SerializeField]
	private List<Text> shopCloseTimeTexts;

	[SerializeField]
	private List<GameObject> shopCloseTimeNextDay;

	[SerializeField]
	private List<GameObject> shopCloseTimeSelect;

	private Dictionary<string, TestModeValue> options = new Dictionary<string, TestModeValue>
	{
		["Monday"] = new TestModeValue(ClosingTimeOptionsDef.ShopCloseSetting, isLoop: true, TestModeValue.TextColorType.Default, TestModeValue.ToAbleString),
		["TimeMonday"] = new TestModeValue(ClosingTimeOptionsDef.ShopCloseTimeSetting, isLoop: true, TestModeValue.TextColorType.Default, (int n) => TimeSpan.FromMinutes(n).ToString("hh\\:mm") ?? ""),
		["Tuesday"] = new TestModeValue(ClosingTimeOptionsDef.ShopCloseSetting, isLoop: true, TestModeValue.TextColorType.Default, TestModeValue.ToAbleString),
		["TimeTuesday"] = new TestModeValue(ClosingTimeOptionsDef.ShopCloseTimeSetting, isLoop: true, TestModeValue.TextColorType.Default, (int n) => TimeSpan.FromMinutes(n).ToString("hh\\:mm") ?? ""),
		["Wednesday"] = new TestModeValue(ClosingTimeOptionsDef.ShopCloseSetting, isLoop: true, TestModeValue.TextColorType.Default, TestModeValue.ToAbleString),
		["TimeWednesday"] = new TestModeValue(ClosingTimeOptionsDef.ShopCloseTimeSetting, isLoop: true, TestModeValue.TextColorType.Default, (int n) => TimeSpan.FromMinutes(n).ToString("hh\\:mm") ?? ""),
		["Thursday"] = new TestModeValue(ClosingTimeOptionsDef.ShopCloseSetting, isLoop: true, TestModeValue.TextColorType.Default, TestModeValue.ToAbleString),
		["TimeThursday"] = new TestModeValue(ClosingTimeOptionsDef.ShopCloseTimeSetting, isLoop: true, TestModeValue.TextColorType.Default, (int n) => TimeSpan.FromMinutes(n).ToString("hh\\:mm") ?? ""),
		["Friday"] = new TestModeValue(ClosingTimeOptionsDef.ShopCloseSetting, isLoop: true, TestModeValue.TextColorType.Default, TestModeValue.ToAbleString),
		["TimeFriday"] = new TestModeValue(ClosingTimeOptionsDef.ShopCloseTimeSetting, isLoop: true, TestModeValue.TextColorType.Default, (int n) => TimeSpan.FromMinutes(n).ToString("hh\\:mm") ?? ""),
		["Saturday"] = new TestModeValue(ClosingTimeOptionsDef.ShopCloseSetting, isLoop: true, TestModeValue.TextColorType.Default, TestModeValue.ToAbleString),
		["TimeSaturday"] = new TestModeValue(ClosingTimeOptionsDef.ShopCloseTimeSetting, isLoop: true, TestModeValue.TextColorType.Default, (int n) => TimeSpan.FromMinutes(n).ToString("hh\\:mm") ?? ""),
		["Sunday"] = new TestModeValue(ClosingTimeOptionsDef.ShopCloseSetting, isLoop: true, TestModeValue.TextColorType.Default, TestModeValue.ToAbleString),
		["TimeSunday"] = new TestModeValue(ClosingTimeOptionsDef.ShopCloseTimeSetting, isLoop: true, TestModeValue.TextColorType.Default, (int n) => TimeSpan.FromMinutes(n).ToString("hh\\:mm") ?? "")
	};

	private RequestSateInStateMenu request;

	protected override string TitleName => "<l>CLOSING TIME OPTIONS</l>";

	private IEnumerator Start()
	{
		menuElements.FindAll((TestModeMenuElement e) => e.Event == "CabinetId").ForEach(delegate(TestModeMenuElement e)
		{
			e.State = TestModeMenuElement.TextState.NotActive;
		});
		foreach (DayOfWeek value in Enum.GetValues(typeof(DayOfWeek)))
		{
			options[value.ToString()].Set(CabinetManager.Game.Data.ShopCloseDayOfWeek[(int)value], shopCloseTexts[(int)value]);
			options[$"Time{value}"].Set(CabinetManager.Game.Data.ShopCloseTimeDayOfWeek[(int)value], shopCloseTimeTexts[(int)value]);
			TimeSpan timeSpan = TimeSpan.FromMinutes(options[$"Time{value}"].Current);
			shopCloseTimeNextDay[(int)value].SetActive(timeSpan.Hours < 7 && options[value.ToString()].CurrentString == "有効");
		}
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyUp.SetActive(value: true);
		ArrowkeyDown.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
		AllShopCloseTimeOnOff();
		StateFuncManager manager = new StateFuncManager(StateMenu);
		while (true)
		{
			manager.Update();
			yield return null;
		}
	}

	private void StateMenu(StateFuncManager manager, object param)
	{
		if (manager.IsStart)
		{
			StartOrResumeMainCoroutine();
		}
		RequestSateInStateMenu requestSateInStateMenu = request;
		if (requestSateInStateMenu != 0 && requestSateInStateMenu == RequestSateInStateMenu.ClockError)
		{
			request = RequestSateInStateMenu.Waiting;
			PauseMainCoroutine();
			manager.Change(StateClockError);
		}
	}

	private void StateClockError(StateFuncManager manager, object param)
	{
		if (manager.IsStart)
		{
			ArrowkeyUp.SetActive(value: false);
			ArrowkeyDown.SetActive(value: false);
			ArrowkeyLeft.SetActive(value: false);
			ArrowkeyRight.SetActive(value: false);
			ArrowkeyEnter.SetActive(value: true);
			SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
			notice.StartFirstSetClock(delegate
			{
				help.SaveAndSet(TestModeHelp.HelpType.Exit);
			});
		}
		if (SingletonBehaviour<TestModeInput>.Instance.IsMenuExecute())
		{
			ArrowkeyUp.SetActive(value: true);
			ArrowkeyDown.SetActive(value: true);
			ArrowkeyLeft.SetActive(value: true);
			ArrowkeyRight.SetActive(value: true);
			ArrowkeyEnter.SetActive(value: false);
			SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
			notice.Stop(delegate
			{
				help.SetFromSave();
			});
			manager.Change(StateMenu);
		}
	}

	protected override void OnUpdate(TestModeMenuElement element)
	{
		if ((SingletonBehaviour<TestModeInput>.Instance.IsAddValue() || SingletonBehaviour<TestModeInput>.Instance.IsSubValue()) && options.ContainsKey(element.Event))
		{
			string text = element.Event.Replace("Time", "");
			if (element.Event.Contains("Time"))
			{
				options[element.Event].Change(SingletonBehaviour<TestModeInput>.Instance.IsAddValue());
				TimeSpan timeSpan = TimeSpan.FromMinutes(options[element.Event].Current);
				shopCloseTimeNextDay[(int)Enum.Parse(typeof(DayOfWeek), text)].SetActive(timeSpan.Hours < 7);
			}
			else if (!CabinetManager.Clock.Data.Initialized && options[text].CurrentString == "無効")
			{
				request = RequestSateInStateMenu.ClockError;
			}
			else
			{
				options[text].Change(SingletonBehaviour<TestModeInput>.Instance.IsAddValue());
				ShopCloseTimeOnOff(text);
			}
		}
	}

	protected override void OnChangeMenuOn(TestModeMenuElement element)
	{
		switch (element.Event)
		{
		case "Monday":
		case "Tuesday":
		case "Wednesday":
		case "Thursday":
		case "Friday":
		case "Saturday":
		case "Sunday":
			help.Set(TestModeHelp.HelpType.SelectChange);
			CursorUnable();
			ArrowkeyLeft.SetActive(value: true);
			ArrowkeyRight.SetActive(value: true);
			ArrowkeyEnter.SetActive(value: false);
			break;
		case "TimeMonday":
		case "TimeTuesday":
		case "TimeWednesday":
		case "TimeThursday":
		case "TimeFriday":
		case "TimeSaturday":
		case "TimeSunday":
			help.Set(TestModeHelp.HelpType.SelectChange);
			ArrowkeyLeft.SetActive(value: true);
			ArrowkeyRight.SetActive(value: true);
			ArrowkeyEnter.SetActive(value: false);
			shopCloseTimeSelect[(int)Enum.Parse(typeof(DayOfWeek), element.Event.Replace("Time", ""))].SetActive(value: true);
			break;
		default:
			help.Set(TestModeHelp.HelpType.SelectExecute);
			CursorUnable();
			ArrowkeyLeft.SetActive(value: false);
			ArrowkeyRight.SetActive(value: false);
			ArrowkeyEnter.SetActive(value: true);
			SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
			break;
		}
	}

	private void CursorUnable()
	{
		foreach (GameObject item in shopCloseTimeSelect)
		{
			item.SetActive(value: false);
		}
	}

	protected override void OnFactorySettings(TestModeMenuElement element)
	{
		options.Values.ToList().ForEach(delegate(TestModeValue v)
		{
			v.ToDefault();
		});
		AllShopCloseTimeOnOff();
	}

	protected override void OnSave()
	{
		foreach (DayOfWeek value in Enum.GetValues(typeof(DayOfWeek)))
		{
			CabinetManager.Game.Data.ShopCloseDayOfWeek[(int)value] = options[value.ToString()].Current;
			CabinetManager.Game.Data.ShopCloseTimeDayOfWeek[(int)value] = options["Time" + value].Current;
		}
		CabinetManager.Game.Save(CabinetManager.Clock.GetTime(), CabinetManager.Coin.Data);
	}

	protected override bool IsModification()
	{
		return options.Values.Any((TestModeValue v) => v.IsModification);
	}

	private void ShopCloseTimeOnOff(string day)
	{
		bool isActive = options[day].CurrentString == "有効";
		menuElements.FindAll((TestModeMenuElement e) => e.Event == "Time" + day).ForEach(delegate(TestModeMenuElement e)
		{
			e.State = ((!isActive) ? TestModeMenuElement.TextState.UnSelctable : TestModeMenuElement.TextState.Default);
		});
		if (isActive)
		{
			options["Time" + day].FromUnSelectable();
			TimeSpan timeSpan = TimeSpan.FromMinutes(options["Time" + day].Current);
			shopCloseTimeNextDay[(int)Enum.Parse(typeof(DayOfWeek), day)].SetActive(timeSpan.Hours < 7);
		}
		else
		{
			options["Time" + day].ToUnSelectable(isTime: true);
			shopCloseTimeNextDay[(int)Enum.Parse(typeof(DayOfWeek), day)].SetActive(value: false);
		}
	}

	private void AllShopCloseTimeOnOff()
	{
		foreach (DayOfWeek value in Enum.GetValues(typeof(DayOfWeek)))
		{
			ShopCloseTimeOnOff(value.ToString());
		}
	}
}
