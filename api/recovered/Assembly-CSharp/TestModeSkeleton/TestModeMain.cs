using System;
using System.Collections;
using System.Text;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameContents;
using KAM;
using KAMUNITY;
using MFG.GameRequest;
using TestModeSkeleton.CabinetDef;
using UnityEngine;

namespace TestModeSkeleton;

[RequireComponent(typeof(TestModeNotice))]
[RequireComponent(typeof(TestModeHelp))]
[RequireComponent(typeof(TestModeInput))]
[RequireComponent(typeof(TestModeLocalization))]
public class TestModeMain : TestModeBase
{
	[SerializeField]
	[Tooltip("シーン固有オブジェクト")]
	private GameObject[] uniqueObjects;

	[SerializeField]
	[Tooltip("デバッグ項目（Releaseで非表示となる）")]
	private GameObject[] debugElements;

	private bool bChildScene;

	protected override string TitleName => "<l>MAIN MENU</l>";

	private IEnumerator Start()
	{
		ResidentFlow.GetInstance().IsForceTestModeEffective = false;
		ResidentFlow.GetInstance().m_fadePanel.GetComponent<CanvasGroup>().DOFade(0f, 0f);
		LEDManager.Instance.Stop();
		if (!GameUtility.IsKonastaOrNetcafeBehavior)
		{
			Win10ActLog.GetLastActLog(out var info);
			StringBuilder stringBuilder = new StringBuilder(32);
			Win10ActLog.GetTimestampString(info.lastGenerateTime, stringBuilder, 32);
			Debug.Log("TestModeMain:アクティベーション情報" + stringBuilder);
		}
		GameUtility.SendPcbEvent(PCB_EVENT.EVENT.TEST_MODE);
		GameObject.Find("TestModeMain/Canvas/BlackScreen").SetActive(value: true);
		ResidentFlow instance = ResidentFlow.GetInstance();
		instance.IsTestModeRequest = false;
		instance.IsTestModeEffective = false;
		instance.IsErrorModeEffective = false;
		GameUtility.IsReservedGameOver = false;
		CreditCoinDisp.IsForceDisp = false;
		if (!string.IsNullOrEmpty(GameDataManager.Account.AppSessionID))
		{
			yield return UniTask.ToCoroutine(async delegate
			{
				try
				{
					await RequestApiBase<Logout, Logout.ResultData>.Send(enableErrorScreenTransition: false);
				}
				catch (Exception arg)
				{
					Debug.Log($"TestModeMain Logout exception:{arg}");
				}
			});
		}
		GameDataManager.Reset();
		DebugMenuItemRemove();
		menuElements.FindAll((TestModeMenuElement e) => e.Event == "LanguageOptions").ForEach(delegate(TestModeMenuElement e)
		{
			e.State = TestModeMenuElement.TextState.NotActive;
		});
		menuElements.FindAll((TestModeMenuElement e) => e.Event == "QCMode").ForEach(delegate(TestModeMenuElement e)
		{
			e.State = TestModeMenuElement.TextState.NotActive;
		});
		menuElements.Find((TestModeMenuElement e) => e.Event == "EventMode").State = ((!GameUtility.IsFreePlay()) ? TestModeMenuElement.TextState.UnSelctable : TestModeMenuElement.TextState.Default);
		help.Set(TestModeHelp.HelpType.SelectExecute);
		notice.Stop();
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyUp.SetActive(value: true);
		ArrowkeyDown.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
		ArrowkeyLeft.SetActive(value: false);
		ArrowkeyRight.SetActive(value: false);
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
		SoundManager.StopAll().Forget();
		ResidentFlow.GetInstance().SetInGameDisp(isDisp: false, edgeTouchingWarning: false, OnlineUpdateMessage.DispType.Detail);
		Bi2xInput.Instance.SetPillarLED(Color.black);
		Bi2xInput.Instance.EnableUsbCharger(is_open: false);
		Bi2xInput.Instance.ICReaderOff();
		yield return CoInitializeBeforeTestMode();
		GameObject.Find("TestModeMain/Canvas/BlackScreen").SetActive(value: false);
		if (IsPayable())
		{
			notice.Stop();
		}
		else
		{
			notice.StartPaymentWarning();
		}
		StartOrResumeMainCoroutine();
		CabinetManager.Book.DisposeGamePlayTime();
		StartCoroutine(CoInputCheckToQcmode());
	}

