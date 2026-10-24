using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using KAMUNITY;
using TestModeSkeleton.TestModeExtensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TestModeSkeleton;

[RequireComponent(typeof(VirtualCoinPassWord))]
public class IncomeLog : VirtualCoinBase
{
	private Dictionary<string, VirtualCoinPassWord> passWords;

	private AvsEa3CoinTypes.ea3_eacoin_logs logs;

	[SerializeField]
	[Tooltip("過去7日間のインカム集計オブジェクト")]
	private GameObject last7DaysObject;

	[SerializeField]
	[Tooltip("過去2ヵ月間のインカム集計オブジェクト")]
	private GameObject last2MonthsObject;

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
	[Tooltip("詳細オブジェクト")]
	private GameObject detailObject;

	[SerializeField]
	[Tooltip("集計期間テキスト")]
	private Text countingText;

	protected override string SubTitleName => "<l>INCOME LOG</l>";

	private IEnumerator Start()
	{
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
		ArrowkeyUp.SetActive(value: false);
		ArrowkeyDown.SetActive(value: false);
		ArrowkeyLeft.SetActive(value: true);
		ArrowkeyRight.SetActive(value: true);
		passWords = GetComponents<VirtualCoinPassWord>().ToDictionary((VirtualCoinPassWord c) => c.Body.name);
		countingText.text = "";
		StateFuncManager manager = new StateFuncManager(StatePassWord);
		while (true)
		{
			manager.Update();
			yield return null;
		}
	}

	private void StatePassWord(StateFuncManager manager, object param)
	{
		VirtualCoinPassWord virtualCoinPassWord = passWords["NormalPassWord"];
		if (manager.IsStart)
		{
			ResidentFlow.GetInstance().GetTouchEffect().SetEnable(enable: false);
			ResidentFlow.GetInstance().GetMouseCursor().SetEnable(enable: false);
			virtualCoinPassWord.SetActive(isEnable: true);
			tenkeyGameObject.SetActive(value: true);
			notice.StartInputPassWord();
			help.Set(TestModeHelp.HelpType.ChangePassWord);
		}
		if (SingletonBehaviour<TestModeInput>.Instance.IsSpecialOperation())
		{
			ResidentFlow.GetInstance().GetTouchEffect().SetEnable(enable: true);
			ResidentFlow.GetInstance().GetMouseCursor().SetEnable(enable: true);
			virtualCoinPassWord.SetActive(isEnable: false);
			manager.Change(StateChangePassWord);
		}
		else if (SingletonBehaviour<TestModeInput>.Instance.IsMenuExecute())
		{
			ResidentFlow.GetInstance().GetTouchEffect().SetEnable(enable: true);
			ResidentFlow.GetInstance().GetMouseCursor().SetEnable(enable: true);
			virtualCoinPassWord.SetActive(isEnable: false);
			tenkeyGameObject.SetActive(value: false);
			manager.Change(StateExit);
		}
		else if (virtualCoinPassWord.IsFinished())
		{
			ResidentFlow.GetInstance().GetTouchEffect().SetEnable(enable: true);
			ResidentFlow.GetInstance().GetMouseCursor().SetEnable(enable: true);
			virtualCoinPassWord.SetActive(isEnable: false);
			tenkeyGameObject.SetActive(value: false);
			manager.Change(StateGetLog);
		}
	}

