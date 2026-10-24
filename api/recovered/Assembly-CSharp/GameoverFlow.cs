using System;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameContents;
using MFG.GameRequest;
using TestModeSkeleton;
using UnityEngine;

public class GameoverFlow : BaseFlow
{
	[SerializeField]
	private GameObject m_gameover;

	[SerializeField]
	private GameObject m_dataSaveFailed;

	private GameoverFlow()
	{
		_enableDebugLogin = false;
	}

	protected override async UniTask OnInitialize(CancellationToken cancellationToken)
	{
		try
		{
			if (m_dataSaveFailed != null)
			{
				m_dataSaveFailed.SetActive(GameDataManager.PlayerConfig.IsDataSaveFailed);
			}
		}
		catch (OperationCanceledException)
		{
			Debug.Log(GetType().Name + MethodBase.GetCurrentMethod().Name + ": OperationCanceledException Catched.");
		}
	}

	protected override async UniTask OnExecute(CancellationToken cancellationToken)
	{
		_ = 5;
		try
		{
			SoundManager.PlayBgm("BGM_GAMEOVER").Forget();
			SoundManager.PlayMyCharaVoice("VOX_GXX_END1").Forget();
			if (GameDataManager.Account.IsGuestUser)
			{
				m_gameover.GetComponent<Animator>().SetTrigger("NoCard");
				await UniTask.WaitUntil(() => m_gameover.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("GameOverLoopNoCard"));
			}
			else
			{
				m_gameover.GetComponent<Animator>().GetComponent<Animator>().SetTrigger("WithCard");
				await UniTask.WaitUntil(() => m_gameover.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("GameOverLoopWithCard"));
			}
			await UniTask.Delay(TimeSpan.FromSeconds(0.5), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(8.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0), PlayerLoopTiming.Update, cancellationToken));
			m_gameover.GetComponent<Animator>().GetComponent<Animator>().SetTrigger("Out");
			await UniTask.WaitWhile(() => m_gameover.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime < 1f, PlayerLoopTiming.Update, cancellationToken);
			await ExitProcess();
			SoundManager.StopBgmWithFadeout().Forget();
			if ((GameUtility.IsNetcafeBehavior && GameUtility.IsAppTerminatDecided) || (GameUtility.IsCloudStreaming && Cloudstrap.IsDisconnect))
			{
				GotoNextScene("ScopeClose");
			}
			else
			{
				GotoNextScene("Logo");
			}
		}
		catch (OperationCanceledException)
		{
			Debug.Log(GetType().Name + MethodBase.GetCurrentMethod().Name + ": OperationCanceledException Catched.");
		}
	}

	private async UniTask ExitProcess()
	{
		if (!string.IsNullOrEmpty(GameDataManager.Account.AppSessionID))
		{
			try
			{
				await RequestApiBase<Logout, Logout.ResultData>.Send(enableErrorScreenTransition: false);
			}
			catch (Exception arg)
			{
				Debug.Log($"{GetType().Name}{MethodBase.GetCurrentMethod().Name}: Exception Catched:{arg}");
			}
		}
		CabinetManager.Book.EndGamePlayTime();
		PaseliUtility.CheckOut();
		if (GameDataManager.Account.IsGuestUser)
		{
			GameUtility.SendPcbEvent(PCB_EVENT.EVENT.GUEST_END);
		}
		Bi2xInput.Instance.EnableUsbCharger(is_open: false);
		GameUtility.SendPcbEvent(PCB_EVENT.EVENT.GAME_OVER);
		GameDataManager.Reset();
		Debug.Log("UnloadUnusedAssets");
		await Resources.UnloadUnusedAssets();
	}
}
