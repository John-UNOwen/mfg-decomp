using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.GameData.EventTaku;
using MFG.GameRequest;
using MFG.Taikyoku;
using MFG.Taikyoku.Command;
using MFG.Taikyoku.Command.Receive;
using MFG.Taikyoku.Command.Send;
using MFG.Types;
using MFG.Types.Competition;
using MFG.Types.Customize.CutinItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class WebRequestTest : MonoBehaviour
{
	[SerializeField]
	private CommandClient m_commandClient = new CommandClient();

	[SerializeField]
	private List<GetHaifuList.HaifuIndexData> m_haifuIndexes;

	[SerializeField]
	private GetHaifuData.ResultData m_haifuData;

	[SerializeField]
	private TMP_Dropdown m_cutinItemIDDropdown;

	[SerializeField]
	private TMP_Dropdown m_cutinNakiTypeDropdown;

	private GetMenuData.ReconnectData m_reconnectData;

	[SerializeField]
	private Button m_debugExecButton;

	private Button m_loginButton;

	private Button m_taikyokuStartButton;

	private Button m_kikenButton;

	private Button m_haifuListButton;

	private Button m_haifuDataButton;

	private InputField m_takuIDInputField;

	private Text m_taikyokuProgressText;

	private Text m_taikyokuInfoText;

	private Text m_taikyokuNokoriHaiText;

	private Text m_playerInfoText;

	private Text m_mgText;

	private int m_myTsumoHaiID;

	private int m_nokoriHaiNum;

	private bool m_isTaikyokuActive;

	private bool m_isKikenRequest;

	private int m_step;

	private int m_subStep;

	private SerialPortTouchDevice m_touchDevice = new SerialPortTouchDevice();

	[SerializeField]
	private HaiNavi m_haiNavi = new HaiNavi();

	private GameObject m_cutinInstance;

	private CutinAssetCache m_assetCache = new CutinAssetCache();

	[SerializeField]
	private StandingCharaManager _standingCharaManager;

	[SerializeField]
	private GameObject _eventWindowParent;

	[SerializeField]
	private Animator _playerAnimator;

	[SerializeField]
	private int _waitTimeMillisec;

	[SerializeField]
	private Text m_debugText;

	private EntryTakuInfo m_entryTakuInfo => GameDataManager.Taikyoku.TakuInfo;

	[SerializeField]
	private PlayerGameData m_playerGameData => GameDataManager.Player;

	[SerializeField]
	private PlayerItemGameData m_playeritemGameData => GameDataManager.PlayerItem;

	[SerializeField]
	private SystemConfigGameData m_systemConfigGameData => GameDataManager.System;

	[SerializeField]
	private AccountGameData m_accountGameData => GameDataManager.Account;

	public async void DebugParseClicked()
	{
		MotionTest_Away();
		_ = 0 + 1;
	}

	private void PlayerAnimator_Play(string triggerName)
	{
		_playerAnimator.Update(0f);
		_playerAnimator.SetBool("IsReach", value: true);
		_playerAnimator.SetFloat("AwaySpeed", 1f);
		_playerAnimator.SetInteger("LookDirection", 0);
		_playerAnimator.SetTrigger(triggerName);
		_playerAnimator.Update(0f);
	}

	private void MotionTest_Init()
	{
		PlayerAnimator_Play("Reach0");
	}

	private async void MotionTest_Away()
	{
		PlayerAnimator_Play("Away1");
		await UniTask.Delay(_waitTimeMillisec);
		PlayerAnimator_Play("Think");
	}

	private void JsonTest2()
	{
	}

	private void ScrambleMatchTest()
	{
	}

	private async UniTask EventResultWindowTest2()
	{
		IDisposableAsset<GameObject> disposableAsset = await AssetLoader.LoadAssetAsync<GameObject>("Mentanpin/Prefabs/ResultWindow.prefab");
		disposableAsset.AddTo(this);
		GameObject instance = UnityEngine.Object.Instantiate(disposableAsset.Asset, _eventWindowParent.transform, worldPositionStays: false);
		BaseEventTakuResultWindow eventResultWindow = instance.GetComponent<BaseEventTakuResultWindow>();
		ResultInfo resultInfo = new ResultInfo
		{
			IsJandriEffective = true,
			RankBonus = new RankBonusInfo
			{
				PointRatioByRank = new Dictionary<Rank, decimal>
				{
					{
						Rank.Rank1,
						5m
					},
					{
						Rank.Rank2,
						3m
					},
					{
						Rank.Rank3,
						2m
					},
					{
						Rank.Rank4,
						1m
					}
				}
			},
			BeforeTotalAcquiredPoint = 10
		};
		await eventResultWindow.Open(resultInfo, () => ResultFlow.Skip());
		await UniTask.WaitWhile(() => eventResultWindow.IsOpen);
		UnityEngine.Object.Destroy(instance);
	}

	private async UniTask KeepAliveTest()
	{
		await RequestApiBase<KeepAlive, KeepAlive.ResultData>.Send();
	}

	private async UniTask CompetitionEntryTest()
	{
		await RequestApiBase<CompetitionEntry, CompetitionEntry.ResultData, CompetitionEntry.Param>.Send(new CompetitionEntry.Param
		{
			CompetitionID = CompetitionID.Competition001,
			IsEntryRequest = false
		});
	}

	public async UniTask CharaTest()
	{
		_ = 0 + 1;
	}

	private async UniTask ApliInfoTest()
	{
		await RequestApiBase<AppliInfo, AppliInfo.ResultData>.Send();
	}

	private async UniTask LogoutTest()
	{
		await RequestApiBase<Logout, Logout.ResultData>.Send();
	}

	private async UniTask TabooWordTest()
	{
		await RequestApiBase<ChkTabooword, ChkTabooword.ResultData, ChkTabooword.ChkParam>.Send(new ChkTabooword.ChkParam
		{
			CheckString = "うんちく"
		});
		await RequestApiBase<ChkTabooword, ChkTabooword.ResultData, ChkTabooword.ChkParam>.Send(new ChkTabooword.ChkParam
		{
			CheckString = "ばかちょん"
		});
	}

	private async UniTask NoticeApiTest()
	{
		await RequestApiBase<NoticeDone, NoticeDone.ResultData, List<int>>.Send(new List<int> { 1, 2, 3 }, enableErrorScreenTransition: false);
		PresentDone.ResultData resultData = await RequestApiBase<PresentDone, PresentDone.ResultData, List<int>>.Send(new List<int> { 1, 2, 3 }, enableErrorScreenTransition: false);
		GameDataManager.PresentBox.Apply(resultData.PresentInfos);
	}

	private async UniTask DojoApiTest()
	{
		int slotID = 0;
		CharaType item = CharaType.Chara02;
		bool item2 = true;
		List<DojoSetSlot.TimeSavingItem> item3 = new List<DojoSetSlot.TimeSavingItem> { DojoSetSlot.TimeSavingItem.OID_DOJO_DEVELOP_5MINUTES };
		await RequestApiBase<DojoSetSlot, DojoSetSlot.ResultData, (int, CharaType, bool, List<DojoSetSlot.TimeSavingItem>)>.Send((slotID, item, item2, item3), enableErrorScreenTransition: false);
		await RequestApiBase<DojoGetStatus, DojoGetStatus.ResultData>.Send(enableErrorScreenTransition: false);
		await RequestApiBase<DojoGainSoul, DojoGainSoul.ResultData, int>.Send(slotID, enableErrorScreenTransition: false);
	}

	private async UniTask CutinTest()
	{
		this.GetCancellationTokenOnDestroy();
		m_debugExecButton.interactable = false;
		new List<CustomizeItemInfo>();
		m_debugExecButton.interactable = true;
	}

	private async UniTask HainaviTest()
	{
		m_haiNavi.m_taikyokuState.IsMainPlayer = true;
		await m_haiNavi.DebugAnalyze(16);
	}

	private void DanLevelTest()
	{
		DanLevelMaster.GetGetAcquiredFightStar(TakuType.Tonpu, DanLevelType.JANSEI2, GainLossType.Risky, Rank.Rank4);
		DanLevelMaster.GetTotalFightStarRequired(DanLevelType.JANSEI2);
		DanLevelMaster.GetCurrentDanLevelAndFightStarBorder(11800);
		PlayerGameData player = GameDataManager.Player;
		_ = player.DanLevel;
		player.ExpectedDanLevelAndFightStar(100);
		player.FightStar += 100;
		_ = player.DanLevel;
		GameDataManager.Player.ExpectedDanLevelAndFightStar(2900);
		player.FightStar += 2900;
		_ = player.DanLevel;
		DanLevelMaster.GetCurrentDanLevelAndFightStarBorder(20000);
		DanLevelMaster.GetTotalFightStarRequired(DanLevelType.Max);
		for (int i = 1; i <= 1604; i++)
		{
			DanLevelType danLevelType = (DanLevelType)i;
			int totalFightStarRequired = DanLevelMaster.GetTotalFightStarRequired(danLevelType);
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();
			List<int> list3 = new List<int>();
			List<int> list4 = new List<int>();
			for (int j = 0; j < 4; j++)
			{
				list.Add(DanLevelMaster.GetGetAcquiredFightStar(TakuType.Tonpu, danLevelType, GainLossType.Risky, (Rank)j));
			}
			for (int k = 0; k < 3; k++)
			{
				list2.Add(DanLevelMaster.GetGetAcquiredFightStar(TakuType.Sanma, danLevelType, GainLossType.Risky, (Rank)k));
			}
			for (int l = 0; l < 4; l++)
			{
				list3.Add(DanLevelMaster.GetGetAcquiredFightStar(TakuType.Tonpu, danLevelType, GainLossType.Steadily, (Rank)l));
			}
			for (int m = 0; m < 3; m++)
			{
				list4.Add(DanLevelMaster.GetGetAcquiredFightStar(TakuType.Sanma, danLevelType, GainLossType.Steadily, (Rank)m));
			}
			Debug.Log($"{danLevelType},{totalFightStarRequired},{list3[0]},{list3[1]},{list3[2]},{list3[3]},{list4[0]},{list4[1]},{list4[2]},{list[0]},{list[1]},{list[2]},{list[3]},{list2[0]},{list2[1]},{list2[2]}");
		}
		_ = 0 + 1;
	}

	private async UniTask CustomizeItemTest()
	{
		List<CutinItemInfo> cutinItems = GameDataManager.Customize.CutinItems;
		cutinItems.Clear();
		foreach (ItemID value in Enum.GetValues(typeof(ItemID)))
		{
			if (value != 0)
			{
				cutinItems.Add(new CutinItemInfo
				{
					ItemID = value,
					IsOn = false
				});
			}
		}
		foreach (CutinItemInfo item in cutinItems)
		{
			Debug.Log($"{item.ItemID}");
			await AssetLoader.LoadAssetAsync<Sprite>(item.Master.IconGraphicPath);
			await AssetLoader.LoadAssetAsync<GameObject>(item.Master.CutInPrefabPath);
		}
		_ = 0 + 1;
	}

	private async void Start()
	{
		MotionTest_Init();
		List<string> obj = new List<string> { "Pon", "Chi", "Kan" };
		m_cutinNakiTypeDropdown.options.Clear();
		foreach (string item in obj)
		{
			m_cutinNakiTypeDropdown.options.Add(new TMP_Dropdown.OptionData
			{
				text = item
			});
		}
		m_cutinNakiTypeDropdown.RefreshShownValue();
		m_cutinItemIDDropdown.options.Clear();
		foreach (ItemID value in Enum.GetValues(typeof(ItemID)))
		{
			m_cutinItemIDDropdown.options.Add(new TMP_Dropdown.OptionData
			{
				text = value.ToString()
			});
		}
		m_cutinItemIDDropdown.RefreshShownValue();
		m_taikyokuInfoText = GameObject.Find("TaikyokuInfoText").GetComponent<Text>();
		m_taikyokuProgressText = GameObject.Find("TaikyokuProgressText").GetComponent<Text>();
		m_taikyokuNokoriHaiText = GameObject.Find("TaikyokuNokoriHaiText").GetComponent<Text>();
		m_playerInfoText = GameObject.Find("PlayerInfoText").GetComponent<Text>();
		m_mgText = GameObject.Find("MgText").GetComponent<Text>();
		m_loginButton = GameObject.Find("LoginButton").GetComponent<Button>();
		m_taikyokuStartButton = GameObject.Find("TaikyokuStartButton").GetComponent<Button>();
		m_kikenButton = GameObject.Find("KikenButton").GetComponent<Button>();
		m_haifuListButton = GameObject.Find("HaifuListButton").GetComponent<Button>();
		m_haifuDataButton = GameObject.Find("HaifuDataButton").GetComponent<Button>();
		m_takuIDInputField = GameObject.Find("TakuIDInputField").GetComponent<InputField>();
		m_loginButton.interactable = true;
		m_taikyokuStartButton.interactable = false;
		m_loginButton.OnClickAsObservable().Subscribe(async delegate
		{
			m_loginButton.interactable = false;
			m_taikyokuStartButton.interactable = false;
			Login.ResultData resultData2 = await RequestApiBase<Login, Login.ResultData>.Send();
			GameDataManager.Account.AppSessionID = resultData2.SessionID;
			GetMenuData.ResultData resultData3 = await RequestApiBase<GetMenuData, GetMenuData.ResultData>.Send();
			m_accountGameData.PlayerID = resultData3.PlayerData.PlayerID;
			m_playerGameData.Apply(resultData3);
			m_reconnectData = resultData3.Reconnect;
			m_taikyokuStartButton.interactable = true;
			m_haifuListButton.interactable = true;
			m_haifuDataButton.interactable = true;
			m_loginButton.GetComponentInChildren<Text>().text = "ログイン完了";
		}).AddTo(this);
		m_haifuListButton.OnClickAsObservable().Subscribe(async delegate
		{
			m_haifuListButton.interactable = false;
			m_haifuListButton.GetComponentInChildren<Text>().text = "取得中";
			m_haifuIndexes = (await RequestApiBase<GetHaifuList, GetHaifuList.ResultData>.Send()).HaifuIndexDatas;
			m_haifuListButton.GetComponentInChildren<Text>().text = "牌譜リスト取得";
			m_haifuListButton.interactable = true;
		}).AddTo(this);
		m_haifuDataButton.OnClickAsObservable().Subscribe(async delegate
		{
			m_haifuDataButton.interactable = false;
			m_haifuDataButton.GetComponentInChildren<Text>().text = "取得中";
			m_haifuData = await RequestApiBase<GetHaifuData, GetHaifuData.ResultData, int>.Send(int.Parse(m_takuIDInputField.text));
			m_haifuDataButton.GetComponentInChildren<Text>().text = "牌譜データ取得";
			m_haifuDataButton.interactable = true;
		}).AddTo(this);
		m_taikyokuStartButton.OnClickAsObservable().Subscribe(async delegate
		{
			m_taikyokuStartButton.interactable = false;
			m_taikyokuStartButton.GetComponentInChildren<Text>().text = "対局中";
			if (m_reconnectData.IsActive)
			{
				await m_commandClient.Send(new MFG.Taikyoku.Command.Send.Reconnect
				{
					SeqNo = m_entryTakuInfo.NextSeqNo
				});
			}
			else
			{
				EntryGame.ResultData resultData = await RequestApiBase<EntryGame, EntryGame.ResultData>.Send();
				GameDataManager.Taikyoku.Apply(resultData.TakuInfo);
				m_commandClient.Reset(m_entryTakuInfo.NextSeqNo);
				m_commandClient.TakuID = m_entryTakuInfo.TakuID;
				m_commandClient.PlayerIndex = m_entryTakuInfo.PlayerIndex;
				m_commandClient.PlayerID = m_accountGameData.PlayerID;
				await m_commandClient.Send(new EntryOK());
			}
			m_isTaikyokuActive = true;
			m_isKikenRequest = false;
			await UniTask.WaitUntil(() => !m_isTaikyokuActive);
			if (!m_isKikenRequest)
			{
				await RequestApiBase<EndGame, EndGame.ResultData, (int, int, int)>.Send((m_entryTakuInfo.TakuID, m_entryTakuInfo.PlayerIndex, m_accountGameData.PlayerID));
			}
			else
			{
				await UniTask.WaitUntil(() => !m_commandClient.IsBusy);
				await m_commandClient.Send(new Kiken());
				await RequestApiBase<KikenGame, KikenGame.ResultData, (int, int, int)>.Send((m_entryTakuInfo.TakuID, m_entryTakuInfo.PlayerIndex, m_accountGameData.PlayerID));
			}
			m_step = 0;
			m_subStep = 0;
			m_isKikenRequest = false;
			m_taikyokuStartButton.interactable = true;
			m_taikyokuStartButton.GetComponentInChildren<Text>().text = "対局開始";
		}).AddTo(this);
		m_kikenButton.OnClickAsObservable().Subscribe(delegate
		{
			m_isTaikyokuActive = false;
			m_isKikenRequest = true;
			m_kikenButton.interactable = false;
		}).AddTo(this);
		m_commandClient.OnMatchingData.Subscribe(delegate(MatchingData matchingData)
		{
			bool flag = false;
			bool isMatchingEnd = matchingData.IsMatchingEnd;
			foreach (PlayerInfo playerInfo in matchingData.PlayerInfos)
			{
				if (playerInfo.IsActive)
				{
					GameDataManager.Taikyoku.PlayerInfos[playerInfo.PlayerIndex] = playerInfo;
					flag = true;
				}
			}
			if (flag)
			{
				string text = $"isMatchnigEnd:{isMatchingEnd}\r\n";
				foreach (PlayerInfo playerInfo2 in GameDataManager.Taikyoku.PlayerInfos)
				{
					text += $"{playerInfo2.PlayerName} pindex:{playerInfo2.PlayerIndex} Zaseki:{playerInfo2.Zaseki} IsHuman:{playerInfo2.IsHuman} IsCpu:{playerInfo2.IsCpu} \r\n";
				}
				m_playerInfoText.text = text;
			}
		}).AddTo(this);
		m_commandClient.OnReconnectData.Subscribe(delegate(TaikyokuStartInfo reconnectData)
		{
			int myPlayerIndex = m_commandClient.PlayerIndex;
			Predicate<ReceiveCommand> match = (ReceiveCommand cmd) => cmd.Kind == RECEIVE_COMMAND_TYPE.KYOKUEND;
			Predicate<ReceiveCommand> match2 = (ReceiveCommand cmd) => cmd.PlayerIndexFlag.HasFlag((PLAYER_INDEX_FLAG)(1 << myPlayerIndex)) && (cmd.Kind == RECEIVE_COMMAND_TYPE.TSUMOCHOICES || cmd.Kind == RECEIVE_COMMAND_TYPE.SUTECHOICES);
			string arg = ((reconnectData.Ba == 0) ? "東" : "南");
			m_taikyokuInfoText.text = $"{arg}{reconnectData.Kyoku + 1}局{reconnectData.Honba}本場";
			m_nokoriHaiNum = reconnectData.Nokori;
			m_taikyokuNokoriHaiText.text = $"残り {m_nokoriHaiNum}";
			m_myTsumoHaiID = reconnectData.Players[myPlayerIndex].TsumoPai;
			if (m_commandClient.Find(match))
			{
				m_commandClient.Advance(match);
			}
			else
			{
				m_commandClient.Advance(match2);
			}
		}).AddTo(this);
	}

	private void Update()
	{
		if (!m_isTaikyokuActive)
		{
			return;
		}
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
			ReceiveCommand receiveCommand = m_commandClient.Current();
			m_taikyokuProgressText.text = $"{receiveCommand.Kind} {receiveCommand.SeqNo}";
			bool flag = true;
			switch (receiveCommand.Kind)
			{
			case RECEIVE_COMMAND_TYPE.KYOKUSTART:
				ExecKyokuStart((KyokuStart)receiveCommand);
				break;
			case RECEIVE_COMMAND_TYPE.TSUMO:
				ExecTsumo((Tsumo)receiveCommand);
				break;
			case RECEIVE_COMMAND_TYPE.TSUMOCHOICES:
				flag = ExecTsumoChoices((TsumoChoices)receiveCommand);
				break;
			case RECEIVE_COMMAND_TYPE.SUTECHOICES:
				flag = ExecSuteChoices((SuteChoices)receiveCommand);
				break;
			case RECEIVE_COMMAND_TYPE.KYOKUEND:
				flag = ExecKyokuEnd((KyokuEnd)receiveCommand);
				break;
			}
			if (flag)
			{
				m_commandClient.MoveNext();
			}
			break;
		}
		}
	}

	private void ExecKyokuStart(KyokuStart cmd)
	{
		string arg = ((cmd.Ba == 0) ? "東" : "南");
		m_taikyokuInfoText.text = $"{arg}{cmd.Kyoku + 1}局{cmd.Honba}本場";
		m_nokoriHaiNum = cmd.Nokori;
		m_taikyokuNokoriHaiText.text = $"残り {m_nokoriHaiNum}";
		m_kikenButton.interactable = true;
	}

	private void ExecTsumo(Tsumo cmd)
	{
		m_nokoriHaiNum--;
		m_taikyokuNokoriHaiText.text = $"残り {m_nokoriHaiNum}";
		if (cmd.PlayerIndexFlag.HasFlag((PLAYER_INDEX_FLAG)(1 << m_entryTakuInfo.PlayerIndex)))
		{
			m_myTsumoHaiID = cmd.PaiID;
		}
	}

	private bool ExecTsumoChoices(TsumoChoices cmd)
	{
		switch (m_subStep)
		{
		case 0:
			if (!cmd.PlayerIndexFlag.HasFlag((PLAYER_INDEX_FLAG)(1 << m_entryTakuInfo.PlayerIndex)))
			{
				m_subStep = 0;
				return true;
			}
			if (!cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.SUTE))
			{
				m_subStep = 0;
				return true;
			}
			m_subStep++;
			break;
		case 1:
			m_commandClient.Send(new MFG.Taikyoku.Command.Send.Sutehai(13, m_myTsumoHaiID, bRichi: false, bTsumoGiri: true, ReachLevel.Normal)).Forget();
			m_subStep++;
			break;
		case 2:
			if (!m_commandClient.IsBusy)
			{
				m_subStep = 0;
				return true;
			}
			break;
		}
		return false;
	}

	private bool ExecSuteChoices(SuteChoices cmd)
	{
		switch (m_subStep)
		{
		case 0:
			if (!cmd.PlayerIndexFlag.HasFlag((PLAYER_INDEX_FLAG)(1 << m_entryTakuInfo.PlayerIndex)))
			{
				m_subStep = 0;
				return true;
			}
			m_subStep++;
			break;
		case 1:
			m_commandClient.Send(new Tsugi()).Forget();
			m_subStep++;
			break;
		case 2:
			if (!m_commandClient.IsBusy)
			{
				m_subStep = 0;
				return true;
			}
			break;
		}
		return false;
	}

	private bool ExecKyokuEnd(KyokuEnd cmd)
	{
		switch (m_subStep)
		{
		case 0:
		{
			if (cmd.IsEnd)
			{
				m_taikyokuStartButton.interactable = true;
				m_isTaikyokuActive = false;
				m_subStep = 0;
				m_kikenButton.interactable = false;
				return true;
			}
			NextKyokuReady tasCmd = new NextKyokuReady();
			m_commandClient.Send(tasCmd).Forget();
			m_kikenButton.interactable = false;
			m_subStep++;
			break;
		}
		case 1:
			if (!m_commandClient.IsBusy)
			{
				m_subStep = 0;
				return true;
			}
			break;
		}
		return false;
	}

	private void OnDestroy()
	{
		m_haiNavi.Dispose();
	}
}
