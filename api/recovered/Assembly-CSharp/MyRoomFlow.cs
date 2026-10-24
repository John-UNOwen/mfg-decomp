using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Konami.System.Net;
using MFG.GameData;
using MFG.GameRequest;
using MFG.MahjongPlay.Cutin;
using MFG.Types;
using MFG.Types.Customize.Catchphrase;
using MFG.Types.Customize.Costume;
using MFG.Types.Mission;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MyRoomFlow : BaseFlow
{
	public enum WindowType
	{
		None,
		FavoWindow,
		FanWindow,
		SpiritWindow,
		CutinWindow,
		CustomizeWindow,
		CharaSelectWindow,
		CatchphraseWindow,
		MatchingBGWindow,
		FriendConfirmWindow,
		StickerEditor
	}

	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	public GameObject m_charaMove;

	[SerializeField]
	public StandingCharaManager m_charaManager;

	[SerializeField]
	private GameObject m_introduction;

	[SerializeField]
	private MyRoomMainWindow m_mainWindow;

	[SerializeField]
	private MyRoomCharaStatus m_charaStatus;

	[SerializeField]
	private DebutEffect m_debutEffect;

	[SerializeField]
	private Button m_btCharaChange;

	[SerializeField]
	private Button m_backButton;

	[SerializeField]
	private MissionWindowAcquisition m_missionWindowAcquisition;

	public CutinManager CutinManager;

	private bool _isAfterIntroduction;

	public bool IsGotoNextScene;

	protected override async UniTask OnInitialize(CancellationToken cancellationToken)
	{
		SoundManager.PlayMyCharaMyRoomBGM().Forget();
		ResidentFlow.GetInstance().GetHeader().SetDisp(disp: true);
		ResidentFlow.GetInstance().GetHeader().SetSceneName("マイルーム");
		try
		{
			cancellationToken.ThrowIfCancellationRequested();
			m_charaStatus.SetStatus();
			m_mainWindow.Init(this, cancellationToken);
			m_debutEffect.gameObject.SetActive(value: false);
			m_btCharaChange.interactable = !GameDataManager.Player.IsPro();
			(from _ in m_btCharaChange.OnClickAsObservable()
				where _isAfterIntroduction
				select _).Subscribe(async delegate
			{
				try
				{
					await m_mainWindow.CharaSelectWindowOpen(cancellationToken);
				}
				catch (OperationCanceledException)
				{
					Debug.Log("MyRoomFlow.MyRoomFavoDisplay.Onclick Canceled");
				}
			}).AddTo(this);
			(from _ in m_backButton.OnClickAsObservable()
				where _isAfterIntroduction
				select _).Subscribe(async delegate
			{
				await GotoNextScene();
			}).AddTo(this);
			base.OnTimerEnd.Where((Unit _) => _isAfterIntroduction).First().Subscribe(async delegate
			{
				await GotoNextScene();
			})
				.AddTo(this);
			await m_charaManager.SingleCharaLoad(GameDataManager.Player.GetCurrentChara().CostumeItemEquip);
		}
		catch (OperationCanceledException ex)
		{
			Debug.Log("OperationCanceledException Catched. " + ex.Message);
		}
	}

	protected override async UniTask OnExecute(CancellationToken cancellationToken)
	{
		try
		{
			ResidentFlow.GetInstance().GetTimer().MaxTime = (GameDataManager.Account.IsPremiumMode() ? 199 : 99);
			ResidentFlow.GetInstance().GetTimer().SetTimer(GameDataManager.Account.IsPremiumMode() ? 199 : 99, disp: true);
			ResidentFlow.GetInstance().GetTimer().StartTimer();
			m_charaMove.GetComponent<Animator>().SetTrigger("In");
			SoundManager.PlayVoice("VOX_GXX_APPEAL1", GameDataManager.Player.SelectChara).Forget();
			await ExecIntroduction(cancellationToken);
			_isAfterIntroduction = true;
		}
		catch (OperationCanceledException ex)
		{
			Debug.Log("OperationCanceledException Catched. " + ex.Message);
		}
	}

	private async UniTask ExecIntroduction(CancellationToken cancellationToken)
	{
		if (!GameDataManager.PlayerConfig.IsDispHelpFirstPlayMyRoomScene && !DebugFlag.IsActive(DebugFlag.Flag.SkipGuideDisp))
		{
			await GuideDisp(cancellationToken);
			GameDataManager.PlayerConfig.IsDispHelpFirstPlayMyRoomScene = true;
		}
		await MissionOpen();
		SetFightSpirit(cancellationToken);
	}

	public void CharaStatusRefresh(CancellationToken cancellationToken)
	{
		m_mainWindow.Refresh(cancellationToken);
		m_charaStatus.SetStatus();
	}

	public void StoryPlay()
	{
		GameDataManager.Story.m_forcePlaying = false;
		GameDataManager.Story.m_forceEventStoryPlaying = false;
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		SoundManager.StopBgmWithFadeout().Forget();
		GotoNextScene("StoryPlay");
	}

	public async UniTask DebutPlay(CharaType charaType, MFG.Types.Customize.Costume.ItemID itemID, CancellationToken cancellationToken = default(CancellationToken))
	{
		m_debutEffect.gameObject.SetActive(value: true);
		await m_debutEffect.Play(charaType, itemID, cancellationToken);
	}

	public void SetFightSpirit(CancellationToken cancellationToken)
	{
		m_charaStatus.SetFightSpirit(cancellationToken);
	}

	public void SetCatchphrase(MFG.Types.Customize.Catchphrase.ItemID itemID, CancellationToken cancellationToken)
	{
		m_charaStatus.SetCatchphrase(itemID, cancellationToken);
	}

	public async UniTask MissionOpen()
	{
		await m_missionWindowAcquisition.Open(JudgeLocation.MyRoom);
	}

	public void SceneButtonSetActive(bool isActive)
	{
		m_backButton.gameObject.SetActive(isActive);
		m_btCharaChange.gameObject.SetActive(isActive);
	}

	public async UniTask OdekakeDecide(FriendInfo friend)
	{
		if (!IsGotoNextScene && !GameDataManager.Player.IsOdekake)
		{
			try
			{
				Debug.Log("OdekakeDecide");
				GameDataManager.Player.BeforeChara = GameDataManager.Player.SelectChara;
				GameDataManager.Player.OdekakeChara = friend.CharaType;
				GameDataManager.Player.SelectChara = GameDataManager.Player.OdekakeChara;
				OdekakeDone.ResultData resultData = await RequestApiBase<OdekakeDone, OdekakeDone.ResultData, long>.Send(friend.OdekakeDataID);
				GameDataManager.Player.OdekakeCharaGameData = resultData.OdekakeInfo.CharaData;
				GameDataManager.Player.OdekakeCharaCostume = resultData.OdekakeInfo.CharaData.CostumeItemEquip;
				GameDataManager.Player.OdekakeCustomizeData = resultData.OdekakeInfo.Customize;
				GameDataManager.PlayerConfig.IsStoryRead = true;
			}
			catch
			{
			}
			await GotoNextScene();
		}
	}

	private async UniTask GotoNextScene()
	{
		if (!_isNextSceneDecided)
		{
			_isNextSceneDecided = true;
			IsGotoNextScene = true;
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
			_canvas.GetComponent<GraphicRaycaster>().enabled = false;
			m_charaManager.IsTouchReactionEnable = false;
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			m_mainWindow.StopPreview();
			SoundManager.PlayMyCharaMyRoomBGM().Forget();
			CancellationToken token = this.GetCancellationTokenOnDestroy();
			try
			{
				await GameDataManager.Player.Save(token);
				await GameDataManager.Customize.Save(token);
				await MissionFlow.Save(JudgeLocation.MyRoom);
			}
			catch (OperationCanceledException ex)
			{
				Debug.Log("OperationCanceledException Catched. " + ex.Message);
			}
			catch (NetworkResponseException ex2)
			{
				GameDataManager.PlayerConfig.IsDataSaveFailed = true;
				Debug.Log("NetworkResponseException Catched. " + ex2.Message);
			}
			GotoNextScene("Home", enableDecidedCheck: false);
		}
	}

	private async UniTask GuideDisp(CancellationToken cancellationToken = default(CancellationToken))
	{
		float timeLimit = 20f;
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
		m_introduction.SetActive(value: true);
		m_introduction.GetComponent<Animator>().SetTrigger("Start");
		await UniTask.Delay(TimeSpan.FromSeconds(2.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(timeLimit), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0), PlayerLoopTiming.Update, cancellationToken));
		m_introduction.GetComponent<Animator>().SetTrigger("Skip");
		await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(timeLimit), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0), PlayerLoopTiming.Update, cancellationToken));
		m_introduction.GetComponent<Animator>().SetTrigger("Skip");
		await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(timeLimit), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0), PlayerLoopTiming.Update, cancellationToken));
		m_introduction.SetActive(value: false);
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
	}
}
