using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.Voice;
using UnityEngine;

internal static class VoiceManager
{
	public enum ReactionVoiceID
	{
		Comment1,
		Comment2,
		Comment3,
		Comment4,
		Comment5,
		Comment6,
		Comment7
	}

	public enum VoiceMahjongStartKind
	{
		Tong,
		Renchan1,
		Renchan2,
		Agari1,
		Agari2,
		Furi1,
		Furi2,
		Furi3,
		Tenpai,
		Noten,
		NoScore,
		Rank1,
		Rank2,
		Rank3,
		Rank4,
		None
	}

	private static bool m_isTalkPlayed = false;

	public static bool m_charaUniqueYakuAgari = false;

	public static List<Kaze> m_charaUniqueYakuAgariIndex = new List<Kaze>();

	private static bool m_isChunBeamEnable = true;

	private static int m_chunContinueCount;

	private static int m_firstChunDahaiPlayerIndex;

	private static VoiceMahjongStartKind m_voiceMahjongStartKind = VoiceMahjongStartKind.Tong;

	private static Dictionary<Cha, List<string>> PlayVoiceHistories = new Dictionary<Cha, List<string>>();

	private static readonly RandomValueGenerator m_randomValueCommand = new RandomValueGenerator();

	private static readonly RandomValueGenerator m_randomValueCutin = new RandomValueGenerator();

	public static bool PlayVoiceTalk(int restTileNum)
	{
		if (!m_isTalkPlayed)
		{
			switch (GameDataManager.Mahjong.TakuType)
			{
			case TakuType.Nima:
				if (restTileNum < 18 || 26 < restTileNum)
				{
					return false;
				}
				break;
			case TakuType.Sanma:
				if (restTileNum < 23 || 30 < restTileNum)
				{
					return false;
				}
				break;
			default:
				if (restTileNum < 33 || 38 < restTileNum)
				{
					return false;
				}
				break;
			}
			if (m_randomValueCommand.Range(0, 100) <= 50)
			{
				m_isTalkPlayed = true;
				PlayVoice(new List<Category> { Category.Talk }).Forget();
				return true;
			}
		}
		return false;
	}

	public static void KyokuInit()
	{
		m_isChunBeamEnable = true;
		m_chunContinueCount = 0;
		m_firstChunDahaiPlayerIndex = -1;
		m_charaUniqueYakuAgari = false;
		m_charaUniqueYakuAgariIndex = new List<Kaze>();
		m_isTalkPlayed = false;
	}

	public static void PlayVoiceScore1(Player player)
	{
		PlayVoiceChara(new List<Category> { Category.RyukyokuPlus }, player).Forget();
	}

	public static void PlayVoiceScore2(Player player)
	{
		PlayVoiceChara(new List<Category> { Category.ScorePlus1 }, player).Forget();
	}

	public static void PlayVoiceScore3(Player player)
	{
		PlayVoiceChara(new List<Category> { Category.ScorePlus2 }, player).Forget();
	}

	public static void PlayVoiceScore4(Player player)
	{
		PlayVoiceChara(new List<Category> { Category.ScorePlus3 }, player).Forget();
	}

	public static void PlayVoiceScore5(Player player)
	{
		PlayVoiceChara(new List<Category> { Category.ScorePlus4 }, player).Forget();
	}

	public static void PlayVoiceScore6(Player player)
	{
		PlayVoiceChara(new List<Category> { Category.RyukyokuMinus }, player).Forget();
	}

	public static void PlayVoiceScore7(Player player)
	{
		PlayVoiceChara(new List<Category> { Category.ScoreMinus1 }, player).Forget();
	}

	public static void PlayVoiceScore8(Player player)
	{
		PlayVoiceChara(new List<Category> { Category.ScoreMinus2 }, player).Forget();
	}

	public static void PlayVoiceScore9(Player player)
	{
		PlayVoiceChara(new List<Category> { Category.ScoreMinus3 }, player).Forget();
	}

