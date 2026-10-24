using System;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using IO;
using KAMUNITY;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ErrorModeMenu : BaseFlow
{
	private enum ExitType
	{
		Service,
		Test,
		OldCompatible
	}

	[SerializeField]
	private ErrorModeDialog m_Dialog;

	[SerializeField]
	private GameObject m_ObjHelpService;

	[SerializeField]
	private GameObject m_ObjHelpTest;

	[SerializeField]
	private Button _endButton;

	private ErrorLogData m_errorLogData;

	private bool IsKonastaOrNetcafeBehavior => GameUtility.IsKonastaOrNetcafeBehavior;

	private ErrorModeMenu()
	{
		_enableDebugLogin = false;
	}

	protected override async UniTask OnInitialize(CancellationToken cancellationToken)
	{
		try
		{
			m_errorLogData = ErrorControl.GetLastError();
			if (m_errorLogData == null)
			{
				m_errorLogData = new ErrorLogData();
			}
			if (IsKonastaOrNetcafeBehavior)
			{
				ResidentFlow.GetInstance().IsTestModeEffective = false;
				ResidentFlow.GetInstance().IsErrorModeEffective = false;
			}
			else
			{
				ResidentFlow.GetInstance().IsTestModeEffective = m_errorLogData.m_level != ERROR_LEVEL.HALT;
				ResidentFlow.GetInstance().IsErrorModeEffective = false;
			}
			ErrorData.Initialize();
			ErrorData.Item item = ErrorData.GetInstance().GetItem(m_errorLogData.m_code, m_errorLogData.m_option);
			if (IsKonastaOrNetcafeBehavior)
			{
				item.title = "エラーが発生しました";
				item.message = ErrorData.GetInstance().ExtraMessage;
				item.message = item.message + (string.IsNullOrEmpty(item.message) ? "" : "\n") + "終了後、しばらく待ってからもう一度お試しください。";
				m_Dialog.SetData(item);
				_endButton.gameObject.SetActive(value: true);
			}
			else
			{
				m_Dialog.SetData(item, ErrorData.GetInstance().ExtraMessage);
				_endButton.gameObject.SetActive(value: false);
			}
			ErrorData.GetInstance().ExtraMessage = "";
			ResidentFlow.GetInstance().SetInGameDisp(isDisp: false, edgeTouchingWarning: false);
			if (IsKonastaOrNetcafeBehavior)
			{
				m_ObjHelpService.SetActive(value: false);
				m_ObjHelpTest.SetActive(value: false);
			}
			else
			{
				m_ObjHelpService.SetActive(m_errorLogData.m_level < ERROR_LEVEL.ERROR_UNCANCELABLE);
				m_ObjHelpTest.SetActive(m_errorLogData.m_level != ERROR_LEVEL.HALT);
			}
			DeviceManager.Instance.AllActivate(bActive: false);
			Bi2xInput.Instance.ICReaderOff();
			CreditCoinDisp.IsForceDisp = true;
			SoundManager.StopAll().Forget();
			SoundManager.SetSpeakerVolume(0f);
			SoundManager.SetHeadPhoneVolume(0f);
			Bi2xInput.Instance.SetPillarLED(Color.black);
			if (KamunityInstance.isEa3Boot)
			{
				AvsEa3Coin.Ea3EacoinAbort();
				await UniTask.WaitUntil(() => AvsEa3Coin.Ea3EacoinQueryAbortStatus() != AvsEa3CoinTypes.EA3_EACOIN_ERROR.PROCESSING, PlayerLoopTiming.Update, cancellationToken);
			}
		}
		catch (OperationCanceledException)
		{
			Debug.Log(GetType().Name + MethodBase.GetCurrentMethod().Name + ": OperationCanceledException Catched.");
		}
	}

	protected override async UniTask OnExecute(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (IsKonastaOrNetcafeBehavior)
		{
			_endButton.OnClickAsObservable().Subscribe(delegate
			{
				Debug.Log("End button clicked.");
				_endButton.enabled = false;
				GameUtility.IsAppTerminatDecided = true;
				ResidentFlow.GetInstance().IsAppTerminateEffective = false;
				ResidentFlow.GetInstance().IsAppTerminated = true;
				GotoNextScene("ScopeClose");
			}).AddTo(this);
			return;
		}
		try
		{
			if (m_errorLogData != null && m_errorLogData.m_level != ERROR_LEVEL.HALT)
			{
				ExitType exitType = await ExitWait(cancellationToken);
				await ResetIOError(cancellationToken);
				ExecExit(exitType);
			}
		}
		catch (OperationCanceledException)
		{
			Debug.Log(GetType().Name + MethodBase.GetCurrentMethod().Name + ": OperationCanceledException Catched.");
		}
	}

	private async UniTask<ExitType> ExitWait(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		ExitType exitType = ExitType.Service;
		await UniTask.WaitUntil(delegate
		{
			if (m_errorLogData.m_level < ERROR_LEVEL.ERROR_UNCANCELABLE && Bi2xInput.IsTrigger(BUTTON.SERVICE))
			{
				exitType = ExitType.Service;
				return true;
			}
			if (Bi2xInput.IsPress(BUTTON.TEST))
			{
				exitType = ExitType.Test;
				return true;
			}
			if (ErrorControl.GetLastError() == null)
			{
				exitType = ExitType.OldCompatible;
				return true;
			}
			return false;
		}, PlayerLoopTiming.Update, cancellationToken);
		return exitType;
	}

	private async UniTask ResetIOError(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		Bi2xInput.Instance.ResetError();
		if (GameUtility.IsIOEnable)
		{
			await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			await UniTask.WaitUntil(() => Bi2xInput.IsBi2xReady, PlayerLoopTiming.Update, cancellationToken);
		}
	}

	private void ExecExit(ExitType exitType)
	{
		switch (exitType)
		{
		case ExitType.Service:
			ResetError();
			ResidentFlow.GetInstance().IsTestModeRequest = false;
			ResidentFlow.GetInstance().IsTestModeEffective = true;
			ResidentFlow.GetInstance().IsErrorModeEffective = true;
			GotoNextScene("Logo");
			break;
		case ExitType.Test:
			ResetError();
			ResidentFlow.GetInstance().IsTestModeRequest = true;
			OnDestroy();
			break;
		case ExitType.OldCompatible:
			ResetError();
			ResidentFlow.GetInstance().IsTestModeRequest = false;
			OnDestroy();
			break;
		}
	}

	private void ResetError()
	{
		SoundManager.StopAll().Forget();
		ErrorControl.ResetError();
		m_errorLogData = null;
		ResidentFlow.GetInstance().IsErrorModeRequest = false;
	}

	public void OnDestroy()
	{
		CreditCoinDisp.IsForceDisp = false;
		DeviceManager.Instance.AllActivate(bActive: true);
		ResidentFlow.GetInstance().IsErrorModeEffective = true;
	}
}