	private void StateChangePassWord(StateFuncManager manager, object param)
	{
		UnityAction<VirtualCoinPassWord, string> unityAction = delegate(VirtualCoinPassWord passWord, string nextStep)
		{
			if (manager.IsStepStart)
			{
				passWord.SetActive(isEnable: true);
			}
			if (passWord.IsFinished())
			{
				manager.SetStep(nextStep);
			}
		};
		string step = manager.Step;
		if (step == null || step.Length != 0)
		{
			switch (step)
			{
			case "old":
				unityAction(passWords["OldPassWord"], "try1");
				break;
			case "try1":
				unityAction(passWords["NewPassWordTry1"], "try2");
				break;
			case "try2":
				unityAction(passWords["NewPassWordTry2"], "check");
				break;
			case "check":
				if (manager.IsStepStart)
				{
					tenkeyGameObject.SetActive(value: false);
					VirtualCoinPassWord virtualCoinPassWord = passWords["OldPassWord"];
					VirtualCoinPassWord virtualCoinPassWord2 = passWords["NewPassWordTry1"];
					VirtualCoinPassWord virtualCoinPassWord3 = passWords["NewPassWordTry2"];
					KAMUNITY.Logger.Log("old:" + virtualCoinPassWord.InputWithHyphen() + ", try1:" + virtualCoinPassWord2.InputWithHyphen() + ", try2:" + virtualCoinPassWord3.InputWithHyphen());
					List<VirtualCoinPassWord> list = new List<VirtualCoinPassWord>();
					list.Add(virtualCoinPassWord);
					list.Add(virtualCoinPassWord2);
					list.Add(virtualCoinPassWord3);
					list.ForEach(delegate(VirtualCoinPassWord c)
					{
						c.SetActive(isEnable: false);
					});
					if (virtualCoinPassWord2.VInput != virtualCoinPassWord3.VInput)
					{
						notice.StartWrongConfirmationPassWord();
						manager.SetStep("result");
						return;
					}
					if (virtualCoinPassWord.VInput == virtualCoinPassWord2.VInput)
					{
						notice.StartNotChangePassWord();
						manager.SetStep("result");
						return;
					}
					notice.StartConfirming();
					AvsEa3Coin.Ea3EacoinOpchpass(virtualCoinPassWord.VInput, virtualCoinPassWord2.VInput);
				}
				switch (AvsEa3Coin.Ea3EacoinQuertGeneralStatus())
				{
				case (AvsEa3CoinTypes.EA3_EACOIN_ERROR)0:
					notice.StartChangedPassWord();
					manager.SetStep("result");
					break;
				case AvsEa3CoinTypes.EA3_EACOIN_ERROR.AUTHACCOUNT:
					notice.StartWrongPassWord();
					manager.SetStep("result");
					break;
				case AvsEa3CoinTypes.EA3_EACOIN_ERROR.MAINTENANCE:
					notice.StartPaseliMaintenance();
					manager.SetStep("result");
					break;
				case AvsEa3CoinTypes.EA3_EACOIN_ERROR.LOCATION:
				case AvsEa3CoinTypes.EA3_EACOIN_ERROR.DISABLED:
					notice.StartPaseliNotAvailable();
					manager.SetStep("result");
					break;
				default:
					notice.StartUnExpectedPassWordError();
					manager.SetStep("result");
					break;
				case AvsEa3CoinTypes.EA3_EACOIN_ERROR.PROCESSING:
					break;
				}
				break;
			case "result":
				if (manager.StepSec > TestModeBase.MessageWaitingSec)
				{
					manager.Change(StateExit);
				}
				break;
			}
		}
		else
		{
			notice.StartChangingPassWord();
			help.Set(TestModeHelp.HelpType.Exit);
			ArrowkeyLeft.SetActive(value: false);
			ArrowkeyRight.SetActive(value: false);
			SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
			manager.SetStep("old");
		}
		if (manager.Step != "check" && manager.Step != "result" && SingletonBehaviour<TestModeInput>.Instance.IsMenuExecute())
		{
			tenkeyGameObject.SetActive(value: false);
			manager.Change(StateExit);
		}
	}

	private void StateGetLog(StateFuncManager manager, object param)
	{
		string step = manager.Step;
		if (step == null || step.Length != 0)
		{
			switch (step)
			{
			case "wait":
				switch (AvsEa3Coin.Ea3EacoinQuertGeneralStatus())
				{
				case (AvsEa3CoinTypes.EA3_EACOIN_ERROR)0:
					notice.StartConfirmed();
					manager.SetStep("ok");
					break;
				case AvsEa3CoinTypes.EA3_EACOIN_ERROR.AUTHACCOUNT:
					notice.StartWrongPassWord();
					manager.SetStep("ng");
					break;
				case AvsEa3CoinTypes.EA3_EACOIN_ERROR.LOCATION:
				case AvsEa3CoinTypes.EA3_EACOIN_ERROR.DISABLED:
					notice.StartPaseliNotAvailable();
					manager.SetStep("ng");
					break;
				case AvsEa3CoinTypes.EA3_EACOIN_ERROR.MAINTENANCE:
					notice.StartPaseliMaintenance();
					manager.SetStep("ng");
					break;
				default:
					notice.StartPaseliUnExpectedError();
					manager.SetStep("ng");
					break;
				case AvsEa3CoinTypes.EA3_EACOIN_ERROR.PROCESSING:
					break;
				}
				break;
			case "ok":
				if (manager.StepSec > TestModeBase.MessageWaitingSec)
				{
					notice.Stop();
					manager.Change(StateSetLog);
				}
				break;
			case "ng":
				if (manager.StepSec > TestModeBase.MessageWaitingSec)
				{
					manager.Change(StateExit);
				}
				break;
			}
		}
		else
		{
			notice.StartConfirming();
			AvsEa3Coin.Ea3EacoinIncomelog(passWords["NormalPassWord"].VInput);
			manager.SetStep("wait");
		}
	}

