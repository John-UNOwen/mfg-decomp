using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.MasterGameData;
using MFG.Taikyoku.Command.Receive;
using MFG.Types;
using UniRx;
using UnityEngine;

public class CharaReaction
{
	private List<CharaReactionData.ReactionData> m_reactionDatas;

	private Player m_player;

	private List<Player> m_players;

	private HaiNavi m_hainavi;

	private Subject<(Player, CharaReactionData.ReactionData)> m_backScreenEffect = new Subject<(Player, CharaReactionData.ReactionData)>();

	private bool m_isActive;

	private int m_tsumogiriContinueCount;

	private bool m_isNaki;

	public bool m_isReceivedOkkakeReach;

	public bool m_isIppatsuChance;

	private static bool m_isFewLeftPlayed;

	public IObservable<(Player, CharaReactionData.ReactionData)> OnBackScreenEffect => m_backScreenEffect.AsObservable();

	public CharaReactionData.ReactionID PrevReactionID { private get; set; }

	public void Init(Character character)
	{
		m_player = character.player;
		m_players = character.m_players;
		m_reactionDatas = MasterGameDataManager.Character.ReactionDatas(m_player.m_charaType).m_reactionDatas;
		PrevReactionID = CharaReactionData.ReactionID.None;
		m_isActive = true;
	}

	public void ReactionKyokuStart(KyokuStart cmd, HaiNavi hainavi, CancellationToken cancellationToken = default(CancellationToken))
	{
		m_hainavi = hainavi;
		RankType rankType = MahjongUtility.GetRankType(GameDataManager.Mahjong.TakuType, (Rank)m_player.m_rank);
		m_tsumogiriContinueCount = 0;
		m_isReceivedOkkakeReach = false;
		m_isIppatsuChance = false;
		m_isFewLeftPlayed = false;
		foreach (CharaReactionData.ReactionData reactionData in m_reactionDatas)
		{
			reactionData.executed = false;
		}
		PrevReactionID = CharaReactionData.ReactionID.None;
		if (cmd.Ba == 0 && cmd.Kyoku == 0 && cmd.Honba == 0)
		{
			AddRequest(CharaReactionData.ReactionID.TaikyokuStart);
			m_player.character.Action(Character.EventID.TaikyokuStart).Forget();
			return;
		}
		if (cmd.IsAllLast && cmd.Honba == 0)
		{
			AddRequest(CharaReactionData.ReactionID.TaikyokuStartOrasu);
			return;
		}
		switch (rankType)
		{
		case RankType.RankTypeTop:
			AddRequest(CharaReactionData.ReactionID.TaikyokuStartTop);
			break;
		case RankType.RankTypeWorst:
			AddRequest(CharaReactionData.ReactionID.TaikyokuStartBottom);
			break;
		}
	}

	public void ReactionTsumoChoice(TsumoChoices cmd)
	{
		CharaReactionManager.GetInstance().SutehaiReaction_ExecOther();
	}

