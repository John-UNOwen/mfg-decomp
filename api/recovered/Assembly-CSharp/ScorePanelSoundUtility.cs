using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.Types;
using MFG.Types.Customize.Voice;
using UnityEngine;

public static class ScorePanelSoundUtility
{
	private static readonly RandomValueGenerator m_randomValueScoreMoveCommand = new RandomValueGenerator();

	public static void SetScoreMoveCommandRandomSeed(int seed)
	{
		m_randomValueScoreMoveCommand.Init(seed);
	}

	private static PlayerScorePanel GetPlayerFromRandomValue(IEnumerable<PlayerScorePanel> scoreMoves)
	{
		List<PlayerScorePanel> list = scoreMoves.OrderBy((PlayerScorePanel x) => x.StartKaze).ToList();
		int index = m_randomValueScoreMoveCommand.Range(0, list.Count());
		return list[index];
	}

	public static async UniTask PlayProgress(List<PlayerScorePanel> rankList, bool isOrasu, bool isRyukyoku)
	{
		SoundManager.PlayBgm("BGM_RESULT1").Forget();
		try
		{
			Han maxHan = rankList.Max((PlayerScorePanel x) => x.scoreMove.Han);
			PlayerScorePanel playerScorePanel = (from a in rankList
				orderby a.scoreMove.BeforeScore descending, a.StartKaze
				select a).ToList().Last();
			PlayerScorePanel afterLastPlayer = rankList.Last();
			bool flag = isOrasu && playerScorePanel == afterLastPlayer && afterLastPlayer.JiKaze != Kaze.Ton;
			if (isRyukyoku)
			{
				IEnumerable<PlayerScorePanel> scoreMoves;
				if (!flag)
				{
					IEnumerable<PlayerScorePanel> enumerable = rankList;
					scoreMoves = enumerable;
				}
				else
				{
					scoreMoves = rankList.Where((PlayerScorePanel x) => x != afterLastPlayer);
				}
				PlayerScorePanel playerFromRandomValue = GetPlayerFromRandomValue(scoreMoves);
				if (rankList.All((PlayerScorePanel x) => x.scoreMove.DiffScore == 0L))
				{
					VoiceManager.PlayVoiceScore12(playerFromRandomValue.player);
				}
				else if (playerFromRandomValue.scoreMove.DiffScore > 0L)
				{
					VoiceManager.PlayVoiceScore1(playerFromRandomValue.player);
				}
				else if (playerFromRandomValue.scoreMove.DiffScore < 0L)
				{
					VoiceManager.PlayVoiceScore6(playerFromRandomValue.player);
				}
				return;
			}
			if (VoiceManager.m_charaUniqueYakuAgari)
			{
				await VoiceManager.PlayVoiceScore13(GetPlayerFromRandomValue(rankList.Where((PlayerScorePanel x) => VoiceManager.m_charaUniqueYakuAgariIndex.Contains(x.StartKaze))).player);
				await UniTask.WaitWhile(() => SoundManager.IsPlayLastCalledVoice());
				return;
			}
			if ((!GameDataManager.Mahjong.IsAotenjoMode && maxHan >= Han.Yakuman) || (GameDataManager.Mahjong.IsAotenjoMode && rankList.Exists((PlayerScorePanel x) => x.scoreMove.Han >= Han.Yakuman && !x.scoreMove.IsKazoe)))
			{
				PlayerScorePanel playerFromRandomValue2 = GetPlayerFromRandomValue(rankList.Where((PlayerScorePanel x) => x.scoreMove.Han >= Han.Yakuman || x.scoreMove.DiffScore < 0L));
				if (playerFromRandomValue2.scoreMove.DiffScore > 0L)
				{
					VoiceManager.PlayVoiceScore5(playerFromRandomValue2.player);
				}
				else
				{
					VoiceManager.PlayVoiceScore10(playerFromRandomValue2.player);
				}
				return;
			}
			if (GameDataManager.Mahjong.TakuType != TakuType.Nima && rankList.Exists((PlayerScorePanel x) => x.scoreMove.NewScore <= 0L))
			{
				VoiceManager.PlayVoiceScore11(GetPlayerFromRandomValue(rankList.Where((PlayerScorePanel x) => x.scoreMove.NewScore <= 0L)).player);
				return;
			}
			IEnumerable<PlayerScorePanel> source;
			if (!flag)
			{
				IEnumerable<PlayerScorePanel> enumerable = rankList;
				source = enumerable;
			}
			else
			{
				source = rankList.Where((PlayerScorePanel x) => x != afterLastPlayer);
			}
			PlayerScorePanel playerFromRandomValue3 = GetPlayerFromRandomValue(source.Where((PlayerScorePanel x) => x.scoreMove.Han == maxHan || x.scoreMove.DiffScore < 0L));
			BigInteger bigInteger = rankList.Find((PlayerScorePanel x) => x.scoreMove.Han == maxHan)?.scoreMove.DiffScore ?? ((BigInteger)5000);
			if (playerFromRandomValue3.scoreMove.DiffScore > 0L)
			{
				if (bigInteger >= 12000L)
				{
					VoiceManager.PlayVoiceScore4(playerFromRandomValue3.player);
				}
				else if (bigInteger >= 5000L)
				{
					VoiceManager.PlayVoiceScore3(playerFromRandomValue3.player);
				}
				else
				{
					VoiceManager.PlayVoiceScore2(playerFromRandomValue3.player);
				}
			}
			else if (bigInteger >= 12000L)
			{
				VoiceManager.PlayVoiceScore9(playerFromRandomValue3.player);
			}
			else if (bigInteger >= 5000L)
			{
				VoiceManager.PlayVoiceScore8(playerFromRandomValue3.player);
			}
			else
			{
				VoiceManager.PlayVoiceScore7(playerFromRandomValue3.player);
			}
		}
		catch
		{
			Debug.Log("ScorePanelVoice 参照エラー");
		}
	}