	private void StateSetLog(StateFuncManager manager, object param)
	{
		AvsEa3Coin.Ea3EacoinIncomelogGet(out logs);
		Regex regex = new Regex("^\\d\\d$");
		AvsEa3CoinTypes.ea3_eacoin_log_last7days last7days = logs.last7days;
		List<int> list = new List<int>();
		list.Add(last7days.today);
		list.Add(last7days.average);
		list.Add(last7days.total);
		list.AddRange(last7days.summary);
		TestModeUtility.SetBookKeepingText(list, last7DaysObject);
		AvsEa3CoinTypes.ea3_eacoin_log_lastmonths lastmonths = logs.lastmonths;
		for (int i = 0; i < lastmonths.monthly.Length; i++)
		{
			AvsEa3CoinTypes.ea3_eacoin_log_monthly_item ea3_eacoin_log_monthly_item = lastmonths.monthly[i];
			GameObject gameObject = last2MonthsObject.FindChild($"Month{i + 1}");
			if (gameObject != null)
			{
				gameObject.GetComponent<Text>().text = ((i < lastmonths.monthly_len) ? $"{ea3_eacoin_log_monthly_item.year}-{ea3_eacoin_log_monthly_item.month:00}" : "");
				TestModeUtility.SetBookKeepingText(ea3_eacoin_log_monthly_item.summary, gameObject);
				gameObject.FindChildGameObjectsRecursively<Text>(regex).Skip(ea3_eacoin_log_monthly_item.mdays).ToList()
					.ForEach(delegate(GameObject o)
					{
						o.SetActive(value: false);
					});
			}
		}
		TestModeUtility.SetBookKeepingText(logs.last52weeks.summary, last52WeeksObject);
		TestModeUtility.SetBookKeepingText(logs.eachday.summary, eachDayObject);
		TestModeUtility.SetBookKeepingText(logs.eachhour.summary, eachHourObject);
		AvsEa3CoinTypes.ea3_eacoin_log_detail detail = logs.detail;
		Func<string, List<Text>> func = (string s) => detailObject.FindChild(s)?.FindChildComponents<Text>(regex);
		int num = 0;
		foreach (Text item in func("DateTime"))
		{
			item.text = ((num < detail.detail_len) ? detail.detail[num].date.Trim('\0') : "");
			num++;
		}
		num = 0;
		foreach (Text item2 in func("Consume"))
		{
			item2.text = ((num < detail.detail_len) ? TestModeUtility.BookKeepingTextConverter(detail.detail[num].consume) : "");
			num++;
		}
		num = 0;
		foreach (Text item3 in func("Supply"))
		{
			item3.text = ((num < detail.detail_len) ? TestModeUtility.BookKeepingTextConverter(detail.detail[num].service) : "");
			num++;
		}
		num = 0;
		foreach (Text item4 in func("Card"))
		{
			item4.text = ((num < detail.detail_len) ? detail.detail[num].cardno.Trim('\0') : "");
			num++;
		}
		help.Set(TestModeHelp.HelpType.IncomeLog);
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
		ArrowkeyUp.SetActive(value: false);
		ArrowkeyDown.SetActive(value: false);
		ArrowkeyLeft.SetActive(value: true);
		ArrowkeyRight.SetActive(value: true);
		manager.Change(StateLast7Days);
	}

	private void StateLast7Days(StateFuncManager manager, object param)
	{
		GameObject obj = last7DaysObject.gameObject;
		if (manager.IsStart)
		{
			AvsEa3CoinTypes.ea3_eacoin_log_last7days last7days = logs.last7days;
			SetSubSubTitle("<l>COIN DATA OF LAST 7 DAYS</l>");
			SetCountingText(last7days.sumfrom, last7days.sumto);
			if (last7days.processing != 0)
			{
				notice.StartPaseliErrorTryAgain();
			}
			else
			{
				obj.SetActive(value: true);
			}
		}
		CheckNextState(manager, StateLast2Months, StateDetail, delegate
		{
			obj.SetActive(value: false);
			notice.Stop();
		});
	}

