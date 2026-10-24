using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.MahjongPlay.Character;
using MFG.Taikyoku;
using MFG.Taikyoku.Command;
using MFG.Taikyoku.Command.Receive;
using MFG.Taikyoku.Command.Send;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;
using UnityEngine;
using UnityEngine.UI;

public class ExecKyoku
{
	private MahjongPlayFlow m_mahjongPlayFlow;

	private Tween m_pleaseWaitTween;

	private EntryTakuInfo m_entryTakuInfo => GameDataManager.Taikyoku.TakuInfo;

	private int m_nokoriHaiNum => m_mahjongPlayFlow.m_nokoriHaiNum;

	private Wanpai wanpai => m_mahjongPlayFlow.wanpai;

	private CommandClient m_commandClient => GameDataManager.CommandClient;

	private List<Player> Players => m_mahjongPlayFlow.Players;

	private List<HaiNavi> m_haiNavi => m_mahjongPlayFlow.m_haiNavi;

	private HaiConjecture m_haiConjecture => m_mahjongPlayFlow.m_haiConjecture;

	private ThinkCount thinkCount => m_mahjongPlayFlow.thinkCount;

	private StageBG m_StageBG => m_mahjongPlayFlow.m_StageBG;

	private int m_doraHyoujiNum => m_mahjongPlayFlow.m_doraHyoujiNum;

	private KyokuCall m_kyokuCall => m_mahjongPlayFlow.m_kyokuCall;

	private HaiNaviPanel m_haiNaviPanel => m_mahjongPlayFlow.m_haiNaviPanel;

	private Button m_stampButton => m_mahjongPlayFlow.m_stampButton;

	private Help m_help => m_mahjongPlayFlow.m_help;

	private GameObject m_kyokuTransition => m_mahjongPlayFlow.m_kyokuTransition;

	private bool m_firstAllLast => m_mahjongPlayFlow.m_firstAllLast;

	private bool m_firstKyoku => m_mahjongPlayFlow.m_firstKyoku;

	private List<MachihaiPanel> machihaiPanels => m_mahjongPlayFlow.machihaiPanels;

	public async UniTask<bool> ProcessCommandTyoko(TyoKo cmd, MahjongPlayFlow mpf)
	{
		m_mahjongPlayFlow = mpf;
		m_mahjongPlayFlow.m_cancellationToken.ThrowIfCancellationRequested();
		if (cmd.PlayerIndex != 0)
		{
			thinkCount.m_tyoukouPlayers[(int)Players[cmd.PlayerIndex].m_cha] = true;
		}
		return true;
	}

