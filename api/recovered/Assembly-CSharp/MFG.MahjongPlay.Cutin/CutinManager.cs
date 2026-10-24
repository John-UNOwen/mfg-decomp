using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;
using MFG.Types.Customize.CutinItem;
using UniRx;
using UnityEngine;

namespace MFG.MahjongPlay.Cutin;

[Serializable]
public class CutinManager : MonoBehaviour
{
	[SerializeField]
	public CutinPlayMode PlayMode;

	[SerializeField]
	public bool AudioOutputEnable = true;

	[SerializeField]
	private AtomicCutin atomicCutin;

	public bool EnableScreenShot;

	private CutinAssetCache m_assetCache = new CutinAssetCache();

	public PlaybackContentDecider m_decider = new PlaybackContentDecider();

	private int m_playRequestPosition;

	private int m_currentPlayPosition;

	private GameObject m_currentCutin;

	private IDisposableAsset<GameObject> m_currentThrownReachStickPrefab;

	private IDisposableAsset<GameObject> m_currentThrownReachStickEffectPrefab;

	private RandomValueGenerator m_randomValueGenerator = new RandomValueGenerator();

	private RandomValueGenerator m_randomValueForVoiceGenerator = new RandomValueGenerator();

	private RandomValueGenerator m_randomValueForReachGenerator = new RandomValueGenerator();

	[SerializeField]
	[Tooltip("各家のPlayer\nElement0…自家 Element1…下家 Element2…対面 Element3…上家")]
	private List<PlayerInteractor> m_playerInteractors;

	private IPlayerInteractor m_sourcePlayerInteractor;

	public MFG.Types.Customize.CustomizeItem.ItemID LatestReachBgmID;

	[SerializeField]
	private GameObject m_reachStickLandingEffectController;

	private Subject<IPlayerInteractor> m_onReachStickLanded = new Subject<IPlayerInteractor>();

	private Subject<(IPlayerInteractor, MFG.Types.Customize.CustomizeItem.ItemID)> m_onPrepareLandedReachStick = new Subject<(IPlayerInteractor, MFG.Types.Customize.CustomizeItem.ItemID)>();

	[NonSerialized]
	public bool IsPlaying;

	public bool IsLoadded;

	[SerializeField]
	private Camera m_thrownReachStickCamera;

	[SerializeField]
	private Camera m_nakiCamera;

	public List<PlayerInteractor> PlayerInteractors => m_playerInteractors;

	public IPlayerInteractor SourcePlayerInteractor => m_sourcePlayerInteractor;

	public IObservable<IPlayerInteractor> OnReachStickLanded => m_onReachStickLanded.AsObservable();

	public IObservable<(IPlayerInteractor playerInteractor, MFG.Types.Customize.CustomizeItem.ItemID reachStickItemID)> OnPrepareLandedReachStick => m_onPrepareLandedReachStick.AsObservable();

	public void PlayerInteractorsResetReachStick()
	{
		foreach (PlayerInteractor playerInteractor in PlayerInteractors)
		{
			playerInteractor.SetReachStick(MFG.Types.Customize.CustomizeItem.ItemID.None);
		}
	}

	public void NotifyReachStickLanded()
	{
		m_onReachStickLanded.OnNext(m_sourcePlayerInteractor);
	}

	public void RandomValueGenerator_SetSeed(int seed)
	{
		m_randomValueGenerator.Init(seed);
		m_randomValueForVoiceGenerator.Init(seed);
		m_randomValueForReachGenerator.Init(seed);
	}

	private void Awake()
	{
		m_decider.m_random = m_randomValueGenerator;
		m_decider.m_randomForReach = m_randomValueForReachGenerator;
	}

	private void OnDestroy()
	{
		m_thrownReachStickCamera.targetTexture = null;
		m_nakiCamera.targetTexture = null;
	}

	private async void Start()
	{
		IDisposableAsset<RenderTexture> disposableAsset = await AssetLoader.LoadAssetAsync<RenderTexture>("Assets/MFG/Scenes/MahjongPlay/Prefabs/ReachBou.renderTexture");
		disposableAsset.AddTo(this);
		m_thrownReachStickCamera.targetTexture = disposableAsset.Asset;
		IDisposableAsset<RenderTexture> disposableAsset2 = await AssetLoader.LoadAssetAsync<RenderTexture>("Assets/MFG/Scenes/MahjongPlay/Prefabs/NakiTexture.renderTexture");
		disposableAsset2.AddTo(this);
		m_nakiCamera.targetTexture = disposableAsset2.Asset;
	}