	private void StateLast2Months(StateFuncManager manager, object param)
	{
		GameObject obj = last2MonthsObject.gameObject;
		if (manager.IsStart)
		{
			AvsEa3CoinTypes.ea3_eacoin_log_lastmonths lastmonths = logs.lastmonths;
			SetSubSubTitle("<l>COIN DATA OF LAST 2 MONTHS</l>");
			SetCountingText(lastmonths.sumfrom, lastmonths.sumto);
			if (lastmonths.processing != 0)
			{
				notice.StartPaseliErrorTryAgain();
			}
			else
			{
				obj.SetActive(value: true);
			}
		}
		CheckNextState(manager, StateLast52Weeks, StateLast7Days, delegate
		{
			obj.SetActive(value: false);
			notice.Stop();
		});
	}

	private void StateLast52Weeks(StateFuncManager manager, object param)
	{
		GameObject obj = last52WeeksObject.gameObject;
		if (manager.IsStart)
		{
			AvsEa3CoinTypes.ea3_eacoin_log_last52weeks last52weeks = logs.last52weeks;
			SetSubSubTitle("<l>COIN DATA OF LAST 52 WEEKS</l>");
			SetCountingText(last52weeks.sumfrom, last52weeks.sumto);
			if (last52weeks.processing != 0)
			{
				notice.StartPaseliErrorTryAgain();
			}
			else
			{
				obj.SetActive(value: true);
			}
		}
		CheckNextState(manager, StateEachDay, StateLast2Months, delegate
		{
			obj.SetActive(value: false);
			notice.Stop();
		});
	}

	private void StateEachDay(StateFuncManager manager, object param)
	{
		GameObject obj = eachDayObject.gameObject;
		if (manager.IsStart)
		{
			AvsEa3CoinTypes.ea3_eacoin_log_eachday eachday = logs.eachday;
			SetSubSubTitle("<l>COIN DATA OF EACH DAY</l>");
			SetCountingText(eachday.sumfrom, eachday.sumto);
			if (eachday.processing != 0)
			{
				notice.StartPaseliErrorTryAgain();
			}
			else
			{
				obj.SetActive(value: true);
			}
		}
		CheckNextState(manager, StateEachHour, StateLast52Weeks, delegate
		{
			obj.SetActive(value: false);
			notice.Stop();
		});
	}

	private void StateEachHour(StateFuncManager manager, object param)
	{
		GameObject obj = eachHourObject.gameObject;
		if (manager.IsStart)
		{
			AvsEa3CoinTypes.ea3_eacoin_log_eachhour eachhour = logs.eachhour;
			SetSubSubTitle("<l>COIN DATA OF EACH HOUR</l>");
			SetCountingText(eachhour.sumfrom, eachhour.sumto);
			if (eachhour.processing != 0)
			{
				notice.StartPaseliErrorTryAgain();
			}
			else
			{
				obj.SetActive(value: true);
			}
		}
		CheckNextState(manager, StateDetail, StateEachDay, delegate
		{
			obj.SetActive(value: false);
			notice.Stop();
		});
	}

	private void StateDetail(StateFuncManager manager, object param)
	{
		GameObject obj = detailObject.gameObject;
		if (manager.IsStart)
		{
			AvsEa3CoinTypes.ea3_eacoin_log_detail detail = logs.detail;
			SetSubSubTitle("<l>DETAIL</l>");
			SetCountingText(detail.sumdate);
			if (detail.processing != 0)
			{
				notice.StartPaseliErrorTryAgain();
			}
			else
			{
				obj.SetActive(value: true);
			}
		}
		CheckNextState(manager, StateLast7Days, StateEachHour, delegate
		{
			obj.SetActive(value: false);
			notice.Stop();
		});
	}

	private void StateExit(StateFuncManager manager, object param)
	{
		notice.Stop();
		manager.ChangeIdle();
		Exit();
	}

	private void CheckNextState(StateFuncManager manager, StateFuncManager.State go, StateFuncManager.State back, UnityAction action)
	{
		if (SingletonBehaviour<TestModeInput>.Instance.IsBookKeepingPageGo())
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
			manager.Change(StateExit);
		}
	}

	private void SetCountingText(string sumdate)
	{
		countingText.text = "集計日時\n";
		if (sumdate.Length == 0 || sumdate[0] == '\0')
		{
			countingText.text += "<color=red>未集計です</color>";
		}
		else
		{
			countingText.text += sumdate.Trim('\0');
		}
	}

	private void SetCountingText(string sumfrom, string sumto)
	{
		countingText.text = "集計期間\n";
		if (sumfrom.Length == 0 || sumto.Length == 0 || sumfrom[0] == '\0' || sumto[0] == '\0')
		{
			countingText.text += "<color=red>未集計です</color>";
			return;
		}
		Text text = countingText;
		text.text = text.text + sumfrom.Trim('\0') + "～" + sumto.Trim('\0');
	}
}
