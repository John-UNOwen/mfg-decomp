using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MFG.Taikyoku;
using MFG.Taikyoku.Command;
using MFG.Taikyoku.Command.Receive;
using MFG.Types;
using UnityEngine;

public class ExecSutehai
{
	private MahjongPlayFlow m_mahjongPlayFlow;

	private EntryTakuInfo m_entryTakuInfo => GameDataManager.Taikyoku.TakuInfo;

	private int m_nokoriHaiNum => m_mahjongPlayFlow.m_nokoriHaiNum;

	private LastTsumo m_lastTsumo => m_mahjongPlayFlow.m_lastTsumo;

	private Wanpai wanpai => m_mahjongPlayFlow.wanpai;

	private CommandClient m_commandClient => GameDataManager.CommandClient;

	private List<Player> Players => m_mahjongPlayFlow.Players;

	private ThinkCount thinkCount => m_mahjongPlayFlow.thinkCount;

	private List<HaiNavi> m_haiNavi => m_mahjongPlayFlow.m_haiNavi;

	private HaiConjecture m_haiConjecture => m_mahjongPlayFlow.m_haiConjecture;

	public async UniTask<bool> ProcessCommand(Sutehai cmd, MahjongPlayFlow mpf)
	{
		m_mahjongPlayFlow = mpf;
		m_mahjongPlayFlow.m_cancellationToken.ThrowIfCancellationRequested();
		if (m_commandClient.PlayerIndex == cmd.PlayerIndex && !GameDataManager.Mahjong.IsHaifuPlay)
		{
			Player player = Players[cmd.PlayerIndex];
			thinkCount.ThinkingTyoukouEnd();
			if (m_haiNavi[m_entryTakuInfo.PlayerIndex].CurShanten > 0)
			{
				Players[m_commandClient.PlayerIndex].StopTenpaiEffect();
			}
			player.character.m_charaReaction.ReactionSutehai(cmd, m_nokoriHaiNum);
			if (cmd.IsRichi)
			{
				Players[m_commandClient.PlayerIndex].m_furitenPais.Clear();
			}
			_ = player.hand.m_isKikenhai;
			return true;
		}
		if (Players[m_commandClient.PlayerIndex].m_isFireReach)
		{
			Players[m_commandClient.PlayerIndex].m_isFireReachUsed = true;
		}
		else if (Players[m_commandClient.PlayerIndex].m_isOpenReach)
		{
			Players[m_commandClient.PlayerIndex].m_isOpenReachUsed = true;
		}
		await MahjongUtility.CpuWait(Players[cmd.PlayerIndex], isSute: true);
		thinkCount.ThinkingTyoukouEnd();
		await m_mahjongPlayFlow.DiscardProcess(cmd.PlayerIndex, cmd.IsRichi, cmd.IsTsumoGiri, null, (Pai)cmd.PaiID, cmd.IsFireRichi, cmd.IsOpenRichi, cmd.IsAtomicRichi);
		Players[cmd.PlayerIndex].hand.SetNowTsumoChoice(flag: false);
		await Players[cmd.PlayerIndex].hand.Discard(cmd.IsRichi);
		if (Players[cmd.PlayerIndex].m_isOpenReach)
		{
			Player p = Players[cmd.PlayerIndex];
			List<Pai> openMachiPais = p.m_openMachiPais;
			if (openMachiPais == null || openMachiPais.Count <= 0)
			{
				HaiNavi haiNavi = m_haiNavi[cmd.PlayerIndex];
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
		Players[m_commandClient.PlayerIndex].m_furitenPais.Add((Pai)cmd.PaiID);
		foreach (Player player2 in Players)
		{
			player2.SetIsOpenFuritenInMinogashi((Pai)cmd.PaiID);
		}
		Players[m_commandClient.PlayerIndex].m_machihaiPanel.RefleshMachihaiNum();
		if (MahjongUtility.IsDora((Pai)cmd.PaiID))
		{
			await Players[cmd.PlayerIndex].character.Action(Character.EventID.DoraDiscard);
		}
		await m_haiNavi[cmd.PlayerIndex].Analyze();
		if (GameDataManager.Mahjong.IsHaifuPlay && m_commandClient.PlayerIndex == cmd.PlayerIndex)
		{
			m_mahjongPlayFlow.m_haiNaviPanel.Reload(m_haiNavi[cmd.PlayerIndex].m_yakuInfo, m_haiNavi[cmd.PlayerIndex].m_oneByOneDiscardedYakuPossibilityInfos, Players[cmd.PlayerIndex].m_isFireReach, Players[cmd.PlayerIndex].m_isOpenReach, Players[cmd.PlayerIndex].m_isAtomicReach, Players[cmd.PlayerIndex].m_isNaki).Forget();
			Players[cmd.PlayerIndex].hand.TenpaiTargetIconSetDisp(isDisp: false);
			Players[cmd.PlayerIndex].hand.OpenHoujuTargetIconSetDisp(isDisp: false);
			Players[cmd.PlayerIndex].hand.m_isTenpaiTargetIconDispTiming = true;
			Players[cmd.PlayerIndex].hand.m_isOpenTargetIconDispTiming = true;
			if (m_haiNavi[cmd.PlayerIndex].CurShanten == 0)
			{
				bool isRestart = cmd.IsRichi || !Players[cmd.PlayerIndex].m_isReach;
				Players[cmd.PlayerIndex].StartTenpaiEffect(m_haiNavi[cmd.PlayerIndex].GetHan() >= Han.Mangan, isRestart);
			}
		}
		m_mahjongPlayFlow.DebugMachiDisp(DebugFlag.IsActive(DebugFlag.Flag.DispMachiHai)).Forget();
		Players[cmd.PlayerIndex].character.m_charaReaction.ReactionSutehai(cmd, m_nokoriHaiNum);
		_ = Players[cmd.PlayerIndex].hand.m_isKikenhai;
		if (Players[cmd.PlayerIndex].IsProAnimEnable())
		{
			Players[cmd.PlayerIndex].m_proStand.GetComponent<Animator>().SetTrigger("Out");
		}
		Debug.Log($"ExecSutehai {cmd.PlayerIndex} Finished");
		return true;
	}
}