	public async UniTask<bool> ProcessCommandKyokuStart(KyokuStart cmd, MahjongPlayFlow mpf)
	{
		m_mahjongPlayFlow = mpf;
		m_mahjongPlayFlow.m_cancellationToken.ThrowIfCancellationRequested();
		CancellationToken token = m_mahjongPlayFlow.m_cancellationToken;
		m_mahjongPlayFlow.DestructAll();
		MahjongUtility.ResetOpenPaiNum();
		MahjongUtility.m_StartScore = cmd.Players[0].Score;
		if (m_firstKyoku)
		{
			GameDataManager.Mission.StartScore = cmd.Players[0].Score;
		}
		m_StageBG.StageState = StageState.Start;
		m_mahjongPlayFlow.m_cutinManager.LatestReachBgmID = ItemID.None;
		if (m_pleaseWaitTween != null)
		{
			m_pleaseWaitTween.Kill();
			m_pleaseWaitTween = null;
			m_mahjongPlayFlow.pleaseWait.SetActive(value: false);
		}
		if (!m_mahjongPlayFlow.m_firstKyoku)
		{
			m_kyokuTransition.GetComponent<Animator>().ResetTrigger("Out");
			m_kyokuTransition.GetComponent<Animator>().SetTrigger("In");
			SoundManager.PlaySe("SFX_MJG_DISPLAY12").Forget();
			await UniTask.Delay(500, ignoreTimeScale: false, PlayerLoopTiming.Update, token);
		}
		for (int i = 0; i < MahjongUtility.GetPlayerNum(GameDataManager.Mahjong.TakuType); i++)
		{
			await PlayerInit(i, cmd, token);
		}
		m_mahjongPlayFlow.CalculateDiffScore();
		foreach (Player player in Players)
		{
			player.RefleshScoreDisp();
			m_mahjongPlayFlow.m_openMachiList = new List<Pai>();
			player.m_openMachiPais = new List<Pai>();
			player.m_machihaiPanel.OpenMachiDisp(isDisp: false, null);
		}
		MahjongUtility.Aotenjo.AgariInfos = new List<MahjongUtility.AotenjoAgariInfo>();
		m_mahjongPlayFlow.m_bombDisplay.KyokuInit(Players);
		m_mahjongPlayFlow.m_cutinManager.PlayerInteractorsResetReachStick();
		m_kyokuTransition.GetComponent<Animator>().SetTrigger("Out");
		m_mahjongPlayFlow.ViewType_Change(MahjongUtility.GetViewType());
		int num = (GameDataManager.Mahjong.IsAccelDoraMode ? 9 : 5);
		for (int j = 0; j < num; j++)
		{
			wanpai.dora.Add((Pai)cmd.OmoteDoraData[j]);
			wanpai.uradora.Add((Pai)cmd.UraDoraData[j]);
		}
		MahjongUtility.ResetDora();
		m_mahjongPlayFlow.m_doraHyoujiNum = cmd.OpenDoraNum;
		Debug.Log($"OpenDoraNum:{cmd.OpenDoraNum}");
		for (int k = 0; k < m_doraHyoujiNum; k++)
		{
			wanpai.dora.SetOpenAt(k, isPlayOpenSe: false).Forget();
		}
		m_mahjongPlayFlow.m_nokoriHaiNum = cmd.Nokori;
		m_mahjongPlayFlow.m_junmeNum = 0;
		m_mahjongPlayFlow.m_actionCount = 0;
		m_mahjongPlayFlow.m_takuInfo.KyokuInit(cmd, m_nokoriHaiNum);
		m_mahjongPlayFlow._accelDoraRemainCounter.Reflesh(m_nokoriHaiNum).Forget();
		GameDataManager.EventTaku.OnKyokuInit(Players);
		m_mahjongPlayFlow.DaisouFinish();
		PlayerData myPlayer = cmd.Players[m_commandClient.PlayerIndex];
		await m_kyokuCall.Play(cmd.IsAllLast, cmd.Ba, cmd.Kyoku, cmd.Honba, (Kaze)myPlayer.Jikaze, (Rank)myPlayer.Rank, m_mahjongPlayFlow.MusicText, token);
		foreach (Player player2 in Players)
		{
			int index = Players.IndexOf(player2);
			for (int l = 0; l < 13; l++)
			{
				player2.hand.HaipaiAdd((Pai)cmd.Players[index].TePai[l], l);
			}
			player2.hand.m_isReach = false;
		}
		if (myPlayer.Jikaze == 0 && GameDataManager.PlayerConfig.IsTaikyokuSupport)
		{
			await m_kyokuCall.OyaTurnEffect();
		}
		m_haiConjecture.Reset();
		VoiceManager.KyokuInit();
		if (m_firstKyoku)
		{
			VoiceManager.SetMahjongStartKind(VoiceManager.VoiceMahjongStartKind.Tong);
		}
		else if (cmd.IsAllLast && m_firstAllLast)
		{
			Rank rank = (Rank)Players[m_commandClient.PlayerIndex].m_rank;
			if ((GameDataManager.Mahjong.TakuType == TakuType.Tonpu || GameDataManager.Mahjong.TakuType == TakuType.Hanchan) && rank == Rank.Rank3)
			{
				VoiceManager.SetMahjongStartKind(VoiceManager.VoiceMahjongStartKind.Rank3);
			}
			else
			{
				switch (MahjongUtility.GetRankType(GameDataManager.Mahjong.TakuType, rank))
				{
				case RankType.RankTypeTop:
					VoiceManager.SetMahjongStartKind(VoiceManager.VoiceMahjongStartKind.Rank1);
					break;
				case RankType.RankTypeWorst:
					VoiceManager.SetMahjongStartKind(VoiceManager.VoiceMahjongStartKind.Rank4);
					break;
				case RankType.RankTypeOther:
					VoiceManager.SetMahjongStartKind(VoiceManager.VoiceMahjongStartKind.Rank2);
					break;
				}
			}
			m_mahjongPlayFlow.m_firstAllLast = false;
		}
		await m_mahjongPlayFlow.InitVariables(cmd, token);
		m_mahjongPlayFlow.DebugMachiDisp(DebugFlag.IsActive(DebugFlag.Flag.DispMachiHai)).Forget();
		await UniTask.WaitWhile(() => m_haiNaviPanel.IsPlaying, PlayerLoopTiming.Update, token);
		m_help.IconInOption();
		m_help.IconInSyanten();
		m_mahjongPlayFlow.m_isFirstTsumo = true;
		m_mahjongPlayFlow.m_firstKyoku = false;
		GameDataManager.Mission.FinishBa = cmd.Ba;
		GameDataManager.Mission.FinishKyoku = cmd.Kyoku;
		return true;
	}

