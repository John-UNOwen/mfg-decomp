using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.Assets;
using MFG.GameData;
using MFG.GameData.EventTaku;
using MFG.GameRequest;
using MFG.MahjonPlay.Hand;
using MFG.MahjongPlay.Character;
using MFG.MahjongPlay.Cutin;
using MFG.MasterGameData;
using MFG.Sytem.Utilities;
using MFG.Taikyoku;
using MFG.Taikyoku.Command;
using MFG.Taikyoku.Command.Receive;
using MFG.Taikyoku.Command.Send;
using MFG.Types;
using MFG.Types.Customize.Costume;
using MFG.Types.Customize.CustomizeItem;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.UI;

public class MahjongPlayFlow : BaseFlow
{
	private class FlickJudge
	{
		private const float flickTime = 0.2f;

		private const float flickMagnitude = 50f;

		public float m_time;

		public Vector3 m_position;

		public Tile m_tile;

		public void FlickStart(float time, Vector3 position, Tile tile)
		{
			m_time = time;
			m_position = position;
			m_tile = tile;
		}

		public bool IsFlicked()
		{
			if (ResidentFlow.CustomInput.mousePosition.y > m_position.y + 50f && Time.time - m_time < 0.2f)
			{
				return true;
			}
			return false;
		}

		public Tile GetFlickedTile()
		{
			return m_tile;
		}
	}

	[SerializeField]
	[Tooltip("テスト用キャラ表示時(四麻)のキャラを指定します\nElement0…自家 Element1…下家 Element2…対面 Element3…上家")]
	private List<MFG.Types.Customize.Costume.ItemID> m_testCharaAssign = new List<MFG.Types.Customize.Costume.ItemID>
	{
		MFG.Types.Customize.Costume.ItemID.Costume01_001,
		MFG.Types.Customize.Costume.ItemID.Costume02_001,
		MFG.Types.Customize.Costume.ItemID.Costume03_001,
		MFG.Types.Customize.Costume.ItemID.Costume04_001
	};

	[SerializeField]
	[Tooltip("テスト用順位指定時、各家の順位を指定します\nElement0…自家 Element1…下家 Element2…対面 Element3…上家")]
	private List<Rank> m_testRankAssign = new List<Rank>
	{
		Rank.Rank1,
		Rank.Rank2,
		Rank.Rank3,
		Rank.Rank4
	};

	[SerializeField]
	public ViewManager m_viewManager;

	[SerializeField]
	private GameObject mainCamera;

	[SerializeField]
	public Camera m_3dCamera;

	[SerializeField]
	public Camera m_3dCameraTehai;

	[SerializeField]
	public Camera m_3dCameraMyChara;

	[SerializeField]
	public List<Camera> m_3dCharaStandCameras;

	[SerializeField]
	private List<Camera> m_3dSimpleViewCameras;

	[SerializeField]
	public Camera m_bgCamera;

	[SerializeField]
	private GameObject m_3dTexture;

	[SerializeField]
	private GameObject m_3dTextureTehai;

	[SerializeField]
	private GameObject m_fadePanel;

	[SerializeField]
	private GameObject m_3dTextureTehaiBack;

	[SerializeField]
	public GameObject myTurn;

	[SerializeField]
	public GameObject pleaseWait;

	[SerializeField]
	public Wanpai wanpai;

	[SerializeField]
	private GameObject wanpaiTexture;

	[SerializeField]
	public NakiPattern naki;

	[SerializeField]
	public ControlPanel m_controlPanel;

	[SerializeField]
	public TakuInfo m_takuInfo;

	[SerializeField]
	private Light m_jichaDirectionalLight;

	[SerializeField]
	private List<Player> players = new List<Player>();

	[SerializeField]
	private GameObject playerName;

	[SerializeField]
	private List<GameObject> playerNames = new List<GameObject>();

	[SerializeField]
	public Taku m_taku;

	[SerializeField]
	private List<GameObject> turnLines = new List<GameObject>();

	private GameObject mySpotLight;

	[SerializeField]
	private List<GameObject> spotLights = new List<GameObject>();

	[SerializeField]
	private List<GameObject> spotLightBehinds = new List<GameObject>();

	[SerializeField]
	public List<MachihaiPanel> machihaiPanels = new List<MachihaiPanel>();

	[SerializeField]
	public List<GameObject> m_proStands;

	[SerializeField]
	public GameObject m_view2ShadowBG;

	[SerializeField]
	public GameObject m_view3ShadowBG;

	[SerializeField]
	public GameObject agariHaiEffect;

	[SerializeField]
	private GameObject m_otherCharaParent;

	[SerializeField]
	private GameObject m_otherCharaParentTop;

	[SerializeField]
	private List<GameObject> m_charaParent;

	[SerializeField]
	private List<GameObject> m_charaParentTop;

	[SerializeField]
	private List<GameObject> m_charaArmParent;

	[SerializeField]
	private List<GameObject> m_chara3DParent;

	[SerializeField]
	private PlayableDirector m_challengerAppearance;

	[SerializeField]
	public PlayableDirector m_ryukyokuCall;

	[SerializeField]
	public KyokuCall m_kyokuCall;

	[SerializeField]
	public GameObject m_scorePanelRoot;

	[SerializeField]
	[HideInInspector]
	public ScorePanel m_scorePanel;

	[SerializeField]
	public AttentionMessage m_attentionMessage;

	[SerializeField]
	private TaikyokuStart m_taikyokuStart;

	[SerializeField]
	public LastTsumo m_lastTsumo;

	[SerializeField]
	public Help m_help;

	[SerializeField]
	public Agari agari;

	[SerializeField]
	public ThinkCount thinkCount;

	[SerializeField]
	public DanBanner danBanner;

	[SerializeField]
	public GameObject agariCharaEffectRoot;

	[SerializeField]
	public GameObject agariCharaEffectInstantiateRoot;

	public GameObject agariCharaEffectChara;

	[SerializeField]
	private GameObject m_SDCharaTop;

	public SdChara m_sdChara;

	public SdChara m_sdCharaTsumo;

	[SerializeField]
	public GameObject agariEffect;

	[SerializeField]
	public GameObject agariManganEffect;

	[SerializeField]
	public Shake shake;

	[SerializeField]
	private List<Image> m_stampIcons = new List<Image>();

	[SerializeField]
	public Button m_stampButton;

	[SerializeField]
	public CutinManager m_cutinManager;

	[SerializeField]
	public GameObject _blowAwayCutinRoot;

	[HideInInspector]
	public BlowAwayEventCutin _blowAwayEventCutin;

	public List<Player> Players = new List<Player>();

	[SerializeField]
	public List<HaiNavi> m_haiNavi = new List<HaiNavi>();

	[SerializeField]
	public HaiConjecture m_haiConjecture;

	[SerializeField]
	public HaiConjectureIconList m_haiConjectureIcon;

	[SerializeField]
	public HaiShantenIconList m_haiShantenIcon;

	[SerializeField]
	public HaiShantenCounter m_haiShantenCounter;

	[SerializeField]
	public HaiNaviPanel m_haiNaviPanel;

	[SerializeField]
	public MentsuCompleteEffect m_mentsuCompleteEffect;

	[SerializeField]
	public GameObject m_concentratedLlineEffect;

	[SerializeField]
	public GameObject m_stageBGParent;

	[SerializeField]
	[HideInInspector]
	public StageBG m_StageBG;

	[SerializeField]
	public Transform m_stageBGMyCharacter;

	[SerializeField]
	public GameObject m_stageBGMyCharacterTesAdd;

	[SerializeField]
	[HideInInspector]
	public GameObject m_backScreen;

	[SerializeField]
	public GameObject m_backScreenTrigger;

	[SerializeField]
	public List<GameObject> m_backScreenGrows;

	[SerializeField]
	public GameObject m_kyokuTransition;

	[SerializeField]
	public GameObject m_maAgari;

	[SerializeField]
	public AccelDoraRemainCounter _accelDoraRemainCounter;

	[SerializeField]
	public MeldBonusTakuBonusCounter _meldBonusCounter;

	[SerializeField]
	public BombEventDisplay m_bombDisplay;

	[SerializeField]
	public List<GameObject> m_indicateAgaris = new List<GameObject>();

	[SerializeField]
	public Sprite m_indicateAgariSpriteTsumo;

	[SerializeField]
	public Sprite m_indicateAgariSpriteRon;

	[SerializeField]
	public GameObject CoercionTumoText;

	[SerializeField]
	public GameObject DaiuchiText;

	[SerializeField]
	public MahjongMusicTextDisplay MusicText;

	private CameraContext _cameraContext = new CameraContext
	{
		LocalPosition = new Vector3(5f, 1.91f, -2.21f),
		FieldOfView = 20f
	};

	private GetMenuData.ReconnectData m_reconnectData;

	private Tile m_touchedTile;

	public int m_nokoriHaiNum;

	public int m_doraHyoujiNum;

	public bool m_isTaikyokuActive;

	public bool m_isKikenRequest;

	public bool m_isKanExecuted;

	public NakiType m_isKanExecutedType = (NakiType)(-1);

	public int m_minkanPlayerIndex;

	private bool m_isHaiSelectEnable;

	public bool m_isRuleDisplaying;

	public bool m_taikyokuEnd;

	public bool m_isFirstTsumo = true;

	public Cha m_lastReachedCha;

	public bool m_isDaiuchi;

	public List<Pai> m_openMachiList = new List<Pai>();

	public int m_junmeNum;

	public int m_actionCount;

	private int m_step;

	public RECEIVE_COMMAND_TYPE m_cmdKind = RECEIVE_COMMAND_TYPE.TSUMO;

	private bool m_isNextCmd;

	public bool m_isCameraMyCharaAdulationbefore;

	public List<string> m_aotenjoLastScore = new List<string> { "", "", "" };

	private ExecTsumo execTsumo = new ExecTsumo();

	private ExecSutehai execSutehai = new ExecSutehai();

	private ExecAgari execAgari = new ExecAgari();

	private ExecTsumoChoices execTsumoChoices = new ExecTsumoChoices();

	private ExecSuteChoices execSuteChoices = new ExecSuteChoices();

	private ExecNaki execNaki = new ExecNaki();

	private ExecKyoku execKyoku = new ExecKyoku();

	public bool m_isMahjongCoreError;

	private CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource();

	public CancellationToken m_cancellationToken;

	private Cha m_worstPlayerCha;

	private bool _isStageInitialized;

	private int m_width = -1;

	private int m_height = -1;

	public bool m_haiDiscardExecute;

	private bool m_isHaiSelecting;

	[SerializeField]
	private FlickJudge m_flickJudge = new FlickJudge();

	private Dictionary<KeyCode, (bool PressedBeforeFrame, bool PressedThisFrame)> CheckKeyCode = new Dictionary<KeyCode, (bool, bool)>
	{
		{
			KeyCode.Alpha1,
			(false, false)
		},
		{
			KeyCode.Alpha2,
			(false, false)
		},
		{
			KeyCode.Alpha3,
			(false, false)
		},
		{
			KeyCode.Alpha4,
			(false, false)
		},
		{
			KeyCode.Alpha5,
			(false, false)
		},
		{
			KeyCode.Alpha6,
			(false, false)
		},
		{
			KeyCode.Alpha7,
			(false, false)
		},
		{
			KeyCode.Alpha8,
			(false, false)
		},
		{
			KeyCode.Alpha9,
			(false, false)
		},
		{
			KeyCode.Alpha0,
			(false, false)
		},
		{
			KeyCode.Minus,
			(false, false)
		},
		{
			KeyCode.Equals,
			(false, false)
		},
		{
			KeyCode.None,
			(false, false)
		},
		{
			KeyCode.Backspace,
			(false, false)
		}
	};

