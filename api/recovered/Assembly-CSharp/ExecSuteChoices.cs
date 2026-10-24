using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using IO;
using MFG.Taikyoku;
using MFG.Taikyoku.Command;
using MFG.Taikyoku.Command.Receive;
using MFG.Taikyoku.Command.Send;
using MFG.Types;
using UnityEngine;
using UnityEngine.EventSystems;

public class ExecSuteChoices
{
	private ControlPanel m_controlPanel;

	private MahjongPlayFlow m_mahjongPlayFlow;

	private float m_startTime;

	private TouchPanelTelemetry _startTouchPanelTelemetry = new TouchPanelTelemetry();

	private int m_nakiType;

	private CommandClient m_commandClient => GameDataManager.CommandClient;

	private EntryTakuInfo m_entryTakuInfo => GameDataManager.Taikyoku.TakuInfo;

	private PlayerConfigGameData m_playerConfigGameData => GameDataManager.PlayerConfig;

	private List<Player> Players => m_mahjongPlayFlow.Players;

	private List<HaiNavi> m_haiNavi => m_mahjongPlayFlow.m_haiNavi;

	private StageBG m_StageBG => m_mahjongPlayFlow.m_StageBG;

	private ThinkCount thinkCount => m_mahjongPlayFlow.thinkCount;

	private HaiNaviPanel m_haiNaviPanel => m_mahjongPlayFlow.m_haiNaviPanel;

	private HaiShantenCounter m_haiShantenCounter => m_mahjongPlayFlow.m_haiShantenCounter;

	private HaiShantenIconList m_haiShantenIcon => m_mahjongPlayFlow.m_haiShantenIcon;

	private GameObject agariCharaEffectChara => m_mahjongPlayFlow.agariCharaEffectChara;

	private GameObject agariCharaEffectRoot => m_mahjongPlayFlow.agariCharaEffectRoot;

	private AttentionMessage m_attentionMessage => m_mahjongPlayFlow.m_attentionMessage;

	private NakiPattern naki => m_mahjongPlayFlow.naki;

	public GameObject agariHaiEffect => m_mahjongPlayFlow.agariHaiEffect;

	private RECEIVE_COMMAND_TYPE m_cmdKind => m_mahjongPlayFlow.m_cmdKind;

	private bool m_isNakinashi
	{
		get
		{
			if (!m_mahjongPlayFlow.m_takuInfo.m_isNakinashi)
			{
				return m_mahjongPlayFlow.m_isDaiuchi;
			}
			return true;
		}
	}

	public async UniTask<bool> ProcessCommand(SuteChoices cmd, MahjongPlayFlow mpf)
	{
		m_mahjongPlayFlow = mpf;
		m_controlPanel = m_mahjongPlayFlow.m_controlPanel;
		int playerIndex = cmd.PlayerIndexFlag.ToPlayerIndex();
		if (!cmd.PlayerIndexFlag.HasFlag((PLAYER_INDEX_FLAG)(1 << m_entryTakuInfo.PlayerIndex)) || GameDataManager.Mahjong.IsHaifuPlay)
		{
			return true;
		}
		try
		{
			if (await ChoiceInit(cmd, playerIndex))
			{
				return true;
			}
			if (await Choosing(cmd, playerIndex))
			{
				return true;
			}
			await End(cmd);
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
			Debug.Log("ExecSuteChocies Exception Catched. " + ex.Message + ex.StackTrace + " ");
		}
		return true;
	}