	protected override IEnumerator vOnBeforeUnload()
	{
		KAMUNITY.Logger.Log("テストモード終了");
		ResidentFlow instance = ResidentFlow.GetInstance();
		instance.IsTestModeRequest = false;
		instance.IsTestModeEffective = true;
		instance.IsErrorModeEffective = true;
		instance.IsForceTestModeEffective = true;
		instance.LoadScene("Logo");
		yield break;
	}

	protected override void OnCall(TestModeMenuElement element)
	{
		Array.ForEach(uniqueObjects, delegate(GameObject o)
		{
			o.SetActive(value: false);
		});
		SetSoftIdCodeDisp(disp: false);
		bChildScene = true;
		global::SingletonBehaviour<OnlineUpdateMessage>.Instance.SetDispType(OnlineUpdateMessage.DispType.None);
		notice.Stop();
	}

	protected override void OnReturn(TestModeMenuElement element)
	{
		global::SingletonBehaviour<OnlineUpdateMessage>.Instance.SetDispType(OnlineUpdateMessage.DispType.Detail);
		Array.ForEach(uniqueObjects, delegate(GameObject o)
		{
			o.SetActive(value: true);
		});
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyUp.SetActive(value: true);
		ArrowkeyDown.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
		ArrowkeyLeft.SetActive(value: false);
		ArrowkeyRight.SetActive(value: false);
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
		SetSoftIdCodeDisp(disp: true);
		bChildScene = false;
		menuElements.Find((TestModeMenuElement e) => e.Event == "EventMode").State = ((!GameUtility.IsFreePlay()) ? TestModeMenuElement.TextState.UnSelctable : TestModeMenuElement.TextState.Default);
		if (IsPayable())
		{
			notice.Stop();
		}
		else
		{
			notice.StartPaymentWarning();
		}
	}

	private IEnumerator CoInitializeBeforeTestMode()
	{
		DeviceManager.Instance.PreEnterTestMode();
		if (GameUtility.IsKamunityBootEnable())
		{
			AvsEa3Coin.Ea3EacoinAbort();
			yield return new WaitUntil(() => AvsEa3Coin.Ea3EacoinQueryAbortStatus() != AvsEa3CoinTypes.EA3_EACOIN_ERROR.PROCESSING);
			AvsEa3Coin.Ea3EacoinReport(AvsEa3CoinTypes.REPORT_TYPE.RESET);
			AvsEa3.Ea3PosevReport("/coin/reset");
			PaseliUtility.Clear();
		}
	}

	private void DebugMenuItemRemove()
	{
		if (debugElements == null)
		{
			return;
		}
		for (int i = 0; i < debugElements.Length; i++)
		{
			debugElements[i].SetActive(value: false);
		}
		int num = menuElements.Count - 1;
		while (0 <= num)
		{
			if (menuElements[num].Event == "CardTest")
			{
				menuElements.RemoveAt(num);
			}
			num--;
		}
	}

	private bool IsPayable()
	{
		if (CabinetManager.Coin.Data.FreePlay != CoinOptionsDef.FreePlay.On)
		{
			if (TestModeUtility.IsPayableAtCoinSettings())
			{
				return TestModeUtility.IsPayableAtVirtualCoinSettingsWithActiveSchedule();
			}
			return false;
		}
		return true;
	}

	protected IEnumerator CoInputCheckToQcmode()
	{
		int triggerCount = 0;
		float pushTimer = 0f;
		float releseTimer = 0f;
		while (true)
		{
			if (SingletonBehaviour<TestModeInput>.Instance.IsTrigger(TestModeInput.Key.Test))
			{
				pushTimer = 0f;
				releseTimer = 0f;
				triggerCount++;
			}
			if (SingletonBehaviour<TestModeInput>.Instance.IsPress(TestModeInput.Key.Test))
			{
				pushTimer += Time.deltaTime;
				if (triggerCount == 3 && pushTimer >= 5f)
				{
					break;
				}
			}
			else
			{
				releseTimer += Time.deltaTime;
				if (releseTimer >= 0.5f)
				{
					releseTimer = 0f;
					pushTimer = 0f;
					triggerCount = 0;
				}
			}
			yield return new WaitForEndOfFrame();
		}
		menuElements.FindAll((TestModeMenuElement e) => e.Event == "QCMode").ForEach(delegate(TestModeMenuElement e)
		{
			e.State = TestModeMenuElement.TextState.Default;
		});
	}
}