	public static void PlayRankSE(int rank)
	{
		Dictionary<TakuType, List<string>> obj = new Dictionary<TakuType, List<string>>
		{
			{
				TakuType.Tonpu,
				new List<string> { "SFX_MJG_RANK1", "SFX_MJG_RANK2", "SFX_MJG_RANK3", "SFX_MJG_RANK4" }
			},
			{
				TakuType.Sanma,
				new List<string> { "SFX_MJG_RANK1", "SFX_MJG_RANK2", "SFX_MJG_RANK4" }
			},
			{
				TakuType.Nima,
				new List<string> { "SFX_MJG_RANK1", "SFX_MJG_RANK4" }
			}
		};
		SoundManager.PlaySe(delay: (75f - (float)rank * 10f) / 60f, label: obj[GameDataManager.Mahjong.TakuType][rank]).Forget();
	}

	public static void PlayRankVoice(int rank)
	{
		Dictionary<TakuType, List<Category>> dictionary = new Dictionary<TakuType, List<Category>>
		{
			{
				TakuType.Tonpu,
				new List<Category>
				{
					Category.ResultScorePanel1,
					Category.ResultScorePanel2,
					Category.ResultScorePanel3,
					Category.ResultScorePanel4
				}
			},
			{
				TakuType.Sanma,
				new List<Category>
				{
					Category.ResultScorePanel1,
					Category.ResultScorePanel2,
					Category.ResultScorePanel4
				}
			},
			{
				TakuType.Nima,
				new List<Category>
				{
					Category.ResultScorePanelWin,
					Category.ResultScorePanelLose
				}
			}
		};
		Category category = dictionary[GameDataManager.Mahjong.TakuType][rank];
		List<string> gachaVoices = (from x in GameDataManager.Player.GetCurrentChara().GachaVoiceItems
			where x.Master.Category == category
			select x.Master.Label).ToList();
		List<string> list = VoiceManager.JoinVoiceList(new List<Category> { category }, gachaVoices);
		SoundManager.PlayMyCharaVoice(list[Random.Range(0, list.Count())]).Forget();
	}
}
