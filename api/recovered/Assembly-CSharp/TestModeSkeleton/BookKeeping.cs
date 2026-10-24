using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KAMUNITY;
using TestModeSkeleton.CabinetDef;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class BookKeeping : TestModeBase
{
	private enum RequestSateInStateMenu
	{
		Waiting,
		ClockError,
		Finished
	}

	private TestModeValue bookKeeping = new TestModeValue(BookKeepingDef.BookKeepingSetting, isLoop: true, TestModeValue.TextColorType.Default, TestModeValue.ToOffOnString);

	private RequestSateInStateMenu request;

	[SerializeField]
	[Tooltip("BookKeeping設定")]
	private Text bookKeepingText;

	[SerializeField]
	[Tooltip("メニューオブジェクト")]
	private GameObject menuObject;

	[SerializeField]
	[Tooltip("過去7日間のインカム集計オブジェクト")]
	private GameObject last7DaysObject;

	[SerializeField]
	[Tooltip("過去52週のインカム集計オブジェクト")]
	private GameObject last52WeeksObject;

	[SerializeField]
	[Tooltip("曜日ごとの集計オブジェクト")]
	private GameObject eachDayObject;

	[SerializeField]
	[Tooltip("時間ごとの集計オブジェクト")]
	private GameObject eachHourObject;

	[SerializeField]
	[Tooltip("要約オブジェクト")]
	private GameObject summaryObject;

	[SerializeField]
	[Tooltip("要約内の時間・日付テキスト")]
	private Text dateTimeText;

	protected override string TitleName => "<l>TL/BOOKKEEPING</l>";

	private IEnumerator Start()
	{
		Array.ForEach(new GameObject[6] { menuObject, last7DaysObject, last52WeeksObject, eachDayObject, eachHourObject, summaryObject }, delegate(GameObject o)
		{
			o.SetActive(value: false);
		});
		bookKeeping.Set((int)CabinetManager.Book.Data.BookKeeping, bookKeepingText);
		StateFuncManager manager = new StateFuncManager((bookKeeping.CurrentString == "OFF") ? new StateFuncManager.State(StateMenu) : new StateFuncManager.State(StateSetLog));
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
		ArrowkeyUp.SetActive(value: true);
		ArrowkeyDown.SetActive(value: true);
		ArrowkeyLeft.SetActive(value: false);
		ArrowkeyRight.SetActive(value: false);
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
		while (true)
		{
			manager.Update();
			yield return null;
		}
	}

	private void StateMenu(StateFuncManager manager, object param)
	{
		GameObject gameObject = menuObject.gameObject;
		if (manager.IsStart)
		{
			SetSubTitle("<l>MENU</l>");
			gameObject.SetActive(value: true);
			StartOrResumeMainCoroutine();
		}
		switch (request)
		{
		case RequestSateInStateMenu.ClockError:
			request = RequestSateInStateMenu.Waiting;
			PauseMainCoroutine();
			manager.Change(StateClockError);
			break;
		case RequestSateInStateMenu.Finished:
			request = RequestSateInStateMenu.Waiting;
			StopMainCoroutine();
			if (CabinetManager.Book.Data.BookKeeping == BookKeepingDef.BookKeeping.On)
			{
				bookKeeping.Set(1);
				gameObject.SetActive(value: false);
				manager.Change(StateSetLog);
			}
			else
			{
				manager.ChangeIdle();
				Exit();
			}
			break;
		case RequestSateInStateMenu.Waiting:
			break;
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

	private void StateSetLog(StateFuncManager manager, object param)
	{
		List<int> list = new List<int>(AvsBookkeeping.BookkeepingGetIncomeLast7days(svc: false));
		int num;
		int num2;
		try
		{
			num = (from x in list.Skip(1)
				where x >= 0
				select x).Sum();
			num2 = (int)(from x in list.Skip(1)
				where x >= 0
				select x).Average();
		}
		catch (InvalidOperationException)
		{
			num = -1;
			num2 = -1;
		}
		list.InsertRange(1, new int[2] { num2, num });
		TestModeUtility.SetBookKeepingText(list, last7DaysObject);
		List<int> list2 = new List<int>(AvsBookkeeping.BookkeepingGetIncomeLast52weeks(svc: false));
		list2.RemoveAt(0);
		TestModeUtility.SetBookKeepingText(list2, last52WeeksObject);
		TestModeUtility.SetBookKeepingText(AvsBookkeeping.BookkeepingGetIncomeEachDays(svc: false), eachDayObject);
		TestModeUtility.SetBookKeepingText(AvsBookkeeping.BookkeepingGetIncomeEachHour(svc: false), eachHourObject);
		Func<uint, bool, string> func = delegate(uint seconds, bool isHour)
		{
			TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
			return isHour ? $"{(int)timeSpan.TotalHours}<l>H</l> {timeSpan:mm}<l>M</l> {timeSpan:ss}<l>S</l>" : $"{(int)timeSpan.TotalMinutes:00}<l>M</l> {timeSpan:ss}<l>S</l>";
		};
		uint playCount = CabinetManager.Book.GetPlayCount(BookKeepingSettings.Variable.GameStarting);
		uint playCount2 = CabinetManager.Book.GetPlayCount(BookKeepingSettings.Variable.Continuation);
		uint playCount3 = CabinetManager.Book.GetPlayCount(BookKeepingSettings.Variable.Gacha);
		uint playCount4 = CabinetManager.Book.GetPlayCount(BookKeepingSettings.Variable.Gacha1);
		uint playCount5 = CabinetManager.Book.GetPlayCount(BookKeepingSettings.Variable.Gacha5);
		uint playCount6 = CabinetManager.Book.GetPlayCount(BookKeepingSettings.Variable.MusicGacha);
		TestModeUtility.SetBookKeepingText(new string[10]
		{
			func(AvsBookkeeping.BookkeepingGetTotalActiveTime(), arg2: true),
			func(AvsBookkeeping.BookkeepingGetTotalPlayTime(), arg2: true),
			func(AvsBookkeeping.BookkeepingGetAveragePlayTime(3u), arg2: false),
			func(AvsBookkeeping.BookkeepingGetLongestPlayTime(), arg2: false),
			func(AvsBookkeeping.BookkeepingGetShortestPlayTime(), arg2: false),
			$"{playCount + playCount2}",
			$"{playCount3}",
			$"{playCount4}",
			$"{playCount5}",
			$"{playCount6}"
		}, summaryObject);
		help.Set(TestModeHelp.HelpType.BookKeeping);
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
		ArrowkeyLeft.SetActive(value: true);
		ArrowkeyRight.SetActive(value: true);
		ArrowkeyUp.SetActive(value: false);
		ArrowkeyDown.SetActive(value: false);
		manager.Change(StateLast7Days);
	}

	private void StateLast7Days(StateFuncManager manager, object param)
	{
		GameObject obj = last7DaysObject.gameObject;
		if (manager.IsStart)
		{
			SetSubTitle("<l>COIN DATA OF LAST 7 DAYS</l>");
			obj.SetActive(value: true);
		}
		CheckNextState(manager, StateLast52Weeks, StateSummary, delegate
		{
			obj.SetActive(value: false);
		});
	}

	private void StateLast52Weeks(StateFuncManager manager, object param)
	{
		GameObject obj = last52WeeksObject.gameObject;
		if (manager.IsStart)
		{
			SetSubTitle("<l>COIN DATA OF LAST 52 WEEKS</l>");
			obj.SetActive(value: true);
		}
		CheckNextState(manager, StateEachDay, StateLast7Days, delegate
		{
			obj.SetActive(value: false);
		});
	}

	private void StateEachDay(StateFuncManager manager, object param)
	{
		GameObject obj = eachDayObject.gameObject;
		if (manager.IsStart)
		{
			SetSubTitle("<l>COIN DATA OF EACH DAY</l>");
			eachDayObject.gameObject.SetActive(value: true);
		}
		CheckNextState(manager, StateEachHour, StateLast52Weeks, delegate
		{
			obj.SetActive(value: false);
		});
	}

	private void StateEachHour(StateFuncManager manager, object param)
	{
		GameObject obj = eachHourObject.gameObject;
		if (manager.IsStart)
		{
			SetSubTitle("<l>COIN DATA OF EACH HOUR</l>");
			obj.SetActive(value: true);
		}
		CheckNextState(manager, StateSummary, StateEachDay, delegate
		{
			obj.SetActive(value: false);
		});
	}

	private void StateSummary(StateFuncManager manager, object param)
	{
		GameObject obj = summaryObject.gameObject;
		if (manager.IsStart)
		{
			SetSubTitle("<l>PLAY DATA SUMMARY</l>");
			obj.SetActive(value: true);
		}
		DateTime time = CabinetManager.Clock.GetTime();
		dateTimeText.text = TestModeUtility.ToTimeString(time) + "\n" + TestModeUtility.ToDateString(time);
		CheckNextState(manager, StateLast7Days, StateEachHour, delegate
		{
			obj.SetActive(value: false);
		});
	}

	protected override void OnUpdate(TestModeMenuElement element)
	{
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyEnter.SetActive(!(element.Event == "BookKeeping"));
		ArrowkeyUp.SetActive(value: true);
		ArrowkeyDown.SetActive(value: true);
		ArrowkeyLeft.SetActive(element.Event == "BookKeeping");
		ArrowkeyRight.SetActive(element.Event == "BookKeeping");
		if (!(element.Event != "BookKeeping") && (SingletonBehaviour<TestModeInput>.Instance.IsAddValue() || SingletonBehaviour<TestModeInput>.Instance.IsSubValue()))
		{
			if (!CabinetManager.Clock.Data.Initialized && bookKeeping.CurrentString == "OFF")
			{
				request = RequestSateInStateMenu.ClockError;
			}
			else
			{
				bookKeeping.Change(SingletonBehaviour<TestModeInput>.Instance.IsAddValue());
			}
		}
	}

	protected override IEnumerator OnCoDecide(TestModeMenuElement element)
	{
		if (element.Event != "ClearAllData")
		{
			yield break;
		}
		ArrowkeyEnter.SetActive(value: true);
		ArrowkeyUp.SetActive(value: false);
		ArrowkeyDown.SetActive(value: false);
		ArrowkeyLeft.SetActive(value: true);
		ArrowkeyRight.SetActive(value: true);
		notice.StartDataClearOrNot(delegate
		{
			help.SaveAndSet(TestModeHelp.HelpType.YesOrNo);
		});
		yield return new WaitUntil(() => notice.DecidedAnswer != TestModeNotice.Answer.NoAnswer);
		if (notice.DecidedAnswer == TestModeNotice.Answer.No)
		{
			notice.StartNotCleared();
			yield return new WaitForSeconds(TestModeBase.MessageWaitingSec);
			notice.Stop(delegate
			{
				help.SetFromSave();
			});
			yield break;
		}
		notice.StartSure();
		yield return new WaitUntil(() => notice.DecidedAnswer != TestModeNotice.Answer.NoAnswer);
		if (notice.DecidedAnswer == TestModeNotice.Answer.No)
		{
			notice.StartNotCleared();
			yield return new WaitForSeconds(TestModeBase.MessageWaitingSec);
			notice.Stop(delegate
			{
				help.SetFromSave();
			});
			yield break;
		}
		float start = Time.time;
		notice.StartNowErasing();
		yield return null;
		ClearBookKeeping();
		yield return new WaitUntil(() => Time.time - start >= TestModeBase.MessageWaitingSec);
		notice.Stop(delegate
		{
			help.SetFromSave();
		});
	}

	protected override void OnChangeMenuOn(TestModeMenuElement element)
	{
		help.Set((element.Event == "BookKeeping") ? TestModeHelp.HelpType.SelectChange : TestModeHelp.HelpType.SelectExecute);
	}

	protected override void OnSave()
	{
		CabinetManager.Book.Data.BookKeeping = (BookKeepingDef.BookKeeping)bookKeeping.Current;
		if (bookKeeping.IsModification)
		{
			ClearBookKeeping();
		}
		CabinetManager.Book.Save();
	}

	protected override void OnBeforeExit()
	{
		request = RequestSateInStateMenu.Finished;
	}

	protected override bool IsModification()
	{
		return bookKeeping.IsModification;
	}

	private void CheckNextState(StateFuncManager manager, StateFuncManager.State go, StateFuncManager.State back, UnityAction action)
	{
		if (SingletonBehaviour<TestModeInput>.Instance.IsSpecialOperation())
		{
			manager.Change(StateMenu);
			ArrowkeyGameObject.SetActive(value: true);
			ArrowkeyEnter.SetActive(value: true);
			ArrowkeyUp.SetActive(value: true);
			ArrowkeyDown.SetActive(value: true);
			ArrowkeyLeft.SetActive(value: false);
			ArrowkeyRight.SetActive(value: false);
			SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
			action();
		}
		else if (SingletonBehaviour<TestModeInput>.Instance.IsBookKeepingPageGo())
		{
			manager.Change(go);
			action();
		}
		else if (SingletonBehaviour<TestModeInput>.Instance.IsBookKeepingPageBack())
		{
			manager.Change(back);
			action();
		}
		else if (SingletonBehaviour<TestModeInput>.Instance.IsMenuExecute())
		{
			manager.ChangeIdle();
			Exit();
		}
	}

	private void ClearBookKeeping()
	{
		AvsBookkeeping.BookkeepingDataSetFactory();
		AvsBookkeeping.BookkeepingSetInitState();
	}
}
