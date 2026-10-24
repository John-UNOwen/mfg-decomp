using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MFG.Taikyoku.Command;
using MFG.Taikyoku.Command.Receive;
using MFG.Types;

public class ExecTsumo
{
	private MahjongPlayFlow m_mahjongPlayFlow;

	private int m_nokoriHaiNum => m_mahjongPlayFlow.m_nokoriHaiNum;

	private RestTile restTile => m_mahjongPlayFlow.m_takuInfo.restTile;

	private LastTsumo m_lastTsumo => m_mahjongPlayFlow.m_lastTsumo;

	private bool m_isKanExecuted => m_mahjongPlayFlow.m_isKanExecuted;

	private int m_isMinkanPlayerIndex => m_mahjongPlayFlow.m_minkanPlayerIndex;

	private Wanpai wanpai => m_mahjongPlayFlow.wanpai;

	private CommandClient m_commandClient => GameDataManager.CommandClient;

	private List<Player> Players => m_mahjongPlayFlow.Players;

	private SdChara m_sdCharaTsumo => m_mahjongPlayFlow.m_sdCharaTsumo;

	public async UniTask<bool> ProcessCommand(Tsumo cmd, MahjongPlayFlow mpf)
	{
		m_mahjongPlayFlow = mpf;
		m_mahjongPlayFlow.m_nokoriHaiNum--;
		m_mahjongPlayFlow.m_takuInfo.SetRestTileNum(m_nokoriHaiNum);
		m_mahjongPlayFlow._accelDoraRemainCounter.Reflesh(m_nokoriHaiNum).Forget();
		foreach (Player player in m_mahjongPlayFlow.Players)
		{
			player.m_isReachSengen = false;
		}
		m_mahjongPlayFlow.m_junmeNum++;
		if (m_isKanExecuted)
		{
			m_mahjongPlayFlow.m_junmeNum++;
			m_mahjongPlayFlow.m_actionCount++;
			foreach (Player player2 in m_mahjongPlayFlow.Players)
			{
				player2.m_isIppatsuVoice = false;
			}
		}
		m_mahjongPlayFlow.m_bombDisplay.Reflesh(m_mahjongPlayFlow.m_junmeNum, m_mahjongPlayFlow.m_actionCount, m_mahjongPlayFlow.Players.Count((Player x) => x.m_isReach));
		restTile.SetRestTileNum(m_nokoriHaiNum);
		if (m_mahjongPlayFlow.m_isFirstTsumo)
		{
			m_mahjongPlayFlow.DispoffAllTurnLine(takuWait: false);
			Players[cmd.PlayerIndex].SetActiveLine(active: true);
			Players[cmd.PlayerIndex].hand.SetNowTsumoChoice(flag: true);
			await Players[cmd.PlayerIndex].hand.Add((Pai)cmd.PaiID);
			await m_mahjongPlayFlow.HaipaiProcess();
			m_mahjongPlayFlow.SetHaiSelectEnable(!GameDataManager.Mahjong.IsHaifuPlay);
			m_mahjongPlayFlow.m_isFirstTsumo = false;
			return true;
		}
		if (m_nokoriHaiNum == 0)
		{
			await m_lastTsumo.Play(Players[cmd.PlayerIndex].m_charaType);
		}
		if (m_isKanExecuted)
		{
			wanpai.dora.SetOpenAt(m_mahjongPlayFlow.m_doraHyoujiNum++).Forget();
			Players[m_commandClient.PlayerIndex].StartDoraEffect();
			wanpai.rinshan.StartRinshanEffect();
			await Players[cmd.PlayerIndex].character.m_charaReaction.ReactionNaki(m_mahjongPlayFlow.m_isKanExecutedType, cmd.PaiID, Players[cmd.PlayerIndex], Players[m_isMinkanPlayerIndex], cmd.SeqNo);
		}
		if (MahjongUtility.AccelDora.IsNextDoraOpenTurn(m_nokoriHaiNum))
		{
			wanpai.dora.SetOpenAt(m_mahjongPlayFlow.m_doraHyoujiNum++).Forget();
			Players[m_commandClient.PlayerIndex].StartDoraEffect();
			Players[cmd.PlayerIndex].character.m_charaReaction.ReactionAccelDoraTakuNewDora();
		}
		Character.EventID id = ((m_nokoriHaiNum <= 10) ? Character.EventID.Tsumo2 : Character.EventID.Tsumo1);
		Players[cmd.PlayerIndex].character.ResetTsumohaiTiming();
		m_mahjongPlayFlow.ResetTimingFailSafeCouner();
		await Players[cmd.PlayerIndex].character.Action(id);
		m_mahjongPlayFlow.DispoffAllTurnLine(takuWait: false);
		Players[cmd.PlayerIndex].SetActiveLine(active: true);
		if (m_commandClient.PlayerIndex != cmd.PlayerIndex)
		{
			await UniTask.WaitUntil(() => Players[cmd.PlayerIndex].character.IsTsumohaiTiming() || m_mahjongPlayFlow.IsTsumoTimingFailSafe(Players[cmd.PlayerIndex].m_cha));
		}
		Players[cmd.PlayerIndex].hand.SetNowTsumoChoice(flag: true);
		await Players[cmd.PlayerIndex].hand.Add((Pai)cmd.PaiID, tsumoEffect: true, m_isKanExecuted);
		Players[m_commandClient.PlayerIndex].m_machihaiPanel.RefleshFuriten(Players[m_commandClient.PlayerIndex].m_furitenPais, isDispFuriten: true);
		if (m_commandClient.PlayerIndex == cmd.PlayerIndex)
		{
			m_sdCharaTsumo?.TsumoAnime(Players[m_commandClient.PlayerIndex].hand.GetLastItem());
		}
		if (MahjongUtility.IsDora((Pai)cmd.PaiID))
		{
			await Players[cmd.PlayerIndex].character.Action(Character.EventID.DoraTsumo);
		}
		return true;
	}
}