	public void ReactionSutehai(Sutehai cmd, int nokoriHaiNum)
	{
		CharaReactionManager.GetInstance().TurnInit(cmd.SeqNo);
		if (cmd.IsTsumoGiri && !m_player.m_isReach)
		{
			m_tsumogiriContinueCount++;
		}
		else
		{
			m_tsumogiriContinueCount = 0;
		}
		if (cmd.IsRichi)
		{
			m_isIppatsuChance = true;
			if (m_players.Find((Player a) => a.m_isReach && a != m_player) != null)
			{
				AddRequest(CharaReactionData.ReactionID.OkkakeReach);
				VoiceManager.PlayVoiceComment6(m_player);
			}
			else
			{
				AddRequest(CharaReactionData.ReactionID.Reach);
				VoiceManager.PlayVoiceComment5(m_player);
			}
		}
		else
		{
			if (m_player.m_isReach && m_isReceivedOkkakeReach)
			{
				AddRequest(CharaReactionData.ReactionID.OkkakeReachOther);
				m_isReceivedOkkakeReach = false;
			}
			m_isIppatsuChance = false;
		}
		if (!m_player.m_isReach)
		{
			if (MahjongUtility.IsDora((Pai)cmd.PaiID))
			{
				AddRequest(CharaReactionData.ReactionID.DoraSute);
			}
			if (m_player.hand.m_isKikenhai)
			{
				AddRequest(CharaReactionData.ReactionID.KikenHai);
			}
			if (m_player.hand.m_isAnpai)
			{
				AddRequest(CharaReactionData.ReactionID.AnzenHai);
			}
			if (m_tsumogiriContinueCount == 3)
			{
				AddRequest(CharaReactionData.ReactionID.Tsumogiri3);
				m_tsumogiriContinueCount = 0;
			}
			if (cmd.IsTsumoGiri)
			{
				Tile lastTile = m_player.dump.GetLastTile();
				Tile lastBeforeTile = m_player.dump.GetLastBeforeTile();
				if (lastTile != null && lastBeforeTile != null)
				{
					HaiID haiA = lastTile.pai.ToHaiID();
					HaiID haiB = lastBeforeTile.pai.ToHaiID();
					if (m_hainavi.IsBackfired(haiA, haiB))
					{
						AddRequest(CharaReactionData.ReactionID.UrameHai);
					}
				}
			}
		}
		if (m_player.hand.GetTilesCount() == 1 && m_isNaki)
		{
			AddRequest(CharaReactionData.ReactionID.HadakaTanki);
		}
		if (cmd.PaiID == 37 && VoiceManager.IsChunBeamEnable())
		{
			AddRequest((ChunBeam)VoiceManager.GetChunBeamContinueCount() switch
			{
				ChunBeam.Single => CharaReactionData.ReactionID.TyunBeam, 
				ChunBeam.Double => CharaReactionData.ReactionID.TyunBeam2, 
				ChunBeam.Triple => CharaReactionData.ReactionID.TyunBeam3, 
				ChunBeam.Final => CharaReactionData.ReactionID.TyunBeam4, 
				_ => CharaReactionData.ReactionID.TyunBeam, 
			});
		}
		if (MahjongUtility.GetRemainingTurn(GameDataManager.Mahjong.TakuType, nokoriHaiNum) <= 3 && !m_isFewLeftPlayed)
		{
			AddRequest(CharaReactionData.ReactionID.FewLeft);
		}
		foreach (Player player in m_players)
		{
			if (player != m_player)
			{
				player.character.m_charaReaction.ReactionSutehaiOtherPlayer(cmd, m_player);
			}
		}
		CharaReactionManager.GetInstance().SutehaiReaction_Decide(cmd.PlayerIndex);
		CharaReactionManager.GetInstance().SutehaiReaction_ExecOwn();
		m_isNaki = false;
	}

	public void ReactionSutehaiOtherPlayer(Sutehai cmd, Player targetPlayer)
	{
		if (!m_isActive)
		{
			return;
		}
		if (!targetPlayer.m_isReach)
		{
			if (targetPlayer.hand.m_isKikenhai && !m_player.m_isReach)
			{
				AddRequest(CharaReactionData.ReactionID.KikenHaiOther);
			}
			if (MahjongUtility.IsDora((Pai)cmd.PaiID))
			{
				AddRequest(CharaReactionData.ReactionID.DoraSuteOther);
			}
		}
		if (cmd.IsRichi)
		{
			if (m_player.m_isReach)
			{
				m_isReceivedOkkakeReach = true;
			}
			else if (targetPlayer.dump.TileList.Count <= 6)
			{
				AddRequest(CharaReactionData.ReactionID.ReachFirst);
			}
			else
			{
				AddRequest(CharaReactionData.ReactionID.ReachOther);
			}
		}
	}

	public async UniTask ReactionNaki(NakiType type, int paiID, Player nakiPlayer, Player sutePlayer, int seqNo)
	{
		CharaReactionManager.GetInstance().TurnInit(seqNo);
		VoiceManager.ChunBeamNakiEndJudge();
		await m_hainavi.Analyze();
		if (type == NakiType.Pon || type == NakiType.Chi || type == NakiType.Minkan)
		{
			CharaReactionManager.GetInstance().ReactionVoiceRequest(m_player, VoiceManager.ReactionVoiceID.Comment1);
			if (MahjongUtility.IsDora((Pai)paiID))
			{
				AddRequest(CharaReactionData.ReactionID.NakiDora);
			}
			else if (!m_hainavi.IsYakuhai(((Pai)paiID).ToHaiID()))
			{
				AddRequest(CharaReactionData.ReactionID.Naki);
			}
			m_isNaki = true;
		}
		if (type == NakiType.Ankan || type == NakiType.Minkan || type == NakiType.Kakan)
		{
			Debug.Log($"GetLastAddDora 自家 : {MahjongUtility.GetLastAddDora()}");
			if (m_player.meld.IsContainDora(MahjongUtility.GetLastAddDora()))
			{
				AddRequest(CharaReactionData.ReactionID.NewDoraMeld);
			}
		}
		foreach (Player player in m_players)
		{
			if (player != m_player && player.character.m_charaReaction.m_isIppatsuChance)
			{
				AddRequest(CharaReactionData.ReactionID.IppatsuBlock);
			}
		}
		foreach (Player player2 in m_players)
		{
			if (player2 != m_player)
			{
				player2.character.m_charaReaction.ReactionNakiOther(type, paiID, m_player, nakiPlayer, sutePlayer);
			}
		}
		CharaReactionManager.GetInstance().TurnExec();
		foreach (Player player3 in m_players)
		{
			player3.character.m_charaReaction.m_isIppatsuChance = false;
		}
	}

