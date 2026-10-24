using System;
using System.Collections;
using TestModeSkeleton.CabinetDef;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class Clock : TestModeBase
{
	private enum TimeCursor
	{
		Default,
		Hour,
		Min,
		Sec
	}

	[SerializeField]
	[Tooltip("時刻")]
	private Text timeText;

	[SerializeField]
	[Tooltip("日付")]
	private Text dateText;

	[SerializeField]
	[Tooltip("未初期化")]
	private Text notSetText;

	private TimeCursor timeCursor;

	private TestModeValue timeOffset = new TestModeValue(ClockDef.OffsetTimeSetting);

	protected override string TitleName => "<l>TL/CLOCK</l>";

	protected override bool IsMenuLocked => timeCursor != TimeCursor.Default;

	private void Start()
	{
		bool isInitialized = CabinetManager.Clock.Data.Initialized;
		timeOffset.Set(CabinetManager.Clock.Data.OffsetTime);
		menuElements.FindAll((TestModeMenuElement e) => e.Mode == TestModeMenuElement.EventMode.Exit).ForEach(delegate(TestModeMenuElement e)
		{
			e.State = ((!isInitialized) ? TestModeMenuElement.TextState.NotActive : TestModeMenuElement.TextState.Default);
		});
		notSetText.gameObject.SetActive(!isInitialized);
		StartOrResumeMainCoroutine();
		StartCoroutine(CoUpdateText());
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
		ArrowkeyUp.SetActive(value: true);
		ArrowkeyDown.SetActive(value: true);
		ArrowkeyLeft.SetActive(value: false);
		ArrowkeyRight.SetActive(value: false);
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
	}

	protected override IEnumerator CoMainExit()
	{
		notice.StartNotSet();
		yield return new WaitForSeconds(TestModeBase.MessageWaitingSec);
		notice.Stop();
	}

	protected override void OnUpdate(TestModeMenuElement element)
	{
		if (element.Event != "SetTime")
		{
			ArrowkeyEnter.SetActive(value: true);
			ArrowkeyUp.SetActive(value: true);
			ArrowkeyDown.SetActive(value: true);
			ArrowkeyLeft.SetActive(value: false);
			ArrowkeyRight.SetActive(value: false);
			return;
		}
		ArrowkeyEnter.SetActive(value: false);
		ArrowkeyUp.SetActive(value: true);
		ArrowkeyDown.SetActive(value: true);
		ArrowkeyLeft.SetActive(value: true);
		ArrowkeyRight.SetActive(value: true);
		if (SingletonBehaviour<TestModeInput>.Instance.IsClockAddTimeCursor() || SingletonBehaviour<TestModeInput>.Instance.IsClockSubTimeCursor())
		{
			timeCursor = GetNextCursor(timeCursor, SingletonBehaviour<TestModeInput>.Instance.IsClockAddTimeCursor());
			TestModeMenuElement testModeMenuElement = menuElements.Find((TestModeMenuElement e) => e.Event == "SetTime");
			if (testModeMenuElement != null)
			{
				testModeMenuElement.State = ((timeCursor == TimeCursor.Default) ? TestModeMenuElement.TextState.Selecting : TestModeMenuElement.TextState.Default);
			}
			help.Set((timeCursor == TimeCursor.Default) ? TestModeHelp.HelpType.ClockChange : TestModeHelp.HelpType.ClockSetTime);
		}
		else if (SingletonBehaviour<TestModeInput>.Instance.IsClockAddOffset() || SingletonBehaviour<TestModeInput>.Instance.IsClockSubOffset())
		{
			switch (timeCursor)
			{
			case TimeCursor.Hour:
				timeOffset.Delta = (int)TimeSpan.FromHours(1.0).TotalSeconds;
				break;
			case TimeCursor.Min:
				timeOffset.Delta = (int)TimeSpan.FromMinutes(1.0).TotalSeconds;
				break;
			case TimeCursor.Sec:
				timeOffset.Delta = (int)TimeSpan.FromSeconds(1.0).TotalSeconds;
				break;
			default:
				timeOffset.Delta = 0;
				break;
			}
			timeOffset.Change(SingletonBehaviour<TestModeInput>.Instance.IsClockAddOffset());
		}
	}

	protected override void OnChangeMenuOn(TestModeMenuElement element)
	{
		help.Set((element.Event == "SetTime") ? TestModeHelp.HelpType.ClockChange : TestModeHelp.HelpType.SelectExecute);
	}

	protected override void OnSave()
	{
		CabinetManager.Clock.Data.Initialized = true;
		CabinetManager.Clock.Data.OffsetTime = timeOffset.Current;
		CabinetManager.Clock.Save();
	}

	private IEnumerator CoUpdateText()
	{
		while (true)
		{
			DateTime dateTime = CabinetManager.Clock.GetRawTime().AddSeconds(timeOffset.Current);
			Func<TimeCursor, Color> func = (TimeCursor c) => (c != timeCursor) ? Color.white : Color.red;
			timeText.text = TestModeUtility.ToTimeString(dateTime, func(TimeCursor.Hour), func(TimeCursor.Min), func(TimeCursor.Sec));
			dateText.text = TestModeUtility.ToDateString(dateTime);
			yield return null;
		}
	}

	private TimeCursor GetNextCursor(TimeCursor current, bool isAdd)
	{
		current = (TimeCursor)Mathf.Repeat((float)(timeCursor + (isAdd ? 1 : (-1))), CabinetUtility.EnumLength<TimeCursor>());
		return current;
	}
}