	[SerializeField]
	private int _debugParallelCount = 100;

	[SerializeField]
	private bool _enableDebugDivide = true;

	private float m_timingCounter;

	public bool m_firstKyoku = true;

	public bool m_firstAllLast = true;

	private GameObject m_prevBackScreenBase;

	private List<GameObject> m_prevBackScreenTriggerBase = new List<GameObject>();

	private bool m_isBackScreenPlaying;

	private CharaReactionData.ReactionData m_currentReactionData;

	private CancellationTokenSource _backScreenCts;

	private bool _isFadePanelOpen;

	private CommandClient m_commandClient => GameDataManager.CommandClient;

	private EntryTakuInfo m_entryTakuInfo => GameDataManager.Taikyoku.TakuInfo;

	private PlayerGameData m_playerGameData => GameDataManager.Player;

	private SystemConfigGameData m_systemConfigGameData => GameDataManager.System;

	private AccountGameData m_accountGameData => GameDataManager.Account;

	private PlayerConfigGameData m_playerConfigGameData => GameDataManager.PlayerConfig;

	public bool m_isRecNakiCutin => GameDataManager.Mahjong.IsRecNakiCutIn;

	public bool m_isRecReachCutin => GameDataManager.Mahjong.IsRecReachCutIn;

	public bool m_isRecAgariCutin => GameDataManager.Mahjong.IsRecAgariCutIn;

	public bool IsProAnimEnable(int index = -1)
	{
		if (index < 0)
		{
			return Players.Exists((Player x) => x.IsProAnimEnable());
		}
		return Players[index].IsProAnimEnable();
	}

	public void SetOpenMachiHaiList()
	{
		List<Pai> list = new List<Pai>();
		foreach (Player player in Players)
		{
			if (player.m_cha != 0 && player.m_isOpenReach && !player.m_isOpenFuriten)
			{
				list.AddRange(player.m_openMachiPais);
			}
		}
		m_openMachiList = (from x in list.Distinct()
			orderby x
			select x).ToList();
	}

	private MahjongPlayFlow()
	{
		_enableLoadingTipsDisp = true;
		_enableDebugLogin = false;
	}

	protected override void Awake()
	{
		foreach (Transform item in m_otherCharaParent.transform)
		{
			if (item.gameObject.tag == "Debug")
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
	}

	private void OnDestroy()
	{
		if (m_cancellationTokenSource != null)
		{
			m_cancellationTokenSource.Cancel();
		}
		foreach (HaiNavi item in m_haiNavi)
		{
			item.Dispose();
		}
		ResidentFlow.GetInstance().CameraContext_Restore();
	}

	public Player GetPlayerWork(Cha cha)
	{
		return players[(int)cha];
	}

	public List<Player> GetPlayerWorks()
	{
		return players;
	}

	protected override async UniTask OnInitialize(CancellationToken cancellationToken)
	{
		m_cancellationToken = m_cancellationTokenSource.Token;
		ResidentFlow.GetInstance().CameraContext_Reserve();
		ResidentFlow.GetInstance().CameraContext_Restore(_cameraContext);
		Stopwatch sw = new Stopwatch();
		GameDataManager.Log.TaikyokuInit();
		await StageBG_Create();
		await ScorePanel_Create();
		VoiceManager.VoiceHistoryInit();
		if (string.IsNullOrEmpty(m_accountGameData.AppSessionID) && !GameDataManager.Mahjong.IsHaifuPlay)
		{
			await FadePanel_Close();
			return;
		}
		SetHaiSelectEnable(enable: false);
		MahjongUtility.ResetNoInputCount();
		DestructAll();
		m_haiConjecture.OnMyConjectureChanged.Subscribe(delegate(HaiConjectureList c)
		{
			m_haiConjectureIcon.Disp(c);
			Players[m_commandClient.PlayerIndex].hand.OpenHoujuTargetIconSet(m_openMachiList);
		}).AddTo(this);
		try
		{
			int num = 0;
			_ = num - 3;
			_ = 2;
			try
			{
				if (GameDataManager.Mahjong.IsHaifuPlay)
				{
					MahjongPlayDebug.PrepareDebugHaifu();
				}
				else
				{
					GameUtility.SendPcbGameModeStartEvent(GameDataManager.Mahjong.TakuType);
					m_commandClient.OnCpuPlayerChangedData.Subscribe(delegate((int playerIndex, bool isCpu) cpuPlayerData)
					{
						GameDataManager.Taikyoku.PlayerInfos[cpuPlayerData.playerIndex].PlayerType = ((!cpuPlayerData.isCpu) ? PlayerType.Human : PlayerType.Cpu);
						if (Players.Count > cpuPlayerData.playerIndex)
						{
							Player player = Players[cpuPlayerData.playerIndex];
							playerNames[(int)player.m_cha].transform.Find("CpuIcon").gameObject.SetActive(cpuPlayerData.isCpu);
						}
					}).AddTo(this);
				}
				m_cutinManager.RandomValueGenerator_SetSeed(m_entryTakuInfo.TakuID);
				VoiceManager.SetCutinRandomSeed(m_entryTakuInfo.TakuID);
				sw.Start();
				await CharaModel_Init(m_cancellationToken);
				sw.Stop();
				UnityEngine.Debug.Log("MahjongPlay:CharaModelInit" + sw.ElapsedMilliseconds + "ms");
				await PlayerConfig_Affect(m_cancellationToken);
				GameDataManager.BingoEvent.Init();
				GameDataManager.BingoEventRemake.Init();
				GameDataManager.EventTaku.ResetForBeforeTaikyokuStart(Players);
				if (!GameDataManager.Mahjong.IsHaifuPlay)
				{
					m_commandClient.NotifyReadyToKyokustart();
					while (!m_commandClient.IsReadyToKyokuStartAll)
					{
						await m_commandClient.Fetch(cancellationToken);
					}
				}
			}
			catch (OperationCanceledException)
			{
				UnityEngine.Debug.Log(GetType().Name + MethodBase.GetCurrentMethod().Name + ": OperationCanceledException Catched.");
			}
		}
		finally
		{
			await FadePanel_Close();
		}
		_isStageInitialized = true;
	}

	protected override async UniTask OnExecute(CancellationToken cancellationToken)
	{
		if (!_isStageInitialized)
		{
			return;
		}
		try
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (m_isRecNakiCutin || m_isRecReachCutin || m_isRecAgariCutin)
			{
				DebugRecordStart();
			}
			if (SystemConfigGameData.IsDualMonitor)
			{
				if (!GameDataManager.Mahjong.IsHaifuPlay)
				{
					await SubMonitorFlow.GetInstance().TaikyokuStart(Players, m_jichaDirectionalLight);
				}
				else
				{
					try
					{
						m_jichaDirectionalLight.enabled = false;
						SubMonitorFlow.DemoCharaCostume = Players?[0].CostumeItemEquip ?? MFG.Types.Customize.Costume.ItemID.Costume01_001;
					}
					catch
					{
					}
				}
			}
			await m_taikyokuStart.TaikyokuStartEffectPlay(m_cancellationToken);
			await TaikyokuStart();
		}
		catch (OperationCanceledException)
		{
			UnityEngine.Debug.Log(GetType().Name + MethodBase.GetCurrentMethod().Name + ": OperationCanceledException Catched.");
		}
		SoundManager.StopBgmWithFadeout().Forget();
		m_taikyokuEnd = true;
		GameDataManager.Account.IsTaikyokuEnd = m_taikyokuEnd;
		if (!GameDataManager.Mahjong.IsHaifuPlay)
		{
			GotoNextScene("Result");
		}
	}

	private async UniTask TaikyokuStart()
	{
		m_cancellationToken.ThrowIfCancellationRequested();
		m_isTaikyokuActive = true;
		m_isNextCmd = false;
		m_firstKyoku = true;
		m_firstAllLast = true;
		UnityEngine.Debug.Log("ExecTaikyoku begin");
		await ExecTaikyoku();
		UnityEngine.Debug.Log("ExecTaikyoku end");
		MahjongResultInfo mahjongResultInfo;
		PlayerResultInfo playerResultInfo;
		if (GameDataManager.Mahjong.IsHaifuPlay)
		{
			await UniTask.WaitUntil(() => !m_commandClient.IsBusy);
			mahjongResultInfo = null;
			playerResultInfo = null;
		}
		else if (m_isKikenRequest)
		{
			await UniTask.WaitUntil(() => !m_commandClient.IsBusy);
			await m_commandClient.Send(new Kiken());
			mahjongResultInfo = (await RequestApiBase<KikenGame, KikenGame.ResultData, (int, int, int)>.Send((m_entryTakuInfo.TakuID, m_entryTakuInfo.PlayerIndex, m_accountGameData.PlayerID))).MahjongResultInfo;
			playerResultInfo = mahjongResultInfo.PlayerResultInfos[m_entryTakuInfo.PlayerIndex];
			GameDataManager.Mahjong.IsKiken = true;
			if (GameDataManager.Mahjong.IsKiken)
			{
				GameUtility.IsReservedGameOver = true;
			}
		}
		else
		{
			mahjongResultInfo = (await RequestApiBase<EndGame, EndGame.ResultData, (int, int, int)>.Send((m_entryTakuInfo.TakuID, m_entryTakuInfo.PlayerIndex, m_accountGameData.PlayerID))).MahjongResultInfo;
			playerResultInfo = mahjongResultInfo.PlayerResultInfos[m_entryTakuInfo.PlayerIndex];
			if (MahjongUtility.IsNoInputAutoTsumogiri())
			{
				GameUtility.IsReservedGameOver = true;
			}
		}
		LEDManager.Instance.AnimColor(new Color[1] { Color.white }, isSpread: true);
		if (!GameDataManager.Mahjong.IsHaifuPlay)
		{
			GameDataManager.PlayerConfig.ViewType = MahjongUtility.GetViewType();
		}
		if (!GameDataManager.Mahjong.IsHaifuPlay)
		{
			GameDataManager.Mahjong.ResultInfo = mahjongResultInfo;
			GameDataManager.Mahjong.Rank = playerResultInfo.Rank;
			GameDataManager.Mahjong.Score = playerResultInfo.Score + playerResultInfo.Uma;
			await TaikyokuEndResult(mahjongResultInfo);
			await m_taikyokuStart.TaikyokuEndEffectPlay();
		}
	}