	private async UniTask<bool> ExecOnClickPon()
	{
		m_controlPanel.ponButton.GetComponent<Animator>().SetTrigger("Push");
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		m_controlPanel.ControlButtonSetActive(m_controlPanel.chiButton, active: false);
		m_controlPanel.ControlButtonSetActive(m_controlPanel.kanButton, active: false);
		m_controlPanel.ControlButtonSetActive(m_controlPanel.agariButton, active: false);
		m_controlPanel.ControlButtonSetActive(m_controlPanel.nextButton, active: false);
		m_nakiType = 1;
		SuteChoices suteChoices = (SuteChoices)m_commandClient.Current();
		if (suteChoices.Choices.PonPatterns.Count == 1)
		{
			return await SuteChoices_SendPon();
		}
		thinkCount.ThinkStart(ThinkCount.AttentionMessage.MessageNakiSelect);
		naki.Play();
		foreach (ChoicesData.PonPatternData ponPattern in suteChoices.Choices.PonPatterns)
		{
			naki.AddNakiPattern((Pai)ponPattern.PaiID[0], (Pai)ponPattern.PaiID[1]);
		}
		return await PatternSelect(suteChoices);
	}

	private async UniTask<bool> SuteChoices_SendPon(int no = 0)
	{
		if (no < 0)
		{
			return false;
		}
		SuteChoices suteChoices = (SuteChoices)m_commandClient.Current();
		int num = suteChoices.Choices.PonPatterns[no].PaiID[0];
		int num2 = suteChoices.Choices.PonPatterns[no].PaiID[1];
		List<int> paiPos = Players[m_commandClient.PlayerIndex].hand.GetPaiPos(new List<Pai>
		{
			(Pai)num,
			(Pai)num2
		});
		MFG.Taikyoku.Command.Send.Pon tasCmd = new MFG.Taikyoku.Command.Send.Pon(suteChoices.Choices.PaiID, paiPos[0], num, paiPos[1], num2);
		m_commandClient.Send(tasCmd).Forget();
		naki.Exit();
		return await End(suteChoices);
	}

	private async UniTask<bool> ExecOnClickChi()
	{
		m_controlPanel.chiButton.GetComponent<Animator>().SetTrigger("Push");
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		m_controlPanel.ControlButtonSetActive(m_controlPanel.ponButton, active: false);
		m_controlPanel.ControlButtonSetActive(m_controlPanel.kanButton, active: false);
		m_controlPanel.ControlButtonSetActive(m_controlPanel.agariButton, active: false);
		m_controlPanel.ControlButtonSetActive(m_controlPanel.nextButton, active: false);
		m_nakiType = 0;
		SuteChoices suteChoices = (SuteChoices)m_commandClient.Current();
		if (suteChoices.Choices.ChiPatterns.Count == 1)
		{
			return await SuteChoices_SendChi();
		}
		thinkCount.ThinkStart(ThinkCount.AttentionMessage.MessageNakiSelect);
		naki.Play();
		foreach (ChoicesData.ChiPatternData chiPattern in suteChoices.Choices.ChiPatterns)
		{
			naki.AddNakiPattern((Pai)chiPattern.PaiID[0], (Pai)chiPattern.PaiID[1]);
		}
		return await PatternSelect(suteChoices);
	}

	private async UniTask<bool> SuteChoices_SendChi(int no = 0)
	{
		if (no < 0)
		{
			return false;
		}
		SuteChoices suteChoices = (SuteChoices)m_commandClient.Current();
		int num = suteChoices.Choices.ChiPatterns[no].PaiID[0];
		int num2 = suteChoices.Choices.ChiPatterns[no].PaiID[1];
		List<int> paiPos = Players[m_commandClient.PlayerIndex].hand.GetPaiPos(new List<Pai>
		{
			(Pai)num,
			(Pai)num2
		});
		ChiPos chiPos = ChiPos.Center;
		if (suteChoices.Choices.PaiID < num && suteChoices.Choices.PaiID < num2)
		{
			chiPos = ChiPos.Left;
		}
		else if (suteChoices.Choices.PaiID > num && suteChoices.Choices.PaiID > num2)
		{
			chiPos = ChiPos.Right;
		}
		Debug.Log($"チー選択 {suteChoices.Choices.PaiID}");
		Debug.Log($"チー選択 {paiPos[0]}");
		Debug.Log($"チー選択 {num}");
		Debug.Log($"チー選択 {paiPos[1]}");
		Debug.Log($"チー選択 {num2}");
		Debug.Log($"チー選択 {chiPos}");
		MFG.Taikyoku.Command.Send.Chi tasCmd = new MFG.Taikyoku.Command.Send.Chi(suteChoices.Choices.PaiID, paiPos[0], num, paiPos[1], num2, (int)chiPos);
		m_commandClient.Send(tasCmd).Forget();
		naki.Exit();
		return await End(suteChoices);
	}