	public async UniTask PlayPon(Cha sourceCha, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await Play(CutinType.Pon, sourceCha, null, cancellationToken);
	}

	public async UniTask PlayChi(Cha sourceCha, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await Play(CutinType.Chi, sourceCha, null, cancellationToken);
	}

	public async UniTask PlayKan(Cha sourceCha, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await Play(CutinType.Kan, sourceCha, null, cancellationToken);
	}

	public async UniTask PlayReach(Cha sourceCha, MahjongMusicTextDisplay musicText = null, Pai pai = Pai.NONE, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		m_playerInteractors[(int)sourceCha].SetDahai(pai);
		await Play(CutinType.Reach, sourceCha, musicText, cancellationToken);
	}

	public async UniTask PlayTsumo(Cha agariCha, Han han, int doraCount, int meldBonusHan, int comebackBonusHan, int allgreenBonusHan, List<Yaku> yakus, int akadoraCount, bool isBombCha, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		foreach (PlayerInteractor playerInteractor in m_playerInteractors)
		{
			playerInteractor.SetPaymentInfo(han, doraCount, meldBonusHan, comebackBonusHan, allgreenBonusHan, yakus, playerInteractor.Cha() != agariCha, akadoraCount, isBombCha, isCatharsisEffectPlay: false);
		}
		await Play(CutinType.Tsumo, agariCha, null, cancellationToken);
	}

	public async UniTask PlayRon(Cha agariCha, Cha furikomiCha, Han han, int doraCount, int meldBonusHan, int comebackBonusHan, int allgreenBonusHan, List<Yaku> yakus, int akadoraCount, Cha bombCha, bool isCatharsisEffectPlay, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		bool isContainBombCha = GameDataManager.Mahjong.IsBombEnableMode && (bombCha == agariCha || bombCha == furikomiCha);
		foreach (PlayerInteractor playerInteractor in m_playerInteractors)
		{
			if (playerInteractor.Cha() == agariCha)
			{
				playerInteractor.SetPaymentInfo(han, doraCount, meldBonusHan, comebackBonusHan, allgreenBonusHan, yakus, isScorePaymentPlayer: false, akadoraCount, isContainBombCha, isCatharsisEffectPlay);
			}
			else if (playerInteractor.Cha() == furikomiCha)
			{
				playerInteractor.SetPaymentInfo(han, doraCount, meldBonusHan, comebackBonusHan, allgreenBonusHan, yakus, isScorePaymentPlayer: true, akadoraCount, isContainBombCha, isCatharsisEffectPlay);
			}
			else
			{
				playerInteractor.SetPaymentInfo(han, doraCount, meldBonusHan, comebackBonusHan, allgreenBonusHan, yakus, isScorePaymentPlayer: false, akadoraCount, isContainBombCha, isCatharsisEffectPlay);
			}
		}
		await Play(CutinType.Ron, agariCha, null, cancellationToken);
	}

	private async UniTask Play(CutinType cutinType, Cha sourceCha, MahjongMusicTextDisplay musicText = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		PlayerInteractor playerInteractor = m_playerInteractors[(int)sourceCha];
		CutinItemInfo item = m_decider.DecideItem(cutinType, playerInteractor.CharaType(), playerInteractor.CutinItems(), playerInteractor.PaymentAmount() >= Han.Mangan);
		await Play(cutinType, item, sourceCha, m_playerInteractors, musicText, cancellationToken);
	}