	public static void PlayVoiceScore10(Player player)
	{
		PlayVoiceChara(new List<Category> { Category.ScoreMinus4 }, player).Forget();
	}

	public static void PlayVoiceScore11(Player player)
	{
		PlayVoiceChara(new List<Category> { Category.ScoreTobi }, player).Forget();
	}

	public static void PlayVoiceScore12(Player player)
	{
		PlayVoiceChara(new List<Category> { Category.ScoreNothing }, player).Forget();
	}

	public static async UniTask PlayVoiceScore13(Player player)
	{
		await PlayVoiceChara(new List<Category> { Category.ScoreIndigenousYaku }, player);
	}

	public static void PlayVoiceAgariN(bool isPlay, Player player)
	{
		PlayVoiceChara(isPlay, new List<Category> { Category.AgariN }, player).Forget();
	}

	public static void PlayVoiceAgariSR(bool isPlay, Player player)
	{
		PlayVoiceChara(isPlay, new List<Category> { Category.AgariSR }, player).Forget();
	}

	public static async UniTask PlayVoiceComment1(Player player)
	{
		await UniTask.Delay(1000);
		PlayVoiceCha(new List<Category> { Category.OwnMeld }, player).Forget();
	}

	public static void PlayVoiceComment2(Player player)
	{
		PlayVoiceCha(new List<Category> { Category.Melded }, player).Forget();
	}

	public static void PlayVoiceComment3(Player player)
	{
		PlayVoiceCha(new List<Category> { Category.MyTurnSkipped }, player).Forget();
	}

	public static void PlayVoiceComment4(Player player)
	{
		PlayVoiceCha(new List<Category> { Category.DoraMelded }, player).Forget();
	}

	public static void PlayVoiceComment5(Player player)
	{
		PlayVoiceCha(new List<Category> { Category.Reach }, player).Forget();
	}

	public static void PlayVoiceComment6(Player player)
	{
		PlayVoiceCha(new List<Category> { Category.OkkakeReach }, player).Forget();
	}

	public static void PlayVoiceComment7(Player player)
	{
		PlayVoiceCha(new List<Category> { Category.OkkakeReached }, player).Forget();
	}

	public static void PlayVoiceThink(Player player)
	{
		PlayVoiceCha(new List<Category> { Category.LongThinking }, player).Forget();
	}

	public static void PlayVoiceDahai1(Player player)
	{
		if (m_randomValueCommand.Range(0, 100) > 20)
		{
			PlayVoiceCha(new List<Category> { Category.Dahai }, player).Forget();
		}
	}

	public static void PlayVoiceDahai2(Player player)
	{
		if (m_randomValueCommand.Range(0, 100) > 20)
		{
			PlayVoiceCha(new List<Category> { Category.DahaiAnpai }, player).Forget();
		}
	}

	public static void PlayVoiceDahai3(Player player)
	{
		if (m_randomValueCommand.Range(0, 100) > 20)
		{
			PlayVoiceCha(new List<Category> { Category.DahaiDanger }, player).Forget();
		}
	}

	public static void PlayVoiceDahai4(Player player)
	{
		if (m_randomValueCommand.Range(0, 100) > 20)
		{
			PlayVoiceCha(new List<Category> { Category.DahaiTsumoGiri1 }, player).Forget();
		}
	}

	public static void PlayVoiceDahai5(Player player)
	{
		if (m_randomValueCommand.Range(0, 100) > 20)
		{
			PlayVoiceCha(new List<Category>
			{
				Category.DahaiTsumoGiri1,
				Category.DahaiTsumoGiri2
			}, player).Forget();
		}
	}

	public static void PlayVoiceDahai6(Player player)
	{
		PlayVoiceCha(new List<Category>
		{
			Category.DahaiTsumoGiri2,
			Category.DahaiTsumoGiri3
		}, player).Forget();
	}

	public static void PlayVoiceDahai7(Player player)
	{
		PlayVoiceCha(new List<Category> { Category.DahaiDora }, player).Forget();
	}

