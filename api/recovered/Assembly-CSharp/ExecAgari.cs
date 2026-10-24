using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.MahjongPlay.Cutin;
using MFG.MasterGameData;
using MFG.Taikyoku.Command;
using MFG.Taikyoku.Command.Receive;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class ExecAgari
{
	private MahjongPlayFlow m_mahjongPlayFlow;

	private bool[] rons = new bool[Enum.GetNames(typeof(MFG.Types.Player)).Length];

	private bool m_uradoraOpen;

	private CommandClient m_commandClient => GameDataManager.CommandClient;

	private List<Player> Players => m_mahjongPlayFlow.Players;

	private StageBG m_StageBG => m_mahjongPlayFlow.m_StageBG;

	private ThinkCount thinkCount => m_mahjongPlayFlow.thinkCount;

	private Wanpai wanpai => m_mahjongPlayFlow.wanpai;

	private HaiConjectureIconList m_haiConjectureIcon => m_mahjongPlayFlow.m_haiConjectureIcon;

	private HaiShantenIconList m_haiShantenIcon => m_mahjongPlayFlow.m_haiShantenIcon;

	private HaiShantenCounter m_haiShantenCounter => m_mahjongPlayFlow.m_haiShantenCounter;

	private Agari agari => m_mahjongPlayFlow.agari;

	private ScorePanel m_scorePanel => m_mahjongPlayFlow.m_scorePanel;

	private PlayableDirector m_ryukyokuCall => m_mahjongPlayFlow.m_ryukyokuCall;

	private GameObject agariEffect => m_mahjongPlayFlow.agariEffect;

	private GameObject agariManganEffect => m_mahjongPlayFlow.agariManganEffect;

	private Shake shake => m_mahjongPlayFlow.shake;

	private SdChara m_sdChara => m_mahjongPlayFlow.m_sdChara;

	private KyokuCall m_kyokuCall => m_mahjongPlayFlow.m_kyokuCall;

	private CutinManager m_cutinManager => m_mahjongPlayFlow.m_cutinManager;

	private GameObject m_maAgari => m_mahjongPlayFlow.m_maAgari;

	private List<GameObject> m_indicateAgaris => m_mahjongPlayFlow.m_indicateAgaris;

	private Sprite m_indicateAgariSpriteTsumo => m_mahjongPlayFlow.m_indicateAgariSpriteTsumo;

	private Sprite m_indicateAgariSpriteRon => m_mahjongPlayFlow.m_indicateAgariSpriteRon;

	public async UniTask<bool> ProcessCommandTsumoAgari(TsumoAgari cmd, MahjongPlayFlow mpf)
	{
		m_mahjongPlayFlow = mpf;
		CancellationToken token = m_mahjongPlayFlow.m_cancellationToken;
		token.ThrowIfCancellationRequested();
		ScorePanelSoundUtility.SetScoreMoveCommandRandomSeed(cmd.SeqNo + GameDataManager.Taikyoku.TakuInfo.TakuID);
		await MahjongUtility.CpuWait(Players[cmd.PlayerIndex]);
		thinkCount.ThinkingTyoukouEnd();
		Players[cmd.PlayerIndex].hand.TenpaiTargetIconSetDisp(isDisp: false);
		Players[cmd.PlayerIndex].hand.OpenHoujuTargetIconSetDisp(isDisp: false);
		m_StageBG.StageState = StageState.Excite;
		m_mahjongPlayFlow.SetHaiSelectEnable(enable: false);
		await Players[m_commandClient.PlayerIndex].hand.SelectReset(m_sdChara);
		m_haiConjectureIcon.SetIconsDispOff();
		m_haiShantenCounter.SetShantenCountDispOff();
		m_haiShantenIcon.SetHaiShantenIconDispOff();
		m_mahjongPlayFlow._accelDoraRemainCounter.Reflesh(0, isDisp: false).Forget();
		m_mahjongPlayFlow.m_bombDisplay.SetActive(isActive: false);
		m_mahjongPlayFlow._meldBonusCounter.Reflesh(Players, isDisp: false);
		if (m_cutinManager.IsPlaying)
		{
			await UniTask.WaitWhile(() => m_cutinManager.IsPlaying, PlayerLoopTiming.Update, token);
		}
		SoundManager.StopBgmWithFadeout().Forget();
		m_indicateAgaris[(int)Players[cmd.PlayerIndex].m_cha].GetComponent<Animator>().SetTrigger("In");
		m_indicateAgaris[(int)Players[cmd.PlayerIndex].m_cha].transform.Find("Text").GetComponent<Image>().sprite = m_indicateAgariSpriteTsumo;
		m_indicateAgaris[(int)Players[cmd.PlayerIndex].m_cha].transform.Find("Text").Find("Add").GetComponent<Image>()
			.sprite = m_indicateAgariSpriteTsumo;
		SoundManager.PlaySeCha("SFX_MJG_RONTSUMO1", Players[cmd.PlayerIndex].m_cha, 1f).Forget();
		SoundManager.PlaySeCha("SFX_MJG_RONTSUMO2", Players[cmd.PlayerIndex].m_cha).Forget();
		if (Players[cmd.PlayerIndex].m_charaType == CharaType.Chara18 && Players[cmd.PlayerIndex].m_isIppatsuVoice)
		{
			Debug.Log("<color=yellow>イッパツツモ</color>");
			SoundManager.PlayVoiceCha("VOX_G18_MJG_TSUMO_B", Players[cmd.PlayerIndex].m_charaType, Players[cmd.PlayerIndex].m_cha).Forget();
		}
		else
		{
			SoundManager.PlayVoiceCha("VOX_GXX_MJG_TSUMO", Players[cmd.PlayerIndex].m_charaType, Players[cmd.PlayerIndex].m_cha).Forget();
		}
		if (Players[cmd.PlayerIndex].IsJicha())
		{
			LEDManager.Instance.AnimColor(new Color[3]
			{
				new Color(1f, 10f / 51f, 14f / 51f),
				new Color(1f, 40f / 51f, 0f),
				new Color(0.23529412f, 0.8235294f, 11f / 51f)
			}, isSpread: true, 2);
		}
		if (SystemConfigGameData.IsDualMonitor)
		{
			await SubMonitorFlow.GetInstance().AgariOpen();
		}
		m_mahjongPlayFlow.DaisouFinish();
		foreach (Player player in Players)
		{
			if (Players.IndexOf(player) == cmd.PlayerIndex)
			{
				player.character.AgariAction(Character.EventID.Result1).Forget();
			}
			else
			{
				player.character.AgariAction(Character.EventID.Result2).Forget();
			}
		}
		PlayReachBGMJingle(cmd.PlayerIndex);
		await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
		Players[cmd.PlayerIndex].SetTenpai();
		bool isMangan = cmd.Yaku.HanID >= 5;
		AgariEffectStart(isMangan).Forget();
		if (MahjongUtility.IsReachAgari(cmd.Yaku))
		{
			await wanpai.OpenUradora(cmd.OpenDoraNum);
			Players[cmd.PlayerIndex].StartAgariDoraEffect();
		}
		await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
		foreach (Player player2 in Players)
		{
			if (player2 != Players[cmd.PlayerIndex])
			{
				player2.character.Action(Character.EventID.ReceiveAgari).Forget();
			}
		}
		if (Players[cmd.PlayerIndex].IsProAnimEnable())
		{
			m_maAgari.GetComponent<Animator>().SetTrigger("In");
			m_maAgari.transform.Find("MaImage").GetComponent<Image>().sprite = Players[cmd.PlayerIndex].m_maAgariSprite;
			await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
		}
		List<Yaku> yakus = MahjongUtility.CreateYakusFromYakuBit(cmd.Yaku.YakuBit);
		int num = (GameDataManager.Mahjong.IsComebackMode ? Players[cmd.PlayerIndex].m_comebackTakuBonusDisp.bonusHan : 0);
		int num2 = (GameDataManager.Mahjong.IsMeldBonusMode ? MahjongUtility.MeldBonus.MeldCount(Players) : 0);
		int allgreenBonusHan = (GameDataManager.Mahjong.IsAllGreenBonusMode ? (cmd.Yaku.BonusHan - num2 - num) : 0);
		Cha bombCha = m_mahjongPlayFlow.m_bombDisplay.GetCurrentBombCha(m_mahjongPlayFlow.m_junmeNum, m_mahjongPlayFlow.m_actionCount, Players.Count((Player x) => x.m_isReach && !x.m_isReachSengen));
		foreach (Player player3 in Players)
		{
			player3.m_isBomber = player3.m_cha == bombCha;
		}
		Debug.Log($"pckBonus {num2} /  comebackBonus {num}");
		Players[cmd.PlayerIndex].Tsumo((Han)cmd.Yaku.HanID, cmd.Yaku.DoraNum, num2, num, allgreenBonusHan, yakus, MahjongUtility.Kirisame.AkadoraCount(cmd.Yaku.TePai, cmd.Yaku.PaiID), bombCha, token).Forget();
		await m_cutinManager.AgariCutin_WaitIntroductionEnd(token);
		if (GameDataManager.Mahjong.IsHaifuPlay)
		{
			GameDataManager.Mahjong.IsHaifuPlayToExit = true;
			await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
		}
		m_scorePanel.BgCameraSetActive(isActive: false);
		await agari.SetAgariTsumo(cmd, Players, Players[cmd.PlayerIndex], wanpai, m_mahjongPlayFlow.m_bombDisplay.GetKaze(), token);
		m_cutinManager.CurrentCutin_End(3f, token).Forget();
		if (GameDataManager.Mahjong.IsAotenjoMode)
		{
			MahjongUtility.Aotenjo.AgariInfos.Add(new MahjongUtility.AotenjoAgariInfo
			{
				Cha = Players[cmd.PlayerIndex].m_index,
				HanNum = cmd.Yaku.HanNum,
				FuNum = cmd.Yaku.FuNum,
				Kaze = Players[cmd.PlayerIndex].m_jikaze,
				Hoju = -1
			});
		}
		foreach (Player player4 in Players)
		{
			int num3 = Players.IndexOf(player4);
			Han han = ((cmd.PlayerIndex == num3) ? ((Han)cmd.Yaku.HanID) : Han.Nothing);
			m_scorePanel.SetScore(cmd.Scores[num3], Players, player4, han, m_kyokuCall.m_isOrasu, isRyukyoku: false, han == Han.Yakuman && yakus.Contains(Yaku.Kazoeyakuman), bombCha);
		}
		if (Players[cmd.PlayerIndex].IsJicha())
		{
			VoiceManager.SetMahjongStartKind(VoiceManager.VoiceMahjongStartKind.Agari1);
		}
		else
		{
			VoiceManager.SetMahjongStartKind(VoiceManager.VoiceMahjongStartKind.Agari2);
		}
		await m_scorePanel.Play();
		foreach (Player player5 in Players)
		{
			int index = Players.IndexOf(player5);
			player5.SetScore(cmd.Scores[index].NewScore);
		}
		SetRank();
		GameDataManager.Mission.LatestAgariCha = Players[cmd.PlayerIndex].m_cha;
		var (num4, num5, num6, num7) = FanMaster.FansOnTsumo(mpf, cmd);
		GameDataManager.Mahjong.Fans.luck += (float)num4 * 0.8f;
		GameDataManager.Mahjong.Fans.speed += (float)num5 * 0.8f;
		GameDataManager.Mahjong.Fans.attack += (float)num6 * 0.8f;
		GameDataManager.Mahjong.Fans.defence += (float)num7 * 0.8f;
		GameDataManager.EventTaku.OnTsumoAgari(cmd, Players);
		LEDManager.Instance.AnimColor(new Color[1] { Color.white }, isSpread: true);
		if (SystemConfigGameData.IsDualMonitor)
		{
			await SubMonitorFlow.GetInstance().AgariClose();
		}
		return true;
	}

	private void PlayReachBGMJingle(int playerIndex)
	{
		ItemID latestReachBgmID = m_mahjongPlayFlow.m_cutinManager.LatestReachBgmID;
		if (Players[playerIndex].m_isReach && Players[playerIndex].m_cha == m_mahjongPlayFlow.m_lastReachedCha && latestReachBgmID != 0)
		{
			bool flag = latestReachBgmID == ItemID.ReachBgm001;
			string text = ItemIDExtentions.ReachBgmCustomJingle(latestReachBgmID);
			if (flag)
			{
				SoundManager.PlayBgm("BGM_REACH3").Forget();
				return;
			}
			if (!string.IsNullOrEmpty(text))
			{
				SoundManager.PlayBgm(text).Forget();
				return;
			}
			CharaType type = latestReachBgmID.RreachBgmToCharaType();
			SoundManager.PlayBgm(SoundManager.GetCharaVoiceLabel("BGM_GXX_REACH_5", type)).Forget();
		}
	}

	public async UniTask<bool> ProcessCommandRon(Ron cmd, MahjongPlayFlow mpf)
	{
		m_mahjongPlayFlow = mpf;
		CancellationToken token = m_mahjongPlayFlow.m_cancellationToken;
		token.ThrowIfCancellationRequested();
		ScorePanelSoundUtility.SetScoreMoveCommandRandomSeed(cmd.SeqNo + GameDataManager.Taikyoku.TakuInfo.TakuID * 3);
		await MahjongUtility.CpuWait(Players[cmd.PlayerIndex]);
		thinkCount.ThinkingTyoukouEnd();
		Players[cmd.PlayerIndex].hand.TenpaiTargetIconSetDisp(isDisp: false);
		Players[cmd.PlayerIndex].hand.OpenHoujuTargetIconSetDisp(isDisp: false);
		m_uradoraOpen = false;
		foreach (Player player in Players)
		{
			int num = Players.IndexOf(player);
			rons[num] = cmd.RonFlag[num] != 0;
		}
		m_StageBG.StageState = StageState.Excite;
		m_mahjongPlayFlow.SetHaiSelectEnable(enable: false);
		await Players[m_commandClient.PlayerIndex].hand.SelectReset(m_sdChara);
		m_haiConjectureIcon.SetIconsDispOff();
		m_haiShantenCounter.SetShantenCountDispOff();
		m_haiShantenIcon.SetHaiShantenIconDispOff();
		m_mahjongPlayFlow._accelDoraRemainCounter.Reflesh(0, isDisp: false).Forget();
		m_mahjongPlayFlow.m_bombDisplay.SetActive(isActive: false);
		m_mahjongPlayFlow._meldBonusCounter.Reflesh(Players, isDisp: false);
		bool isChankanAgari = false;
		bool isKokushiAnkanRon = false;
		bool isMangan = false;
		if (m_cutinManager.IsPlaying)
		{
			await UniTask.WaitWhile(() => m_cutinManager.IsPlaying, PlayerLoopTiming.Update, token);
		}
		SoundManager.StopBgmWithFadeout().Forget();
		for (int i = 0; i < 4; i++)
		{
			if (!rons[i])
			{
				continue;
			}
			if (cmd.Yakus[i].HanID >= 5)
			{
				isMangan = true;
			}
			if (MahjongUtility.IsReachAgari(cmd.Yakus[i]))
			{
				m_uradoraOpen = true;
			}
			if (MahjongUtility.IsYakuAgari(cmd.Yakus[i], Yaku.Chankan))
			{
				isChankanAgari = true;
			}
			if (MahjongUtility.IsYakuAgari(cmd.Yakus[i], Yaku.KokushiTanki))
			{
				Tile lastKanHai = Players[cmd.PlayerIndex].meld.GetLastKanHai();
				if (lastKanHai != null && lastKanHai.pai == (Pai)cmd.Yakus[i].PaiID)
				{
					isKokushiAnkanRon = true;
				}
			}
			m_indicateAgaris[(int)Players[i].m_cha].GetComponent<Animator>().SetTrigger("In");
			m_indicateAgaris[(int)Players[i].m_cha].transform.Find("Text").GetComponent<Image>().sprite = m_indicateAgariSpriteRon;
			m_indicateAgaris[(int)Players[i].m_cha].transform.Find("Text").Find("Add").GetComponent<Image>()
				.sprite = m_indicateAgariSpriteRon;
			SoundManager.PlaySeCha("SFX_MJG_RONTSUMO1", Players[cmd.PlayerIndex].m_cha, 1f).Forget();
			SoundManager.PlaySeCha("SFX_MJG_RONTSUMO2", Players[i].m_cha).Forget();
			if (Players[i].m_charaType == CharaType.Chara18 && Players[i].m_isIppatsuVoice)
			{
				Debug.Log("<color=yellow>イッパツロン</color>");
				SoundManager.PlayVoiceCha("VOX_G18_MJG_RON_B", Players[i].m_charaType, Players[i].m_cha).Forget();
			}
			else
			{
				SoundManager.PlayVoiceCha("VOX_GXX_MJG_RON", Players[i].m_charaType, Players[i].m_cha).Forget();
			}
			if (Players[i].IsJicha())
			{
				LEDManager.Instance.AnimColor(new Color[3]
				{
					new Color(1f, 10f / 51f, 14f / 51f),
					new Color(1f, 40f / 51f, 0f),
					new Color(0.23529412f, 0.8235294f, 11f / 51f)
				}, isSpread: true, 2);
			}
			if (SystemConfigGameData.IsDualMonitor)
			{
				await SubMonitorFlow.GetInstance().AgariOpen();
			}
			m_mahjongPlayFlow.DaisouFinish();
			Players[i].character.AgariAction(Character.EventID.Result1).Forget();
			PlayReachBGMJingle(i);
		}
		Players[cmd.PlayerIndex].character.AgariAction(Character.EventID.Result2).Forget();
		foreach (Player player2 in Players)
		{
			int num2 = Players.IndexOf(player2);
			player2.character.ReachAura_SetDisp(isDisp: false);
			if (num2 != cmd.PlayerIndex && !rons[num2])
			{
				player2.character.Action(Character.EventID.Tyoko3).Forget();
			}
		}
		await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
		for (int k = 0; k < 4; k++)
		{
			if (rons[k])
			{
				Players[k].SetTenpai();
			}
		}
		if (isChankanAgari || isKokushiAnkanRon)
		{
			Tile lastKanHai2 = Players[cmd.PlayerIndex].meld.GetLastKanHai();
			AgariEffectStart(isMangan, lastKanHai2).Forget();
		}
		else
		{
			Tile lastTile = Players[cmd.PlayerIndex].dump.GetLastTile();
			AgariEffectStart(isMangan, lastTile).Forget();
		}
		if (m_uradoraOpen)
		{
			await wanpai.OpenUradora(cmd.OpenDoraNum);
			for (int l = 0; l < 4; l++)
			{
				if (rons[l] && MahjongUtility.IsReachAgari(cmd.Yakus[l]))
				{
					Players[l].StartAgariDoraEffect();
				}
			}
		}
		await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
		Cha bombCha = m_mahjongPlayFlow.m_bombDisplay.GetCurrentBombCha(m_mahjongPlayFlow.m_junmeNum, m_mahjongPlayFlow.m_actionCount, Players.Count((Player x) => x.m_isReach && !x.m_isReachSengen));
		foreach (Player player3 in Players)
		{
			player3.m_isBomber = player3.m_cha == bombCha;
		}
		for (int i = 0; i < 4; i++)
		{
			if (rons[i])
			{
				m_mahjongPlayFlow.BackScreen_Play_Ron(Players[i].m_charaType, Players[i].m_cha, Players[cmd.PlayerIndex].m_charaType, Players[cmd.PlayerIndex].m_cha).Forget();
				if (Players[i].IsProAnimEnable())
				{
					m_maAgari.GetComponent<Animator>().SetTrigger("In");
					m_maAgari.transform.Find("MaImage").GetComponent<Image>().sprite = Players[i].m_maAgariSprite;
					await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
				}
				List<Yaku> yakus = MahjongUtility.CreateYakusFromYakuBit(cmd.Yakus[i].YakuBit);
				int num3 = (GameDataManager.Mahjong.IsComebackMode ? Players[i].m_comebackTakuBonusDisp.bonusHan : 0);
				int num4 = (GameDataManager.Mahjong.IsMeldBonusMode ? MahjongUtility.MeldBonus.MeldCount(Players) : 0);
				int allgreenBonusHan = (GameDataManager.Mahjong.IsAllGreenBonusMode ? (cmd.Yakus[i].BonusHan - num3 - num4) : 0);
				Players[i].Ron((Han)cmd.Yakus[i].HanID, cmd.Yakus[i].DoraNum, num4, num3, allgreenBonusHan, yakus, MahjongUtility.Kirisame.AkadoraCount(cmd.Yakus[i].TePai, cmd.Yakus[i].PaiID), Players[cmd.PlayerIndex], bombCha).Forget();
				await m_cutinManager.AgariCutin_WaitIntroductionEnd(token);
				m_scorePanel.BgCameraSetActive(isActive: false);
				if (GameDataManager.Mahjong.IsHaifuPlay)
				{
					GameDataManager.Mahjong.IsHaifuPlayToExit = true;
					await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
				}
				await agari.SetAgariRon(cmd, Players, Players[i], Players[cmd.PlayerIndex], wanpai, i, m_mahjongPlayFlow.m_bombDisplay.GetKaze(), token);
				m_cutinManager.CurrentCutin_End(3f, token).Forget();
				await UniTask.WaitWhile(() => m_cutinManager.IsPlaying, PlayerLoopTiming.Update, token);
			}
		}
		if (m_mahjongPlayFlow.m_isRecAgariCutin)
		{
			m_mahjongPlayFlow.DebugRecordEnd();
		}
		Han han = Han.Nothing;
		foreach (Player player4 in Players)
		{
			int num5 = Players.IndexOf(player4);
			if (rons[num5])
			{
				if (cmd.Yakus[num5].HanID > (int)han)
				{
					han = (Han)cmd.Yakus[num5].HanID;
				}
				if (GameDataManager.Mahjong.IsAotenjoMode)
				{
					MahjongUtility.Aotenjo.AgariInfos.Add(new MahjongUtility.AotenjoAgariInfo
					{
						Cha = Players[num5].m_index,
						HanNum = cmd.Yakus[num5].HanNum,
						FuNum = cmd.Yakus[num5].FuNum,
						Kaze = Players[num5].m_jikaze,
						Hoju = Players[cmd.PlayerIndex].m_index
					});
				}
			}
		}
		foreach (Player player5 in Players)
		{
			int num6 = Players.IndexOf(player5);
			Han han2 = (rons[num6] ? ((Han)cmd.Yakus[num6].HanID) : Han.Nothing);
			List<Yaku> list = (rons[num6] ? MahjongUtility.CreateYakusFromYakuBit(cmd.Yakus[num6].YakuBit) : null);
			m_scorePanel.SetScore(cmd.Scores[num6], Players, player5, han2, m_kyokuCall.m_isOrasu, isRyukyoku: false, han2 == Han.Yakuman && list.Contains(Yaku.Kazoeyakuman), bombCha);
		}
		if (rons[m_commandClient.PlayerIndex])
		{
			VoiceManager.SetMahjongStartKind(VoiceManager.VoiceMahjongStartKind.Agari1);
		}
		else if (Players[cmd.PlayerIndex].IsJicha())
		{
			if (han >= Han.Haneman)
			{
				VoiceManager.SetMahjongStartKind(VoiceManager.VoiceMahjongStartKind.Furi2);
			}
			else
			{
				VoiceManager.SetMahjongStartKind(VoiceManager.VoiceMahjongStartKind.Furi1);
			}
		}
		else
		{
			VoiceManager.SetMahjongStartKind(VoiceManager.VoiceMahjongStartKind.Furi3);
		}
		await m_scorePanel.Play();
		foreach (Player player6 in Players)
		{
			int index = Players.IndexOf(player6);
			player6.SetScore(cmd.Scores[index].NewScore);
		}
		GameDataManager.Mission.LatestAgariCha = Players[cmd.PlayerIndex].m_cha;
		SetRank();
		LEDManager.Instance.AnimColor(new Color[1] { Color.white }, isSpread: true);
		if (SystemConfigGameData.IsDualMonitor)
		{
			await SubMonitorFlow.GetInstance().AgariClose();
		}
		var (num7, num8, num9, num10) = FanMaster.FansOnRon(mpf, cmd);
		GameDataManager.Mahjong.Fans.luck += (float)num7 * 0.8f;
		GameDataManager.Mahjong.Fans.speed += (float)num8 * 0.8f;
		GameDataManager.Mahjong.Fans.attack += (float)num9 * 0.8f;
		GameDataManager.Mahjong.Fans.defence += (float)num10 * 0.8f;
		GameDataManager.EventTaku.OnRonAgari(cmd, Players);
		return true;
	}

	public async UniTask<bool> ProcessCommandRyuKyoku(RyuKyoku cmd, MahjongPlayFlow mpf)
	{
		m_mahjongPlayFlow = mpf;
		CancellationToken token = m_mahjongPlayFlow.m_cancellationToken;
		token.ThrowIfCancellationRequested();
		ScorePanelSoundUtility.SetScoreMoveCommandRandomSeed(cmd.SeqNo + GameDataManager.Taikyoku.TakuInfo.TakuID * 5);
		m_ryukyokuCall.gameObject.SetActive(value: true);
		SoundManager.StopBgmWithFadeout(3.0).Forget();
		SoundManager.PlaySe("SFX_MJG_DISPLAY2").Forget();
		m_mahjongPlayFlow.DispoffAllTurnLine();
		m_mahjongPlayFlow.SetHaiSelectEnable(enable: false);
		await Players[m_commandClient.PlayerIndex].hand.SelectReset(m_sdChara);
		m_haiConjectureIcon.SetIconsDispOff();
		m_haiShantenCounter.SetShantenCountDispOff();
		m_haiShantenIcon.SetHaiShantenIconDispOff();
		m_mahjongPlayFlow._accelDoraRemainCounter.Reflesh(0, isDisp: false).Forget();
		m_mahjongPlayFlow.m_bombDisplay.SetActive(isActive: false);
		Players[m_commandClient.PlayerIndex].hand.TenpaiTargetIconSetDisp(isDisp: false);
		Players[m_commandClient.PlayerIndex].hand.OpenHoujuTargetIconSetDisp(isDisp: false);
		m_mahjongPlayFlow.DaisouFinish();
		float num = 1f;
		double ryukyokuCallwaitTime = m_ryukyokuCall.duration - (double)num;
		await UniTask.Delay(TimeSpan.FromSeconds(num), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
		foreach (Player player2 in Players)
		{
			Players.IndexOf(player2);
			player2.character.m_characterModelController.PlayThinkForce(player2.m_charaType);
			player2.character.ReachAura_SetDisp(isDisp: false);
		}
		await UniTask.Delay(TimeSpan.FromSeconds(ryukyokuCallwaitTime), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
		m_ryukyokuCall.gameObject.SetActive(value: false);
		int kaze;
		for (kaze = 0; kaze <= 3; kaze++)
		{
			Player player = Players.Find((Player a) => a.m_jikaze == (Kaze)kaze);
			if (player != null)
			{
				int i = Players.IndexOf(player);
				await player.RyuKyoku(cmd.MachiPaiID[i]);
				m_scorePanel.SetScore(cmd.Scores[i], Players, player, Han.Nothing, m_kyokuCall.m_isOrasu, isRyukyoku: true);
				await UniTask.Delay(TimeSpan.FromSeconds(0.5), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
			}
		}
		if (GameDataManager.Mahjong.IsHaifuPlay)
		{
			GameDataManager.Mahjong.IsHaifuPlayToExit = true;
		}
		m_scorePanel.BgCameraSetActive(isActive: false);
		await m_scorePanel.Play();
		foreach (Player player3 in Players)
		{
			player3.RyuKyokuExit();
		}
		foreach (Player player4 in Players)
		{
			int index = Players.IndexOf(player4);
			player4.SetScore(cmd.Scores[index].NewScore);
		}
		SetRank();
		GameDataManager.EventTaku.OnRyukyoku(cmd, Players);
		var (num2, num3, num4, num5) = FanMaster.FansOnRyukyoku(mpf, cmd);
		GameDataManager.Mahjong.Fans.luck += (float)num2 * 0.8f;
		GameDataManager.Mahjong.Fans.speed += (float)num3 * 0.8f;
		GameDataManager.Mahjong.Fans.attack += (float)num4 * 0.8f;
		GameDataManager.Mahjong.Fans.defence += (float)num5 * 0.8f;
		return true;
	}

	private async UniTask AgariEffectStart(bool isMangan, Tile item = null)
	{
		if (item != null)
		{
			await UniTask.WaitWhile(() => item.IsMoving);
			agariEffect.transform.position = item.transform.position;
		}
		else
		{
			agariEffect.transform.position = new Vector3(0f, 0.52f, 0f);
		}
		if (isMangan)
		{
			agariEffect.GetComponent<Animator>().SetTrigger("Mangan");
		}
		else
		{
			agariEffect.GetComponent<Animator>().SetTrigger("Common");
		}
		shake.ShakeStart();
	}

	private void SetRank()
	{
		List<Player> list = new List<Player>(Players);
		list.Sort(RankCompare);
		foreach (Player item in list)
		{
			int rank = list.IndexOf(item);
			item.SetRank(rank);
		}
		m_mahjongPlayFlow.CalculateDiffScore();
		foreach (Player item2 in list)
		{
			item2.RefleshScoreDisp();
		}
	}

	public static int RankCompare(Player a, Player b)
	{
		if (a.m_score == b.m_score)
		{
			return a.m_startKaze - b.m_startKaze;
		}
		long num = b.m_score - a.m_score;
		if (num >= int.MaxValue)
		{
			return int.MaxValue;
		}
		if (num <= int.MinValue)
		{
			return int.MinValue;
		}
		return (int)num;
	}
}
