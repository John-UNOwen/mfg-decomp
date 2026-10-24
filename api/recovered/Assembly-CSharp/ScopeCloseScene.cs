using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameRequest;
using UnityEngine;

public class ScopeCloseScene : MonoBehaviour
{
	public CancellationTokenSource m_cancellationTokenSource;

	public CancellationToken m_cancellationToken;

	protected async void Start()
	{
		m_cancellationTokenSource = new CancellationTokenSource();
		m_cancellationToken = m_cancellationTokenSource.Token;
		await UniTask.WaitUntil(() => ResidentFlow.IsInitialized);
		SingletonBehaviour<Loading>.Instance.DisConnect();
		ResidentFlow.GetInstance().IsAppTerminateEffective = false;
		ResidentFlow.GetInstance().IsAppTerminateConfirmRequest = false;
		if (ScopeManager.Instance != null)
		{
			ScopeManager.Instance.OnConfirmNo();
		}
		SoundManager.StopAll().Forget();
		SoundManager.SetSpeakerVolume(0f);
		SoundManager.SetHeadPhoneVolume(0f);
		ResidentFlow.GetInstance().SetInGameDisp(isDisp: false);
		await CoMain(m_cancellationToken);
	}

	private async UniTask CoMain(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		Cloudstrap.UserEvent.End(Cloudstrap.UserEventType.Play);
		if (!string.IsNullOrEmpty(GameDataManager.Account.AppSessionID))
		{
			try
			{
				await RequestApiBase<Logout, Logout.ResultData>.Send(enableErrorScreenTransition: false);
			}
			catch (Exception arg)
			{
				Debug.Log($"ErrorMode Logout exception:{arg}");
			}
			GameDataManager.Reset();
		}
		if (SampleDriver.useCapture)
		{
			Debug.Log("Call ScopeCloudGamingTermStart()");
			ScopeLib.ScopeCloudGamingTermStart();
			float startTime = Time.time / Time.timeScale;
			int lapsedSec = -1;
			while (true)
			{
				if (ScopeLib.ScopeCloudGamingTermCheck() == 0)
				{
					Debug.Log("ScopeCloudGamingTermCheck: OK.");
					break;
				}
				float num = Time.time / Time.timeScale - startTime;
				if (num > 15f)
				{
					Debug.Log("ScopeCloudGamingTermCheck: TimeOut.");
					break;
				}
				if (lapsedSec != (int)num)
				{
					lapsedSec = (int)num;
					Debug.Log($"ScopeCloudGamingTermCheck: {lapsedSec} sec");
				}
				await UniTask.Yield();
			}
			Debug.Log("call cstrap_terminate_scope()");
			ScopeLib.cstrap_terminate_scope();
		}
		Debug.Log("ScopeCloseScene finished.");
		Application.Quit();
		await UniTask.Yield();
	}
}