	public static void PlayVoiceDahai8(Player player)
	{
		if (m_randomValueCommand.Range(0, 100) > 20)
		{
			PlayVoiceCha(new List<Category> { Category.DahaiReachDanger }, player).Forget();
		}
	}

	public static void PlayVoiceDahai9(Player player)
	{
		PlayVoiceCha(new List<Category> { Category.DahaiIppatsuMissed }, player).Forget();
	}

	public static bool IsChunBeamEnable()
	{
		return m_isChunBeamEnable;
	}

	public static int GetChunBeamContinueCount()
	{
		return m_chunContinueCount;
	}

	public static void ChunBeamEnableJudge(int playerIndex, Pai pai)
	{
		if (!m_isChunBeamEnable)
		{
			return;
		}
		if (m_firstChunDahaiPlayerIndex == playerIndex)
		{
			m_isChunBeamEnable = false;
		}
		else if (pai == Pai.J7)
		{
			if (m_chunContinueCount < 4)
			{
				m_chunContinueCount++;
			}
			if (m_firstChunDahaiPlayerIndex == -1)
			{
				m_firstChunDahaiPlayerIndex = playerIndex;
			}
			if (GameDataManager.Mahjong.TakuType == TakuType.Sanma && m_chunContinueCount == 3)
			{
				m_chunContinueCount = 4;
			}
		}
	}

	public static void ChunBeamNakiEndJudge()
	{
		if (m_isChunBeamEnable && m_firstChunDahaiPlayerIndex != -1)
		{
			m_isChunBeamEnable = false;
		}
	}

	public static void PlayVoiceChun(Player player)
	{
		string text = "";
		switch (m_chunContinueCount)
		{
		case 1:
			text = "VOX_GXX_MJG_CHUN1";
			break;
		case 2:
			text = "VOX_GXX_MJG_CHUN2";
			break;
		case 3:
			text = "VOX_GXX_MJG_CHUN3";
			break;
		case 4:
			text = "VOX_GXX_MJG_CHUN4";
			break;
		}
		if (text != "")
		{
			SoundManager.PlayVoiceCha(text, player.m_charaType, player.m_cha).Forget();
		}
	}

	public static void PlayVoicePick1()
	{
		PlayVoice(new List<Category> { Category.Tsumo }).Forget();
	}

	public static void PlayVoiceNokori2()
	{
		PlayVoice(new List<Category>
		{
			Category.Tsumo,
			Category.Tsumo2Shanten
		}).Forget();
	}

	public static void PlayVoiceNokori1()
	{
		PlayVoice(new List<Category>
		{
			Category.Tsumo,
			Category.Tsumo2Shanten,
			Category.Tsumo1Shanten
		}).Forget();
	}

	public static void PlayVoiceNokori0()
	{
		PlayVoice(new List<Category>
		{
			Category.Tsumo,
			Category.Tsumo1Shanten
		}).Forget();
	}

	public static void PlayVoicePick2()
	{
		PlayVoice(new List<Category> { Category.TsumoUrame }).Forget();
	}

