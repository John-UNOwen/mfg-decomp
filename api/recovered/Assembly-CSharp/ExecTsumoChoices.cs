using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using IO;
using MFG.Taikyoku;
using MFG.Taikyoku.Command;
using MFG.Taikyoku.Command.Receive;
using MFG.Taikyoku.Command.Send;
using MFG.Types;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ExecTsumoChoices
{
	private ControlPanel m_controlPanel;

	private MahjongPlayFlow m_mahjongPlayFlow;

	private bool m_isReachThisTurn;

	private bool m_isAutoTsumogiri;

	private bool m_isTsumogiri;

	private bool m_isFirstReach;

	private bool m_isFireReachSelectable;

	private bool m_isOpenReachSelectable;

	private bool m_isAtomicReachSelectable;

	private Tile m_discardTile;

	private int m_discardPos;

	private bool m_isKuikae;

	private Tweener m_myTurnTweener;

	private float m_startTime;

	private bool m_fireReachChoosing;

	private bool m_openReachChoosing;

	private bool m_atomicReachChoosing;

	private TouchPanelTelemetry _startTouchPanelTelemetry = new TouchPanelTelemetry();

	private CommandClient m_commandClient => GameDataManager.CommandClient;

	private EntryTakuInfo m_entryTakuInfo => GameDataManager.Taikyoku.TakuInfo;

	private PlayerConfigGameData m_playerConfigGameData => GameDataManager.PlayerConfig;

	private List<Player> Players => m_mahjongPlayFlow.Players;

	private List<HaiNavi> m_haiNavi => m_mahjongPlayFlow.m_haiNavi;

	private GameObject myTurn => m_mahjongPlayFlow.myTurn;

	private StageBG m_StageBG => m_mahjongPlayFlow.m_StageBG;

	private ThinkCount thinkCount => m_mahjongPlayFlow.thinkCount;

	private HaiNaviPanel m_haiNaviPanel => m_mahjongPlayFlow.m_haiNaviPanel;

	private HaiShantenCounter m_haiShantenCounter => m_mahjongPlayFlow.m_haiShantenCounter;

	private HaiShantenIconList m_haiShantenIcon => m_mahjongPlayFlow.m_haiShantenIcon;

	private GameObject agariCharaEffectChara => m_mahjongPlayFlow.agariCharaEffectChara;

	private GameObject agariCharaEffectRoot => m_mahjongPlayFlow.agariCharaEffectRoot;

	private AttentionMessage m_attentionMessage => m_mahjongPlayFlow.m_attentionMessage;

	private MentsuCompleteEffect m_mentsuCompleteEffect => m_mahjongPlayFlow.m_mentsuCompleteEffect;

	private NakiPattern naki => m_mahjongPlayFlow.naki;

	private SdChara m_sdChara => m_mahjongPlayFlow.m_sdChara;

	private RECEIVE_COMMAND_TYPE m_cmdKind => m_mahjongPlayFlow.m_cmdKind;

	private bool m_haiDiscardExecute => m_mahjongPlayFlow.m_haiDiscardExecute;

	public async UniTask<bool> ProcessCommand(TsumoChoices cmd, MahjongPlayFlow mpf)
	{
		m_mahjongPlayFlow = mpf;
		m_controlPanel = m_mahjongPlayFlow.m_controlPanel;
		int playerIndex = cmd.PlayerIndexFlag.ToPlayerIndex();
		VoiceManager.SetCommandRandomSeed(m_entryTakuInfo.TakuID + cmd.SeqNo);
		try
		{
			if (await ChoiceInit(cmd, playerIndex))
			{
				Players[playerIndex].hand.SetNowExecTsumoChoices(flag: false);
				return true;
			}
			if (await Choosing(cmd, playerIndex))
			{
				Players[playerIndex].hand.SetNowExecTsumoChoices(flag: false);
				return true;
			}
			await ChoiceEnd(cmd, playerIndex);
			await End(cmd, playerIndex);
		}
		catch (OperationCanceledException ex)
		{
			if (thinkCount != null)
			{
				thinkCount.ThinkEnd();
			}
			if (m_controlPanel != null)
			{
				m_controlPanel.Destruct();
			}
			Debug.Log("ExecTsumoChocies Exception Catched. " + ex.Message + ex.StackTrace);
		}
		Players[playerIndex].hand.SetNowExecTsumoChoices(flag: false);
		if (m_mahjongPlayFlow.m_isRecReachCutin)
		{
			m_mahjongPlayFlow.DebugRecordEnd();
		}
		return true;
	}

	private async UniTask<bool> ChoiceInit(TsumoChoices cmd, int playerIndex)
	{
		m_mahjongPlayFlow.m_cancellationToken.ThrowIfCancellationRequested();
		m_isReachThisTurn = false;
		Players[m_commandClient.PlayerIndex].hand.SetDiscardTile(null);
		m_mahjongPlayFlow.SetOpenMachiHaiList();
		if (playerIndex == m_entryTakuInfo.PlayerIndex)
		{
			bool flag = Players[m_commandClient.PlayerIndex].hand.EnableDiscardInReachRelease(m_mahjongPlayFlow.m_openMachiList);
			if (cmd.Choices.KuikaePatterns.Count > 0)
			{
				m_isKuikae = true;
				if (flag)
				{
					Players[m_commandClient.PlayerIndex].hand.AllEnableDiscard(enable: true);
				}
				foreach (ChoicesData.KuikaePatternData kuikaePattern in cmd.Choices.KuikaePatterns)
				{
					if (kuikaePattern.PaiID != 0)
					{
						Players[m_commandClient.PlayerIndex].hand.SetEnableDiscard((Pai)kuikaePattern.PaiID, enable: false);
						if (MahjongUtility.IsIncludeAkahai((Pai)kuikaePattern.PaiID))
						{
							Players[m_commandClient.PlayerIndex].hand.SetEnableDiscard(MahjongUtility.NormalToAka((Pai)kuikaePattern.PaiID), enable: false);
						}
					}
				}
			}
			else
			{
				m_isKuikae = false;
			}
		}
		List<Pai> openMachiList = m_mahjongPlayFlow.m_openMachiList;
		if (openMachiList != null && openMachiList.Count > 0)
		{
			Pai selectedPai = Players[m_commandClient.PlayerIndex].hand.GetSelectedPai();
			if (selectedPai != 0 && m_mahjongPlayFlow.m_openMachiList.Contains(MahjongUtility.AkaToNormal(selectedPai)))
			{
				Players[m_commandClient.PlayerIndex].hand.SelectReset(m_sdChara).Forget();
				m_haiNaviPanel.ReloadButtonBySelect(-1);
			}
		}
		await m_haiNavi[playerIndex].Analyze(cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.TSUMOAGARI));
		m_startTime = Time.time;
		_startTouchPanelTelemetry = Bi2xInput.Instance.GetTouchPanelTelemetry().Clone();
		Players[playerIndex].character.m_charaReaction.ReactionTsumoChoice(cmd);
		if (!GameDataManager.Mahjong.IsHaifuPlay)
		{
			thinkCount.ThinkingTyoukouStart(Players[playerIndex]);
		}
		if (Players[playerIndex].IsProAnimEnable())
		{
			Players[playerIndex].m_proStand.GetComponent<Animator>().ResetTrigger("Out");
			Players[playerIndex].m_proStand.GetComponent<Animator>().SetTrigger("In");
			Players[playerIndex].m_proStand.GetComponent<Animator>().SetTrigger("Loop");
		}
		bool isKanExecuted = m_mahjongPlayFlow.m_isKanExecuted;
		m_mahjongPlayFlow.m_isKanExecuted = false;
		m_mahjongPlayFlow.m_isKanExecutedType = (NakiType)(-1);
		if (playerIndex != m_entryTakuInfo.PlayerIndex)
		{
			return true;
		}
		Players[m_commandClient.PlayerIndex].hand.SetNowExecTsumoChoices(flag: true);
		int shanten = m_haiNavi[m_entryTakuInfo.PlayerIndex].CurShanten;
		Han han = m_haiNavi[m_entryTakuInfo.PlayerIndex].GetHan();
		if (Players[playerIndex].hand.IsNowTsumoChoice() && !Players[playerIndex].m_isFirstTsumo && !isKanExecuted)
		{
			if (shanten == 0 || Players[m_commandClient.PlayerIndex].m_isReach)
			{
				VoiceManager.PlayVoiceNokori0();
			}
			else
			{
				Tile lastDumpedTile = Players[m_commandClient.PlayerIndex].dump._lastDumpedTile;
				if (!VoiceManager.PlayVoiceTalk(m_mahjongPlayFlow.m_nokoriHaiNum))
				{
					if (lastDumpedTile != null && lastDumpedTile.pai == Players[m_commandClient.PlayerIndex].hand.GetLastItem().pai)
					{
						VoiceManager.PlayVoicePick2();
					}
					else
					{
						switch (shanten)
						{
						case 1:
							VoiceManager.PlayVoiceNokori1();
							break;
						case 2:
							VoiceManager.PlayVoiceNokori2();
							break;
						default:
							VoiceManager.PlayVoicePick1();
							break;
						}
					}
				}
			}
		}
		if (!Players[m_commandClient.PlayerIndex].m_isReach)
		{
			Players[m_commandClient.PlayerIndex].m_machihaiPanel.gameObject.SetActive(value: false);
		}
		if (Players[m_entryTakuInfo.PlayerIndex].m_cha == Cha.Jicha)
		{
			await m_haiNaviPanel.Reload(m_haiNavi[m_entryTakuInfo.PlayerIndex].m_yakuInfo, m_haiNavi[m_entryTakuInfo.PlayerIndex].m_oneByOneDiscardedYakuPossibilityInfos, Players[m_entryTakuInfo.PlayerIndex].m_isFireReach, Players[m_entryTakuInfo.PlayerIndex].m_isOpenReach, Players[m_entryTakuInfo.PlayerIndex].m_isAtomicReach, Players[m_entryTakuInfo.PlayerIndex].m_isNaki);
			m_haiNaviPanel.ReloadButtonBySelect(Players[m_commandClient.PlayerIndex].hand.GetSelectingItemPos());
			m_haiShantenCounter.SetShantenCount(m_haiNavi[m_entryTakuInfo.PlayerIndex].CurShanten).Forget();
			m_haiShantenIcon.ReloadHaiShantenInfo(m_haiNavi[m_entryTakuInfo.PlayerIndex].ShantenInfos);
			await Players[m_entryTakuInfo.PlayerIndex].hand.TurnComing();
		}
		if (cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.TSUMOAGARI))
		{
			Tile lastItem = Players[m_commandClient.PlayerIndex].hand.GetLastItem();
			agariCharaEffectChara.SetActive(value: true);
			m_mahjongPlayFlow.AgariHaiEffect(lastItem, isJicha: true);
			agariCharaEffectRoot.SetActive(value: true);
			agariCharaEffectRoot.GetComponent<Animator>().SetTrigger("Next");
			SoundManager.PlaySe("SFX_MJG_DISPLAY6").Forget();
		}
		if ((m_playerConfigGameData.IsAutoAgariEnable || m_mahjongPlayFlow.m_isDaiuchi) && cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.TSUMOAGARI) && !MahjongUtility.IsNoInputAutoTsumogiri())
		{
			SoundManager.PlaySe("SFX_MJG_SELECT3").Forget();
			m_commandClient.Send(new MFG.Taikyoku.Command.Send.TsumoAgari()).Forget();
			Players[m_commandClient.PlayerIndex].StartTenpaiEffect(han >= Han.Mangan, isRestart: true);
			return await End(cmd, playerIndex, isAgari: true);
		}
		m_controlPanel.ControlButtonSetActive(m_controlPanel.agariButton, cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.TSUMOAGARI));
		m_controlPanel.ControlButtonSetActive(m_controlPanel.kanButton, cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.KAN));
		Hand.EnableDiscardType enableDiscardType = Players[m_commandClient.PlayerIndex].hand.EnableDiscardInReachButton(cmd.Choices.MachiPatterns, m_mahjongPlayFlow.m_openMachiList, m_haiNavi[m_commandClient.PlayerIndex]);
		bool IsTenpaiUnableReach = shanten <= 0 && !Players[m_commandClient.PlayerIndex].m_isNaki && m_mahjongPlayFlow.m_nokoriHaiNum > 0 && !Players[m_commandClient.PlayerIndex].m_isReach && !m_mahjongPlayFlow.m_isDaiuchi;
		if (cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.REACH))
		{
			if (enableDiscardType >= Hand.EnableDiscardType.OpenEnable)
			{
				m_controlPanel.ControlButtonSetActive(m_controlPanel.reachButton, active: true);
				m_isReachThisTurn = false;
				m_isFirstReach = true;
				if (GameDataManager.Mahjong.IsFireReachMode)
				{
					if (cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.FIREREACH))
					{
						m_isFireReachSelectable = true;
						m_controlPanel.ControlButtonSetActive(m_controlPanel.fireReachButton, active: true);
					}
					else
					{
						m_isFireReachSelectable = false;
						TenpaiUnableReachSet(isFireReach: true, isOpenReach: false, isAtomicReach: false);
					}
				}
				if (GameDataManager.Mahjong.IsAtomicReachEnableMode)
				{
					if (enableDiscardType == Hand.EnableDiscardType.OpenEnable)
					{
						m_isAtomicReachSelectable = false;
						m_controlPanel.ControlButtonSetActive(m_controlPanel.unableAtomicReachButton, active: true, cautionDisp: true, scoreCaution: false, machiCaution: false, fireReachUsed: false, openReachUsed: false, atomicReachUsed: false, discardCaution: false, gukeiCaution: true);
					}
					else if (cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.ATOMICREACH))
					{
						if (Players[m_commandClient.PlayerIndex].hand.EnableDiscardKaratenInReachButton(cmd.Choices.MachiPatterns, m_haiNavi[m_commandClient.PlayerIndex]))
						{
							m_isAtomicReachSelectable = true;
							m_controlPanel.ControlButtonSetActive(m_controlPanel.atomicReachButton, active: true);
						}
						else
						{
							m_isAtomicReachSelectable = false;
							m_controlPanel.ControlButtonSetActive(m_controlPanel.unableAtomicReachButton, active: true, cautionDisp: true, scoreCaution: false, machiCaution: true);
						}
					}
					else
					{
						m_isAtomicReachSelectable = false;
						TenpaiUnableReachSet(isFireReach: false, isOpenReach: false, isAtomicReach: true);
					}
				}
				if (GameDataManager.Mahjong.IsOpenReachEnableMode)
				{
					if (cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.OPENREACH))
					{
						m_isOpenReachSelectable = true;
						m_controlPanel.ControlButtonSetActive(m_controlPanel.openReachButton, active: true);
					}
					else
					{
						m_isOpenReachSelectable = false;
						TenpaiUnableReachSet(isFireReach: false, isOpenReach: true, isAtomicReach: false);
					}
				}
			}
			else
			{
				m_controlPanel.ControlButtonSetActive(m_controlPanel.unableReachButton, active: true, cautionDisp: true, scoreCaution: false, machiCaution: false, fireReachUsed: false, openReachUsed: false, atomicReachUsed: false, discardCaution: true);
				if (GameDataManager.Mahjong.IsFireReachMode)
				{
					m_controlPanel.ControlButtonSetActive(m_controlPanel.unableFireReachButton, active: true, cautionDisp: true, scoreCaution: false, machiCaution: false, fireReachUsed: false, openReachUsed: false, atomicReachUsed: false, discardCaution: true);
				}
				if (GameDataManager.Mahjong.IsAtomicReachEnableMode)
				{
					if (enableDiscardType == Hand.EnableDiscardType.False)
					{
						m_controlPanel.ControlButtonSetActive(m_controlPanel.unableAtomicReachButton, active: true, cautionDisp: true, scoreCaution: false, machiCaution: false, fireReachUsed: false, openReachUsed: false, atomicReachUsed: false, discardCaution: false, gukeiCaution: true);
					}
					else
					{
						m_controlPanel.ControlButtonSetActive(m_controlPanel.unableAtomicReachButton, active: true, cautionDisp: true, scoreCaution: false, machiCaution: false, fireReachUsed: false, openReachUsed: false, atomicReachUsed: false, discardCaution: true);
					}
				}
				if (GameDataManager.Mahjong.IsOpenReachEnableMode)
				{
					m_controlPanel.ControlButtonSetActive(m_controlPanel.unableOpenReachButton, active: true, cautionDisp: true, scoreCaution: false, machiCaution: false, fireReachUsed: false, openReachUsed: false, atomicReachUsed: false, discardCaution: true);
				}
			}
		}
		else if (IsTenpaiUnableReach)
		{
			TenpaiUnableReachSet(isFireReach: false, isOpenReach: false, isAtomicReach: false);
			if (GameDataManager.Mahjong.IsFireReachMode)
			{
				TenpaiUnableReachSet(isFireReach: true, isOpenReach: false, isAtomicReach: false);
			}
			if (GameDataManager.Mahjong.IsAtomicReachEnableMode)
			{
				TenpaiUnableReachSet(isFireReach: false, isOpenReach: false, isAtomicReach: true);
			}
			if (GameDataManager.Mahjong.IsOpenReachEnableMode)
			{
				TenpaiUnableReachSet(isFireReach: false, isOpenReach: true, isAtomicReach: false);
			}
		}
		if (cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.TSUMOAGARI))
		{
			m_controlPanel.agariButtonBgEffect.SetActive(value: true);
			m_controlPanel.agariButtonBgEffect.GetComponent<Animator>().SetTrigger("Start");
			SoundManager.PlaySe("SFX_MJG_SELECT3").Forget();
		}
		else if (cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.REACH))
		{
			m_controlPanel.reachButtonBgEffect.SetActive(value: true);
			m_controlPanel.reachButtonBgEffect.GetComponent<Animator>().SetTrigger("Start");
			SoundManager.PlaySe("SFX_MJG_SELECT2").Forget();
		}
		else if (cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.KAN))
		{
			m_controlPanel.kanButtonBgEffect.SetActive(value: true);
			m_controlPanel.kanButtonBgEffect.GetComponent<Animator>().SetTrigger("Start");
			SoundManager.PlaySe("SFX_MJG_SELECT1").Forget();
		}
		Players[m_commandClient.PlayerIndex].m_machihaiPanel.Reset();
		m_isAutoTsumogiri = false;
		m_isTsumogiri = false;
		if (!GameDataManager.Mahjong.IsHaifuPlay)
		{
			thinkCount.ThinkStart(ThinkCount.AttentionMessage.MessageHaiSelect);
		}
		Players[m_commandClient.PlayerIndex].hand.TenpaiTargetIconSetDisp(isDisp: false);
		Players[m_commandClient.PlayerIndex].hand.OpenHoujuTargetIconSetDisp(isDisp: false);
		Players[m_commandClient.PlayerIndex].hand.m_isTenpaiTargetIconDispTiming = true;
		Players[m_commandClient.PlayerIndex].hand.m_isOpenTargetIconDispTiming = true;
		if (shanten == 0)
		{
			bool isRestart = m_isReachThisTurn || !Players[playerIndex].m_isReach || cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.TSUMOAGARI);
			Players[m_commandClient.PlayerIndex].StartTenpaiEffect(han >= Han.Mangan, isRestart);
			if (!Players[m_commandClient.PlayerIndex].m_isReach)
			{
				Players[m_commandClient.PlayerIndex].hand.TenpaiTargetIconSet(m_haiNavi[m_entryTakuInfo.PlayerIndex]);
				if (GameDataManager.PlayerConfig.IsTaikyokuSupport)
				{
					Players[m_commandClient.PlayerIndex].hand.TenpaiTargetIconSetDisp(isDisp: true);
				}
			}
		}
		if (!Players[m_commandClient.PlayerIndex].m_isReach)
		{
			Players[m_commandClient.PlayerIndex].hand.OpenHoujuTargetIconSet(m_mahjongPlayFlow.m_openMachiList);
			Players[m_commandClient.PlayerIndex].hand.OpenHoujuTargetIconSetDisp(isDisp: true);
		}
		return false;
		void TenpaiUnableReachSet(bool isFireReach, bool isOpenReach, bool isAtomicReach)
		{
			int num = (isFireReach ? 5000 : 1000);
			if (isFireReach && Players[m_commandClient.PlayerIndex].m_isFireReachUsed)
			{
				m_controlPanel.ControlButtonSetActive(m_controlPanel.unableFireReachButton, active: true, cautionDisp: true, scoreCaution: false, machiCaution: false, fireReachUsed: true);
			}
			else if (isOpenReach && Players[m_commandClient.PlayerIndex].m_isOpenReachUsed)
			{
				m_controlPanel.ControlButtonSetActive(m_controlPanel.unableOpenReachButton, active: true, cautionDisp: true, scoreCaution: false, machiCaution: false, fireReachUsed: false, openReachUsed: true);
			}
			else
			{
				Button button = (isFireReach ? m_controlPanel.unableFireReachButton : (isAtomicReach ? m_controlPanel.unableAtomicReachButton : (isOpenReach ? m_controlPanel.unableOpenReachButton : m_controlPanel.unableReachButton)));
				if (IsTenpaiUnableReach)
				{
					bool machiCaution = false;
					if (cmd.Choices.MachiPatterns.Count > 0)
					{
						machiCaution = true;
						foreach (ChoicesData.MachiPatternData machiPattern in cmd.Choices.MachiPatterns)
						{
							int[] machiPaiID = machiPattern.MachiPaiID;
							for (int i = 0; i < machiPaiID.Length; i++)
							{
								if (machiPaiID[i] != 0 && MahjongUtility.GetRestPaiNum((Pai)machiPaiID[i]) > 0)
								{
									machiCaution = false;
									break;
								}
							}
						}
					}
					m_controlPanel.ControlButtonSetActive(button, active: true, cautionDisp: true, Players[m_commandClient.PlayerIndex].m_score < num, machiCaution);
				}
			}
		}
	}

	private async UniTask<bool> Choosing(TsumoChoices cmd, int playerIndex)
	{
		m_mahjongPlayFlow.m_cancellationToken.ThrowIfCancellationRequested();
		m_isAutoTsumogiri = false;
		if (MahjongUtility.IsNoInputAutoTsumogiri())
		{
			m_isAutoTsumogiri = true;
		}
		else if (Players[m_commandClient.PlayerIndex].m_isReach && !GameDataManager.Mahjong.IsHaifuPlay && !m_controlPanel.agariButton.IsActive() && !m_controlPanel.kanButton.IsActive())
		{
			m_isAutoTsumogiri = true;
		}
		if (m_isAutoTsumogiri)
		{
			m_discardTile = Players[m_commandClient.PlayerIndex].hand.GetLastItem();
			m_discardPos = Players[m_commandClient.PlayerIndex].hand.GetItemPos(m_discardTile);
			RefleshMachipaiPanel(cmd, m_discardTile);
			thinkCount.ThinkEnd();
			thinkCount.ThinkingTyoukouEnd();
			m_controlPanel.Destruct();
			ReachLevel reachLevel = ((m_isFireReachSelectable && m_fireReachChoosing) ? ReachLevel.FireReach : ((m_isAtomicReachSelectable && m_atomicReachChoosing) ? ReachLevel.AtomicReach : ((!m_isOpenReachSelectable || !m_openReachChoosing) ? ReachLevel.Normal : ReachLevel.OpenReach)));
			bool isFireReach = reachLevel == ReachLevel.FireReach && m_isReachThisTurn;
			bool isAtomicReach = reachLevel == ReachLevel.AtomicReach && m_isReachThisTurn;
			bool isOpenReach = reachLevel == ReachLevel.OpenReach && m_isReachThisTurn;
			m_commandClient.Send(new MFG.Taikyoku.Command.Send.Sutehai(m_discardPos, (int)m_discardTile.pai, m_isReachThisTurn, bTsumoGiri: true, reachLevel)).Forget();
			await Players[m_commandClient.PlayerIndex].hand.Select(m_discardTile, m_sdChara, animation: false);
			await UniTask.Delay(300);
			await m_mahjongPlayFlow.DiscardProcess(m_commandClient.PlayerIndex, isReach: false, isTsumogiri: true, m_discardTile, m_discardTile.pai, isFireReach, isOpenReach, isAtomicReach);
			return false;
		}
		if (Players[m_commandClient.PlayerIndex].hand.IsNowTsumoChoice())
		{
			if (m_myTurnTweener != null)
			{
				m_myTurnTweener.Kill();
			}
			myTurn.SetActive(value: true);
			myTurn.GetComponent<CanvasGroup>().alpha = 1f;
			m_myTurnTweener = myTurn.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).SetDelay(1f);
		}
		m_controlPanel.ResetOnClickFlag();
		if (m_controlPanel.agariButton.IsActive())
		{
			LEDManager.Instance.AnimColor(new Color[3]
			{
				Color.red,
				Color.black,
				Color.black
			}, isSpread: false);
		}
		while (true)
		{
			await UniTask.NextFrame();
			m_mahjongPlayFlow.ProcessTyoukouActionPlay();
			m_mahjongPlayFlow.m_cancellationToken.ThrowIfCancellationRequested();
			Players[m_commandClient.PlayerIndex].m_machihaiPanel.gameObject.SetActive(value: false);
			RefleshMachipaiPanel(cmd, Players[m_commandClient.PlayerIndex].hand.GetSelectedTile());
			if (m_controlPanel.m_isOnClickAgari || ((m_playerConfigGameData.IsAutoAgariEnable || Bi2xInput.IsTrigger(BUTTON.Q_BUTTON) || m_mahjongPlayFlow.m_isDaiuchi) && m_controlPanel.agariButton.IsActive()))
			{
				return await ExecOnClickAgari(cmd, playerIndex);
			}
			if (m_isFireReachSelectable && (m_controlPanel.m_isOnClickFireReach || (m_mahjongPlayFlow.m_isDaiuchi && m_controlPanel.fireReachButton.IsActive() && !m_isReachThisTurn)))
			{
				await ExecOnClickReach(cmd, playerIndex, isFireReach: true);
			}
			else if (m_isAtomicReachSelectable && (m_controlPanel.m_isOnClickAtomicReach || (m_mahjongPlayFlow.m_isDaiuchi && m_controlPanel.atomicReachButton.IsActive() && !m_isReachThisTurn)))
			{
				await ExecOnClickReach(cmd, playerIndex, isFireReach: false, isOpenReach: false, isAtomicReach: true);
			}
			else if (m_isOpenReachSelectable && (m_controlPanel.m_isOnClickOpenReach || (m_mahjongPlayFlow.m_isDaiuchi && m_controlPanel.openReachButton.IsActive() && !m_isReachThisTurn)))
			{
				await ExecOnClickReach(cmd, playerIndex, isFireReach: false, isOpenReach: true);
			}
			else if (m_controlPanel.m_isOnClickReach || ((m_mahjongPlayFlow.m_isDaiuchi || m_mahjongPlayFlow.m_isRecReachCutin) && m_controlPanel.reachButton.IsActive() && !m_isReachThisTurn))
			{
				await ExecOnClickReach(cmd, playerIndex);
			}
			else
			{
				if (m_controlPanel.m_isOnClickKan)
				{
					return await ExecOnClickKan(cmd, playerIndex);
				}
				if (thinkCount.IsTimeUp())
				{
					m_discardPos = Players[m_commandClient.PlayerIndex].hand.GetTimeUpPaiPos();
					m_discardTile = Players[m_commandClient.PlayerIndex].hand.GetTile(m_discardPos);
					m_isTsumogiri = Players[m_commandClient.PlayerIndex].hand.IsTsumogiri(m_discardPos);
					if (m_discardPos >= 0)
					{
						thinkCount.ThinkEnd();
						thinkCount.ThinkingTyoukouEnd();
						m_controlPanel.Destruct();
						ReachLevel reachLevel2 = ((m_isFireReachSelectable && m_fireReachChoosing) ? ReachLevel.FireReach : ((m_isAtomicReachSelectable && m_atomicReachChoosing) ? ReachLevel.AtomicReach : ((!m_isOpenReachSelectable || !m_openReachChoosing) ? ReachLevel.Normal : ReachLevel.OpenReach)));
						bool isFireReach2 = reachLevel2 == ReachLevel.FireReach && m_isReachThisTurn;
						bool isAtomicReach2 = reachLevel2 == ReachLevel.AtomicReach && m_isReachThisTurn;
						bool isOpenReach2 = reachLevel2 == ReachLevel.OpenReach && m_isReachThisTurn;
						m_commandClient.Send(new MFG.Taikyoku.Command.Send.Sutehai(m_discardPos, (int)m_discardTile.pai, m_isReachThisTurn, m_isTsumogiri, reachLevel2)).Forget();
						await m_mahjongPlayFlow.DiscardProcess(m_commandClient.PlayerIndex, m_isReachThisTurn, m_isTsumogiri, m_discardTile, m_discardTile.pai, isFireReach2, isOpenReach2, isAtomicReach2);
						break;
					}
				}
				else if (m_mahjongPlayFlow.m_haiDiscardExecute || m_mahjongPlayFlow.m_isDaiuchi || m_mahjongPlayFlow.m_isRecReachCutin)
				{
					m_mahjongPlayFlow.m_haiDiscardExecute = false;
					m_discardTile = Players[m_commandClient.PlayerIndex].hand.GetDiscardTile(m_mahjongPlayFlow.m_isDaiuchi || m_mahjongPlayFlow.m_isRecReachCutin, m_haiNavi[m_commandClient.PlayerIndex].ShantenInfos, m_isReachThisTurn);
					m_discardPos = Players[m_commandClient.PlayerIndex].hand.GetItemPos(m_discardTile);
					m_isTsumogiri = Players[m_commandClient.PlayerIndex].hand.IsTsumogiri(m_discardPos);
					RefleshMachipaiPanel(cmd, m_discardTile);
					if (m_discardPos >= 0)
					{
						m_haiNaviPanel.ReloadButtonBySelect(-1);
						thinkCount.ThinkEnd();
						thinkCount.ThinkingTyoukouEnd();
						m_controlPanel.Destruct();
						if (m_mahjongPlayFlow.m_isDaiuchi || m_mahjongPlayFlow.m_isRecReachCutin)
						{
							await UniTask.Delay(TimeSpan.FromSeconds(UnityEngine.Random.Range(0.5f, 1.5f)));
						}
						ReachLevel reachLevel3 = ((m_isFireReachSelectable && m_fireReachChoosing) ? ReachLevel.FireReach : ((m_isAtomicReachSelectable && m_atomicReachChoosing) ? ReachLevel.AtomicReach : ((!m_isOpenReachSelectable || !m_openReachChoosing) ? ReachLevel.Normal : ReachLevel.OpenReach)));
						bool isFireReach3 = reachLevel3 == ReachLevel.FireReach && m_isReachThisTurn;
						bool isAtomicReach3 = reachLevel3 == ReachLevel.AtomicReach && m_isReachThisTurn;
						bool isOpenReach3 = reachLevel3 == ReachLevel.OpenReach && m_isReachThisTurn;
						m_commandClient.Send(new MFG.Taikyoku.Command.Send.Sutehai(m_discardPos, (int)m_discardTile.pai, m_isReachThisTurn, m_isTsumogiri, reachLevel3)).Forget();
						await m_mahjongPlayFlow.DiscardProcess(m_commandClient.PlayerIndex, m_isReachThisTurn, m_isTsumogiri, m_discardTile, m_discardTile.pai, isFireReach3, isOpenReach3, isAtomicReach3);
						if (m_discardTile.GetHaiJudgeMark() == Tile.HaiJudgeMark.Safety)
						{
							SoundManager.PlaySe("SFX_MJG_TOUCH4").Forget();
						}
						else if (m_discardTile.GetHaiJudgeMark() == Tile.HaiJudgeMark.Danger || m_discardTile.GetHaiJudgeMark() == Tile.HaiJudgeMark.Warning)
						{
							SoundManager.PlaySe("SFX_MJG_TOUCH6").Forget();
						}
						else
						{
							SoundManager.PlaySe("SFX_MJG_TOUCH2").Forget();
						}
						Players[m_commandClient.PlayerIndex].hand.SetDiscardTile(null);
						break;
					}
				}
			}
			if (m_isKuikae)
			{
				Tile touchedTile = MahjongUtility.GetTouchedTile();
				if (touchedTile != null && !touchedTile.m_enableDiscard)
				{
					m_attentionMessage.DispAttention(AttentionMessage.Text.TextKuikae);
				}
			}
			if (GameDataManager.Mahjong.IsHaifuPlay)
			{
				return false;
			}
		}
		return false;
	}

	private async UniTask<bool> ChoiceEnd(TsumoChoices cmd, int playerIndex)
	{
		m_mahjongPlayFlow.m_cancellationToken.ThrowIfCancellationRequested();
		m_attentionMessage.Exit();
		thinkCount.ThinkEnd();
		m_controlPanel.Destruct();
		MahjongUtility.SetTouchedTile(null);
		Players[m_commandClient.PlayerIndex].hand.TenpaiTargetIconSetDisp(isDisp: false);
		Players[m_commandClient.PlayerIndex].hand.m_isTenpaiTargetIconDispTiming = false;
		Players[m_commandClient.PlayerIndex].hand.OpenHoujuTargetIconSetDisp(isDisp: false);
		Players[m_commandClient.PlayerIndex].hand.m_isOpenTargetIconDispTiming = false;
		if (GameDataManager.Mahjong.IsHaifuPlay)
		{
			return false;
		}
		if (m_isAutoTsumogiri)
		{
			Players[m_commandClient.PlayerIndex].hand.SetNowTsumoChoice(flag: false);
			await Players[m_commandClient.PlayerIndex].hand.Discard(m_isReachThisTurn);
		}
		else
		{
			Players[m_commandClient.PlayerIndex].hand.SetNowTsumoChoice(flag: false);
			await Players[m_commandClient.PlayerIndex].hand.Discard(m_isReachThisTurn);
			if (!Players[m_commandClient.PlayerIndex].m_isReach)
			{
				m_mentsuCompleteEffect.PlayEffect(Players[m_commandClient.PlayerIndex], m_haiNavi[m_entryTakuInfo.PlayerIndex].GetCompletedMentsu(m_discardPos)).Forget();
			}
			if (MahjongUtility.IsDora(m_discardTile.pai))
			{
				await Players[m_commandClient.PlayerIndex].character.Action(Character.EventID.DoraDiscard);
			}
		}
		m_isReachThisTurn = false;
		if (Players[m_commandClient.PlayerIndex].m_isOpenReach)
		{
			Player p = Players[m_commandClient.PlayerIndex];
			List<Pai> openMachiPais = p.m_openMachiPais;
			if (openMachiPais == null || openMachiPais.Count <= 0)
			{
				HaiNavi haiNavi = m_haiNavi[m_commandClient.PlayerIndex];
				await haiNavi.Analyze();
				m_mahjongPlayFlow.m_haiNaviPanel.Reload(haiNavi.m_yakuInfo, haiNavi.m_oneByOneDiscardedYakuPossibilityInfos, p.m_isFireReach, p.m_isOpenReach, p.m_isAtomicReach, p.m_isNaki).Forget();
				p.SetTenpai();
				if (!p.IsJicha())
				{
					Pai[] array = (from x in haiNavi.MachiHais()
						select x.HaiIDToPai() into x
						orderby x
						select x).ToArray();
					p.m_machihaiPanel.OpenMachiDisp(isDisp: true, array);
					p.m_openMachiPais = array.OrderBy((Pai x) => x).ToList();
					p.SetIsOpenFuritenInOpenStart();
				}
			}
		}
		if (!Players[m_commandClient.PlayerIndex].m_isReach && !m_mahjongPlayFlow.m_isDaiuchi)
		{
			MahjongUtility.IncrementNoInputCount();
		}
		return false;
	}

	private void RefleshMachipaiPanel(TsumoChoices cmd, Tile selectedTile)
	{
		int itemPos = Players[m_commandClient.PlayerIndex].hand.GetItemPos(selectedTile);
		if (itemPos < 0)
		{
			return;
		}
		ShantenInfo shantenInfo = m_haiNavi[m_entryTakuInfo.PlayerIndex].ShantenInfos[itemPos];
		Pai pai = selectedTile.pai;
		if (shantenInfo.Shanten == 0)
		{
			if (!Players[m_commandClient.PlayerIndex].m_isReach)
			{
				Players[m_commandClient.PlayerIndex].m_furitenPais.Clear();
			}
			if (cmd.Choices.MachiPatterns.Count <= 0 || pai == Pai.NONE)
			{
				return;
			}
			{
				foreach (ChoicesData.MachiPatternData machiPattern in cmd.Choices.MachiPatterns)
				{
					if (machiPattern.SutePaiID == (int)MahjongUtility.AkaToNormal(pai))
					{
						Players[m_commandClient.PlayerIndex].m_machihaiPanel.SetMachiPai(machiPattern.MachiPaiID, machiPattern.MachiPaiStatus, m_isReachThisTurn, (Pai)machiPattern.SutePaiID, Players[m_commandClient.PlayerIndex].m_furitenPais);
					}
				}
				return;
			}
		}
		if (shantenInfo.Shanten != 1)
		{
			return;
		}
		List<HaiID> shantenUpHais = shantenInfo.ShantenUpHais;
		int[] array = new int[shantenUpHais.Count];
		if (shantenUpHais.Count > 0)
		{
			for (int i = 0; i < shantenUpHais.Count; i++)
			{
				array[i] = (int)shantenUpHais[i].HaiIDToPai();
			}
			Players[m_commandClient.PlayerIndex].m_machihaiPanel.SetIshantenMachiPai(array, pai);
		}
	}

	private async UniTask<bool> KanChoice(TsumoChoices cmd, int playerIndex)
	{
		m_controlPanel.Destruct();
		int result;
		while (true)
		{
			await UniTask.NextFrame();
			m_mahjongPlayFlow.m_cancellationToken.ThrowIfCancellationRequested();
			if (thinkCount.IsTimeUp() || m_mahjongPlayFlow.m_isDaiuchi)
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				return await TsumoChoices_SendKan(cmd, playerIndex);
			}
			if (ResidentFlow.CustomInput.GetMouseButtonDown(0))
			{
				List<RaycastResult> list = new List<RaycastResult>();
				PointerEventData eventData = new PointerEventData(EventSystem.current)
				{
					position = ResidentFlow.CustomInput.mousePosition
				};
				EventSystem.current.RaycastAll(eventData, list);
				foreach (RaycastResult item in list)
				{
					if (naki.Select(item.gameObject) >= 0)
					{
						SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
						return await TsumoChoices_SendKan(cmd, playerIndex, naki.Select(item.gameObject));
					}
				}
			}
			else if (Input.anyKeyDown && int.TryParse(Input.inputString, out result) && result >= 1 && result <= cmd.Choices.KanPatterns.Count)
			{
				break;
			}
		}
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		return await TsumoChoices_SendKan(cmd, playerIndex, result - 1);
	}

	private async UniTask<bool> End(TsumoChoices cmd, int playerIndex, bool isAgari = false)
	{
		m_mahjongPlayFlow.m_cancellationToken.ThrowIfCancellationRequested();
		await m_haiNavi[playerIndex].Analyze(cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.TSUMOAGARI));
		m_haiNaviPanel.Reload(m_haiNavi[playerIndex].m_yakuInfo, m_haiNavi[playerIndex].m_oneByOneDiscardedYakuPossibilityInfos, Players[playerIndex].m_isFireReach, Players[playerIndex].m_isOpenReach, Players[playerIndex].m_isAtomicReach, Players[playerIndex].m_isNaki).Forget();
		_ = Players[playerIndex].m_isReach;
		m_haiNaviPanel.ReloadButtonBySelect(-1);
		if (Players[m_entryTakuInfo.PlayerIndex].m_cha == Cha.Jicha)
		{
			m_haiShantenCounter.SetShantenCount(m_haiNavi[m_entryTakuInfo.PlayerIndex].CurShanten).Forget();
		}
		thinkCount.ThinkEnd();
		m_controlPanel.Destruct();
		if (isAgari)
		{
			TouchPanelTelemetry touchPanelTelemetry = TouchPanelTelemetry.Diff(Bi2xInput.Instance.GetTouchPanelTelemetry(), _startTouchPanelTelemetry);
			GameDataManager.Log.AddTsumoAgariData(Time.time - m_startTime, touchPanelTelemetry);
		}
		else if (!Players[playerIndex].m_isReach)
		{
			GameDataManager.Log.AddTsumoChoicesData(Time.time - m_startTime);
		}
		int curShanten = m_haiNavi[m_entryTakuInfo.PlayerIndex].CurShanten;
		Han han = m_haiNavi[m_entryTakuInfo.PlayerIndex].GetHan();
		if (curShanten == 0)
		{
			Players[m_commandClient.PlayerIndex].StartTenpaiEffect(han >= Han.Mangan);
		}
		Players[playerIndex].m_isFirstTsumo = false;
		await UniTask.WaitWhile(() => m_commandClient.IsBusy);
		return true;
	}

	private async UniTask<bool> ExecOnClickAgari(TsumoChoices cmd, int playerIndex)
	{
		m_controlPanel.m_isOnClickAgari = false;
		m_controlPanel.agariButton.GetComponent<Animator>().SetTrigger("Push");
		if (Bi2xInput.IsTrigger(BUTTON.Q_BUTTON))
		{
			SoundManager.PlaySe("SFX_MJG_RONTSUMO3").Forget();
			LEDManager.Instance.AnimColor(new Color[2]
			{
				Color.red,
				Color.black
			}, isSpread: true, 2);
		}
		else
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			LEDManager.Instance.AnimColor(new Color[2]
			{
				Color.red,
				Color.black
			}, isSpread: true, 2);
		}
		m_commandClient.Send(new MFG.Taikyoku.Command.Send.TsumoAgari()).Forget();
		m_haiNaviPanel.ReloadActive = true;
		return await End(cmd, playerIndex, isAgari: true);
	}

	private void OnClickReachThisTurnProcess(TsumoChoices cmd, bool isFireReach = false, bool isOpenReach = false, bool isAtomicReach = false)
	{
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		if (m_isFirstReach)
		{
			thinkCount.ThinkStart(ThinkCount.AttentionMessage.MessageHaiSelect);
			thinkCount.ThinkingTyoukouStart(Players[m_commandClient.PlayerIndex]);
			m_isFirstReach = false;
		}
		m_controlPanel.ControlButtonSetActive(m_controlPanel.kanButton, active: false);
		bool flag = m_isFireReachSelectable && isFireReach;
		bool flag2 = m_isOpenReachSelectable && isOpenReach;
		bool flag3 = m_isAtomicReachSelectable && isAtomicReach;
		m_controlPanel.ControlReachButtonInteractable(m_controlPanel.reachButton, !flag && !flag2 && !flag3);
		if (m_isFireReachSelectable)
		{
			m_controlPanel.ControlReachButtonInteractable(m_controlPanel.fireReachButton, flag);
		}
		if (m_isAtomicReachSelectable)
		{
			m_controlPanel.ControlReachButtonInteractable(m_controlPanel.atomicReachButton, flag3);
		}
		if (m_isOpenReachSelectable)
		{
			m_controlPanel.ControlReachButtonInteractable(m_controlPanel.openReachButton, flag2);
		}
		Tile selectedTile = Players[m_commandClient.PlayerIndex].hand.GetSelectedTile();
		Players[m_commandClient.PlayerIndex].hand.EnableDiscardInReach(cmd.Choices.MachiPatterns, m_mahjongPlayFlow.m_openMachiList, m_haiNavi[m_commandClient.PlayerIndex], flag3);
		Debug.Log("MachiPatterns/" + cmd.Choices.MachiPatterns.Count);
		if (selectedTile == null || !selectedTile.m_enableDiscard)
		{
			Players[m_commandClient.PlayerIndex].hand.Select((Pai)cmd.Choices.MachiPatterns[0].SutePaiID, m_sdChara);
		}
		else
		{
			Players[m_commandClient.PlayerIndex].hand.Select(selectedTile, m_sdChara, animation: false).Forget();
		}
	}

	private void OnClickReachIsNotReachThisTurnProcess(TsumoChoices cmd)
	{
		SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
		m_controlPanel.ControlButtonSetActive(m_controlPanel.kanButton, cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.KAN));
		m_controlPanel.ControlReachButtonInteractable(m_controlPanel.reachButton, isActive: true);
		if (m_isFireReachSelectable)
		{
			m_controlPanel.ControlReachButtonInteractable(m_controlPanel.fireReachButton, isActive: true);
		}
		if (m_isAtomicReachSelectable)
		{
			m_controlPanel.ControlReachButtonInteractable(m_controlPanel.atomicReachButton, isActive: true);
		}
		if (m_isOpenReachSelectable)
		{
			m_controlPanel.ControlReachButtonInteractable(m_controlPanel.openReachButton, isActive: true);
		}
		Tile selectedTile = Players[m_commandClient.PlayerIndex].hand.GetSelectedTile();
		Players[m_commandClient.PlayerIndex].hand.AllEnableDiscard(enable: true);
		Players[m_commandClient.PlayerIndex].hand.EnableDiscardInReachRelease(m_mahjongPlayFlow.m_openMachiList);
		Players[m_commandClient.PlayerIndex].hand.Select(selectedTile, m_sdChara).Forget();
	}

	private async UniTask ExecOnClickReach(TsumoChoices cmd, int playerIndex, bool isFireReach = false, bool isOpenReach = false, bool isAtomicReach = false)
	{
		m_controlPanel.m_isOnClickReach = false;
		m_controlPanel.m_isOnClickFireReach = false;
		m_controlPanel.m_isOnClickOpenReach = false;
		m_controlPanel.m_isOnClickAtomicReach = false;
		m_isReachThisTurn = !m_isReachThisTurn;
		m_fireReachChoosing = isFireReach && m_isReachThisTurn;
		m_openReachChoosing = isOpenReach && m_isReachThisTurn;
		m_atomicReachChoosing = isAtomicReach && m_isReachThisTurn;
		if (m_isReachThisTurn)
		{
			OnClickReachThisTurnProcess(cmd, isFireReach, isOpenReach, isAtomicReach);
		}
		else
		{
			OnClickReachIsNotReachThisTurnProcess(cmd);
		}
		if (isFireReach)
		{
			m_controlPanel.FireReachCautionActive(m_isReachThisTurn);
		}
		else if (isAtomicReach)
		{
			m_controlPanel.AtomicReachCautionActive(m_isReachThisTurn);
		}
		else if (isOpenReach)
		{
			m_controlPanel.OpenReachCautionActive(m_isReachThisTurn);
		}
		else
		{
			m_controlPanel.ReachCautionActive(m_isReachThisTurn);
		}
	}

	private async UniTask<bool> ExecOnClickKan(TsumoChoices cmd, int playerIndex)
	{
		m_controlPanel.m_isOnClickKan = false;
		m_controlPanel.kanButton.GetComponent<Animator>().SetTrigger("Push");
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		Players[m_commandClient.PlayerIndex].hand.TenpaiTargetIconSetDisp(isDisp: false);
		Players[m_commandClient.PlayerIndex].hand.OpenHoujuTargetIconSetDisp(isDisp: false);
		if (cmd.Choices.KanPatterns.Count == 1)
		{
			return await TsumoChoices_SendKan(cmd, playerIndex);
		}
		thinkCount.ThinkStart(ThinkCount.AttentionMessage.MessageNakiSelect);
		naki.Play();
		foreach (ChoicesData.KanPatternData kanPattern in cmd.Choices.KanPatterns)
		{
			naki.AddNakiPattern((Pai)kanPattern.PaiID);
		}
		return await KanChoice(cmd, playerIndex);
	}

	private async UniTask<bool> TsumoChoices_SendKan(TsumoChoices cmd, int playerIndex, int no = 0)
	{
		if (no < 0)
		{
			return false;
		}
		Pai paiID = (Pai)cmd.Choices.KanPatterns[no].PaiID;
		Pai pai = MahjongUtility.AkaToNormal(paiID);
		Pai pai2 = MahjongUtility.NormalToAka(paiID);
		Debug.Log($"TsumoChoices_SendKan paiID/{paiID}/{pai}/{pai2}");
		List<int> paiPos = Players[m_commandClient.PlayerIndex].hand.GetPaiPos(new List<Pai> { pai, pai2 });
		int num = ((paiPos.Count != 1) ? Mathf.Min(paiPos[0], paiPos[1]) : paiPos[0]);
		foreach (Pai pai3 in Players[m_commandClient.PlayerIndex].hand.GetPais())
		{
			Debug.Log("TsumoChoices_SendKan hand" + pai3);
		}
		switch (cmd.Choices.KanPatterns[no].KanType)
		{
		case 1:
		{
			Debug.Log($"TsumoChoices_SendKan poss/{num}");
			MFG.Taikyoku.Command.Send.Ankan tasCmd2 = new MFG.Taikyoku.Command.Send.Ankan(cmd.Choices.KanPatterns[no].PaiID, num);
			m_commandClient.Send(tasCmd2).Forget();
			break;
		}
		case 3:
		{
			Debug.Log($"TsumoChoices_SendKan poss/{num}");
			MFG.Taikyoku.Command.Send.Kakan tasCmd = new MFG.Taikyoku.Command.Send.Kakan(cmd.Choices.KanPatterns[no].PaiID, num);
			m_commandClient.Send(tasCmd).Forget();
			break;
		}
		}
		naki.Exit();
		return await End(cmd, playerIndex);
	}
}
