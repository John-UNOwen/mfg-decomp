using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameContents;
using Konami.System.Net;
using MFG.Assets;
using MFG.GameData;
using MFG.MahjongPlay.Cutin;
using MFG.MasterGameData;
using MFG.Sytem.Utilities;
using MFG.Types;
using MFG.Types.Customize.CutinItem;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class GachaPlayFlow : BaseFlow
{
	[SerializeField]
	private Gacha3DManager m_gachaAnimator;

	[SerializeField]
	private GachaPlayDecision m_decisionGameObject;

	[SerializeField]
	private GachaCanvasManager m_gachaCanvas;

	[SerializeField]
	private SpriteRenderer m_gachaKindSpriteRenderer;

	[SerializeField]
	private SpriteRenderer m_gachaBgMonitorSpriteRenderer;

	[SerializeField]
	private GameObject m_decisionBGGameObject;

	[SerializeField]
	private GameObject m_cutinInstanceRoot;

	[SerializeField]
	private GameObject m_cutinPrepareRoot;

	[SerializeField]
	private List<GameObject> m_rarityThumbnailPrefabs;

	[SerializeField]
	private GameObject m_customizeItemGameObject;

	[SerializeField]
	private List<Sprite> m_charaAppearanceNameSprites;

	[SerializeField]
	private GachaResultDialog m_resultDialog;

	[SerializeField]
	private GameObject _stageBGParent;

	[SerializeField]
	private GameObject _stage3DParent;

	[SerializeField]
	private Button m_skipButton;

	private int CurrentIndex = -1;

	[SerializeField]
	private CutinManager m_cutinManager;

	[SerializeField]
	private List<PreviewPlayerInteractor> m_previewPlayerInteractors = new List<PreviewPlayerInteractor>();

	[SerializeField]
	private Dictionary<ItemID, int> m_simulationResultDict = new Dictionary<ItemID, int>();

	[SerializeField]
	private GachaSimulationDialog m_simulationDialog;

	private GachaPlayMode _gachaPlayMode;

	private GachaSeriesServerInfo _seriesInfo;

	public bool IsForceSkip;

	private ItemID CurrentItemID => GameDataManager.Gacha.GachaResult.GetItemID(CurrentIndex);

	public Item CurrentItemMaster => CutinItemMaster.GetItem(CurrentItemID);

	public int GachaCount()
	{
		return GachaPlayUtility.Count(_gachaPlayMode);
	}

	protected override async UniTask OnInitialize(CancellationToken cancellationToken)
	{
		m_resultDialog.AllRetryButtonUnable();
		try
		{
			await PaseliUtility.RefleshPrice(Fraction.One, isFetchExternalBasicRateInfo: true);
		}
		catch
		{
		}
		SoundManager.StopAll().Forget();
		SoundManager.PlaySe("SFX_SYS_GACYA1").Forget();
		SoundManager.PlayBgm("BGM_GACYA").Forget();
		_seriesInfo = GameDataManager.Gacha.SelectedSeriesInfo ?? GameDataManager.GachaInfo.GachaSeriesInfosFromServer[0];
		_gachaPlayMode = GameDataManager.Gacha.SelectedMode;
		try
		{
			if (GameUtility.IsCloudStreaming && !Cloudstrap.UserEvent.NeedCompensation(Cloudstrap.UserEventType.Gacha1) && !Cloudstrap.UserEvent.NeedCompensation(Cloudstrap.UserEventType.Gacha5))
			{
				await GameUtility.SendConsumeLog(GameDataManager.Gacha.ConsumeMode, GachaCount());
			}
		}
		catch
		{
		}
		await StageBG_Create();
		try
		{
			await GameDataManager.Gacha.GachaResult.GachaApply(_gachaPlayMode, _seriesInfo);
			if (GachaPlayUtility.IsConsumeGacha(_gachaPlayMode))
			{
				GameUtility.SendPpEvent(CONSUME_MODE.GACHA_CREDIT, GachaCount());
				if (GameUtility.IsKonastaOrNetcafeBehavior)
				{
					Cloudstrap.UserEvent.End((GachaCount() == 1) ? Cloudstrap.UserEventType.Gacha1 : Cloudstrap.UserEventType.Gacha5);
				}
				if (!GameUtility.IsCloudStreaming)
				{
					await GameUtility.SendConsumeLog(GameDataManager.Gacha.ConsumeMode, GachaCount());
				}
			}
			GachaPlay_Init();
		}
		catch (NetworkResponseException)
		{
			ErrorData.GetInstance().ExtraMessage = "「" + GachaPlayUtility.ToString(_gachaPlayMode) + "」が実行できませんでした。";
			ErrorControl.SetError(ERROR.CONNECTION, 0u, ERROR_LEVEL.ERROR_STOP);
			_isNextSceneDecided = true;
		}
		catch (OperationCanceledException)
		{
			Debug.Log(GetType().Name + " OnInitialize: OperationCanceledException Catched.");
		}
	}

	protected override async UniTask OnExecute(CancellationToken cancellationToken)
	{
		if (_isNextSceneDecided)
		{
			return;
		}
		try
		{
			if (_gachaPlayMode != GachaPlayMode.Simulation)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await UniTask.WhenAny(GachaDirection(cancellationToken), UniTask.WaitUntil(() => IsForceSkip, PlayerLoopTiming.Update, cancellationToken));
				cancellationToken.ThrowIfCancellationRequested();
				await m_resultDialog.SetItem(GameDataManager.Gacha.GachaResult, _seriesInfo, _gachaPlayMode);
				await m_resultDialog.Open(cancellationToken);
				await UniTask.WhenAny(UniTask.WaitWhile(() => m_resultDialog.gameObject.activeSelf, PlayerLoopTiming.Update, cancellationToken), UniTask.Delay(TimeSpan.FromSeconds(30.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken));
				await UniTask.WaitWhile(() => m_resultDialog.m_btGachaRetry.IsRetryButtonPushed);
				m_resultDialog.gameObject.SetActive(value: false);
			}
			SoundManager.StopBgmWithFadeout().Forget();
			NextSceneLoad("GachaSelect");
		}
		catch (NetworkResponseException)
		{
			ErrorData.GetInstance().ExtraMessage = "「" + GachaPlayUtility.ToString(_gachaPlayMode) + "」が実行できませんでした。";
			ErrorControl.SetError(ERROR.CONNECTION, 0u, ERROR_LEVEL.ERROR_STOP);
		}
		catch (OperationCanceledException)
		{
			Debug.Log(GetType().Name + " OnExecute: OperationCanceledException Catched.");
		}
	}

	private void GachaPlay_Init()
	{
		GameDataManager.Gacha.GachaResult.DebugLog();
		(Harbinger, bool) tuple = GachaPlayUtility.HarbingerLotate(_seriesInfo, GameDataManager.Gacha.GachaResult.ItemInfos);
		m_gachaCanvas.SDCharaInit((int)tuple.Item1);
		m_gachaAnimator.Init(GameDataManager.Gacha.GachaResult, tuple.Item2);
		m_skipButton.OnClickAsObservable().Subscribe(delegate
		{
			IsForceSkip = true;
			m_skipButton.gameObject.SetActive(value: false);
		}).AddTo(this);
	}

	public bool IsTouchSkipAction(bool isOnlySkipButton = false)
	{
		if (IsForceSkip)
		{
			return true;
		}
		if (Bi2xInput.Instance.IsTouchTrigger() && !isOnlySkipButton)
		{
			return true;
		}
		return false;
	}

	private async UniTask GachaDirection(CancellationToken cancellationToken = default(CancellationToken))
	{
		CancellationTokenSource previewCts = null;
		try
		{
			m_gachaCanvas.RouletteAnimEnd();
			m_skipButton.gameObject.SetActive(value: true);
			await m_gachaAnimator.PlayIn(cancellationToken);
			await m_gachaCanvas.TouchPlease(cancellationToken);
			await m_gachaAnimator.AnimStart(cancellationToken);
			CurrentIndex = m_gachaAnimator.ThumbnailChange(CurrentIndex);
			while (CurrentIndex < GachaCount() && !IsForceSkip)
			{
				await m_gachaAnimator.RarityChangeWait(CurrentIndex, cancellationToken);
				m_decisionGameObject.CutinPlayIn(CurrentItemID);
				await UniTask.WaitUntil(() => m_cutinInstanceRoot.activeInHierarchy, PlayerLoopTiming.Update, cancellationToken);
				previewCts = new CancellationTokenSource();
				PlayPreview(CurrentItemID, previewCts).Forget();
				int millisecondsDelay = CurrentItemMaster.Category switch
				{
					Category.Reach => 4500, 
					Category.Naki => 3500, 
					_ => 7500, 
				};
				await UniTask.WhenAny(UniTask.Delay(millisecondsDelay, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => IsTouchSkipAction(), PlayerLoopTiming.Update, cancellationToken));
				Cancel(ref previewCts);
				CurrentIndex = m_gachaAnimator.ThumbnailChange(CurrentIndex);
				await m_decisionGameObject.GachaDecisionExit(cancellationToken);
			}
			m_skipButton.gameObject.SetActive(value: false);
			await UniTask.Delay(TimeSpan.FromSeconds(0.5), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		}
		catch (OperationCanceledException ex)
		{
			Debug.Log("OperationCanceledException Catched. " + ex.Message);
		}
		finally
		{
			Cancel(ref previewCts);
		}
	}

	public void Cancel(ref CancellationTokenSource tokenSource)
	{
		if (tokenSource != null)
		{
			tokenSource.Cancel();
			tokenSource.Dispose();
			tokenSource = null;
		}
	}

	private async UniTask PlayPreview(ItemID itemID, CancellationTokenSource cts)
	{
		Item item = CutinItemMaster.GetItem(itemID);
		Cha cha = Cha.Jicha;
		_ = m_previewPlayerInteractors[(int)cha];
		CutinType cutinType;
		switch (item.Category)
		{
		case Category.Naki:
			cutinType = CutinType.Pon;
			break;
		case Category.AgariSmall:
		case Category.AgariBig:
			cutinType = CutinType.Ron;
			break;
		case Category.Reach:
			cutinType = CutinType.Reach;
			break;
		default:
			cutinType = CutinType.Pon;
			break;
		}
		CutinType cutinType2 = cutinType;
		try
		{
			Debug.Log("CutinPreview Play ItemID:" + itemID);
			m_cutinManager.Play(cutinType2, new CutinItemInfo
			{
				ItemID = itemID,
				IsOn = true
			}, cha, m_previewPlayerInteractors, null, cts.Token).Forget();
			if (item.Category == Category.AgariSmall || item.Category == Category.AgariBig)
			{
				await m_cutinManager.AgariCutin_WaitIntroductionEnd(cts.Token);
				m_cutinManager.CurrentCutin_End(cts.Token).Forget();
			}
			if (m_cutinManager.IsPlaying)
			{
				await UniTask.WaitWhile(() => m_cutinManager.IsPlaying, PlayerLoopTiming.Update, cts.Token);
			}
		}
		catch (OperationCanceledException)
		{
			Debug.Log("CutinPreview Canceled ItemID:" + itemID);
		}
		catch (ObjectDisposedException)
		{
			Debug.Log("ObjectDisposedException Canceled ItemID:" + itemID);
		}
	}

	private async UniTask StageBG_Create()
	{
		IDisposableAsset<GameObject> obj = await AssetLoader.LoadAssetAsync<GameObject>("GachaPlayBG/Prefabs/GachaPlayBG.prefab");
		obj.AddTo(this);
		UnityEngine.Object.Instantiate(obj.Asset, _stage3DParent.transform, worldPositionStays: false);
		IDisposableAsset<GameObject> obj2 = await AssetLoader.LoadAssetAsync<GameObject>("MahjongPlayBG/Prefabs/StageBG.prefab");
		obj2.AddTo(this);
		UnityEngine.Object.Instantiate(obj2.Asset, _stageBGParent.transform, worldPositionStays: false);
		IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(_seriesInfo.TitleAddressableAssetPath);
		disposableAsset.AddTo(this);
		m_gachaKindSpriteRenderer.sprite = disposableAsset.Asset;
		IDisposableAsset<Sprite> disposableAsset2 = await AssetLoader.LoadAssetAsync<Sprite>(_seriesInfo.BgMonitorAddressableAssetPath());
		disposableAsset2.AddTo(this);
		m_gachaBgMonitorSpriteRenderer.sprite = disposableAsset2.Asset;
	}

	public void NextSceneLoad(string scene)
	{
		GotoNextScene(scene);
	}
}