	public static void PlayVoiceStart(float delay = -1f)
	{
		switch (m_voiceMahjongStartKind)
		{
		case VoiceMahjongStartKind.Tong:
			PlayVoice(new List<Category> { Category.KyokuStartFirst }).Forget();
			break;
		case VoiceMahjongStartKind.Renchan1:
			PlayVoice(new List<Category> { Category.KyokuStartRenchan }).Forget();
			break;
		case VoiceMahjongStartKind.Renchan2:
			PlayVoice(new List<Category> { Category.KyokuStartRenchanOthers }).Forget();
			break;
		case VoiceMahjongStartKind.Agari1:
			PlayVoice(new List<Category> { Category.KyokuStartAgari }).Forget();
			break;
		case VoiceMahjongStartKind.Agari2:
			PlayVoice(new List<Category> { Category.KyokuStartTsumo }).Forget();
			break;
		case VoiceMahjongStartKind.Furi1:
			PlayVoice(new List<Category> { Category.KyokuStartHoju1 }).Forget();
			break;
		case VoiceMahjongStartKind.Furi2:
			PlayVoice(new List<Category> { Category.KyokuStartHoju2 }).Forget();
			break;
		case VoiceMahjongStartKind.Furi3:
			PlayVoice(new List<Category> { Category.KyokuStartHoju3 }).Forget();
			break;
		case VoiceMahjongStartKind.Tenpai:
			PlayVoice(new List<Category> { Category.KyokuStartTenpai }).Forget();
			break;
		case VoiceMahjongStartKind.Noten:
			PlayVoice(new List<Category> { Category.KyokuStartNoten }).Forget();
			break;
		case VoiceMahjongStartKind.NoScore:
			PlayVoice(new List<Category> { Category.KyokuStartNoScore }).Forget();
			break;
		case VoiceMahjongStartKind.Rank1:
			PlayVoice(new List<Category> { Category.KyokuStartAllLast1 }).Forget();
			break;
		case VoiceMahjongStartKind.Rank2:
			PlayVoice(new List<Category> { Category.KyokuStartAllLast2 }).Forget();
			break;
		case VoiceMahjongStartKind.Rank3:
			PlayVoice(new List<Category> { Category.KyokuStartAllLast3 }).Forget();
			break;
		case VoiceMahjongStartKind.Rank4:
			PlayVoice(new List<Category> { Category.KyokuStartAllLast4 }).Forget();
			break;
		}
		m_voiceMahjongStartKind = VoiceMahjongStartKind.None;
	}

	public static void SetMahjongStartKind(VoiceMahjongStartKind kind)
	{
		m_voiceMahjongStartKind = kind;
	}

	public static void PlayVoiceMatching(Player player, float pan = -2000f)
	{
		PlayVoiceChara(new List<Category> { Category.Matching }, player, -1f, isCancelPrevVoice: false, pan).Forget();
	}

	public static string FakeClear_ToLabel(int charaVariantID)
	{
		charaVariantID--;
		string[] array = new string[4] { "GAL", "KAN", "KYO", "USA" };
		charaVariantID = Math.Clamp(charaVariantID, 0, array.Length - 1);
		return array[charaVariantID];
	}

	public static void FakeClear_PlayVoiceMatching(int charaVariantID, float pan = -2000f)
	{
		SoundManager.PlayVoice("VOX_G12_KAGE_" + FakeClear_ToLabel(charaVariantID) + "_APPEAL", -1f, isCancelPrevVoice: false, pan).Forget();
	}

	public static void PlayVoiceTitle()
	{
		List<CharaType> list = new List<CharaType>();
		foreach (CharaType value in Enum.GetValues(typeof(CharaType)))
		{
			if (GameDataManager.IsCharaEnable(value))
			{
				list.Add(value);
			}
		}
		CharaType type = list[UnityEngine.Random.Range(0, list.Count)];
		SoundManager.PlayVoice("VOX_GXX_TITLE1", type).Forget();
	}

	private static async UniTask PlayVoice(List<Category> categories, float delay = -1f, float pan = -2000f)
	{
		List<string> gachaVoices = (from x in GameDataManager.Player.GetCurrentChara().GachaVoiceItems?.Select((VoiceInfo x) => x.Master)
			where categories.Contains(x.Category)
			select x)?.Select((Item x) => x.Label).ToList() ?? null;
		List<string> list = CreatePlayVoiceList(categories, gachaVoices, GameDataManager.Player.GetCurrentChara().Level, Cha.Jicha);
		int index = UnityEngine.Random.Range(0, list.Count);
		string text = list[index];
		PlayVoiceHistories[Cha.Jicha].Add(text);
		await SoundManager.PlayMyCharaVoice(text, delay, pan);
	}

	private static async UniTask PlayVoiceChara(List<Category> categories, Player player, float delay = -1f, bool isCancelPrevVoice = true, float pan = -2000f)
	{
		List<string> gachaVoices = (from x in player.GachaVoiceItemEquip?.Select((VoiceInfo x) => x.Master)
			where categories.Contains(x.Category)
			select x)?.Select((Item x) => x.Label).ToList() ?? null;
		List<string> list = CreatePlayVoiceList(categories, gachaVoices, player.m_level);
		int index = m_randomValueCutin.Range(0, list.Count);
		await SoundManager.PlayVoice(list[index], player.m_charaType, delay, isCancelPrevVoice, pan);
	}

