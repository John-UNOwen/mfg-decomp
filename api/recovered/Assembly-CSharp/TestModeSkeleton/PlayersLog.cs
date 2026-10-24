using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using KAMUNITY;
using TestModeSkeleton.TestModeExtensions;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

[RequireComponent(typeof(VirtualCoinPassWord))]
public class PlayersLog : VirtualCoinBase
{
	[SerializeField]
	[Tooltip("ログオブジェクト")]
	private GameObject logObject;

	[SerializeField]
	[Tooltip("ログ情報")]
	private Text logInfoText;

	[SerializeField]
	private Image qrImage;

	[SerializeField]
	private GameObject qrImageParent;

	private VirtualCoinPassWord passWord;

	private AvsEa3CoinTypes.ea3_eacoin_log_detail log;

	private string m_cardId = "";

	private AvsTypes.CardType m_cardType = AvsTypes.CardType.ICCARD;

	private float qrTime;

	protected override string SubTitleName => "<l>PLAYER'S LOG</l>";

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
		passWord = GetComponent<VirtualCoinPassWord>();
		logInfoText.text = "";
		qrImageParent.SetActive(value: false);
		bool flag = GameUtility.IsKamunityBootEnable() && !GameUtility.IsKonastaOrNetcafeBehavior && GameDataManager.IsQRLoginEnable();
		if (flag)
		{
			qrTime = Time.time;
		}
		else
		{
			Bi2xInput.Instance.ICReaderOn();
		}
		StateFuncManager manager = new StateFuncManager(flag ? new StateFuncManager.State(StateQRLoad) : new StateFuncManager.State(StateCardOn));
		while (true)
		{
			manager.Update();
			yield return null;
		}
	}

	private void StateQRLoad(StateFuncManager manager, object param)
	{
		if (Time.time - qrTime >= 20f)
		{
			Bi2xInput.Instance.ICReaderOn();
			manager.Change(StateCardOn);
			return;
		}
		QRLogin.STATUS status = QRLogin.GetStatus();
		if (status == QRLogin.STATUS.EAX_SPPASS_ERROR_CLOSE)
		{
			qrTime = Time.time;
			QRLogin.Begin();
			manager.Change(StateQRDisp);
		}
		if (status == QRLogin.STATUS.EAX_SPPASS_ERROR_MAINTENANCE)
		{
			Bi2xInput.Instance.ICReaderOn();
			manager.Change(StateCardOn);
		}
	}

	private void StateQRDisp(StateFuncManager manager, object param)
	{
		if (Time.time - qrTime >= 20f)
		{
			QRLogin.End();
			Bi2xInput.Instance.ICReaderOn();
			manager.Change(StateCardOn);
		}
		else if (QRLogin.GetStatus() >= QRLogin.STATUS.EAX_SPPASS_ERROR_PROMPTED)
		{
			try
			{
				qrImage.sprite = QRLogin.GetQRCode();
				qrImageParent.SetActive(value: true);
			}
			catch
			{
			}
			Bi2xInput.Instance.ICReaderOn();
			manager.Change(StateCardOn);
		}
	}

	private void StateCardOn(StateFuncManager manager, object param)
	{
		if (SingletonBehaviour<TestModeInput>.Instance.IsMenuExecute())
		{
			QRLogin.End();
			qrImageParent.SetActive(value: false);
			manager.Change(StateExit);
			return;
		}
		if (manager.IsStart)
		{
			if (qrImageParent.activeSelf)
			{
				notice.StartEAmusementPassAndQROn();
			}
			else
			{
				notice.StartEAmusementPassOn();
			}
		}
		if (Bi2xInput.Instance.IsCardOn())
		{
			m_cardType = Bi2xInput.Instance.GetCardType();
			m_cardId = Bi2xInput.Instance.GetCardID();
			QRLogin.End();
			qrImageParent.SetActive(value: false);
			manager.Change(StatePassWord);
		}
		if (qrImageParent.activeSelf)
		{
			(int, string) cardTypeID = QRLogin.GetCardTypeID();
			if (cardTypeID.Item1 != -1 && !string.IsNullOrEmpty(cardTypeID.Item2))
			{
				m_cardType = (AvsTypes.CardType)cardTypeID.Item1;
				m_cardId = cardTypeID.Item2;
				QRLogin.End();
				qrImageParent.SetActive(value: false);
				manager.Change(StatePassWord);
			}
		}
	}

	private void StatePassWord(StateFuncManager manager, object param)
	{
		if (SingletonBehaviour<TestModeInput>.Instance.IsMenuExecute())
		{
			ResidentFlow.GetInstance().GetTouchEffect().SetEnable(enable: true);
			ResidentFlow.GetInstance().GetMouseCursor().SetEnable(enable: true);
			passWord.SetActive(isEnable: false);
			manager.Change(StateExit);
			tenkeyGameObject.SetActive(value: false);
			return;
		}
		if (manager.IsStart)
		{
			ResidentFlow.GetInstance().GetTouchEffect().SetEnable(enable: false);
			ResidentFlow.GetInstance().GetMouseCursor().SetEnable(enable: false);
			passWord.SetActive(isEnable: true);
			notice.StartInputPassWordNumber();
			tenkeyGameObject.SetActive(value: true);
		}
		if (passWord.IsFinished())
		{
			ResidentFlow.GetInstance().GetTouchEffect().SetEnable(enable: true);
			ResidentFlow.GetInstance().GetMouseCursor().SetEnable(enable: true);
			passWord.SetActive(isEnable: false);
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
			case "wait":
				switch (AvsEa3Coin.Ea3EacoinQuertGeneralStatus())
				{
				case (AvsEa3CoinTypes.EA3_EACOIN_ERROR)0:
					notice.StartConfirmed();
					manager.SetStep("ok");
					break;
				case AvsEa3CoinTypes.EA3_EACOIN_ERROR.AUTHACCOUNT:
					notice.StartErrorAuthAccount();
					manager.SetStep("ng");
					break;
				case AvsEa3CoinTypes.EA3_EACOIN_ERROR.LOCATION:
				case AvsEa3CoinTypes.EA3_EACOIN_ERROR.DISABLED:
				case AvsEa3CoinTypes.EA3_EACOIN_ERROR.NOACCOUNT:
				case AvsEa3CoinTypes.EA3_EACOIN_ERROR.NOTECCARD:
				case AvsEa3CoinTypes.EA3_EACOIN_ERROR.ECINVALID:
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
			AvsEa3Coin.Ea3EacoinPlayerlog(m_cardType, m_cardId, passWord.VInput);
			manager.SetStep("wait");
		}
	}

	private void StateSetLog(StateFuncManager manager, object param)
	{
		AvsEa3Coin.Ea3EacoinPlayerlogGet(out log);
		Regex regex = new Regex("^\\d\\d$");
		Func<string, List<Text>> func = (string s) => logObject.FindChild(s)?.FindChildComponents<Text>(regex);
		int num = 0;
		foreach (Text item in func("DateTime"))
		{
			item.text = ((num < log.detail_len) ? log.detail[num].date.Trim('\0') : "");
			num++;
		}
		num = 0;
		foreach (Text item2 in func("Consume"))
		{
			item2.text = ((num < log.detail_len) ? TestModeUtility.BookKeepingTextConverter(log.detail[num].consume) : "");
			num++;
		}
		num = 0;
		foreach (Text item3 in func("Supply"))
		{
			item3.text = ((num < log.detail_len) ? TestModeUtility.BookKeepingTextConverter(log.detail[num].service) : "");
			num++;
		}
		num = 0;
		foreach (Text item4 in func("Card"))
		{
			item4.text = ((num < log.detail_len) ? log.detail[num].cardno.Trim('\0') : "");
			num++;
		}
		string arg = ((log.sumdate[0] == '\0') ? "<color=red>未集計です</color>" : log.sumdate.Trim('\0'));
		logInfoText.text = $"集計日時\n{arg}\n残高 {log.balance}";
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
			if (log.processing != 0)
			{
				notice.StartPaseliErrorTryAgain();
			}
			else
			{
				logObject.SetActive(value: true);
			}
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
		QRLogin.End();
		Bi2xInput.Instance.ICReaderOff();
		base.Exit();
	}
}
