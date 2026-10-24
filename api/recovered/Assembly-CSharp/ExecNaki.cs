using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MFG.Taikyoku;
using MFG.Taikyoku.Command;
using MFG.Taikyoku.Command.Receive;
using MFG.Types;
using UniRx;
using UnityEngine;

public class ExecNaki
{
	private MahjongPlayFlow m_mahjongPlayFlow;

	private CommandClient m_commandClient => GameDataManager.CommandClient;

	private EntryTakuInfo m_entryTakuInfo => GameDataManager.Taikyoku.TakuInfo;

	private List<Player> Players => m_mahjongPlayFlow.Players;

	private SdChara m_sdChara => m_mahjongPlayFlow.m_sdChara;

	private StageBG m_StageBG => m_mahjongPlayFlow.m_StageBG;

	private ThinkCount thinkCount => m_mahjongPlayFlow.thinkCount;

	private HaiConjectureIconList m_haiConjectureIcon => m_mahjongPlayFlow.m_haiConjectureIcon;

	private HaiShantenIconList m_haiShantenIcon => m_mahjongPlayFlow.m_haiShantenIcon;

	private Wanpai wanpai => m_mahjongPlayFlow.wanpai;

	public async UniTask<bool> ProcessCommandPon(Pon cmd, MahjongPlayFlow mpf)
	{
		m_mahjongPlayFlow = mpf;
		m_mahjongPlayFlow.m_cancellationToken.ThrowIfCancellationRequested();
		foreach (Player player in m_mahjongPlayFlow.Players)
		{
			player.m_isReachSengen = false;
			player.m_isIppatsuVoice = false;
		}
		m_mahjongPlayFlow.m_junmeNum++;
		m_mahjongPlayFlow.m_actionCount++;
		await MahjongUtility.CpuWait(Players[cmd.PlayerIndex]);
		NakiCommonProcessBeforeCutin(cmd.PlayerIndex == m_entryTakuInfo.PlayerIndex);
		await Players[cmd.PlayerIndex].Pon(Players[cmd.SutePlayerIndex], (Pai)cmd.PonPaiID[0], (Pai)cmd.PonPaiID[1], m_mahjongPlayFlow.m_cancellationToken);
		NakiCommonProcessAfterCutin(Players[cmd.PlayerIndex]);
		await Players[cmd.PlayerIndex].character.m_charaReaction.ReactionNaki(NakiType.Pon, cmd.PaiID, Players[cmd.PlayerIndex], Players[cmd.SutePlayerIndex], cmd.SeqNo);
		m_mahjongPlayFlow._meldBonusCounter.Reflesh(Players);
		m_mahjongPlayFlow.m_bombDisplay.Reflesh(m_mahjongPlayFlow.m_junmeNum, m_mahjongPlayFlow.m_actionCount, m_mahjongPlayFlow.Players.Count((Player x) => x.m_isReach));
		if (m_mahjongPlayFlow.m_isRecNakiCutin)
		{
			await UniTask.Delay(TimeSpan.FromSeconds(5.0), ignoreTimeScale: false, PlayerLoopTiming.Update, m_mahjongPlayFlow.m_cancellationToken);
			m_mahjongPlayFlow.DebugRecordEnd();
		}
		return true;
	}

	public async UniTask<bool> ProcessCommandChi(Chi cmd, MahjongPlayFlow mpf)
	{
		m_mahjongPlayFlow = mpf;
		m_mahjongPlayFlow.m_cancellationToken.ThrowIfCancellationRequested();
		foreach (Player player in m_mahjongPlayFlow.Players)
		{
			player.m_isReachSengen = false;
			player.m_isIppatsuVoice = false;
		}
		m_mahjongPlayFlow.m_junmeNum++;
		m_mahjongPlayFlow.m_actionCount++;
		await MahjongUtility.CpuWait(Players[cmd.PlayerIndex]);
		NakiCommonProcessBeforeCutin(cmd.PlayerIndex == m_entryTakuInfo.PlayerIndex);
		await Players[cmd.PlayerIndex].Chi(Players[cmd.SutePlayerIndex], (Pai)cmd.ChiPaiID[0], (Pai)cmd.ChiPaiID[1], m_mahjongPlayFlow.m_cancellationToken);
		NakiCommonProcessAfterCutin(Players[cmd.PlayerIndex]);
		await Players[cmd.PlayerIndex].character.m_charaReaction.ReactionNaki(NakiType.Chi, cmd.PaiID, Players[cmd.PlayerIndex], Players[cmd.SutePlayerIndex], cmd.SeqNo);
		m_mahjongPlayFlow._meldBonusCounter.Reflesh(Players);
		m_mahjongPlayFlow.m_bombDisplay.Reflesh(m_mahjongPlayFlow.m_junmeNum, m_mahjongPlayFlow.m_actionCount, m_mahjongPlayFlow.Players.Count((Player x) => x.m_isReach));
		return true;
	}