	private static async UniTask PlayVoiceChara(bool isPlay, List<Category> categories, Player player, float delay = -1f)
	{
		if (isPlay)
		{
			await PlayVoiceChara(categories, player, delay);
			return;
		}
		List<string> gachaVoices = (from x in player.GachaVoiceItemEquip?.Select((VoiceInfo x) => x.Master)
			where categories.Contains(x.Category)
			select x)?.Select((Item x) => x.Label).ToList() ?? null;
		List<string> list = CreatePlayVoiceList(categories, gachaVoices, player.m_level);
		foreach (string item in list)
		{
			await SoundManager.StopVoice(item);
		}
	}

	private static async UniTask PlayVoiceCha(List<Category> categories, Player player, float delay = -1f)
	{
		List<string> gachaVoices = (from x in player.GachaVoiceItemEquip?.Select((VoiceInfo x) => x.Master)
			where categories.Contains(x.Category)
			select x)?.Select((Item x) => x.Label).ToList() ?? null;
		List<string> list = CreatePlayVoiceList(categories, gachaVoices, player.m_level, player.m_cha);
		int index = m_randomValueCommand.Range(0, list.Count);
		string item = list[index];
		PlayVoiceHistories[player.m_cha].Add(item);
		SoundManager.PlayVoiceCha(list[index], player.m_charaType, player.m_cha, delay).Forget();
	}

	public static void VoiceHistoryInit()
	{
		PlayVoiceHistories = new Dictionary<Cha, List<string>>();
		foreach (Cha value in Enum.GetValues(typeof(Cha)))
		{
			PlayVoiceHistories.Add(value, new List<string>());
		}
	}

	public static List<string> JoinVoiceList(List<Category> categories, List<string> gachaVoices, int level = 99999999)
	{
		List<string> list = new List<string>();
		list.AddRange(from x in VoiceMaster.GetItems((Item x) => categories.Contains(x.Category) && (x.ObtainType == ObtainType.Default || (x.ObtainType == ObtainType.Favo && level >= x.NeedUnlockFavo)))
			select x.Label);
		if (gachaVoices != null)
		{
			list.AddRange(gachaVoices);
		}
		return list;
	}

	private static List<string> CreatePlayVoiceList(List<Category> categories, List<string> gachaVoices, int level, Cha cha = (Cha)(-1))
	{
		List<string> list = JoinVoiceList(categories, gachaVoices, level);
		List<string> list2 = ((cha >= Cha.Jicha) ? list.Where((string x) => !PlayVoiceHistories[cha].TakeLast(5).Contains(x)).ToList() : null);
		if (list2 == null || list2.Count <= 0)
		{
			return list;
		}
		return list2;
	}

	public static void PlayVoiceInReachCutin(bool isPlay, ReachType reachType, CharaType chara, Cha cha, List<VoiceInfo> gachaVoices)
	{
		Category category2 = ((reachType != ReachType.DoubleReach) ? Category.ReachCutin : Category.DoubleReachCutin);
		Category category = category2;
		List<string> gachaVoices2 = gachaVoices.Where((VoiceInfo x) => x.Master.Category == category)?.Select((VoiceInfo x) => x.Master.Label)?.ToList() ?? null;
		List<string> list = JoinVoiceList(new List<Category> { category }, gachaVoices2);
		int index = m_randomValueCommand.Range(0, list.Count);
		string label = list[index];
		SoundManager.PlayVoiceCha(isPlay, label, chara, cha).Forget();
	}

	public static void SetCommandRandomSeed(int seed)
	{
		m_randomValueCommand.Init(seed);
	}

	public static void SetCutinRandomSeed(int seed)
	{
		m_randomValueCutin.Init(seed);
	}
}
