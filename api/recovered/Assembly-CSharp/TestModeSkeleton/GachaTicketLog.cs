using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using KAMUNITY;
using TestModeSkeleton.TestModeExtensions;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

[RequireComponent(typeof(VirtualCoinPassWord))]
public class GachaTicketLog : TestModeBase
{
	[SerializeField]
	[Tooltip("ログオブジェクト")]
	private GameObject m_logObject;

	private VirtualCoinPassWord m_passWord;

	private string m_cardId = "";

	private AvsTypes.CardType m_cardType = AvsTypes.CardType.ICCARD;

	private XrpcResponseHelperSimple m_authPass = new XrpcResponseHelperSimple();

	private XrpcResponseHelper<XrpcModuleCardmngTypes.CardmngInquireResponse> m_inquire = new XrpcResponseHelper<XrpcModuleCardmngTypes.CardmngInquireResponse>();

	protected override string SubTitleName => "<l>GACHA TICKET LOG</l>";

	private IEnumerator Start()
	{
		help.Set(TestModeHelp.HelpType.Exit);
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
		ArrowkeyUp.SetActive(value: false);
		ArrowkeyDown.SetActive(value: false);
		ArrowkeyLeft.SetActive(value: false);
		ArrowkeyRight.SetActive(value: false);
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
		m_passWord = GetComponent<VirtualCoinPassWord>();
		Bi2xInput.Instance.ICReaderOn();
		StateFuncManager manager = new StateFuncManager(StateCardOn);
		while (true)
		{
			manager.Update();
			yield return null;
		}
	}

	private void StateCardOn(StateFuncManager manager, object param)
	{
		if (SingletonBehaviour<TestModeInput>.Instance.IsMenuExecute())
		{
			manager.Change(StateExit);
			return;
		}
		if (manager.IsStart)
		{
			notice.StartEAmusementPassOn();
		}
		if (Bi2xInput.Instance.IsCardOn())
		{
			m_cardType = Bi2xInput.Instance.GetCardType();
			m_cardId = Bi2xInput.Instance.GetCardID();
			manager.Change(StatePassWord);
		}
	}

	private void StatePassWord(StateFuncManager manager, object param)
	{
		if (SingletonBehaviour<TestModeInput>.Instance.IsMenuExecute())
		{
			ResidentFlow.GetInstance().GetTouchEffect().SetEnable(enable: true);
			ResidentFlow.GetInstance().GetMouseCursor().SetEnable(enable: true);
			m_passWord.SetActive(isEnable: false);
			manager.Change(StateExit);
			tenkeyGameObject.SetActive(value: false);
			return;
		}
		if (manager.IsStart)
		{
			ResidentFlow.GetInstance().GetTouchEffect().SetEnable(enable: false);
			ResidentFlow.GetInstance().GetMouseCursor().SetEnable(enable: false);
			m_passWord.SetActive(isEnable: true);
			notice.StartInputPassWordNumber();
			tenkeyGameObject.SetActive(value: true);
		}
		if (m_passWord.IsFinished())
		{
			ResidentFlow.GetInstance().GetTouchEffect().SetEnable(enable: true);
			ResidentFlow.GetInstance().GetMouseCursor().SetEnable(enable: true);
			m_passWord.SetActive(isEnable: false);
			manager.Change(StateGetLog);
			tenkeyGameObject.SetActive(value: false);
		}
	}

	private void StateGetLog(StateFuncManager manager, object param)
	{
		string step = manager.Step;
		if (step == null || step.Length != 0)
		{
			switch (step)
			{
			case "inquire_wait":
				if (!m_inquire.isEnd)
				{
					break;
				}
				if (!m_inquire.isError)
				{
					manager.SetStep("auth_pass_init");
					break;
				}
				switch (m_inquire.status.GetStatusCode())
				{
				case AvsEa3XrpcTypes.XPRC_STATUS_CODE.ENOTTIED:
					notice.StartEAmusementPassNoData();
					break;
				case AvsEa3XrpcTypes.XPRC_STATUS_CODE.ENRCARDID:
				case AvsEa3XrpcTypes.XPRC_STATUS_CODE.EXPCARDID:
					notice.StartEAmusementPassNoID();
					break;
				default:
					notice.StartEAmusementPassNetworkError();
					break;
				}
				manager.SetStep("ng");
				break;
			case "auth_pass_init":
				m_authPass = new XrpcResponseHelperSimple();
				XrpcModuleCardmng.XrpcMethodAuthpass.Request(m_inquire.result.refid, m_passWord.VInput, m_authPass.Callback, null);
				manager.SetStep("auth_pass_wait");
				break;
			case "auth_pass_wait":
				if (!m_authPass.isEnd)
				{
					break;
				}
				if (!m_authPass.isError)
				{
					notice.StartConfirmed();
					manager.SetStep("ok");
					break;
				}
				switch (m_authPass.status.GetStatusCode())
				{
				case AvsEa3XrpcTypes.XPRC_STATUS_CODE.ENRCARDID:
					notice.StartEAmusementPassNoID();
					break;
				case AvsEa3XrpcTypes.XPRC_STATUS_CODE.EPASSWD:
					notice.StartErrorAuthAccount();
					break;
				default:
					notice.StartEAmusementPassNetworkError();
					break;
				}
				manager.SetStep("ng");
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
			m_inquire = new XrpcResponseHelper<XrpcModuleCardmngTypes.CardmngInquireResponse>();
			XrpcModuleCardmng.XrpcMethodInquire.Request(null, m_cardType, m_cardId, update: false, m_inquire.Callback, null);
			manager.SetStep("inquire_wait");
		}
	}

	private void StateSetLog(StateFuncManager manager, object param)
	{
		List<GachaTicketLogSettings.SaveData.LogData> list = (from x in CabinetManager.GachaTicketLog.Data.Logs
			where x.RefID == m_inquire.result.refid
			orderby x.Date descending
			select x).Take(20).ToList();
		Regex regex = new Regex("^\\d\\d$");
		Func<string, List<Text>> func = (string s) => m_logObject.FindChild(s)?.FindChildComponents<Text>(regex);
		int num = 0;
		foreach (Text item in func("DateTime"))
		{
			item.text = ((num < list.Count) ? (TestModeUtility.ToDateString(list[num].Date) + " " + TestModeUtility.ToTimeString(list[num].Date)) : "");
			num++;
		}
		num = 0;
		foreach (Text item2 in func("Purchase"))
		{
			item2.text = ((num < list.Count) ? TestModeUtility.BookKeepingTextConverter(list[num].PurchaseCount) : "");
			num++;
		}
		num = 0;
		foreach (Text item3 in func("Consume"))
		{
			item3.text = ((num < list.Count) ? TestModeUtility.BookKeepingTextConverter(list[num].ConsumeCount) : "");
			num++;
		}
		num = 0;
		foreach (Text item4 in func("Card"))
		{
			item4.text = ((num < list.Count) ? (list[num].CardNumber.Substring(0, 4) + "...") : "");
			num++;
		}
		manager.Change(StateDisplayLog);
	}

	private void StateDisplayLog(StateFuncManager manager, object param)
	{
		if (SingletonBehaviour<TestModeInput>.Instance.IsMenuExecute())
		{
			manager.Change(StateExit);
		}
		else if (manager.IsStart)
		{
			m_logObject.SetActive(value: true);
		}
	}

	private void StateExit(StateFuncManager manager, object param)
	{
		notice.Stop();
		manager.ChangeIdle();
		Exit();
	}

	public override void Exit()
	{
		Bi2xInput.Instance.ICReaderOff();
		base.Exit();
	}
}
