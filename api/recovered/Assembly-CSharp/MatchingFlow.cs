using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using Konami.System.Net;
using MFG.Assets;
using MFG.GameData;
using MFG.GameRequest;
using MFG.MasterGameData;
using MFG.Sytem.Utilities;
using MFG.Taikyoku;
using MFG.Taikyoku.Command;
using MFG.Taikyoku.Command.Send;
using MFG.Types;
using MFG.Types.Customize.Costume;
using MFG.Types.Customize.CustomizeItem;
using MFG.Types.Customize.MatchingBackground;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MatchingFlow : BaseFlow
{
	[SerializeField]
	private List<GameObject> m_players = new List<GameObject>();

	[SerializeField]
	private List<GameObject> m_playersParent2Ds = new List<GameObject>();

	[SerializeField]
	private StandingCharaManager m_charaManager;

	[SerializeField]
	private List<GameObject> m_playerMovies = new List<GameObject>();

	[SerializeField]
	private GameObject m_baseMovie;

	[SerializeField]
	private List<GameObject> m_baseMoviePlayers = new List<GameObject>();

	[SerializeField]
	private List<GameObject> m_windows = new List<GameObject>();

	[SerializeField]
	private List<Sprite> m_kazeSprites = new List<Sprite>();

	[SerializeField]
	private Transform m_standingCharaParent3D;

	[SerializeField]
	private List<StandingCharaFPSAdjuster> m_standingCharaCamera;

	[SerializeField]
	private DanBanner m_danBanner;

	[SerializeField]
	private List<GameObject> m_cpuImages = new List<GameObject>();

	[SerializeField]
	private List<GameObject> m_odekakeImages = new List<GameObject>();

	private List<GameObject> m_clearAppearances = new List<GameObject> { null, null, null, null };

	[SerializeField]
	private List<GameObject> m_clearAppearanceParents = new List<GameObject>();

	private CameraContext _prevCameraContext = new CameraContext();

	private CameraContext _cameraContext = new CameraContext
	{
		LocalPosition = new Vector3(0f, 1f, -10f),
		FieldOfView = 60f
	};

	[SerializeField]
	private List<MatchingCharaCamera> m_matchingCharaCamera;

	[SerializeField]
	private StampSelectWindow _stampSelectWindow;

	[SerializeField]
	private Button _stampSelectWindowButton;

	[Header("マッチングアニメ待ち時間")]
	[SerializeField]
	private float _matchingAnimeWaitSec = 2f;

	[SerializeField]
	private float _proMatchingAnimeWaitSec = 4f;

	[SerializeField]
	private MatchingScrollText m_matchingScrollText;

	[Header("デバッグ")]
	[SerializeField]
	[Tooltip("テスト用キャラ表示時(四麻)のキャラを指定します\nElement0…自家 Element1…下家 Element2…対面 Element3…上家")]
	private List<MFG.Types.Customize.Costume.ItemID> m_testCharaAssign = new List<MFG.Types.Customize.Costume.ItemID>
	{
		MFG.Types.Customize.Costume.ItemID.Costume01_001,
		MFG.Types.Customize.Costume.ItemID.Costume02_001,
		MFG.Types.Customize.Costume.ItemID.Costume03_001,
		MFG.Types.Customize.Costume.ItemID.Costume04_001
	};

	private bool m_isMatchingEnd;

	private bool[] m_isMatchingEstablished = new bool[4];

	private long _stampPosition = -1L;

	private MFG.Types.Customize.CustomizeItem.ItemID[] _latestStampIDs = new MFG.Types.Customize.CustomizeItem.ItemID[4];

	private int _humanPlayerNum;

	private StandingCharaController[] _standingCharaControllers = new StandingCharaController[4];

	private GameObject[] m_bgObject = new GameObject[4];

	private CharaType[] debugCharaType = new CharaType[4];

	private MFG.Types.Customize.MatchingBackground.ItemID[] debugMatcingBGs = new MFG.Types.Customize.MatchingBackground.ItemID[4];

	private bool[] MatchingTestEstablished = new bool[4] { true, false, false, false };

	private int index = 1;

	private float _matchingAnimPlayingTime;

	private float _elapsedTimeOfMatchingAnimePlayed;

	private float _currentNeedWaitSec;

	private CancellationTokenSource _stampCancellationTokenSource = new CancellationTokenSource();

	private object _stampLock = new object();

	[SerializeField]
	private PlayerGameData m_playerGameData => GameDataManager.Player;

	[SerializeField]
	private AccountGameData m_accountGameData => GameDataManager.Account;

	[SerializeField]
	private CommandClient m_commandClient => GameDataManager.CommandClient;

	private EntryTakuInfo m_entryTakuInfo => GameDataManager.Taikyoku.TakuInfo;

	protected override async UniTask OnInitialize(CancellationToken cancellationToken)
	{
		try
		{
			ResidentFlow.GetInstance().CameraContext_Reserve();
			ResidentFlow.GetInstance().CameraContext_Restore(_cameraContext);
			string label = "BGM_CASTING";
			if (GameDataManager.IsReachSongToytoyNoticeEnable() && (PaseliUtility.PPEX.IsEnablePPEX || GameUtility.IsKonastaOrNetcafeBehavior))
			{
				label = "BGM_CASTING_CM";
			}
			SoundManager.PlayBgm(label).Forget();
			Stamp_Init(cancellationToken);
			m_matchingScrollText.Init();
			MatchingFunction.SetNextStatic();
			if (GameDataManager.Mahjong.TakuType == TakuType.Nima)
			{
				_matchingAnimeWaitSec = 4f;
			}
			_elapsedTimeOfMatchingAnimePlayed = Math.Max(_matchingAnimeWaitSec, _proMatchingAnimeWaitSec);
			_currentNeedWaitSec = _elapsedTimeOfMatchingAnimePlayed;
			GameDataManager.YellCampaign.RefreshYellRank();
			if (GameDataManager.Mahjong.TakuType == TakuType.Nima)
			{
				m_players[2].SetActive(value: false);
				m_isMatchingEstablished[2] = true;
				m_players[3].SetActive(value: false);
				m_isMatchingEstablished[3] = true;
			}
			else if (GameDataManager.Mahjong.TakuType == TakuType.Sanma)
			{
				m_players[3].SetActive(value: false);
				m_isMatchingEstablished[3] = true;
			}
			for (int i = 1; i < 4; i++)
			{
				if (m_players[i].activeSelf)
				{
					IDisposableAsset<GameObject> disposableAsset = await AssetLoader.LoadAssetAsync<GameObject>("MatchingClearAppearance/Prefabs/ClearAppearance.prefab");
					disposableAsset.AddTo(this);
					m_clearAppearances[i] = UnityEngine.Object.Instantiate(disposableAsset.Asset, m_clearAppearanceParents[i].transform, worldPositionStays: false);
				}
			}
			m_commandClient.OnMatchingData.Subscribe(delegate(MatchingData matchingData)
			{
				m_isMatchingEnd = matchingData.IsMatchingEnd;
				foreach (PlayerInfo playerInfo in matchingData.PlayerInfos)
				{
					if (playerInfo.IsActive)
					{
						GameDataManager.Taikyoku.PlayerInfos[playerInfo.PlayerIndex] = playerInfo;
					}
				}
				if (m_isMatchingEnd)
				{
					GameDataManager.Taikyoku.IsScrambleMatch = matchingData.IsScrambleMatch;
					GameDataManager.Taikyoku.IsMotherWars = matchingData.IsMotherWars;
					_humanPlayerNum = matchingData.HumanPlayerNum;
				}
			}).AddTo(this);
			m_commandClient.OnStampData.Subscribe(delegate(StampData stampData)
			{
				Stamp_Received(stampData, cancellationToken);
			}).AddTo(this);
			EntryGame.ResultData resultData = await RequestApiBase<EntryGame, EntryGame.ResultData>.Send(enableErrorScreenTransition: true, enableSendLog: true, cancellationToken);
			GameDataManager.Taikyoku.Apply(resultData.TakuInfo);
			m_commandClient.Reset(m_entryTakuInfo.NextSeqNo);
			m_commandClient.TakuID = m_entryTakuInfo.TakuID;
			m_commandClient.PlayerIndex = m_entryTakuInfo.PlayerIndex;
			m_commandClient.PlayerID = m_accountGameData.PlayerID;
			VoiceManager.SetCutinRandomSeed(m_entryTakuInfo.TakuID);
			int machingLimitTime = GameDataManager.Taikyoku.TakuInfo.MachingLimitTime;
			if (GameDataManager.Mahjong.TakuType != TakuType.Nima)
			{
				ResidentFlow.GetInstance().GetTimer().SetTimer(machingLimitTime, disp: true);
				ResidentFlow.GetInstance().GetTimer().StartTimer();
			}
			m_matchingScrollText.Play().Forget();
			await m_commandClient.Send(new EntryOK(), cancellationToken);
		}
		catch (OperationCanceledException)
		{
			Debug.Log(GetType().Name + MethodBase.GetCurrentMethod().Name + ": OperationCanceledException Catched.");
		}
	}

	protected override async UniTask OnExecute(CancellationToken cancellationToken)
	{
		_ = 6;
		try
		{
			while (!m_isMatchingEnd)
			{
				await m_commandClient.Fetch(cancellationToken);
			}
			foreach (GameObject player in m_players)
			{
				int num = m_players.IndexOf(player);
				Kaze zaseki = GameDataManager.Taikyoku.PlayerInfos[num].Zaseki;
				m_baseMoviePlayers[num].transform.Find("Seat").Find("Decide").GetComponent<Image>()
					.sprite = m_kazeSprites[(int)zaseki];
			}
			await UniTask.WaitUntil(() => m_isMatchingEstablished[0] && m_isMatchingEstablished[1] && m_isMatchingEstablished[2] && m_isMatchingEstablished[3], PlayerLoopTiming.Update, cancellationToken);
			ResidentFlow.GetInstance().GetTimer().SetDisp(disp: false);
			await UniTask.Delay(TimeSpan.FromSeconds(8.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			switch (GameDataManager.Mahjong.TakuType)
			{
			case TakuType.Nima:
				m_baseMovie.GetComponent<Animator>().SetTrigger("Roulette2");
				await UniTask.Delay(TimeSpan.FromSeconds(2.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
				break;
			case TakuType.Sanma:
				m_baseMovie.GetComponent<Animator>().SetTrigger("Roulette3");
				await UniTask.Delay(TimeSpan.FromSeconds(2.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
				break;
			case TakuType.Tonpu:
			case TakuType.Hanchan:
				m_baseMovie.GetComponent<Animator>().SetTrigger("Roulette4");
				await UniTask.Delay(TimeSpan.FromSeconds(2.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
				break;
			}
			SoundManager.PlayMyCharaVoice((new string[4] { "VOX_GXX_MJG_START15", "VOX_GXX_MJG_START16", "VOX_GXX_MJG_START17", "VOX_GXX_MJG_START18" })[(int)GameDataManager.Taikyoku.PlayerInfos[m_commandClient.PlayerIndex].Zaseki]).Forget();
			await UniTask.Delay(TimeSpan.FromSeconds(2.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			SoundManager.PlaySe("SFX_SYS_FADEOUT1").Forget();
			SoundManager.StopBgmWithFadeout().Forget();
			GotoNextScene("MahjongPlay");
		}
		catch (OperationCanceledException)
		{
			Debug.Log(GetType().Name + MethodBase.GetCurrentMethod().Name + ": OperationCanceledException Catched.");
		}
	}

	protected override void OnUpdate()
	{
		_elapsedTimeOfMatchingAnimePlayed += Time.deltaTime;
		for (int i = 0; i < GameDataManager.Taikyoku.PlayerInfos.Count; i++)
		{
			PlayerInfo playerInfo = GameDataManager.Taikyoku.PlayerInfos[i];
			if (!m_isMatchingEstablished[i] && playerInfo.IsActive && _elapsedTimeOfMatchingAnimePlayed >= _currentNeedWaitSec && Time.time - _matchingAnimPlayingTime >= _currentNeedWaitSec)
			{
				_currentNeedWaitSec = (playerInfo.PlayerGameData.IsPro() ? _proMatchingAnimeWaitSec : _matchingAnimeWaitSec);
				_elapsedTimeOfMatchingAnimePlayed = 0f;
				if (playerInfo.IsJicha)
				{
					Stamp_SetActive(isActive: true);
				}
				m_isMatchingEstablished[i] = true;
				StartCharaMatchingAnime(i, playerInfo, this.GetCancellationTokenOnDestroy()).Forget();
			}
		}
	}

	private void OnDestroy()
	{
		ResidentFlow.GetInstance().CameraContext_Restore();
	}

	private async UniTask StartCharaMatchingAnime(int i, PlayerInfo info, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		bool isPro = info.PlayerGameData.IsPro();
		CharaType charaType = info.PlayerGameData.SelectChara;
		bool isFakeClear = info.PlayerGameData.GetCurrentChara().IsFakeClear;
		bool inAdvanceAppearEventChara = info.IsCpu && ((GameDataManager.IsClearProEventEnable() && charaType == CharaType.Chara12) || (GameDataManager.IsIyoProEventEnable() && charaType == CharaType.Chara13));
		MFG.Types.Customize.Costume.ItemID costumeItemID = info.PlayerGameData.GetCurrentChara().CostumeItemEquip;
		int variantID = info.PlayerGameData.GetCurrentChara().CharaVariantID;
		StandingCharaController[] standingCharaControllers = _standingCharaControllers;
		int num = i;
		standingCharaControllers[num] = await m_charaManager.Load(costumeItemID, m_playersParent2Ds[i]);
		m_standingCharaCamera.Add(m_standingCharaParent3D.Find($"{costumeItemID}/Camera").GetComponent<StandingCharaFPSAdjuster>());
		for (int j = 0; j <= i; j++)
		{
			m_standingCharaCamera[j].gameObject.SetActive(value: false);
		}
		if (i > 0)
		{
			m_standingCharaParent3D.Find($"{costumeItemID}/Parent/Directional Light").GetComponent<Light>().enabled = false;
		}
		if (variantID > 0)
		{
			int partsIndex = variantID - 1;
			m_charaManager.Parts_SetExclusiveDisp(i, partsIndex);
		}
		else
		{
			m_charaManager.Parts_SetDispAllOff(i);
		}
		var (matchingBG, trophy) = await m_playerMovies[i].transform.GetComponent<MatchingCharaMovie>().MatchingCutinSetup(info, charaType, m_danBanner, debugMatcingBGs[i]);
		m_bgObject[i] = m_baseMoviePlayers[i].transform.GetComponent<MatchingScreenPlayer>().MatchingScreenPlayerSetup(info, charaType, m_danBanner, matchingBG, trophy);
		await m_players[i].GetComponent<MatchingPlayer>().ProCompSet(charaType, isPro);
		if (inAdvanceAppearEventChara)
		{
			m_clearAppearances[i].GetComponent<Animator>().SetTrigger("In");
		}
		MatchingFunction.ResetNext();
		_matchingAnimPlayingTime = Time.time;
		m_players[i].GetComponent<Animator>().SetTrigger("Next");
		if (GameDataManager.YellCampaign.IsEnable() && !info.IsCpu && GameDataManager.YellCampaign.IsCharacterEnable(charaType))
		{
			m_players[i].transform.Find("Yell").gameObject.SetActive(value: true);
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.ResolveYellRankImage(info.PlayerGameData.YellRank, isTaikyoku: true));
			disposableAsset.AddTo(this);
			m_players[i].transform.Find("Yell").Find("YellRank").Find("RankText")
				.GetComponent<Image>()
				.sprite = disposableAsset.Asset;
			m_players[i].transform.Find("Yell").Find("YellRank").GetComponent<Animator>()
				.SetTrigger("In");
		}
		m_windows[i].transform.Find("PlayerName").GetComponent<TextMeshProUGUI>().text = info.GetPlayerName();
		m_cpuImages[i].SetActive(info.IsCpu);
		m_odekakeImages[i].SetActive(info.PlayerGameData.IsOdekake);
		m_playerMovies[i].SetActive(value: true);
		PlayMotion(i, charaType).Forget();
		if (isFakeClear)
		{
			VoiceManager.FakeClear_PlayVoiceMatching(info.PlayerGameData.GetCurrentChara().CharaVariantID);
		}
		else
		{
			SoundManager.StopVoiceAll(0.2f).Forget();
			VoiceManager.PlayVoiceMatching(new Player
			{
				m_charaType = charaType,
				m_level = info.PlayerGameData.GetCurrentChara().Level
			});
		}
		switch (i)
		{
		case 0:
			m_baseMovie.GetComponent<Animator>().SetTrigger("Matching1");
			break;
		case 1:
			m_baseMovie.GetComponent<Animator>().SetTrigger("Matching2");
			break;
		case 2:
			m_baseMovie.GetComponent<Animator>().SetTrigger("Matching3");
			break;
		case 3:
			m_baseMovie.GetComponent<Animator>().SetTrigger("Matching4");
			break;
		}
		m_matchingCharaCamera[i].CameraMove(charaType).Forget();
		Stamp_OnStamp(i, _latestStampIDs[i], cancellationToken).Forget();
		await UniTask.Delay(TimeSpan.FromSeconds(4.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		if (!(m_playerMovies[i] != null))
		{
			return;
		}
		m_playerMovies[i].GetComponent<Animator>().SetTrigger("End");
		await UniTask.Delay(TimeSpan.FromSeconds(0.4166666567325592));
		int playerNum = MahjongUtility.GetPlayerNum(GameDataManager.Mahjong.TakuType);
		if ((from x in m_isMatchingEstablished.ToList().Select((bool p, int q) => new
			{
				Content = p,
				Index = q
			})
			where x.Index > i && x.Index < playerNum && x.Content
			select x).Count() <= 0 || i == playerNum - 1)
		{
			SoundManager.PlaySe("SFX_SYS_ENTRY3").Forget();
			for (int k = 0; k <= i; k++)
			{
				m_standingCharaCamera[k].gameObject.SetActive(value: true);
			}
		}
		m_playersParent2Ds[i].SetActive(value: true);
		if (_standingCharaControllers[i] != null)
		{
			_standingCharaControllers[i].PlayMotion("MatchingOut");
		}
		m_matchingCharaCamera[i].Cam.enabled = false;
		await UniTask.WaitUntil(() => m_playerMovies[i].GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("CharacterMovieStay"), PlayerLoopTiming.Update, cancellationToken);
		m_playerMovies[i].SetActive(value: false);
	}

	private async UniTask PlayMotion(int i, CharaType chara)
	{
		if (!(_standingCharaControllers[i] != null))
		{
			return;
		}
		_standingCharaControllers[i].PlayMotion("MatchingIn");
		await UniTask.Delay(TimeSpan.FromSeconds((float)MasterGameDataManager.Character.Properties(chara).MatchingCharaKimePoseParam / 60f));
		m_players[i].GetComponent<MatchingPlayer>().ProCompIn();
		for (int j = 0; j <= i; j++)
		{
			MatchingBGController component = m_bgObject[j].transform.GetComponent<MatchingBGController>();
			if (component != null)
			{
				component.CharaCenterAdjuster(i);
				component.NameParentAdjuster(i);
			}
		}
	}

	private void Stamp_Init(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (_stampSelectWindow == null || _stampSelectWindowButton == null)
		{
			return;
		}
		_stampPosition = -1L;
		if (GameDataManager.Account.IsGuestUser)
		{
			_stampSelectWindowButton.gameObject.SetActive(value: false);
		}
		Stamp_SetActive(isActive: false);
		List<MFG.Types.Customize.CustomizeItem.ItemID> stampHistory = GameDataManager.Player.StampHistory;
		List<MFG.Types.Customize.CustomizeItem.ItemID> list = (from a in GameDataManager.Customize.CustomizeItems
			where a.Master.Category == MFG.Types.Customize.CustomizeItem.Category.TableSticker && a.ItemID != MFG.Types.Customize.CustomizeItem.ItemID.TableStickerNone && a.ItemID != MFG.Types.Customize.CustomizeItem.ItemID.None
			orderby a.Master.Sort, a.Master.charaType, a.ItemID
			select a.ItemID).ToList();
		if (GameUtility.IsKonastaOrNetcafeBehavior)
		{
			GameDataManager.Player.StampHistory = stampHistory.Where((MFG.Types.Customize.CustomizeItem.ItemID x) => !CustomizeItemMaster.IsPonnomichi(x)).ToList();
			stampHistory = GameDataManager.Player.StampHistory;
			list = list.Where((MFG.Types.Customize.CustomizeItem.ItemID x) => !CustomizeItemMaster.IsPonnomichi(x)).ToList();
		}
		_stampSelectWindow.SetItem(stampHistory, list);
		_stampSelectWindow.Reflesh();
		_stampSelectWindowButton.OnClickAsObservable().Subscribe(async delegate
		{
			try
			{
				cancellationToken.ThrowIfCancellationRequested();
				_stampSelectWindowButton.enabled = false;
				await Stamp_OpenClose(cancellationToken);
				cancellationToken.ThrowIfCancellationRequested();
				_stampSelectWindowButton.enabled = true;
			}
			catch (OperationCanceledException)
			{
				Debug.Log("_stampSelectWindowButton: OperationCanceledException");
			}
		}).AddTo(this);
		_stampSelectWindow.OnStampSelected.Subscribe(async delegate(MFG.Types.Customize.CustomizeItem.ItemID itemID)
		{
			cancellationToken.ThrowIfCancellationRequested();
			_stampCancellationTokenSource.Cancel();
			_stampCancellationTokenSource.Dispose();
			_stampCancellationTokenSource = new CancellationTokenSource();
			using CancellationTokenSource internalCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _stampCancellationTokenSource.Token);
			CancellationToken token = internalCts.Token;
			try
			{
				_latestStampIDs[m_commandClient.PlayerIndex] = itemID;
				Stamp_OnStamp(m_commandClient.PlayerIndex, itemID, token).Forget();
				GChat.ResultData resultData2 = await RequestApiBase<GChat, GChat.ResultData, (MFG.Types.Customize.CustomizeItem.ItemID, int, int, int)>.Send((itemID, m_commandClient.TakuID, m_commandClient.PlayerIndex, m_commandClient.PlayerID), enableErrorScreenTransition: false, enableSendLog: false, token);
				Stamp_Received(resultData2.Stamp, token);
			}
			catch (NetworkResponseException)
			{
			}
			catch (OperationCanceledException)
			{
			}
		}).AddTo(this);
		(from x in Observable.Interval(TimeSpan.FromSeconds(1.0))
			where m_isMatchingEnd
			select x).Subscribe(async delegate
		{
			try
			{
				GChat.ResultData resultData = await RequestApiBase<GChat, GChat.ResultData, (MFG.Types.Customize.CustomizeItem.ItemID, int, int, int)>.Send((MFG.Types.Customize.CustomizeItem.ItemID.None, m_commandClient.TakuID, m_commandClient.PlayerIndex, m_commandClient.PlayerID), enableErrorScreenTransition: false, enableSendLog: false, cancellationToken);
				Stamp_Received(resultData.Stamp, cancellationToken);
			}
			catch (NetworkResponseException)
			{
			}
			catch (OperationCanceledException)
			{
			}
		}).AddTo(this);
	}

	private void Stamp_SetActive(bool isActive)
	{
		if (!(_stampSelectWindowButton == null))
		{
			_stampSelectWindowButton.interactable = isActive;
		}
	}

	private async UniTask Stamp_OpenClose(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		await _stampSelectWindow.OpenClose(cancellationToken);
	}

	private void Stamp_Received(StampData stampData, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		lock (_stampLock)
		{
			try
			{
				foreach (StampData.Element item in stampData.Elements.Where((StampData.Element x) => x.SeqID > _stampPosition && x.PlayerIndex != m_commandClient.PlayerIndex).GroupBy((StampData.Element x) => x.PlayerIndex, (int key, IEnumerable<StampData.Element> value) => value.OrderByDescending((StampData.Element x) => x.SeqID).First()))
				{
					_latestStampIDs[item.PlayerIndex] = item.TableStickerItemID;
					Stamp_OnStamp(item.PlayerIndex, item.TableStickerItemID, cancellationToken).Forget();
				}
			}
			catch
			{
			}
			if (stampData.Elements.Count > 0)
			{
				_stampPosition = stampData.Elements.Max((StampData.Element x) => x.SeqID);
			}
		}
	}

	private async UniTask Stamp_OnStamp(int playerIndex, MFG.Types.Customize.CustomizeItem.ItemID itemID, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (m_players[playerIndex].activeSelf && m_isMatchingEstablished[playerIndex] && itemID != 0)
		{
			StampBalloon component = m_players[playerIndex].transform.Find("StampBalloon").GetComponent<StampBalloon>();
			CharaType selectChara = GameDataManager.Taikyoku.PlayerInfos[playerIndex].PlayerGameData.SelectChara;
			Vector2 matchingStampOffset = MasterGameDataManager.Character.Properties(selectChara).MatchingStampOffset;
			if (!(component == null))
			{
				await component.Play(itemID, matchingStampOffset, cancellationToken);
			}
		}
	}
}