	private async UniTask PlayerInit(int i, KyokuStart cmd, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		Player player = Players[i];
		if (m_firstKyoku)
		{
			player.SetStartKaze((Kaze)cmd.Players[i].Jikaze);
			if (player.IsJicha())
			{
				MahjongUtility.m_jichaStartKaze = player.m_startKaze;
			}
		}
		await player.character.KyokuInit();
		player.SetJikaze((Kaze)cmd.Players[i].Jikaze);
		player.SetScore(cmd.Players[i].Score);
		player.SetRank(cmd.Players[i].Rank);
		MotionID motionID = MotionID.Wait0;
		player.character.m_characterModelController.PlayMotion(motionID, cancellationToken).Forget();
	}

	public async UniTask<bool> ProcessCommandKyokuEnd(KyokuEnd cmd, MahjongPlayFlow mpf)
	{
		m_mahjongPlayFlow = mpf;
		m_mahjongPlayFlow.m_cancellationToken.ThrowIfCancellationRequested();
		if (cmd.IsEnd)
		{
			foreach (Player player in Players)
			{
				player.character.m_characterModelController.PlayMotion(MotionID.Wait0).Forget();
			}
			m_kyokuTransition.GetComponent<Animator>().SetTrigger("Out");
			m_mahjongPlayFlow.m_isTaikyokuActive = false;
		}
		else
		{
			foreach (Player player2 in Players)
			{
				if (player2.IsProAnimEnable())
				{
					player2.m_proStand.GetComponent<Animator>().ResetTrigger("Out");
					if (player2.m_proStand.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("ProStandLoop"))
					{
						player2.m_proStand.GetComponent<Animator>().SetTrigger("Out");
					}
				}
			}
			m_commandClient.Send(new NextKyokuReady()).Forget();
			m_mahjongPlayFlow.pleaseWait.SetActive(value: true);
			m_pleaseWaitTween = DOVirtual.DelayedCall(2f, delegate
			{
				if (m_mahjongPlayFlow.pleaseWait.activeSelf)
				{
					m_mahjongPlayFlow.pleaseWait.GetComponent<Animator>().SetTrigger("In");
				}
			}, ignoreTimeScale: false);
			await UniTask.WaitWhile(() => m_commandClient.IsBusy);
		}
		return true;
	}

	public async UniTask<bool> ProcessCommandScoreRank(ScoreRank cmd, MahjongPlayFlow mpf)
	{
		m_mahjongPlayFlow = mpf;
		m_mahjongPlayFlow.m_cancellationToken.ThrowIfCancellationRequested();
		m_mahjongPlayFlow.m_takuInfo.SetReachNum(cmd.Kyotaku);
		foreach (Player player in Players)
		{
			int index = Players.IndexOf(player);
			player.SetScore(cmd.RichiAfterDatas[index].score);
			player.SetRank(cmd.RichiAfterDatas[index].rank);
		}
		m_mahjongPlayFlow.CalculateDiffScore();
		foreach (Player player2 in Players)
		{
			player2.RefleshScoreDisp();
		}
		return true;
	}
}