	private async UniTask TaikyokuEndResult(MahjongResultInfo mahjongResult)
	{
		if (SystemConfigGameData.IsDualMonitor)
		{
			await SubMonitorFlow.GetInstance().TaikyokuFinish();
		}
		PlayerResultInfo playerResultInfo = mahjongResult.PlayerResultInfos[m_entryTakuInfo.PlayerIndex];
		bool flag = false;
		bool isProMatching = false;
		foreach (Player player in Players)
		{
			int index = Players.IndexOf(player);
			PlayerResultInfo playerResultInfo2 = mahjongResult.PlayerResultInfos[index];
			player.SetFinalRank(playerResultInfo2.Rank);
			m_scorePanel.SetFinalScore(playerResultInfo2.Score, playerResultInfo2.Rank, playerResultInfo2.Uma, player);
			if (playerResultInfo2.Rank == MahjongUtility.GetWorstRank(GameDataManager.Mahjong.TakuType))
			{
				m_worstPlayerCha = player.m_cha;
			}
			if (!player.IsJicha() && player.m_isPro)
			{
				isProMatching = true;
				MahjongActorID mahjongActorID = MahjongActorMaster.GetItem(player.m_charaType)?.MahjongActorID ?? MahjongActorID.None;
				flag = playerResultInfo.Rank < playerResultInfo2.Rank;
				GameDataManager.Player.MahjongActorMatchRecord_AddCount(mahjongActorID, flag);
				GameDataManager.Mission.WinningPro = ((flag && !m_isKikenRequest) ? mahjongActorID : MahjongActorID.None);
			}
		}
		GameDataManager.EventTaku.OnEvaluateResult(GameDataManager.Mahjong, GameDataManager.Mission.IsMissionProgressBoost);
		GameDataManager.Vote.OnEvaluateResult((Rank)GameDataManager.Mahjong.Rank, GameDataManager.Mission.IsMissionProgressBoost, isProMatching, flag, GameDataManager.Mahjong.IsKiken);
		ResultInfo blowAwayResult = GetBlowawayResultInfo();
		if (blowAwayResult != null)
		{
			await BlowAwayEventCutin_Play(blowAwayResult);
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(10.0)), UniTask.WaitUntil(() => ResultFlow.Skip()));
		}
		m_StageBG.StageState = StageState.FinalResult;
		await m_scorePanel.Play(isfinal: true);
		if (blowAwayResult != null)
		{
			await BlowAwayEventCutin_End();
		}
		GameDataManager.Log.TaikyokuFinish((Rank)Players[m_commandClient.PlayerIndex].m_rank);
		SoundManager.StopBgmWithFadeout().Forget();
	}

	private ResultInfo GetBlowawayResultInfo()
	{
		BaseEventGameData baseEventGameData = GameDataManager.EventTaku.Current(MFG.Types.EventType.ToitoiButtobiGalaxy);
		BaseEventGameData baseEventGameData2 = GameDataManager.EventTaku.Current(MFG.Types.EventType.Aotenjo);
		BaseEventGameData baseEventGameData3 = GameDataManager.EventTaku.Current(MFG.Types.EventType.FireGalaxy);
		BaseEventGameData baseEventGameData4 = GameDataManager.EventTaku.Current(MFG.Types.EventType.PCKGalaxy);
		ResultInfo resultInfo = null;
		if (baseEventGameData.IsActiveAtTaikyoku(GameDataManager.Mahjong.EventType, GameDataManager.Mahjong.TakuType, GameDataManager.Mahjong.IsConstancyTaku, GameDataManager.Mahjong.IsCompetitionTaku))
		{
			resultInfo = (ResultInfo)baseEventGameData.ResultInfo;
			if (resultInfo.BlowAwayInfo.ChaInfos.Values.Any((ChaInfo x) => x.BlowawayState == BlowawayState.Blowawayed))
			{
				return resultInfo;
			}
		}
		else if (baseEventGameData2.IsActiveAtTaikyoku(GameDataManager.Mahjong.EventType, GameDataManager.Mahjong.TakuType, GameDataManager.Mahjong.IsConstancyTaku, GameDataManager.Mahjong.IsCompetitionTaku))
		{
			resultInfo = (ResultInfo)baseEventGameData2.ResultInfo;
			if (resultInfo.BlowAwayInfo.ChaInfos.Values.Any((ChaInfo x) => x.BlowawayState == BlowawayState.Blowawayed))
			{
				return resultInfo;
			}
		}
		else if (baseEventGameData3.IsActiveAtTaikyoku(GameDataManager.Mahjong.EventType, GameDataManager.Mahjong.TakuType, GameDataManager.Mahjong.IsConstancyTaku, GameDataManager.Mahjong.IsCompetitionTaku))
		{
			resultInfo = (ResultInfo)baseEventGameData3.ResultInfo;
			if (resultInfo.BlowAwayInfo.ChaInfos.Values.Any((ChaInfo x) => x.BlowawayState == BlowawayState.Blowawayed))
			{
				return resultInfo;
			}
		}
		else if (baseEventGameData4.IsActiveAtTaikyoku(GameDataManager.Mahjong.EventType, GameDataManager.Mahjong.TakuType, GameDataManager.Mahjong.IsConstancyTaku, GameDataManager.Mahjong.IsCompetitionTaku))
		{
			resultInfo = (ResultInfo)baseEventGameData4.ResultInfo;
			if (resultInfo.BlowAwayInfo.ChaInfos.Values.Any((ChaInfo x) => x.BlowawayState == BlowawayState.Blowawayed))
			{
				return resultInfo;
			}
		}
		return null;
	}

	private async UniTask BlowAwayEventCutin_Create(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!(_blowAwayEventCutin != null))
		{
			IDisposableAsset<GameObject> obj = await AssetLoader.LoadAssetAsync<GameObject>("ToitoiGalaxy/Prefabs/RobotCutIn.prefab");
			obj.AddTo(this);
			GameObject gameObject = UnityEngine.Object.Instantiate(obj.Asset, _blowAwayCutinRoot.transform, worldPositionStays: false);
			_blowAwayEventCutin = gameObject.GetComponent<BlowAwayEventCutin>();
		}
	}

	private void BlowAwayEventCutin_Destroy()
	{
		if (!(_blowAwayEventCutin == null))
		{
			UnityEngine.Object.Destroy(_blowAwayEventCutin.gameObject);
			_blowAwayEventCutin = null;
		}
	}

	public async UniTask BlowAwayEventCutin_Play(ResultInfo resultInfo, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (SystemConfigGameData.IsDualMonitor)
		{
			await SubMonitorFlow.GetInstance().AgariOpen();
		}
		await BlowAwayEventCutin_Create();
		await _blowAwayEventCutin.Play(resultInfo, cancellationToken);
	}

	public async UniTask BlowAwayEventCutin_End(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!(_blowAwayEventCutin == null))
		{
			await _blowAwayEventCutin.End(cancellationToken);
			if (SystemConfigGameData.IsDualMonitor)
			{
				await SubMonitorFlow.GetInstance().AgariClose();
			}
			BlowAwayEventCutin_Destroy();
		}
	}

	private int GetDebugUmaScore(int rank)
	{
		return new List<List<int>>
		{
			new List<int> { 5000, 0, 0, -5000 },
			new List<int> { 5000, 0, 0, -5000 },
			new List<int> { 3000, 0, -3000, 0 },
			new List<int> { 0, 0, 0, 0 }
		}[(int)GameDataManager.Mahjong.TakuType][rank];
	}

	private void UpdateCamera()
	{
		if (m_width != Screen.width || m_height != Screen.height)
		{
			Camera component = mainCamera.GetComponent<Camera>();
			m_3dCamera.GetComponent<Camera>();
			float num = Screen.width;
			float num2 = Screen.height;
			float num3 = 1920f;
			float num4 = 1080f;
			float num5 = num / num2;
			float num6 = num3 / num4;
			float num7 = num4 / 2f / 100f;
			if (num5 < num6)
			{
				float num8 = num3 / num;
				float num9 = num4 / (num2 * num8);
				component.rect = new Rect(0f, (1f - num9) * 0.5f, 1f, num9);
			}
			else
			{
				float num10 = num5 / num6;
				num7 *= num10;
				float num11 = num4 / num2;
				float num12 = num3 / (num * num11);
				component.rect = new Rect((1f - num12) * 0.5f, 0f, num12, 1f);
			}
			component.orthographicSize = num7;
			m_width = Screen.width;
			m_height = Screen.height;
		}
	}

	private void UpdateMyCamera()
	{
		CharacterModelController characterModelController = players.Find((Player x) => x.m_cha == Cha.Jicha).character.m_characterModelController;
		if (characterModelController.IsCharaAdulation())
		{
			m_isCameraMyCharaAdulationbefore = true;
			Transform charaHead = characterModelController.GetCharaHead();
			if (charaHead != null)
			{
				Vector3 forward = charaHead.transform.position - m_3dCameraMyChara.transform.position;
				forward.y = 0f;
				Quaternion rotation = Quaternion.LookRotation(forward);
				m_3dCameraMyChara.transform.rotation = rotation;
			}
		}
		else if (m_isCameraMyCharaAdulationbefore)
		{
			m_isCameraMyCharaAdulationbefore = false;
			MyCameraReset();
		}
	}

	public async UniTask HaiSelect()
	{
		if (!m_isHaiSelectEnable || m_isRuleDisplaying)
		{
			return;
		}
		Player player = Players[m_commandClient.PlayerIndex];
		Tile mouseOnTile = GetMouseOnTile();
		if (ResidentFlow.CustomInput.GetMouseButtonDown(0))
		{
			if (mouseOnTile != null)
			{
				m_isHaiSelecting = true;
				m_flickJudge.FlickStart(Time.time, ResidentFlow.CustomInput.mousePosition, mouseOnTile);
				if (player.hand.IsSelected(mouseOnTile))
				{
					player.hand.SetDiscardTile(mouseOnTile);
				}
				else
				{
					player.hand.Select(mouseOnTile, m_sdChara).Forget();
					if (player.hand.IsNowExecTsumoChoices() && !player.m_isReach)
					{
						m_haiNaviPanel.ReloadButtonBySelect(player.hand.GetItemPos(mouseOnTile));
					}
				}
			}
			else
			{
				List<RaycastResult> list = new List<RaycastResult>();
				PointerEventData eventData = new PointerEventData(EventSystem.current)
				{
					position = ResidentFlow.CustomInput.mousePosition
				};
				EventSystem.current.RaycastAll(eventData, list);
				if (list.Count > 0)
				{
					if (list.First().gameObject.name == "3DTextureTehaiBack")
					{
						player.hand.SelectReset(m_sdChara).Forget();
						m_haiNaviPanel.ReloadButtonBySelect(-1);
					}
					else if (list.First().gameObject.name == "ScoreDiffChangeArea")
					{
						foreach (Player player2 in Players)
						{
							player2.m_isDispDiffScore = !player2.m_isDispDiffScore;
							player2.RefleshScoreDisp();
						}
					}
				}
			}
			if (!m_isDaiuchi)
			{
				return;
			}
			List<RaycastResult> list2 = new List<RaycastResult>();
			PointerEventData eventData2 = new PointerEventData(EventSystem.current)
			{
				position = ResidentFlow.CustomInput.mousePosition
			};
			EventSystem.current.RaycastAll(eventData2, list2);
			if (list2.Count > 0)
			{
				string text = list2.First().gameObject.name;
				if (text != "BtAdd" && !list2.Exists((RaycastResult x) => x.gameObject.name.Contains("Window")) && text != "BtIconGameOption" && text != "Yaku" && text != "CloseText" && !list2.Exists((RaycastResult x) => x.gameObject.name.Contains("Frame")))
				{
					DaisouFinish();
				}
			}
			UnityEngine.Debug.Log("代打ちキャンセル？ " + string.Join(',', list2.Select((RaycastResult x) => x.gameObject.name).ToList()));
		}
		else if (ResidentFlow.CustomInput.GetMouseButton(0))
		{
			if (mouseOnTile != null && player.hand.GetSelectedTile() != mouseOnTile)
			{
				player.hand.Select(mouseOnTile, m_sdChara).Forget();
				player.hand.SetDiscardTile(null);
				m_isHaiSelecting = false;
				if (player.hand.IsNowExecTsumoChoices() && !player.m_isReach)
				{
					m_haiNaviPanel.ReloadButtonBySelect(player.hand.GetItemPos(mouseOnTile));
				}
			}
		}
		else
		{
			if (!ResidentFlow.CustomInput.GetMouseButtonUp(0))
			{
				return;
			}
			if (m_isHaiSelecting)
			{
				if (mouseOnTile != null)
				{
					if (player.hand.GetDiscardTile() == mouseOnTile)
					{
						m_haiDiscardExecute = true;
					}
					else
					{
						Tile flickedTile = m_flickJudge.GetFlickedTile();
						if (m_flickJudge.IsFlicked() && player.hand.IsSelected(flickedTile))
						{
							player.hand.SetDiscardTile(flickedTile);
							m_haiDiscardExecute = true;
						}
					}
				}
				else
				{
					Tile flickedTile2 = m_flickJudge.GetFlickedTile();
					if (m_flickJudge.IsFlicked() && player.hand.IsSelected(flickedTile2))
					{
						player.hand.SetDiscardTile(flickedTile2);
						m_haiDiscardExecute = true;
					}
				}
			}
			m_isHaiSelecting = false;
		}
	}

	private List<string> GetFrontObjects(Vector2 pos)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		List<RaycastResult> list = new List<RaycastResult>();
		pointerEventData.position = pos;
		EventSystem.current.RaycastAll(pointerEventData, list);
		List<string> list2 = new List<string>();
		if (list.Count() > 0)
		{
			foreach (RaycastResult item in list)
			{
				string text = item.gameObject.name;
				if (text.Equals("BtAdd"))
				{
					break;
				}
				list2.Add(text);
			}
		}
		return list2;
	}

	private void CheckRayCastButton()
	{
		Vector2[] obj = new Vector2[5]
		{
			new Vector2(1333f, 400f),
			new Vector2(1600f, 510f),
			new Vector2(1333f, 510f),
			new Vector2(1600f, 400f),
			new Vector2(1480f, 450f)
		};
		List<string> list = new List<string>();
		Vector2[] array = obj;
		foreach (Vector2 pos in array)
		{
			list.AddRange(GetFrontObjects(pos));
			list = list.Distinct().ToList();
		}
		if (list.Count > 0)
		{
			UnityEngine.Debug.Log(string.Join(",", list) + "がアガリボタンの上にかかってるかも");
			GameDataManager.Log.AddOverlapsAgariData(list);
		}
	}

	private async UniTask GenerateDebugCharaModel()
	{
		RuntimeAnimatorController motionController = await ModelManager.MotionLoad(MFG.Types.Customize.Costume.ItemID.Costume01_001, isMahjong: true);
		GameObject obj = UnityEngine.Object.Instantiate((await AssetLoader.LoadAssetAsync<GameObject>("Standing3D/Costume01_001_Model.prefab")).Asset, m_chara3DParent[2].transform, worldPositionStays: false);
		obj.transform.Find("CharaModel(Clone)").GetComponent<ModelManager>().Init(MFG.Types.Customize.Costume.ItemID.Costume01_001, motionController, isMahjong: true);
		obj.SetLayerRecursive("Chara3DRight");
	}

	private Tile GetKeyToTile(Player player, int i)
	{
		try
		{
			if (i == 13)
			{
				i = 13 - player.meld.MeldCount * 3;
			}
		}
		catch
		{
		}
		Transform transform = player.transform.Find($"Hand/TileRoot{i + 1}");
		if (!(transform != null))
		{
			return null;
		}
		return transform.GetComponent<Tile>();
	}

	[DllImport("user32.dll")]
	private static extern short GetKeyState(int nVirtKey);

	private void KeyCheck()
	{
		Dictionary<KeyCode, (bool, bool)> dictionary = new Dictionary<KeyCode, (bool, bool)>(CheckKeyCode);
		foreach (KeyCode key in CheckKeyCode.Keys)
		{
			if (GameUtility.IsCloudStreaming)
			{
				dictionary[key] = (CheckKeyCode[key].PressedThisFrame, Bi2xInput.MahjongGetKey(key));
			}
			else
			{
				dictionary[key] = (CheckKeyCode[key].PressedThisFrame, GetKeyState(MahjongUtility.KeyBoard.VirtualKeyCode[key]) < 0);
			}
		}
		CheckKeyCode = dictionary;
	}

	private bool IsCustomKeyDown(KeyCode key)
	{
		if (CheckKeyCode[key].PressedThisFrame)
		{
			return !CheckKeyCode[key].PressedBeforeFrame;
		}
		return false;
	}

	private bool IsCustomKey(KeyCode key)
	{
		if (CheckKeyCode[key].PressedThisFrame)
		{
			return CheckKeyCode[key].PressedBeforeFrame;
		}
		return false;
	}

	private bool IsCustomKeyUp(KeyCode key)
	{
		if (!CheckKeyCode[key].PressedThisFrame)
		{
			return CheckKeyCode[key].PressedBeforeFrame;
		}
		return false;
	}

	private int GetPushingCount()
	{
		return CheckKeyCode.Where((KeyValuePair<KeyCode, (bool PressedBeforeFrame, bool PressedThisFrame)> x) => x.Value.PressedThisFrame).Count();
	}

	public async UniTask KeyHaiSelect()
	{
		int pushingCount = GetPushingCount();
		if (pushingCount > 0)
		{
			MahjongUtility.ResetNoInputCount();
		}
		if (!m_isHaiSelectEnable || m_isRuleDisplaying || naki.gameObject.activeSelf)
		{
			return;
		}
		Player player = Players[m_commandClient.PlayerIndex];
		foreach (KeyCode tileKeyCode in MahjongUtility.KeyBoard.TileKeyCodes)
		{
			Tile keyToTile = GetKeyToTile(player, MahjongUtility.KeyBoard.TileKeyCodes.IndexOf(tileKeyCode));
			if (IsCustomKeyDown(tileKeyCode))
			{
				UnityEngine.Debug.Log($"MFG CustomKeyDown {tileKeyCode}\n{m_isHaiSelecting}/{pushingCount}");
				if (keyToTile != null)
				{
					m_isHaiSelecting = true;
					if (player.hand.IsSelected(keyToTile) && pushingCount == 1)
					{
						player.hand.SetDiscardTile(keyToTile);
						break;
					}
					player.hand.Select(keyToTile, m_sdChara).Forget();
					if (player.hand.IsNowExecTsumoChoices() && !player.m_isReach)
					{
						m_haiNaviPanel.ReloadButtonBySelect(player.hand.GetItemPos(keyToTile));
					}
				}
				else if (pushingCount == 1)
				{
					player.hand.SelectReset(m_sdChara).Forget();
					m_haiNaviPanel.ReloadButtonBySelect(-1);
				}
				break;
			}
			if (IsCustomKey(tileKeyCode))
			{
				UnityEngine.Debug.Log($"MFG CustomKey {tileKeyCode}\n{m_isHaiSelecting}/{pushingCount}");
				if (keyToTile != null && player.hand.GetSelectedTile() != keyToTile)
				{
					player.hand.Select(keyToTile, m_sdChara).Forget();
					player.hand.SetDiscardTile(null);
					m_isHaiSelecting = false;
					if (player.hand.IsNowExecTsumoChoices() && !player.m_isReach)
					{
						m_haiNaviPanel.ReloadButtonBySelect(player.hand.GetItemPos(keyToTile));
					}
				}
				break;
			}
			if (IsCustomKeyUp(tileKeyCode))
			{
				UnityEngine.Debug.Log($"MFG CustomKeyUp {tileKeyCode}\n{m_isHaiSelecting}/{pushingCount}");
				if (m_isHaiSelecting && pushingCount == 0 && keyToTile != null && player.hand.GetDiscardTile() == keyToTile)
				{
					m_haiDiscardExecute = true;
				}
				m_isHaiSelecting = false;
				break;
			}
		}
	}

	private async UniTask DebugDivide()
	{
		while (_enableDebugDivide)
		{
			List<UniTask> list = new List<UniTask>();
			for (int i = 0; i < _debugParallelCount; i++)
			{
				list.Add(UniTask.Run(async delegate
				{
					BlockDivider blockDivider = new BlockDivider();
					HaiID[] concealdHais = new HaiID[13]
					{
						HaiID.M1,
						HaiID.M1,
						HaiID.M1,
						HaiID.M2,
						HaiID.M3,
						HaiID.M4,
						HaiID.M5,
						HaiID.M6,
						HaiID.M7,
						HaiID.M8,
						HaiID.M9,
						HaiID.M9,
						HaiID.M9
					};
					BlockSet meldedBlocks = new BlockSet();
					await blockDivider.DivideAsOneByOneDiscarded(concealdHais, meldedBlocks);
				}));
			}
			await list;
		}
	}

	protected override void OnUpdate()
	{
		UpdateCamera();
		UpdateMyCamera();
		if (!m_isTaikyokuActive)
		{
			return;
		}
		if (ResidentFlow.CustomInput.GetMouseButtonDown(0))
		{
			MahjongUtility.ResetNoInputCount();
			if (m_controlPanel.agariButton.IsActive())
			{
				CheckRayCastButton();
				UnityEngine.Debug.Log($"GetUIRaycast Touchpos : {ResidentFlow.CustomInput.mousePosition}");
				if (!m_controlPanel.agariButton.interactable)
				{
					GameDataManager.Log.AddOverlapsAgariData(new List<string> { "interactableFalse" });
					UnityEngine.Debug.Log("アガリボタン表示されてるのになぜか押せない");
				}
			}
		}
		if (MahjongUtility.IsNoInputAutoTsumogiri())
		{
			if (!CoercionTumoText.activeSelf)
			{
				CoercionTumoText.SetActive(value: true);
			}
		}
		else if (CoercionTumoText.activeSelf)
		{
			CoercionTumoText.SetActive(value: false);
		}
		if (GameDataManager.PlayerConfig.IsKeyboardPlay())
		{
			KeyCheck();
			KeyHaiSelect().Forget();
		}
		HaiSelect().Forget();
		if (GameUtility.IsCloudStreaming)
		{
			m_isDaiuchi = Cloudstrap.IsDisconnect;
		}
		if (!(MahjongUtility.GetTouchedTile() != m_touchedTile))
		{
			return;
		}
		m_touchedTile = MahjongUtility.GetTouchedTile();
		wanpai.dora.DouhaiSearchReflesh();
		foreach (Player player in Players)
		{
			player.meld.DouhaiSearchReflesh();
			player.dump.DouhaiSearchReflesh();
		}
	}

	private async UniTask ViewType_Init(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		foreach (Transform item in playerName.transform)
		{
			item.gameObject.SetActive(value: false);
		}
		foreach (Player player in Players)
		{
			Transform obj = playerNames[(int)player.m_cha].transform;
			obj.gameObject.SetActive(value: true);
			await obj.GetComponent<PlayerProfilePanel>().Reflesh(player, danBanner, cancellationToken);
		}
		ViewType viewType = GameDataManager.PlayerConfig.ViewType;
		ViewType_Change(viewType);
		SetStickerOption(GameDataManager.PlayerConfig.IsStickerAllDisp);
	}

	public async UniTask SetPlayerNameAllUIDisp(CancellationToken cancellationToken = default(CancellationToken))
	{
	}

	public void ViewType_Change(ViewType viewType)
	{
		MahjongUtility.SetViewType(viewType);
		for (Cha cha = Cha.Simocha; cha <= Cha.Kamicha; cha++)
		{
			m_3dSimpleViewCameras[(int)cha].enabled = viewType == ViewType.Type3;
		}
		m_viewManager.ChangeViewType(viewType);
		MyCameraReset();
	}

	public void MyCameraReset()
	{
		m_3dCameraMyChara.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
	}

	private async UniTask ExecTaikyoku()
	{
		_ = 16;
		try
		{
			while (m_isTaikyokuActive)
			{
				m_cancellationToken.ThrowIfCancellationRequested();
				await UniTask.NextFrame();
				if (!m_isTaikyokuActive)
				{
					continue;
				}
				ProcessTyoukouActionPlay();
				switch (m_step)
				{
				case 0:
					if (m_commandClient.ExecutableCount() <= 0)
					{
						m_commandClient.Fetch().Forget();
					}
					m_step++;
					break;
				case 1:
					if (!m_commandClient.IsBusy)
					{
						m_step++;
					}
					break;
				case 2:
				{
					if (m_commandClient.ExecutableCount() <= 0)
					{
						m_step = 0;
						break;
					}
					ReceiveCommand cmd = m_commandClient.Current();
					if (m_isNextCmd)
					{
						m_isNextCmd = false;
						m_commandClient.MoveNext();
						break;
					}
					m_cmdKind = cmd.Kind;
					Stopwatch sw = null;
					if (DebugFlag.IsActive(DebugFlag.Flag.EnableCommandClientLog))
					{
						sw = new Stopwatch();
						UnityEngine.Debug.Log($"<color=teal>.{DateTime.Now.Millisecond} Start seqNo:{cmd.SeqNo}, cmd:{cmd.Kind}, index:{cmd.PlayerIndexFlag}</color>");
						sw.Start();
					}
					m_isNextCmd = cmd.Kind switch
					{
						RECEIVE_COMMAND_TYPE.TSUMO => await execTsumo.ProcessCommand((Tsumo)cmd, GetComponent<MahjongPlayFlow>()), 
						RECEIVE_COMMAND_TYPE.SUTEHAI => await execSutehai.ProcessCommand((MFG.Taikyoku.Command.Receive.Sutehai)cmd, GetComponent<MahjongPlayFlow>()), 
						RECEIVE_COMMAND_TYPE.TSUMOAGARI => await execAgari.ProcessCommandTsumoAgari((MFG.Taikyoku.Command.Receive.TsumoAgari)cmd, GetComponent<MahjongPlayFlow>()), 
						RECEIVE_COMMAND_TYPE.RON => await execAgari.ProcessCommandRon((MFG.Taikyoku.Command.Receive.Ron)cmd, GetComponent<MahjongPlayFlow>()), 
						RECEIVE_COMMAND_TYPE.RYUKYOKU => await execAgari.ProcessCommandRyuKyoku((RyuKyoku)cmd, GetComponent<MahjongPlayFlow>()), 
						RECEIVE_COMMAND_TYPE.PON => await execNaki.ProcessCommandPon((MFG.Taikyoku.Command.Receive.Pon)cmd, GetComponent<MahjongPlayFlow>()), 
						RECEIVE_COMMAND_TYPE.CHI => await execNaki.ProcessCommandChi((MFG.Taikyoku.Command.Receive.Chi)cmd, GetComponent<MahjongPlayFlow>()), 
						RECEIVE_COMMAND_TYPE.ANKAN => await execNaki.ProcessCommandAnkan((MFG.Taikyoku.Command.Receive.Ankan)cmd, GetComponent<MahjongPlayFlow>()), 
						RECEIVE_COMMAND_TYPE.MINKAN => await execNaki.ProcessCommandMinkan((MFG.Taikyoku.Command.Receive.Minkan)cmd, GetComponent<MahjongPlayFlow>()), 
						RECEIVE_COMMAND_TYPE.KAKAN => await execNaki.ProcessCommandKakan((MFG.Taikyoku.Command.Receive.Kakan)cmd, GetComponent<MahjongPlayFlow>()), 
						RECEIVE_COMMAND_TYPE.TYOKO => await execKyoku.ProcessCommandTyoko((TyoKo)cmd, GetComponent<MahjongPlayFlow>()), 
						RECEIVE_COMMAND_TYPE.TSUMOCHOICES => await execTsumoChoices.ProcessCommand((TsumoChoices)cmd, GetComponent<MahjongPlayFlow>()), 
						RECEIVE_COMMAND_TYPE.SUTECHOICES => await execSuteChoices.ProcessCommand((SuteChoices)cmd, GetComponent<MahjongPlayFlow>()), 
						RECEIVE_COMMAND_TYPE.KYOKUSTART => await execKyoku.ProcessCommandKyokuStart((KyokuStart)cmd, GetComponent<MahjongPlayFlow>()), 
						RECEIVE_COMMAND_TYPE.KYOKUEND => await execKyoku.ProcessCommandKyokuEnd((KyokuEnd)cmd, GetComponent<MahjongPlayFlow>()), 
						RECEIVE_COMMAND_TYPE.SCORERANK => await execKyoku.ProcessCommandScoreRank((ScoreRank)cmd, GetComponent<MahjongPlayFlow>()), 
						_ => true, 
					};
					if (DebugFlag.IsActive(DebugFlag.Flag.EnableCommandClientLog) && sw != null)
					{
						sw.Stop();
						UnityEngine.Debug.Log($"<color=teal>.{DateTime.Now.Millisecond} End. seqNo:{cmd.SeqNo}, cmd:{cmd.Kind}, index:{cmd.PlayerIndexFlag}, {sw.ElapsedMilliseconds} ms</color>");
					}
					break;
				}
				}
			}
		}
		catch (OperationCanceledException)
		{
			m_isTaikyokuActive = false;
			UnityEngine.Debug.Log(GetType().Name + MethodBase.GetCurrentMethod().Name + ": OperationCanceledException Catched.");
		}
	}

	public void ProcessTyoukouActionPlay()
	{
		if (!thinkCount.IsTyoukou() || thinkCount.m_isThinkingTyoukouActionPlayed)
		{
			return;
		}
		thinkCount.m_isThinkingTyoukouActionPlayed = true;
		foreach (Player player in Players)
		{
			if (thinkCount.GetThinkingPlayer().m_cha == Cha.Jicha && player.m_cha != 0 && !player.m_isReach && UnityEngine.Random.Range(0, 100) < 30)
			{
				AnimatorStateInfo currentAnimatorState = player.character.m_characterModelController.GetCurrentAnimatorState();
				if (player.m_charaType != CharaType.Chara19 || (!currentAnimatorState.IsName("Wait2_Kami") && !currentAnimatorState.IsName("Wait2_Toi") && !currentAnimatorState.IsName("Wait2_Shimo")))
				{
					player.character.Action(Character.EventID.EyeContact).Forget();
				}
			}
		}
	}

	public void StartDoraEffect()
	{
		foreach (Player player in Players)
		{
			player.StartDoraEffect();
		}
	}

	public void ResetTimingFailSafeCouner()
	{
		m_timingCounter = 0f;
	}

	public bool IsTsumoTimingFailSafe(Cha cha)
	{
		if (MahjongUtility.IsHiSpeedMode())
		{
			return true;
		}
		m_timingCounter += Time.deltaTime;
		return true;
	}

	public bool IsSuteTimingFailSafe(Cha cha)
	{
		if (MahjongUtility.IsHiSpeedMode())
		{
			return true;
		}
		m_timingCounter += Time.deltaTime;
		if (m_timingCounter > 3f)
		{
			UnityEngine.Debug.LogError($"{cha}/Sutehai Timing Failed");
			return true;
		}
		return false;
	}

	private async UniTask CharaModel_Init(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		Kaze jikaze = GameDataManager.Taikyoku.PlayerInfos[m_commandClient.PlayerIndex].Zaseki;
		for (int i = 0; i < MahjongUtility.GetPlayerNum(GameDataManager.Mahjong.TakuType); i++)
		{
			ResidentFlow.GetInstance().SetLoadingProgress((float)(i + 1) / (float)MahjongUtility.GetPlayerNum(GameDataManager.Mahjong.TakuType));
			PlayerInfo playerInfo = GameDataManager.Taikyoku.PlayerInfos[i];
			Cha cha2 = MahjongUtility.GetCha(jikaze, playerInfo.Zaseki);
			Players.Add(players[(int)cha2]);
			Player player = players[(int)cha2];
			player.m_cha = cha2;
			player.IsValid = true;
			player.m_isCPU = playerInfo.IsCpu;
			player.m_isPro = playerInfo.PlayerGameData.IsPro();
			player.m_isOdekake = playerInfo.PlayerGameData.IsOdekake;
			player.m_yellRank = playerInfo.PlayerGameData.YellRank;
			CharaType charaType = (playerInfo.PlayerGameData.IsOdekake ? playerInfo.PlayerGameData.OdekakeChara : playerInfo.PlayerGameData.SelectChara);
			int charaVariantID = playerInfo.PlayerGameData.GetCurrentChara().CharaVariantID;
			player.SetCharaType(charaType, charaVariantID);
			player.SetName(playerInfo.GetPlayerName());
			player.m_danLevel = playerInfo.DanLevel;
			if (player.m_isCPU)
			{
				player.m_level = 68;
			}
			else
			{
				player.m_level = playerInfo.PlayerGameData.GetCurrentChara().Level;
			}
			player.m_playerIndexAtCommand = i;
			player.m_index = (int)cha2;
			player.m_machihaiPanel = machihaiPanels[(int)cha2];
			player.m_proStand = m_proStands[(int)cha2];
			player.m_compeID = playerInfo.PlayerGameData.RecentFinishedCompetitionID;
			player.m_trophyType = playerInfo.PlayerGameData.RecentFinishedCompetitionTrophyType;
			if (cha2 == Cha.Jicha)
			{
				GameDataManager.Player.SelectChara = player.m_charaType;
				if (GameDataManager.Mahjong.IsHaifuPlay)
				{
					player.StickerEditInfos = playerInfo.PlayerGameData.GetCurrentChara().StickerEditInfos;
					player.ApplyCustomizeItemEquip(playerInfo.PlayerGameData.GetCurrentChara().CustomizeItemEquips, playerInfo.Customize.CustomizeItems, isJicha: true);
					player.CustomizeItemEquips = playerInfo.PlayerGameData.GetCurrentChara().CustomizeItemEquips;
					player.CutinItems = playerInfo.Customize.CutinItems;
					player.CostumeItemEquip = playerInfo.PlayerGameData.GetCurrentChara().CostumeItemEquip;
					player.CatchphraseItemEquip = playerInfo.PlayerGameData.GetCurrentChara().CatchphraseItemEquip;
					player.GachaVoiceItemEquip = playerInfo.PlayerGameData.GetCurrentChara().GachaVoiceItems;
				}
				else
				{
					CustomizeGameData customizeGameData = (GameDataManager.Player.IsOdekake ? GameDataManager.Player.OdekakeCustomizeData : GameDataManager.Customize);
					player.StickerEditInfos = GameDataManager.Player.GetCurrentChara().StickerEditInfos;
					player.ApplyCustomizeItemEquip(GameDataManager.Player.GetCurrentChara().CustomizeItemEquips, customizeGameData.CustomizeItems, isJicha: true);
					player.CutinItems = (GameUtility.IsKonastaOrNetcafeBehavior ? customizeGameData.CutinItems.Where((CutinItemInfo x) => !CutinItemMaster.IsPonnomichi(x.ItemID)).ToList() : customizeGameData.CutinItems);
					player.CostumeItemEquip = GameDataManager.Player.GetCurrentChara().CostumeItemEquip;
					player.CatchphraseItemEquip = GameDataManager.Player.GetCurrentChara().CatchphraseItemEquip;
					player.GachaVoiceItemEquip = GameDataManager.Player.GetCurrentChara().GachaVoiceItems;
				}
				IDisposableAsset<GameObject> disposableAsset = await AssetLoader.LoadAssetAsync<GameObject>(AssetPathResolver.Resolve(MFG.Assets.PathType.SdChara, player.m_charaType), null, cancellationToken);
				disposableAsset.AddTo(this);
				m_sdChara = UnityEngine.Object.Instantiate(disposableAsset.Asset, m_SDCharaTop.transform).GetComponent<SdChara>();
				m_sdChara.SetCamera(m_3dCameraTehai);
				m_sdCharaTsumo = UnityEngine.Object.Instantiate(disposableAsset.Asset, m_SDCharaTop.transform).GetComponent<SdChara>();
				m_sdCharaTsumo.SetCamera(m_3dCameraTehai);
				IDisposableAsset<GameObject> disposableAsset2 = await AssetLoader.LoadAssetAsync<GameObject>(AssetPathResolver.Resolve(MFG.Assets.PathType.Awake, player.m_charaType), null, cancellationToken);
				disposableAsset2.AddTo(this);
				agariCharaEffectChara = UnityEngine.Object.Instantiate(disposableAsset2.Asset, agariCharaEffectInstantiateRoot.transform);
				m_viewManager.ChangeProView(player.IsProAnimEnable());
			}
			else
			{
				player.StickerEditInfos = playerInfo.PlayerGameData.GetCurrentChara().StickerEditInfos;
				player.ApplyCustomizeItemEquip(playerInfo.PlayerGameData.GetCurrentChara().CustomizeItemEquips, playerInfo.Customize.CustomizeItems, isJicha: false);
				player.CutinItems = playerInfo.Customize.CutinItems;
				player.CostumeItemEquip = playerInfo.PlayerGameData.GetCurrentChara().CostumeItemEquip;
				player.CatchphraseItemEquip = playerInfo.PlayerGameData.GetCurrentChara().CatchphraseItemEquip;
				player.GachaVoiceItemEquip = playerInfo.PlayerGameData.GetCurrentChara().GachaVoiceItems;
			}
			UnityEngine.Debug.Log($"キャラ: Cha:{player.m_cha} Costume:{player.CostumeItemEquip}, VariantID:{player.m_charaVariantID}");
			UnityEngine.Debug.Log($"装備カットイン: {player.m_cha}");
			player.CutinItem_Log((CutinItemInfo x) => x.Master.CharaType == player.m_charaType && x.IsOn);
			if (player.IsProAnimEnable())
			{
				IDisposableAsset<Sprite> disposableAsset3 = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(MFG.Assets.PathType.Pro, player.m_charaType, ProAssetType.Agari), null, cancellationToken);
				disposableAsset3.AddTo(this);
				player.m_maAgariSprite = disposableAsset3.Asset;
			}
			await player.character.Initialize(m_charaArmParent[(int)player.m_cha], m_chara3DParent[(int)player.m_cha], m_3dTexture.transform.localPosition, m_3dTextureTehai.transform.localPosition, MotionID.Wait0, cancellationToken);
			ModifyCharaOffsetParam(charaType, cha2, player);
			await m_taku.Init(cha2, player.CustomizeItemEquips[Category.TableSticker], player.StickerEditInfos, cancellationToken);
			await CustomizeItem_Instantiate<GameObject>(Category.PlayerAccessory, cha2, m_viewManager.players[(int)cha2].accessory.obj2D.transform, cancellationToken);
			player.character.m_charaReaction.OnBackScreenEffect.Subscribe(async delegate((Player, CharaReactionData.ReactionData) x)
			{
				await OnBackScreenEffectBehavior(x, cancellationToken);
			}).AddTo(this);
		}
		for (Cha cha2 = Cha.Jicha; cha2 <= Cha.Kamicha; cha2++)
		{
			await m_scorePanel.Init((int)cha2, players[(int)cha2]);
		}
		m_cutinManager.OnPrepareLandedReachStick.Subscribe(async delegate((IPlayerInteractor playerInteractor, MFG.Types.Customize.CustomizeItem.ItemID reachStickItemID) param)
		{
			await GetPlayerWork(param.playerInteractor.Cha()).ReachStick_Create(param.reachStickItemID, cancellationToken);
		}).AddTo(this);
	}

	public void SetStickerOption(bool isSetAll)
	{
		for (Cha cha = Cha.Jicha; cha <= Cha.Kamicha; cha++)
		{
			if (cha != 0 && players[(int)cha].IsValid)
			{
				m_taku.StickerOption(cha, isSetAll, players[(int)cha].CustomizeItemEquips[Category.TableSticker], players[(int)cha].StickerEditInfos);
			}
		}
	}

	public async UniTask OnBackScreenEffectBehavior((Player player, CharaReactionData.ReactionData reactionData) x, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (m_isBackScreenPlaying)
		{
			if (x.reactionData.priority <= (m_currentReactionData?.priority ?? 0))
			{
				UnityEngine.Debug.Log($"OnBackScreenEffect: Skipped.(Playing) charaType={x.player.m_charaType}, cha={x.player.m_cha}, type={x.reactionData.ID}");
				return;
			}
			UnityEngine.Debug.Log($"BackScreen_Play override: current:{m_currentReactionData?.ID ?? CharaReactionData.ReactionID.None}, next:{x.reactionData.ID}");
			if (_backScreenCts != null)
			{
				_backScreenCts.Cancel();
				_backScreenCts = null;
			}
			UnityEngine.Debug.Log("BackScreen_Play cancel wait begin");
			await UniTask.WaitWhile(() => m_isBackScreenPlaying, PlayerLoopTiming.Update, cancellationToken);
			UnityEngine.Debug.Log("BackScreen_Play cancel wait end");
		}
		x.player.character.m_charaReaction.PrevReactionID = x.reactionData.ID;
		_backScreenCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		OnBackScreenPlay(x, _backScreenCts.Token);
		if (x.player.m_isReach)
		{
			UnityEngine.Debug.Log($"OnBackScreenEffect: Motion Skipped.(Reached) charaType={x.player.m_charaType}, cha={x.player.m_cha}, motionID={x.reactionData.motionID}");
		}
		else
		{
			OnCharaMotion(x, _backScreenCts.Token).Forget();
		}
	}

	private async UniTask<T> CustomizeItem_Instantiate<T>(Category category, Cha cha, Transform parent, CancellationToken cancellationToken = default(CancellationToken)) where T : UnityEngine.Object
	{
		cancellationToken.ThrowIfCancellationRequested();
		MFG.Types.Customize.CustomizeItem.ItemID customizeItemEquip = players[(int)cha].GetCustomizeItemEquip(category);
		return await CustomizeItem_Instantiate<T>(customizeItemEquip, parent, cancellationToken);
	}

	public async UniTask<T> CustomizeItem_Instantiate<T>(MFG.Types.Customize.CustomizeItem.ItemID itemID, Transform parent, CancellationToken cancellationToken = default(CancellationToken)) where T : UnityEngine.Object
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (itemID == MFG.Types.Customize.CustomizeItem.ItemID.PlayerAccessoryNone)
		{
			return null;
		}
		PlayerAccessoryViewData componentInChildren = parent.GetComponentInChildren<PlayerAccessoryViewData>();
		if (componentInChildren != null)
		{
			componentInChildren.gameObject.SetActive(value: false);
		}
		foreach (Transform item in parent)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		IDisposableAsset<GameObject> obj = await AssetLoader.LoadAssetAsync<GameObject>(CustomizeItemMaster.GetItem(itemID).MahjongPlayDataPath, null, cancellationToken);
		obj.AddTo(this);
		return UnityEngine.Object.Instantiate(obj.Asset, parent).GetComponent<T>();
	}

	public void ModifyCharaOffsetParam(CharaType charaType, Cha cha, Player player)
	{
		CharacterProperties characterProperties = MasterGameDataManager.Character.Properties(charaType);
		if (cha == Cha.Jicha)
		{
			m_3dCameraMyChara.GetComponent<TransformReserver>().RestoreSelf();
			Vector3 localPosition = m_3dCameraMyChara.transform.localPosition;
			localPosition += characterProperties.JichaCameraOffsetParam?.OffsetPosition ?? default(Vector3);
			localPosition.y += characterProperties.MahjongHeightOffsetParam;
			m_3dCameraMyChara.transform.localPosition = localPosition;
			if (SystemConfigGameData.IsSXGAMonitor)
			{
				SubMonitorCharaFlow instance = SubMonitorCharaFlow.GetInstance();
				if (instance != null)
				{
					instance.FloorAdjuster(player.CostumeItemEquip);
				}
			}
		}
		else
		{
			GameObject obj = m_chara3DParent[(int)cha];
			obj.GetComponent<TransformReserver>().RestoreSelf();
			Vector3 localPosition2 = obj.transform.localPosition;
			localPosition2 += characterProperties.CharaOffsetParam[(int)cha]?.OffsetPosition ?? default(Vector3);
			localPosition2.y -= characterProperties.MahjongHeightOffsetParam * 0.2f;
			obj.transform.localPosition = localPosition2;
			Vector3 localEulerAngles = obj.transform.localEulerAngles;
			localEulerAngles += characterProperties.CharaOffsetParam[(int)cha]?.OffsetRotation ?? default(Vector3);
			obj.transform.localEulerAngles = localEulerAngles;
			Camera camera = m_3dSimpleViewCameras[(int)cha];
			camera.GetComponent<TransformReserver>().RestoreSelf();
			Vector3 localPosition3 = camera.transform.localPosition;
			localPosition3.y += characterProperties.MahjongHeightOffsetParam;
			camera.transform.localPosition = localPosition3;
		}
		player.character.m_scorePanelCamera.transform.GetComponent<MahjongPlayCameraManager>().Init(player);
		if (SystemConfigGameData.IsDualMonitor)
		{
			player.character.m_subMonitorScoreCamera.transform.GetComponent<MahjongPlayCameraManager>().Init(player);
		}
	}

	public void SetTaikyokuSupport(bool isActive)
	{
		m_mentsuCompleteEffect.SetDisp(isActive);
		m_haiShantenCounter.SetDisp(isActive);
		m_haiShantenIcon.SetDisp(isActive);
		m_haiConjectureIcon.SetDisp(isActive);
		Players[m_commandClient.PlayerIndex].hand.TenpaiTargetIconSetDisp(isActive);
		Players[m_commandClient.PlayerIndex].hand.OpenHoujuTargetIconSetDisp(isActive);
	}

	public void SetConjectureSupport(bool isActive)
	{
		m_haiConjecture.conjectSafeAtOption();
		m_haiConjectureIcon.SetDisp(isActive);
	}

	public void KikenRequenst()
	{
		m_cancellationTokenSource.Cancel();
		thinkCount.ThinkEnd();
		m_isKikenRequest = true;
		m_isTaikyokuActive = false;
	}

	public void DestructAll()
	{
		foreach (Player player in Players)
		{
			player.Destruct();
		}
		wanpai.Destruct();
		m_controlPanel.Destruct();
		m_takuInfo.Destruct();
		DispoffAllTurnLine();
		if (m_sdChara != null)
		{
			m_sdChara.DispOffAnime();
		}
		if ((bool)m_sdCharaTsumo)
		{
			m_sdCharaTsumo.DispOffAnime();
		}
		m_haiConjectureIcon.SetIconsDispOff();
		m_haiShantenCounter.SetShantenCountDispOff();
		m_haiShantenIcon.SetHaiShantenIconDispOff();
		m_haiNaviPanel.Reset();
		_accelDoraRemainCounter.Reflesh(0, isDisp: false).Forget();
		_meldBonusCounter.Reflesh(Players, isDisp: false);
		m_bombDisplay.SetActive(isActive: false);
		thinkCount.ThinkEnd();
		thinkCount.ThinkingTyoukouEnd();
		foreach (HaiNavi item in m_haiNavi)
		{
			item.Reset();
		}
		m_stampButton.gameObject.SetActive(value: false);
	}

	public void DispoffAllTurnLine(bool takuWait = true)
	{
		foreach (GameObject turnLine in turnLines)
		{
			turnLine.SetActive(value: false);
		}
		if (takuWait)
		{
			foreach (Transform item in m_taku.transform)
			{
				if (item.gameObject.activeSelf)
				{
					item.GetComponent<Animator>().SetTrigger("Wait");
				}
			}
		}
		mySpotLight.GetComponent<Animator>().SetTrigger("Wait");
		foreach (GameObject spotLight in spotLights)
		{
			spotLight.SetActive(value: false);
		}
		foreach (GameObject spotLightBehind in spotLightBehinds)
		{
			spotLightBehind.SetActive(value: false);
		}
		Tile lastTargetTile = MahjongUtility.GetLastTargetTile();
		if (lastTargetTile != null)
		{
			lastTargetTile.EndBlink();
		}
	}

	public Tile GetMouseOnTile()
	{
		return GetMouseOnTile(m_3dCameraTehai, m_3dTextureTehai);
	}

	private Tile GetMouseOnTile(Camera camera, GameObject renderTexture)
	{
		float num = 0f;
		float num2 = 0f;
		if (GameUtility.IsCloudStreaming)
		{
			Vector3[] array = new Vector3[4];
			(renderTexture.transform as RectTransform).GetWorldCorners(array);
			Camera camera2 = ResidentFlow.GetInstance().MainCamera();
			Vector3 vector = camera2.WorldToScreenPoint(array[0]);
			Vector3 vector2 = camera2.WorldToScreenPoint(array[2]) - vector;
			num = (ResidentFlow.CustomInput.mousePosition.x - vector.x) / vector2.x * (float)camera.pixelWidth;
			num2 = (ResidentFlow.CustomInput.mousePosition.y - vector.y) / vector2.y * (float)camera.pixelHeight;
		}
		else
		{
			num = ResidentFlow.CustomInput.mousePosition.x / (float)m_width * 1920f - renderTexture.transform.localPosition.x;
			num2 = ResidentFlow.CustomInput.mousePosition.y / (float)m_height * 1080f - renderTexture.transform.localPosition.y;
		}
		Vector3 pos = new Vector3(num, num2, 0f);
		if (Physics.Raycast(camera.ScreenPointToRay(pos), out var hitInfo))
		{
			return hitInfo.collider.gameObject.GetComponent<Tile>();
		}
		return null;
	}

	public async void OnClick_Stamp()
	{
	}

	public async UniTask DiscardProcess(int playerIndex, bool isReach, bool isTsumogiri, Tile discardTile, Pai discardPai, bool isFireReach, bool isOpenReach, bool isAtomicReach)
	{
		bool num = discardTile == null;
		Player player = Players[playerIndex];
		List<Cha> safes = m_haiConjecture.IsSafeOnlyReachChaList(discardPai, player.m_cha);
		List<Cha> dangers = m_haiConjecture.IsDangerChaList(discardPai, player.m_cha);
		List<Cha> source = (from cha in Enum.GetValues(typeof(Cha)).OfType<Cha>()
			where m_haiConjecture.IsTargetForConject(cha, player.m_cha)
			select cha).ToList();
		bool isAnpai = safes.Count > 0 && source.All((Cha cha) => safes.Contains(cha));
		bool isKikenhai = dangers.Count > 0 && source.Any((Cha cha) => dangers.Contains(cha));
		player.character.ResetSutehaiTiming();
		ResetTimingFailSafeCouner();
		if (num)
		{
			player.hand.DiscardPreAction(discardPai, isReach, isOpenReach, isTsumogiri, isAnpai, isKikenhai, thinkCount.IsFast());
		}
		else
		{
			player.hand.DiscardPreAction(discardTile, isReach, isOpenReach, isTsumogiri, isAnpai, isKikenhai, thinkCount.IsFast());
			if (m_sdChara != null)
			{
				m_sdChara.DahaiAnime(discardTile);
			}
		}
		if (isReach)
		{
			bool isDoubleReach = player.dump.TileList.Count == 0 && !Players.Exists((Player x) => x.meld.GetBlockSet().Count() > 0);
			m_lastReachedCha = player.m_cha;
			await player.Reach(isDoubleReach, isFireReach, isOpenReach, isAtomicReach, MusicText, discardPai);
			foreach (Player player2 in Players)
			{
				if (player2 != player)
				{
					player2.character.Action(Character.EventID.ReceiveReach).Forget();
				}
			}
			m_StageBG.StageState = StageState.Reach;
		}
		LEDManager.Instance.AnimColor(new Color[1] { Color.white }, isSpread: false);
		await player.character.Action(Character.EventID.Discard);
	}

	public async UniTask InitVariables(KyokuStart cmd, CancellationToken cancellationToken = default(CancellationToken))
	{
		thinkCount.InitKyoku();
		m_isKanExecuted = false;
		m_isKanExecutedType = (NakiType)(-1);
		for (int i = 0; i < MahjongUtility.GetPlayerNum(GameDataManager.Mahjong.TakuType); i++)
		{
			m_haiNavi[i].SetAnalyzeData(i, i == m_entryTakuInfo.PlayerIndex, Players, wanpai, cmd.Ba, GameDataManager.Mahjong.TakuType);
			await m_haiNavi[i].Analyze();
		}
		m_haiShantenIcon.KyokuInit();
		m_haiShantenCounter.KyokuInit();
		m_haiNaviPanel.ReloadButtonBySelect(-1);
		_accelDoraRemainCounter.KyokuInit();
		_meldBonusCounter.Reflesh(Players);
		m_bombDisplay.SetActive(isActive: true);
		foreach (Player player in Players)
		{
			player.m_isCatharsisKing = MahjongUtility.ComebackTaku.IsEffectiveTaiyoku() && MahjongUtility.ComebackTaku.IsKing(Players, player.m_cha);
			player.m_comebackTakuBonusDisp.Play(Players, player.m_cha);
		}
		GameDataManager.EventTaku.OnKyokuStart(cmd, Players);
		VoiceManager.PlayVoiceStart();
		m_takuInfo.KyokuInitAfterHaipai();
		CharaReactionManager.GetInstance().TurnInit();
		foreach (Player player2 in Players)
		{
			int index = Players.IndexOf(player2);
			player2.character.m_charaReaction.ReactionKyokuStart(cmd, m_haiNavi[index], m_cancellationTokenSource.Token);
		}
		CharaReactionManager.GetInstance().TurnExec();
	}

	public async UniTask HaipaiProcess()
	{
		foreach (Player player in Players)
		{
			player.hand.StartHaipaiAnime().Forget();
		}
		await UniTask.Delay(TimeSpan.FromSeconds(2.0));
		StartDoraEffect();
		if (Players[m_commandClient.PlayerIndex].m_jikaze != 0)
		{
			m_haiShantenCounter.SetShantenCount(m_haiNavi[m_entryTakuInfo.PlayerIndex].CurShanten).Forget();
			await m_haiNaviPanel.Reload(m_haiNavi[m_entryTakuInfo.PlayerIndex].m_yakuInfo, m_haiNavi[m_entryTakuInfo.PlayerIndex].m_oneByOneDiscardedYakuPossibilityInfos, Players[m_commandClient.PlayerIndex].m_isFireReach, Players[m_commandClient.PlayerIndex].m_isOpenReach, Players[m_commandClient.PlayerIndex].m_isAtomicReach, Players[m_commandClient.PlayerIndex].m_isNaki);
		}
	}

	public void AgariHaiEffect(Tile item, bool isJicha)
	{
		agariHaiEffect.layer = (isJicha ? LayerMask.NameToLayer("3DTehai") : 0);
		foreach (Transform item2 in agariHaiEffect.transform)
		{
			item2.gameObject.layer = (isJicha ? LayerMask.NameToLayer("3DTehai") : 0);
		}
		agariHaiEffect.transform.position = item.transform.position;
		agariHaiEffect.transform.Find("ConcentratedLine").GetComponent<ParticleSystem>().Play();
		agariHaiEffect.transform.Find("Light").GetComponent<ParticleSystem>().Play();
		agariHaiEffect.transform.Find("Circle").GetComponent<ParticleSystem>().Play();
	}

	private void OnBackScreenPlay((Player player, CharaReactionData.ReactionData reactionData) data, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (data.reactionData.ID != CharaReactionData.ReactionID.None)
		{
			BackScreen_Play(data.player.m_charaType, data.player.m_cha, data.reactionData, cancellationToken).Forget();
		}
	}

	private async UniTask OnCharaMotion((Player player, CharaReactionData.ReactionData reactionData) data, CancellationToken cancellationToken)
	{
		if (data.reactionData.motionID == MotionID.None)
		{
			return;
		}
		try
		{
			await data.reactionData.ExecDelay(cancellationToken);
			data.player.character.m_characterModelController.PlayMotion(data.reactionData.motionID).Forget();
		}
		catch (OperationCanceledException)
		{
		}
	}

	public async UniTask BackScreen_Play(CharaType charaType, Cha cha, CharaReactionData.ReactionData reactionData, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (string.IsNullOrEmpty(reactionData.backScreenCharaTextureName) || MahjongUtility.GetViewType() == ViewType.Type3)
		{
			return;
		}
		string backScreenTypeName = reactionData.ID.ToString();
		string backScreenCharaTextureName = reactionData.backScreenCharaTextureName;
		if (m_isBackScreenPlaying)
		{
			UnityEngine.Debug.Log($"BackScreen_Play skip: charaType={charaType}, cha={cha}, type={backScreenTypeName}");
			return;
		}
		UnityEngine.Debug.Log($"BackScreen_Play begin: charaType={charaType}, cha={cha}, type={backScreenTypeName}");
		m_isBackScreenPlaying = true;
		m_currentReactionData = reactionData;
		using CompositeDisposable disposables = new CompositeDisposable();
		_ = 3;
		try
		{
			if (reactionData.delayMilliSec > 0)
			{
				UnityEngine.Debug.Log($"BackScreen_Delay begin: millisec{reactionData.delayMilliSec}");
				await reactionData.ExecDelay(cancellationToken);
				UnityEngine.Debug.Log($"BackScreen_Delay end: millisec{reactionData.delayMilliSec}");
			}
			string chaName = AssetPathResolver.getChaName(cha);
			GameObject backScreenBase = m_backScreen.transform.Find("Screen").Find(chaName).gameObject;
			GameObject content = backScreenBase.transform.Find("Contents").gameObject;
			if (m_prevBackScreenBase != null && m_prevBackScreenBase != backScreenBase)
			{
				m_prevBackScreenBase.SetActive(value: false);
			}
			m_prevBackScreenBase = backScreenBase;
			foreach (GameObject item in m_prevBackScreenTriggerBase)
			{
				if (item != null)
				{
					item.SetActive(value: false);
				}
			}
			m_prevBackScreenTriggerBase.Clear();
			m_prevBackScreenTriggerBase.Add(m_backScreenTrigger.transform.Find(chaName).gameObject);
			IDisposableAsset<Sprite> textTex = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(MFG.Assets.PathType.BgScreen, charaType, "Text/" + backScreenTypeName), null, cancellationToken);
			textTex.AddTo(disposables);
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(MFG.Assets.PathType.BgScreen, charaType, "Chara/" + backScreenCharaTextureName), null, cancellationToken);
			disposableAsset.AddTo(disposables);
			content.transform.Find("Chara").GetComponent<Image>().sprite = disposableAsset.Asset;
			content.transform.Find("Text").GetComponent<Image>().sprite = textTex.Asset;
			backScreenBase.SetActive(value: true);
			Animator component = backScreenBase.GetComponent<Animator>();
			component.ResetTrigger("In");
			component.SetTrigger("In");
			foreach (GameObject item2 in m_prevBackScreenTriggerBase)
			{
				item2.SetActive(value: true);
				item2.GetComponentInChildren<Animator>().ResetTrigger("In");
				item2.GetComponentInChildren<Animator>().SetTrigger("In");
			}
			component.Update(0f);
			await UniTask.Delay(2000, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		}
		catch (OperationCanceledException)
		{
			UnityEngine.Debug.Log($"BackScreen_Cancel end: charaType={charaType}, cha={cha}, type={backScreenTypeName}");
		}
		finally
		{
			disposables.Clear();
			m_isBackScreenPlaying = false;
			UnityEngine.Debug.Log($"BackScreen_Play end: charaType={charaType}, cha={cha}, type={backScreenTypeName}");
		}
	}

	public async UniTask BackScreen_Play_Ron(CharaType charaType, Cha cha, CharaType furikomiPlayerCharaType, Cha furikomiPlayerCha)
	{
		if (MahjongUtility.GetViewType() == ViewType.Type3)
		{
			return;
		}
		string[] chaColorString = new string[4] { "#20CA00", "#CB0064", "#C8CA00", "#009CCA" };
		UnityEngine.Debug.Log($"BackScreen_Play_Ron begin: charaType={charaType}, cha={cha}, furikomiCharaType={furikomiPlayerCharaType}, furikomiCha={furikomiPlayerCha}");
		GameObject backScreenBase = m_backScreen.transform.Find("Screen").Find("Agari").gameObject;
		GameObject content = backScreenBase.transform.Find("Contents").Find("Ron").gameObject;
		GameObject item = m_backScreenTrigger.transform.Find(AssetPathResolver.getChaName(cha)).gameObject;
		GameObject item2 = m_backScreenTrigger.transform.Find(AssetPathResolver.getChaName(furikomiPlayerCha)).gameObject;
		if (m_prevBackScreenBase != null && m_prevBackScreenBase != backScreenBase)
		{
			m_prevBackScreenBase.SetActive(value: false);
		}
		m_prevBackScreenBase = backScreenBase;
		foreach (GameObject item3 in m_prevBackScreenTriggerBase)
		{
			if (item3 != null)
			{
				item3.SetActive(value: false);
			}
		}
		m_prevBackScreenTriggerBase.Clear();
		m_prevBackScreenTriggerBase.Add(item);
		m_prevBackScreenTriggerBase.Add(item2);
		IDisposableAsset<Sprite> charaTex = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(MFG.Assets.PathType.BgScreen, charaType, "Chara/Ron"));
		IDisposableAsset<Sprite> furikomiCharaTex = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(MFG.Assets.PathType.BgScreen, furikomiPlayerCharaType, "Chara/Yarare"));
		if (ColorUtility.TryParseHtmlString(chaColorString[(int)furikomiPlayerCha], out var color))
		{
			color.a = 0.5f;
			content.transform.Find("BgColorRival").GetComponent<Image>().color = color;
		}
		content.transform.Find("CharaRival").GetComponent<Image>().sprite = furikomiCharaTex.Asset;
		if (ColorUtility.TryParseHtmlString(chaColorString[(int)cha], out color))
		{
			color.a = 0.5f;
			content.transform.Find("ArrowMask").Find("BgColor").GetComponent<Image>()
				.color = color;
		}
		content.transform.Find("Chara").GetComponent<Image>().sprite = charaTex.Asset;
		backScreenBase.SetActive(value: true);
		Animator backScreenAnimator = backScreenBase.GetComponent<Animator>();
		backScreenAnimator.SetTrigger("In");
		foreach (GameObject item4 in m_prevBackScreenTriggerBase)
		{
			item4.SetActive(value: true);
			item4.GetComponentInChildren<Animator>().SetTrigger("In");
		}
		backScreenAnimator.Update(0f);
		await UniTask.WaitUntil(() => backScreenAnimator.GetCurrentAnimatorStateInfo(0).IsName("BackMonitorWait"));
		UnityEngine.Debug.Log($"BackScreen_Play_Ron end: charaType={charaType}, cha={cha}, furikomiCharaType={furikomiPlayerCharaType}, furikomiCha={furikomiPlayerCha}");
		charaTex.Dispose();
		furikomiCharaTex.Dispose();
	}

	private bool BackScreen_IsPlaying()
	{
		return m_isBackScreenPlaying;
	}

	public void SetHaiSelectEnable(bool enable)
	{
		m_isHaiSelectEnable = enable;
		m_takuInfo.SetExitButtonInteractive(m_isHaiSelectEnable);
		m_takuInfo.SetGameOptionButtonInteractive(m_isHaiSelectEnable);
		m_takuInfo.SetEventButtonInteractive(m_isHaiSelectEnable);
	}

	public bool IsHaiSelectEnalbe()
	{
		return m_isHaiSelectEnable;
	}

	public void CalculateDiffScore()
	{
		long score = Players[m_commandClient.PlayerIndex].m_score;
		foreach (Player player in Players)
		{
			long diffScore = player.m_score - score;
			player.SetDiffScore(diffScore);
		}
	}

	protected override async UniTask ExecLoadingTipsDisp()
	{
		await FadePanel_Open();
	}

	private async UniTask FadePanel_Open()
	{
		if (!_isFadePanelOpen)
		{
			m_fadePanel.GetComponent<CanvasGroup>().DOFade(1f, 0f).ToUniTask()
				.Forget();
			await ResidentFlow.GetInstance().StartLoading();
			_isFadePanelOpen = true;
		}
	}

	private async UniTask FadePanel_Close()
	{
		if (_isFadePanelOpen)
		{
			await ResidentFlow.GetInstance().SetLoadingProgressComplete();
			ResidentFlow.GetInstance().ExitLoading();
			if (ResidentFlow.GetInstance().m_isTitleDemoMahjongPlayAlive)
			{
				ResidentFlow.GetInstance().GetMahjongActorNoticeManager().SetDisp(isLowerDisp: true, isUpperDisp: true);
			}
			else
			{
				ResidentFlow.GetInstance().GetMahjongActorNoticeManager().SetDisp(isLowerDisp: false, isUpperDisp: true);
			}
			m_fadePanel.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).ToUniTask()
				.Forget();
		}
	}

	private async UniTask StageBG_Create()
	{
		IDisposableAsset<GameObject> obj = await AssetLoader.LoadAssetAsync<GameObject>("MahjongPlayBG/Prefabs/StageBG.prefab");
		obj.AddTo(this);
		GameObject gameObject = UnityEngine.Object.Instantiate(obj.Asset, m_stageBGParent.transform, worldPositionStays: false);
		m_StageBG = gameObject.GetComponent<StageBG>();
		MFG.Types.Customize.CustomizeItem.ItemID itemID = CustomizeItemMaster.GetStageBgEquip();
		await m_StageBG.Init(m_concentratedLlineEffect, itemID);
		m_backScreen = m_StageBG.GetBackScreen();
		IDisposableAsset<GameObject> obj2 = await AssetLoader.LoadAssetAsync<GameObject>($"CustomizeItem/MyCharacter/{itemID}.prefab");
		obj2.AddTo(this);
		gameObject = UnityEngine.Object.Instantiate(obj2.Asset, m_stageBGMyCharacter, worldPositionStays: false);
		players[0].spotLight = gameObject;
		mySpotLight = gameObject;
		foreach (GameObject backScreenGrow in m_backScreenGrows)
		{
			backScreenGrow.SetActive(itemID == MFG.Types.Customize.CustomizeItem.ItemID.StageBg001);
		}
		m_stageBGMyCharacterTesAdd.SetActive(itemID == MFG.Types.Customize.CustomizeItem.ItemID.StageBg001);
		m_cutinManager.OnReachStickLanded.Subscribe(delegate(IPlayerInteractor playerInteractor)
		{
			bool flag = playerInteractor.ReachLevel() >= ReachLevel.FireReach;
			bool isAtomicReach = playerInteractor.ReachLevel() == ReachLevel.AtomicReach;
			bool isOpenReach = playerInteractor.ReachLevel() == ReachLevel.OpenReach;
			bool isChocoReach = playerInteractor.IsChocolateReachEffectActive();
			SoundManager.PlaySe(flag ? "SFX_MJG_REACH2" : "SFX_MJG_REACH1").Forget();
			m_StageBG.ReachStage_SetActive(isActive: true, flag, isOpenReach, isAtomicReach, isChocoReach);
		}).AddTo(this);
	}

	private async UniTask ScorePanel_Create()
	{
		IDisposableAsset<GameObject> disposableAsset = await AssetLoader.LoadAssetAsync<GameObject>("MahjongPlayScorePanel/Prefabs/ScorePanel.prefab");
		disposableAsset.AddTo(this);
		GameObject instance = UnityEngine.Object.Instantiate(disposableAsset.Asset, m_scorePanelRoot.transform, worldPositionStays: false);
		m_scorePanel = instance.GetComponent<ScorePanel>();
		m_scorePanel.m_kyokuTransition = m_kyokuTransition;
		m_scorePanel.m_mahjongPlayFlow = GetComponent<MahjongPlayFlow>();
		List<string> chaStrList = new List<string> { "Ji", "Shimo", "Toi", "Kami" };
		for (int i = 0; i < chaStrList.Count; i++)
		{
			IDisposableAsset<RenderTexture> disposableAsset2 = await AssetLoader.LoadAssetAsync<RenderTexture>("MahjongPlayScorePanel/Textures/3DTextureScorePanel" + chaStrList[i] + ".renderTexture");
			disposableAsset2.AddTo(this);
			string text = ((i == 0) ? "Zi" : chaStrList[i]);
			instance.transform.Find("Panel/ScorePanel" + text + "/3DTextureChara").GetComponent<RawImage>().texture = disposableAsset2.Asset;
			players[i].character.m_scorePanelCamera.targetTexture = disposableAsset2.Asset;
		}
	}

	private async UniTask PlayerConfig_Affect(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		_ = GameDataManager.PlayerConfig;
		await ViewType_Init(cancellationToken);
		if (GameDataManager.Mahjong.IsHaifuPlay)
		{
			MFG.Types.Customize.CustomizeItem.ItemID itemID = ((UnityEngine.Random.Range(0, 2) != 0) ? MFG.Types.Customize.CustomizeItem.ItemID.TileSurface002 : MFG.Types.Customize.CustomizeItem.ItemID.TileSurface001);
			await TileManager.GetInstance().Init(itemID, cancellationToken);
		}
		else
		{
			await TileManager.GetInstance().Init(Players[m_commandClient.PlayerIndex].GetCustomizeItemEquip(Category.TileSurface), cancellationToken);
		}
	}

	public void DaisouFinish()
	{
		if (m_isDaiuchi)
		{
			m_takuInfo.SetDaisouForceEnd();
		}
	}

	public async UniTask DebugMachiDisp(bool isActive)
	{
	}

	public void DebugRecordStart()
	{
	}

	public void DebugRecordEnd()
	{
	}
}