	public async UniTask<bool> ProcessCommandAnkan(Ankan cmd, MahjongPlayFlow mpf)
	{
		Debug.Log("ProcessCommandAnkan/" + cmd.PaiID);
		m_mahjongPlayFlow = mpf;
		m_mahjongPlayFlow.m_cancellationToken.ThrowIfCancellationRequested();
		m_mahjongPlayFlow.m_isKanExecutedType = NakiType.Ankan;
		await MahjongUtility.CpuWait(Players[cmd.PlayerIndex]);
		NakiCommonProcessBeforeCutin(cmd.PlayerIndex == m_entryTakuInfo.PlayerIndex);
		Players[cmd.PlayerIndex].hand.SetNowTsumoChoice(flag: false);
		await Players[cmd.PlayerIndex].Ankan((Pai)cmd.PaiID, m_mahjongPlayFlow.m_cancellationToken);
		NakiCommonProcessAfterCutin(Players[cmd.PlayerIndex], isKan: true);
		m_mahjongPlayFlow._meldBonusCounter.Reflesh(Players);
		return true;
	}

	public async UniTask<bool> ProcessCommandMinkan(Minkan cmd, MahjongPlayFlow mpf)
	{
		m_mahjongPlayFlow = mpf;
		m_mahjongPlayFlow.m_cancellationToken.ThrowIfCancellationRequested();
		m_mahjongPlayFlow.m_isKanExecutedType = NakiType.Minkan;
		foreach (Player player in m_mahjongPlayFlow.Players)
		{
			player.m_isIppatsuVoice = false;
		}
		await MahjongUtility.CpuWait(Players[cmd.PlayerIndex]);
		NakiCommonProcessBeforeCutin(cmd.PlayerIndex == m_entryTakuInfo.PlayerIndex);
		await Players[cmd.PlayerIndex].Minkan(Players[cmd.SutePlayerIndex], (Pai)cmd.PaiID, m_mahjongPlayFlow.m_cancellationToken);
		m_mahjongPlayFlow.m_minkanPlayerIndex = cmd.SutePlayerIndex;
		NakiCommonProcessAfterCutin(Players[cmd.PlayerIndex], isKan: true);
		m_mahjongPlayFlow._meldBonusCounter.Reflesh(Players);
		return true;
	}

	public async UniTask<bool> ProcessCommandKakan(Kakan cmd, MahjongPlayFlow mpf)
	{
		m_mahjongPlayFlow = mpf;
		m_mahjongPlayFlow.m_cancellationToken.ThrowIfCancellationRequested();
		m_mahjongPlayFlow.m_isKanExecutedType = NakiType.Kakan;
		await MahjongUtility.CpuWait(Players[cmd.PlayerIndex]);
		NakiCommonProcessBeforeCutin(cmd.PlayerIndex == m_entryTakuInfo.PlayerIndex);
		await Players[cmd.PlayerIndex].Kakan((Pai)cmd.PaiID, m_mahjongPlayFlow.m_cancellationToken);
		Players[m_commandClient.PlayerIndex].m_furitenPais.Add((Pai)cmd.PaiID);
		foreach (Player player in Players)
		{
			player.SetIsOpenFuritenInMinogashi((Pai)cmd.PaiID);
		}
		NakiCommonProcessAfterCutin(Players[cmd.PlayerIndex], isKan: true);
		return true;
	}

	private void NakiCommonProcessBeforeCutin(bool isJicha)
	{
		m_StageBG.StageState = StageState.Excite;
		Observable.Timer(TimeSpan.FromSeconds(3.0)).Subscribe(delegate
		{
			m_StageBG.StageState = ((!Players.Any((Player x) => x.m_isReach)) ? StageState.Start : StageState.Reach);
		}).AddTo(m_mahjongPlayFlow.m_cancellationToken);
		m_haiConjectureIcon.SetIconsDispOff();
		m_haiShantenIcon.SetHaiShantenIconDispOff();
		if (isJicha)
		{
			m_sdChara?.DispOffAnime();
		}
	}

	private void NakiCommonProcessAfterCutin(Player player, bool isKan = false)
	{
		m_mahjongPlayFlow.DispoffAllTurnLine(takuWait: false);
		player.SetActiveLine(active: true);
		m_mahjongPlayFlow.StartDoraEffect();
		thinkCount.ThinkingTyoukouStart(player);
		if (isKan)
		{
			m_mahjongPlayFlow.m_isKanExecuted = true;
		}
	}
}