	public void ReactionNakiOther(NakiType type, int paiID, Player targetPlayer, Player nakiPlayer, Player sutePlayer)
	{
		if (!m_isActive)
		{
			return;
		}
		if (type == NakiType.Pon || type == NakiType.Chi || type == NakiType.Minkan)
		{
			AddRequest(CharaReactionData.ReactionID.NakiOther);
		}
		if (type == NakiType.Pon || type == NakiType.Minkan)
		{
			bool num = MahjongUtility.GetCha(m_player.m_jikaze, sutePlayer.m_jikaze) == Cha.Kamicha;
			bool flag = m_player == nakiPlayer;
			if (num && !flag && GameDataManager.Mahjong.TakuType != TakuType.Nima)
			{
				AddRequest(CharaReactionData.ReactionID.NakiSkip);
				CharaReactionManager.GetInstance().ReactionVoiceRequest(m_player, VoiceManager.ReactionVoiceID.Comment3);
			}
		}
		if (m_player == sutePlayer)
		{
			if ((type == NakiType.Pon || type == NakiType.Minkan) && MahjongUtility.IsDora((Pai)paiID))
			{
				CharaReactionManager.GetInstance().ReactionVoiceRequest(m_player, VoiceManager.ReactionVoiceID.Comment4);
			}
			else
			{
				CharaReactionManager.GetInstance().ReactionVoiceRequest(m_player, VoiceManager.ReactionVoiceID.Comment2);
			}
		}
		if (type == NakiType.Ankan || type == NakiType.Minkan || type == NakiType.Kakan)
		{
			Debug.Log($"GetLastAddDora 他家 : {MahjongUtility.GetLastAddDora()}");
			if (m_player.meld.IsContainDora(MahjongUtility.GetLastAddDora()))
			{
				AddRequest(CharaReactionData.ReactionID.NewDoraMeld);
			}
		}
		if (m_isIppatsuChance)
		{
			AddRequest(CharaReactionData.ReactionID.IppatsuBlockOther);
		}
	}

	public void ReactionTyoukou()
	{
		CharaReactionManager.GetInstance().TurnInit();
		if (!m_player.m_isReach)
		{
			if (m_players.Find((Player a) => a.m_isReach && a != m_player) != null)
			{
				AddRequest(CharaReactionData.ReactionID.TyoukouEnemyReach);
			}
			else
			{
				AddRequest(CharaReactionData.ReactionID.Tyoukou);
			}
		}
		CharaReactionManager.GetInstance().TurnExec();
	}

	public void ReactionAccelDoraTakuNewDora()
	{
		CharaReactionManager.GetInstance().TurnInit();
		if (m_player.meld.IsContainDora(MahjongUtility.GetLastAddDora()))
		{
			AddRequest(CharaReactionData.ReactionID.NewDoraMeld);
		}
		foreach (Player player in m_players)
		{
			if (player != m_player)
			{
				player.character.m_charaReaction.ReactionAccelDoraTakuNewDoraOther();
			}
		}
		CharaReactionManager.GetInstance().TurnExec();
	}

	private void ReactionAccelDoraTakuNewDoraOther()
	{
		if (m_isActive && m_player.meld.IsContainDora(MahjongUtility.GetLastAddDora()))
		{
			AddRequest(CharaReactionData.ReactionID.NewDoraMeld);
		}
	}

	public void Reaction(CharaReactionData.ReactionID id)
	{
		CharaReactionData.ReactionData reactionData = m_reactionDatas.Find((CharaReactionData.ReactionData a) => a.ID == id);
		if (reactionData == null)
		{
			return;
		}
		bool flag = id == CharaReactionData.ReactionID.Tyoukou || id == CharaReactionData.ReactionID.TyoukouEnemyReach;
		if (MahjongUtility.GetViewType() != ViewType.Type3 || flag)
		{
			m_backScreenEffect.OnNext((m_player, reactionData));
			if (id == CharaReactionData.ReactionID.FewLeft)
			{
				m_isFewLeftPlayed = true;
			}
		}
	}

	private void AddRequest(CharaReactionData.ReactionID reactionID)
	{
		if (reactionID != CharaReactionData.ReactionID.Tyoukou && reactionID != CharaReactionData.ReactionID.TyoukouEnemyReach && PrevReactionID == reactionID)
		{
			DebugLog($" ignored PrevReactionID:{PrevReactionID}");
		}
		else
		{
			CharaReactionManager.GetInstance().ReactionRequest(m_player, reactionID);
		}
	}

	private void DebugLog(string str)
	{
		if (DebugFlag.IsActive(DebugFlag.Flag.EnableCharaActionLog))
		{
			Debug.Log("<color=magenta>" + str + "</color>");
		}
	}
}