	public async UniTask Play(CutinType cutinType, CutinItemInfo item, Cha sourceCha, IEnumerable<IPlayerInteractor> playerInteractors, MahjongMusicTextDisplay musicText = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		IsPlaying = true;
		Debug.Log($"CutinManager Begin: {item.ItemID.ToString()} {cutinType}");
		int myPlayRequest = m_playRequestPosition;
		m_playRequestPosition++;
		try
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (myPlayRequest != m_currentPlayPosition)
			{
				Debug.Log($"CutinManager Pushed: {item.ItemID.ToString()} myPos:{myPlayRequest} curPos:{m_currentPlayPosition}");
				await UniTask.WaitUntil(() => myPlayRequest == m_currentPlayPosition, PlayerLoopTiming.Update, cancellationToken);
				Debug.Log($"CutinManager Popped: {item.ItemID.ToString()} myPos:{myPlayRequest} curPos:{m_currentPlayPosition}");
			}
			m_sourcePlayerInteractor = playerInteractors.Where((IPlayerInteractor x) => x.Cha() == sourceCha).First();
			Transform parent = ((item.Master.Category == MFG.Types.Customize.CutinItem.Category.Naki) ? m_sourcePlayerInteractor.NakiCutinParent().transform : m_sourcePlayerInteractor.CutinParent().transform);
			m_currentCutin = await LoadCutin(item, cutinType, parent, cancellationToken);
			IsLoadded = true;
			CutinController cutinController = m_currentCutin.GetComponent<CutinController>();
			int randomValueForVoice = m_randomValueForVoiceGenerator.Range(0, 100);
			cutinController.SetParameters(cutinType, item.Master.ItemID, sourceCha, playerInteractors, PlayMode, AudioOutputEnable, randomValueForVoice);
			CutinEventNotifier component = m_currentCutin.GetComponent<CutinEventNotifier>();
			if (component != null)
			{
				component.m_cutinManager = this;
			}
			if (cutinType == CutinType.Reach)
			{
				ReachCutin reachCutin = m_currentCutin.GetComponent<ReachCutin>();
				ThrownReachStick_Destroy(m_sourcePlayerInteractor);
				MFG.Types.Customize.CustomizeItem.ItemID item2 = await ThrownReachStick_Create(m_sourcePlayerInteractor, cancellationToken);
				m_onPrepareLandedReachStick.OnNext((m_sourcePlayerInteractor, item2));
				reachCutin.OnReachStickThrow.Subscribe(delegate(IPlayerInteractor playerInteractor)
				{
					GameObject gameObject = playerInteractor.ThrownReachStickParent();
					if (gameObject != null)
					{
						gameObject.SetActive(value: true);
						gameObject.GetComponent<Animator>().SetTrigger("In");
					}
					if (m_reachStickLandingEffectController != null)
					{
						ReachStickLandingEffectController componentInChildren = m_reachStickLandingEffectController.GetComponentInChildren<ReachStickLandingEffectController>();
						if (componentInChildren != null)
						{
							componentInChildren.Play(playerInteractor);
						}
					}
				}).AddTo(cancellationToken);
				reachCutin.OnPlayBgmRequest.Subscribe(delegate(IPlayerInteractor playerInteractor)
				{
					CharaType charaType = CutinItemMaster.GetItem(reachCutin.ItemID).CharaType;
					MFG.Types.Customize.CustomizeItem.ItemID itemID = m_decider.DecideReachBgm(playerInteractor.CustomizeItemEquips()).ItemID;
					string charaVoiceLabel = SoundManager.GetCharaVoiceLabel(itemID.ReachBgmToBgmLabel(), charaType);
					LatestReachBgmID = itemID;
					SoundManager.PlayBgm(charaVoiceLabel).Forget();
					if (musicText != null)
					{
						musicText.MusicIn(itemID);
					}
					Debug.Log($"Reach BGM: itemID:{itemID} label:{charaVoiceLabel}");
				}).AddTo(cancellationToken);
				if (m_sourcePlayerInteractor.ReachLevel() == ReachLevel.AtomicReach && atomicCutin != null)
				{
					await atomicCutin.Play(m_sourcePlayerInteractor.Dahai());
				}
			}
			cutinController.Play(cancellationToken).Forget();
			await UniTask.WaitWhile(() => cutinController.IsPlaying(), PlayerLoopTiming.Update, cancellationToken);
			await UniTask.Yield(cancellationToken);
		}
		finally
		{
			Debug.Log("CutinManager UnLoad: " + item.ItemID);
			try
			{
				ThrownReachStick_Destroy(m_sourcePlayerInteractor);
				await UnLoad(item, cutinType);
			}
			catch (MissingReferenceException)
			{
			}
			finally
			{
				IsLoadded = false;
				IsPlaying = false;
				m_currentPlayPosition++;
				Debug.Log($"CutinManager End: {item.ItemID.ToString()} {cutinType}");
			}
		}
	}

	private async UniTask<MFG.Types.Customize.CustomizeItem.ItemID> ThrownReachStick_Create(IPlayerInteractor sourcePlayerInteractor, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		MFG.Types.Customize.CustomizeItem.ItemID reachStickItemID = m_decider.DecideThrownReachStick(sourcePlayerInteractor.CustomizeItemEquips()).ItemID;
		try
		{
			((PlayerInteractor)sourcePlayerInteractor).SetReachStick(reachStickItemID);
		}
		catch
		{
		}
		m_currentThrownReachStickPrefab = await AssetLoader.LoadAssetAsync<GameObject>(AssetPathResolver.ResolveCustomizeItem(reachStickItemID, MFG.Types.Customize.CustomizeItem.Category.ReachStick), null, cancellationToken);
		Transform transform = sourcePlayerInteractor.ThrownReachStickParent().transform.Find("ReachStickParent");
		GameObject reachStick = UnityEngine.Object.Instantiate(m_currentThrownReachStickPrefab.Asset, transform.transform);
		reachStick.name = "ReachStick";
		if (sourcePlayerInteractor.ReachLevel() >= ReachLevel.FireReach)
		{
			m_currentThrownReachStickEffectPrefab = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/ThrownFireReachStickEffect.prefab", null, cancellationToken);
			Transform transform2 = reachStick.transform.Find("Base");
			UnityEngine.Object.Instantiate(m_currentThrownReachStickEffectPrefab.Asset, transform2.transform).GetComponent<ParticleSystem>().Play();
		}
		else if (sourcePlayerInteractor.ReachLevel() >= ReachLevel.AtomicReach)
		{
			m_currentThrownReachStickEffectPrefab = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/AtomicReach/Prefabs/ThrownAtomicReachStickEffect.prefab", null, cancellationToken);
			Transform transform3 = reachStick.transform.Find("Base");
			UnityEngine.Object.Instantiate(m_currentThrownReachStickEffectPrefab.Asset, transform3.transform).GetComponent<ParticleSystem>().Play();
		}
		else if (sourcePlayerInteractor.ReachLevel() == ReachLevel.OpenReach)
		{
			m_currentThrownReachStickEffectPrefab = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/OpenReach/Prefabs/ThrownOpenReachStickEffect.prefab", null, cancellationToken);
			Transform transform4 = reachStick.transform.Find("Base");
			UnityEngine.Object.Instantiate(m_currentThrownReachStickEffectPrefab.Asset, transform4.transform).GetComponent<ParticleSystem>().Play();
		}
		else if (sourcePlayerInteractor.IsChocolateReachEffectActive())
		{
			m_currentThrownReachStickEffectPrefab = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/Chocolate/Prefabs/ThrownChocolateReachStickEffect.prefab", null, cancellationToken);
			Transform transform5 = reachStick.transform.Find("Base");
			UnityEngine.Object.Instantiate(m_currentThrownReachStickEffectPrefab.Asset, transform5.transform).GetComponent<ParticleSystem>().Play();
		}
		Animator component = sourcePlayerInteractor.ThrownReachStickParent().GetComponent<Animator>();
		component.Rebind();
		component.Update(0f);
		return reachStickItemID;
	}

	private void ThrownReachStick_Destroy(IPlayerInteractor sourcePlayerInteractor)
	{
		try
		{
			foreach (Transform item in sourcePlayerInteractor.ThrownReachStickParent().transform.Find("ReachStickParent").transform)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
		catch
		{
		}
		if (m_currentThrownReachStickPrefab != null)
		{
			m_currentThrownReachStickPrefab.Dispose();
			m_currentThrownReachStickPrefab = null;
		}
		if (m_currentThrownReachStickEffectPrefab != null)
		{
			m_currentThrownReachStickEffectPrefab.Dispose();
			m_currentThrownReachStickEffectPrefab = null;
		}
	}

	public async UniTask CurrentCutin_End(CancellationToken cancellationToken = default(CancellationToken))
	{
		await CurrentCutin_End(10f, cancellationToken);
	}

	public async UniTask CurrentCutin_End(float forceTimeoutSeconds, CancellationToken cancellationToken = default(CancellationToken))
	{
		Debug.Log("CutinManager CurrentCutin_End");
		cancellationToken.ThrowIfCancellationRequested();
		if (m_currentCutin == null)
		{
			return;
		}
		using TimeoutController timeoutController = new TimeoutController();
		CancellationToken token = timeoutController.Timeout(TimeSpan.FromSeconds(forceTimeoutSeconds));
		using CancellationTokenSource innerCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, token);
		CancellationToken token2 = innerCts.Token;
		int num = 0;
		try
		{
			await m_currentCutin.GetComponent<CutinController>().End(token2);
		}
		catch (OperationCanceledException)
		{
			num = 1;
		}
		if (num == 1 && timeoutController.IsTimeout())
		{
			Debug.Log("CurrentCutin_End: timeout");
			await CurrentCutin_ForceStop();
		}
	}

	public async UniTask CurrentCutin_ForceStop(CancellationToken cancellationToken = default(CancellationToken))
	{
		Debug.Log("CutinManager CurrentCutin_ForceStop");
		cancellationToken.ThrowIfCancellationRequested();
		if (!(m_currentCutin == null))
		{
			await m_currentCutin.GetComponent<CutinController>().Stop(cancellationToken);
		}
	}

	public bool CurrentCutin_IsURAgari()
	{
		if (m_currentCutin == null)
		{
			return false;
		}
		MFG.Types.Customize.CutinItem.Item item = CutinItemMaster.GetItem(m_currentCutin.GetComponent<CutinController>().ItemID);
		_ = item.CharaType;
		if (item.Category == MFG.Types.Customize.CutinItem.Category.AgariBig)
		{
			return item.Rarity == Rarity.UR;
		}
		return false;
	}

	public async UniTask AgariCutin_WaitIntroductionEnd(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (!IsPlaying)
		{
			return;
		}
		using TimeoutController timeoutController = new TimeoutController();
		CancellationToken token = timeoutController.Timeout(TimeSpan.FromSeconds(30.0));
		using CancellationTokenSource innerCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, token);
		CancellationToken innerToken = innerCts.Token;
		try
		{
			await UniTask.WaitUntil(() => IsLoadded, PlayerLoopTiming.Update, innerToken);
			MFG.Types.Customize.CutinItem.Item item = CutinItemMaster.GetItem(m_currentCutin.GetComponent<CutinController>().ItemID);
			if (item.Category != MFG.Types.Customize.CutinItem.Category.AgariSmall && item.Category != MFG.Types.Customize.CutinItem.Category.AgariBig)
			{
				return;
			}
			if (m_sourcePlayerInteractor.PaymentAmount() >= Han.Yakuman)
			{
				await UniTask.WaitWhile(() => m_currentCutin.GetComponent<CutinController>().IsPlayingIntro(), PlayerLoopTiming.Update, innerToken);
			}
			else
			{
				double num = (item.IsMovie ? 2.5 : 0.0);
				await UniTask.Delay(TimeSpan.FromSeconds(5.0 + num), ignoreTimeScale: false, PlayerLoopTiming.Update, innerToken);
			}
		}
		catch (OperationCanceledException)
		{
			if (timeoutController.IsTimeout())
			{
				Debug.Log("AgariCutin_WaitIntroductionEnd: timeout");
			}
		}
	}

	private async UniTask UnLoad(CutinItemInfo item, CutinType cutinType, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		CutinAssetKey assetKey = new CutinAssetKey
		{
			ObjectID = item.Master.ObjectID,
			CutinAssetType = ToAssetType(cutinType)
		};
		_ = item.Master.Category;
		await m_assetCache.UnLoad(assetKey, cancellationToken);
	}

	private async UniTask<GameObject> LoadCutin(CutinItemInfo item, CutinType cutinType, Transform parent, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		CutinAssetKey assetKey = new CutinAssetKey
		{
			ObjectID = item.Master.ObjectID,
			CutinAssetType = ToAssetType(cutinType)
		};
		_ = item.Master.Category;
		return await m_assetCache.GetInstance(assetKey, parent, playOnActive: true, cancellationToken);
	}

	private CutinAssetType ToAssetType(CutinType _cutinType)
	{
		CutinAssetType result = CutinAssetType.PonPrefab;
		switch (_cutinType)
		{
		case CutinType.Pon:
			result = CutinAssetType.PonPrefab;
			break;
		case CutinType.Chi:
			result = CutinAssetType.ChiPrefab;
			break;
		case CutinType.Kan:
			result = CutinAssetType.KanPrefab;
			break;
		case CutinType.Tsumo:
		case CutinType.Ron:
			result = CutinAssetType.AgariPrefab;
			break;
		case CutinType.Reach:
			result = CutinAssetType.ReachPrefab;
			break;
		default:
			Debug.LogError("CutinManager 想定外のケース " + _cutinType);
			break;
		}
		return result;
	}

	public void ExecScreenShot()
	{
	}
}