	private async UniTask<bool> ExecOnClickKan()
	{
		m_controlPanel.kanButton.GetComponent<Animator>().SetTrigger("Push");
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		m_controlPanel.ControlButtonSetActive(m_controlPanel.ponButton, active: false);
		m_controlPanel.ControlButtonSetActive(m_controlPanel.chiButton, active: false);
		m_controlPanel.ControlButtonSetActive(m_controlPanel.agariButton, active: false);
		m_controlPanel.ControlButtonSetActive(m_controlPanel.nextButton, active: false);
		SuteChoices suteChoices = (SuteChoices)m_commandClient.Current();
		int paiID = suteChoices.Choices.KanPatterns[0].PaiID;
		Pai item = MahjongUtility.AkaToNormal((Pai)paiID);
		Pai item2 = MahjongUtility.NormalToAka((Pai)paiID);
		List<int> paiPos = Players[m_commandClient.PlayerIndex].hand.GetPaiPos(new List<Pai> { item, item2 });
		MFG.Taikyoku.Command.Send.Minkan tasCmd = new MFG.Taikyoku.Command.Send.Minkan(tepai_pos: (paiPos.Count != 1) ? Mathf.Min(paiPos[0], paiPos[1]) : paiPos[0], pai_id: suteChoices.Choices.PaiID);
		m_commandClient.Send(tasCmd).Forget();
		return await End(suteChoices);
	}

	private async UniTask<bool> ExecOnClickAgari()
	{
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
		m_commandClient.Send(new MFG.Taikyoku.Command.Send.Ron()).Forget();
		m_haiNaviPanel.ReloadActive = true;
		SuteChoices cmd = (SuteChoices)m_commandClient.Current();
		return await End(cmd, isAgari: true);
	}

	private async UniTask<bool> ExecOnClickNext()
	{
		LEDManager.Instance.AnimColor(new Color[1] { Color.white }, isSpread: true);
		m_controlPanel.nextButton.GetComponent<Animator>().SetTrigger("Push");
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		m_commandClient.Send(new Tsugi()).Forget();
		SuteChoices cmd = (SuteChoices)m_commandClient.Current();
		return await End(cmd);
	}

	private async UniTask<bool> ChoiceInit(SuteChoices cmd, int playerIndex)
	{
		m_mahjongPlayFlow.m_cancellationToken.ThrowIfCancellationRequested();
		m_startTime = Time.time;
		_startTouchPanelTelemetry = Bi2xInput.Instance.GetTouchPanelTelemetry().Clone();
		if (cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.RON))
		{
			Tile tile = MahjongUtility.GetLastTargetTile();
			await UniTask.WaitWhile(() => tile.IsMoving);
			agariCharaEffectChara.SetActive(value: true);
			m_mahjongPlayFlow.AgariHaiEffect(tile, isJicha: false);
			agariCharaEffectRoot.SetActive(value: true);
			agariCharaEffectRoot.GetComponent<Animator>().SetTrigger("Next");
			SoundManager.PlaySe("SFX_MJG_DISPLAY6").Forget();
		}
		if (MahjongUtility.IsNoInputAutoTsumogiri())
		{
			m_commandClient.Send(new Tsugi()).Forget();
			return await End(cmd);
		}
		if (cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.PON) && m_mahjongPlayFlow.m_isRecNakiCutin)
		{
			return await ExecOnClickPon();
		}
		if ((m_playerConfigGameData.IsAutoAgariEnable || m_mahjongPlayFlow.m_isDaiuchi || m_mahjongPlayFlow.m_isRecAgariCutin) && cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.RON))
		{
			m_commandClient.Send(new MFG.Taikyoku.Command.Send.Ron()).Forget();
			m_haiNaviPanel.ReloadActive = true;
			return await End(cmd, isAgari: true);
		}
		if (!m_isNakinashi)
		{
			m_controlPanel.ControlButtonSetActive(m_controlPanel.ponButton, cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.PON), !Players[m_commandClient.PlayerIndex].m_isNaki);
			m_controlPanel.ControlButtonSetActive(m_controlPanel.chiButton, cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.CHI), !Players[m_commandClient.PlayerIndex].m_isNaki);
			m_controlPanel.ControlButtonSetActive(m_controlPanel.kanButton, cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.KAN), !Players[m_commandClient.PlayerIndex].m_isNaki);
			m_controlPanel.ControlButtonSetActive(m_controlPanel.agariButton, cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.RON));
			m_controlPanel.ControlButtonSetActive(m_controlPanel.nextButton, active: true);
			if (cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.RON))
			{
				m_controlPanel.agariButtonBgEffect.SetActive(value: true);
				m_controlPanel.agariButtonBgEffect.GetComponent<Animator>().SetTrigger("Start");
				SoundManager.PlaySe("SFX_MJG_SELECT3").Forget();
			}
			else if (cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.KAN))
			{
				m_controlPanel.kanButtonBgEffect.SetActive(value: true);
				m_controlPanel.kanButtonBgEffect.GetComponent<Animator>().SetTrigger("Start");
				SoundManager.PlaySe("SFX_MJG_SELECT1").Forget();
			}
			else if (cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.PON))
			{
				m_controlPanel.ponButtonBgEffect.SetActive(value: true);
				m_controlPanel.ponButtonBgEffect.GetComponent<Animator>().SetTrigger("Start");
				SoundManager.PlaySe("SFX_MJG_SELECT1").Forget();
			}
			else if (cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.CHI))
			{
				m_controlPanel.chiButtonBgEffect.SetActive(value: true);
				m_controlPanel.chiButtonBgEffect.GetComponent<Animator>().SetTrigger("Start");
				SoundManager.PlaySe("SFX_MJG_SELECT1").Forget();
			}
		}
		else
		{
			if (!cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.RON))
			{
				m_commandClient.Send(new Tsugi()).Forget();
				return await End(cmd);
			}
			m_controlPanel.ControlButtonSetActive(m_controlPanel.agariButton, active: true);
			m_controlPanel.ControlButtonSetActive(m_controlPanel.nextButton, active: true);
			m_controlPanel.agariButtonBgEffect.SetActive(value: true);
			m_controlPanel.agariButtonBgEffect.GetComponent<Animator>().SetTrigger("Start");
			SoundManager.PlaySe("SFX_MJG_SELECT3").Forget();
		}
		NakiSelectTileSetDark(set: true);
		thinkCount.ThinkStart(ThinkCount.AttentionMessage.MessageActionSelect);
		return false;
	}

	private void NakiSelectTileSetDark(bool set)
	{
		foreach (Player player in Players)
		{
			if (player.IsJicha())
			{
				player.dump.SetDark(set);
				continue;
			}
			player.hand.SetDark(set);
			player.meld.SetDark(set);
			player.dump.SetDark(set);
		}
	}

	private async UniTask<bool> Choosing(SuteChoices cmd, int playerIndex)
	{
		m_mahjongPlayFlow.m_cancellationToken.ThrowIfCancellationRequested();
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
		do
		{
			await UniTask.NextFrame();
			m_mahjongPlayFlow.m_cancellationToken.ThrowIfCancellationRequested();
			if (m_controlPanel.m_isOnClickPon)
			{
				return await ExecOnClickPon();
			}
			if (m_controlPanel.m_isOnClickChi)
			{
				return await ExecOnClickChi();
			}
			if (m_controlPanel.m_isOnClickKan)
			{
				return await ExecOnClickKan();
			}
			if (m_controlPanel.m_isOnClickAgari || ((m_playerConfigGameData.IsAutoAgariEnable || Bi2xInput.IsTrigger(BUTTON.Q_BUTTON) || m_mahjongPlayFlow.m_isDaiuchi) && m_controlPanel.agariButton.IsActive()))
			{
				return await ExecOnClickAgari();
			}
		}
		while (!m_controlPanel.m_isOnClickNext && !thinkCount.IsTimeUp() && (!m_isNakinashi || m_controlPanel.agariButton.IsActive()) && !ResidentFlow.CustomInput.GetMouseButtonDown(1));
		return await ExecOnClickNext();
	}

	private async UniTask<bool> PatternSelect(SuteChoices cmd)
	{
		m_mahjongPlayFlow.m_cancellationToken.ThrowIfCancellationRequested();
		m_controlPanel.Destruct();
		while (true)
		{
			await UniTask.NextFrame();
			m_mahjongPlayFlow.m_cancellationToken.ThrowIfCancellationRequested();
			if (thinkCount.IsTimeUp() || m_mahjongPlayFlow.m_isDaiuchi)
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				switch (m_nakiType)
				{
				case 0:
					return await SuteChoices_SendChi();
				case 1:
					return await SuteChoices_SendPon();
				}
				continue;
			}
			if (ResidentFlow.CustomInput.GetMouseButtonDown(0))
			{
				List<RaycastResult> list = new List<RaycastResult>();
				PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
				pointerEventData.position = ResidentFlow.CustomInput.mousePosition;
				EventSystem.current.RaycastAll(pointerEventData, list);
				foreach (RaycastResult item in list)
				{
					if (naki.Select(item.gameObject) >= 0)
					{
						SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
						switch (m_nakiType)
						{
						case 0:
							return await SuteChoices_SendChi(naki.Select(item.gameObject));
						case 1:
							return await SuteChoices_SendPon(naki.Select(item.gameObject));
						}
					}
				}
				continue;
			}
			int num = ((m_nakiType == 0) ? cmd.Choices.ChiPatterns.Count : cmd.Choices.PonPatterns.Count);
			for (int i = 1; i <= num; i++)
			{
				if (Bi2xInput.GetKeyDown(MahjongUtility.KeyBoard.TileKeyCodes[i - 1]))
				{
					switch (m_nakiType)
					{
					case 0:
						SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
						return await SuteChoices_SendChi(i - 1);
					case 1:
						SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
						return await SuteChoices_SendPon(i - 1);
					}
				}
			}
		}
	}

	private async UniTask<bool> End(SuteChoices cmd, bool isAgari = false)
	{
		m_mahjongPlayFlow.m_cancellationToken.ThrowIfCancellationRequested();
		await m_haiNavi[m_entryTakuInfo.PlayerIndex].Analyze(cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.TSUMOAGARI));
		_ = Players[m_entryTakuInfo.PlayerIndex].m_isReach;
		Players[m_commandClient.PlayerIndex].m_machihaiPanel.RefleshFuriten(Players[m_commandClient.PlayerIndex].m_furitenPais, !isAgari);
		m_haiNaviPanel.ReloadButtonBySelect(-1);
		NakiSelectTileSetDark(set: false);
		thinkCount.ThinkEnd();
		m_controlPanel.Destruct();
		if (isAgari)
		{
			TouchPanelTelemetry touchPanelTelemetry = TouchPanelTelemetry.Diff(Bi2xInput.Instance.GetTouchPanelTelemetry(), _startTouchPanelTelemetry);
			GameDataManager.Log.AddRonAgariData(Time.time - m_startTime, touchPanelTelemetry);
		}
		else
		{
			GameDataManager.Log.AddSuteChoicesData(Time.time - m_startTime);
		}
		await UniTask.WaitWhile(() => m_commandClient.IsBusy);
		return true;
	}
}
