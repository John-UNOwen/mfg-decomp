using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData.EventTaku;
using MFG.MahjonPlay.Hand;
using MFG.Taikyoku.Command.Receive;
using MFG.Types;
using UnityEngine;

internal static class MahjongUtility
{
	public enum DoraRange
	{
		All,
		Omote,
		Ura
	}

	public static class KeyBoard
	{
		public static readonly List<KeyCode> TileKeyCodes = new List<KeyCode>
		{
			KeyCode.Alpha1,
			KeyCode.Alpha2,
			KeyCode.Alpha3,
			KeyCode.Alpha4,
			KeyCode.Alpha5,
			KeyCode.Alpha6,
			KeyCode.Alpha7,
			KeyCode.Alpha8,
			KeyCode.Alpha9,
			KeyCode.Alpha0,
			KeyCode.Minus,
			KeyCode.Equals,
			KeyCode.None,
			KeyCode.Backspace
		};

		public static readonly Dictionary<KeyCode, int> VirtualKeyCode = new Dictionary<KeyCode, int>
		{
			{
				KeyCode.Alpha1,
				49
			},
			{
				KeyCode.Alpha2,
				50
			},
			{
				KeyCode.Alpha3,
				51
			},
			{
				KeyCode.Alpha4,
				52
			},
			{
				KeyCode.Alpha5,
				53
			},
			{
				KeyCode.Alpha6,
				54
			},
			{
				KeyCode.Alpha7,
				55
			},
			{
				KeyCode.Alpha8,
				56
			},
			{
				KeyCode.Alpha9,
				57
			},
			{
				KeyCode.Alpha0,
				48
			},
			{
				KeyCode.Minus,
				189
			},
			{
				KeyCode.Equals,
				222
			},
			{
				KeyCode.None,
				220
			},
			{
				KeyCode.Backspace,
				8
			}
		};

		public static readonly List<string> KeyCodeText = new List<string>
		{
			"1", "2", "3", "4", "5", "6", "7", "8", "9", "0",
			"-", "^", "\\", "BS"
		};
	}

	public static class BlowAway
	{
		public static int DangerScore = 8000;
	}

	public static class Bomb
	{
		public static bool IsEffectiveTaiyoku()
		{
			if (!GameDataManager.EventTaku.Current(MFG.Types.EventType.Bomb).IsActiveAtTaikyoku(GameDataManager.Mahjong.EventType, GameDataManager.Mahjong.TakuType, GameDataManager.Mahjong.IsConstancyTaku, GameDataManager.Mahjong.IsCompetitionTaku))
			{
				return GameDataManager.Mahjong.IsAotenjoMode;
			}
			return true;
		}
	}

	public static class AllGreen
	{
		public static bool IsEffectiveTaiyoku()
		{
			if (!GameDataManager.EventTaku.Current(MFG.Types.EventType.AllGreen).IsActiveAtTaikyoku(GameDataManager.Mahjong.EventType, GameDataManager.Mahjong.TakuType, GameDataManager.Mahjong.IsConstancyTaku, GameDataManager.Mahjong.IsCompetitionTaku))
			{
				return GameDataManager.Mahjong.IsAotenjoMode;
			}
			return true;
		}

		public static int BonusCount(Player player, HaiID addHai = HaiID.None)
		{
			int num = 0;
			for (int i = 0; i < player.hand.GetHais().Length - 1; i++)
			{
				if (HaiUtility.RyuisoHais().Contains(player.hand.GetHais()[i]))
				{
					num++;
				}
			}
			BlockSet blockSet = player.meld.GetBlockSet().Clone();
			for (int j = 0; j < blockSet.Count(); j++)
			{
				switch (blockSet[j].BlockType)
				{
				case BlockType.MinKantsu:
				case BlockType.AnKantsu:
					if (HaiUtility.RyuisoHais().Contains(blockSet[j].HaiID))
					{
						num += 4;
					}
					break;
				case BlockType.MinKoutsu:
					if (HaiUtility.RyuisoHais().Contains(blockSet[j].HaiID))
					{
						num += 3;
					}
					break;
				case BlockType.MinShuntsu:
					switch (blockSet[j].HaiID)
					{
					case HaiID.S1:
					case HaiID.S3:
					case HaiID.S4:
					case HaiID.S6:
						num += 2;
						break;
					case HaiID.S2:
						num += 3;
						break;
					case HaiID.S5:
					case HaiID.S7:
						num++;
						break;
					}
					break;
				}
			}
			if (HaiUtility.RyuisoHais().Contains(addHai))
			{
				num++;
			}
			return num / 2;
		}
	}

	public static class AccelDora
	{
		private static int[] _remainCountBorders = new int[4] { 40, 30, 20, 10 };

		public static bool IsEffectiveTaiyoku()
		{
			if (!GameDataManager.EventTaku.Current(MFG.Types.EventType.AccelDora).IsActiveAtTaikyoku(GameDataManager.Mahjong.EventType, GameDataManager.Mahjong.TakuType, GameDataManager.Mahjong.IsConstancyTaku, GameDataManager.Mahjong.IsCompetitionTaku))
			{
				return GameDataManager.Mahjong.IsAotenjoMode;
			}
			return true;
		}

		public static bool IsNextDoraOpenTurn(int remainCount)
		{
			if (!IsEffectiveTaiyoku())
			{
				return false;
			}
			for (int i = 0; i < _remainCountBorders.Length; i++)
			{
				int num = _remainCountBorders[i];
				if (remainCount == num)
				{
					return true;
				}
			}
			return false;
		}

		public static int GetNextDoraRemainBorder(int remainCount)
		{
			if (!IsEffectiveTaiyoku())
			{
				return -1;
			}
			for (int i = 0; i < _remainCountBorders.Length; i++)
			{
				int num = _remainCountBorders[i];
				if (remainCount > num)
				{
					return num;
				}
			}
			return 0;
		}

		public static int GetNextDoraRemainCount(int remainCount)
		{
			int nextDoraRemainBorder = GetNextDoraRemainBorder(remainCount);
			if (nextDoraRemainBorder < 0)
			{
				return -1;
			}
			return remainCount - nextDoraRemainBorder;
		}
	}

	public static class Kirisame
	{
		public static int AkadoraCount(int[] tepai, int agaripai)
		{
			return (from x in tepai.ToList()
				where IsAkahai((Pai)x)
				select x).Count() + (IsAkahai((Pai)agaripai) ? 1 : 0);
		}
	}

	public static class MeldBonus
	{
		public static bool IsEffectiveTaiyoku()
		{
			if (!GameDataManager.EventTaku.Current(MFG.Types.EventType.MeldBonus).IsActiveAtTaikyoku(GameDataManager.Mahjong.EventType, GameDataManager.Mahjong.TakuType, GameDataManager.Mahjong.IsConstancyTaku, GameDataManager.Mahjong.IsCompetitionTaku) && !GameDataManager.EventTaku.Current(MFG.Types.EventType.PCKGalaxy).IsActiveAtTaikyoku(GameDataManager.Mahjong.EventType, GameDataManager.Mahjong.TakuType, GameDataManager.Mahjong.IsConstancyTaku, GameDataManager.Mahjong.IsCompetitionTaku))
			{
				return GameDataManager.Mahjong.IsAotenjoMode;
			}
			return true;
		}

		public static int MeldCount(List<Player> players)
		{
			return (players?.Select((Player x) => x.meld)?.Sum((Meld x) => x.MeldCount)).GetValueOrDefault();
		}
	}

	public static class ComebackTaku
	{
		public static bool IsEffectiveTaiyoku()
		{
			if (!GameDataManager.EventTaku.Current(MFG.Types.EventType.Comeback).IsActiveAtTaikyoku(GameDataManager.Mahjong.EventType, GameDataManager.Mahjong.TakuType, GameDataManager.Mahjong.IsConstancyTaku, GameDataManager.Mahjong.IsCompetitionTaku))
			{
				return GameDataManager.Mahjong.IsAotenjoMode;
			}
			return true;
		}

		public static long GetDifferenceScore(List<Player> players, Cha myCha)
		{
			long score = players.Find((Player x) => x.m_cha == myCha).m_score;
			long num = players.Max((Player x) => x.m_score);
			Debug.Log($"ComebackTaku JiScore:{score} TopScore:{num}");
			return num - score;
		}

		public static bool IsKing(List<Player> players, Cha myCha)
		{
			Player player = players.Find((Player x) => x.IsValid && x.m_rank == 0);
			if (player == null)
			{
				return false;
			}
			return player.m_cha == myCha;
		}

		public static int BonusHan(List<Player> players, Cha myCha)
		{
			long[] array = new long[3] { 12000L, 24000L, 36000L };
			if (GameDataManager.Mahjong.IsAotenjoMode)
			{
				array = new long[3] { 20000000L, 100000000L, 500000000L };
			}
			long differenceScore = GetDifferenceScore(players, myCha);
			int num = 2;
			while (num >= 0 && array[num] > differenceScore)
			{
				num--;
			}
			return num + 1;
		}
	}

	public class AotenjoAgariInfo
	{
		public int Cha;

		public int HanNum;

		public int FuNum;

		public Kaze Kaze;

		public int Hoju;

		public bool IsTsumo => Hoju < 0;
	}

	public static class Aotenjo
	{
		public static List<AotenjoAgariInfo> AgariInfos = new List<AotenjoAgariInfo>();

		public static long MaxScore = 2000000000L;

		private static BigInteger CalcScore(int han, int fu, bool isOya, bool isTsumo, Kaze bombKaze, Player agariPlayer, Player furikomiPlayer, List<Player> players)
		{
			BigInteger bigInteger = 0;
			if (isTsumo)
			{
				Kaze kaze;
				for (kaze = Kaze.Ton; kaze <= Kaze.Sya; kaze++)
				{
					if (kaze != agariPlayer.m_jikaze)
					{
						bigInteger += CalcPayScore(han, fu, isOya, isTsumo, bombKaze, agariPlayer, players.Find((Player x) => x.IsValid && x.m_jikaze == kaze));
					}
				}
			}
			else
			{
				bigInteger = CalcPayScore(han, fu, isOya, isTsumo, bombKaze, agariPlayer, furikomiPlayer);
			}
			Debug.Log($"CalcScore {bigInteger} / {han}翻 / {fu}符 / {isOya}{isTsumo}");
			return bigInteger;
		}

		private static BigInteger CalcPayScore(int han, int fu, bool isOya, bool isTsumo, Kaze bombKaze, Player agariPlayer, Player agararePlayer)
		{
			BigInteger bigInteger = fu * BigInteger.Pow(2, han + 2);
			BigInteger bigInteger2 = bigInteger;
			Debug.Log("AotenjoBaseScore : " + bigInteger2);
			BigInteger score = bigInteger;
			if (isOya)
			{
				if (isTsumo)
				{
					score *= (BigInteger)6;
				}
				else
				{
					score *= (BigInteger)6;
				}
			}
			else if (isTsumo)
			{
				score *= (BigInteger)4;
			}
			else
			{
				score *= (BigInteger)4;
			}
			score = RoundScore(score);
			if (agariPlayer.m_isCatharsisKing || (agararePlayer != null && agararePlayer.m_isCatharsisKing))
			{
				score *= (BigInteger)2;
			}
			if (agariPlayer.m_jikaze == bombKaze || (agararePlayer != null && agararePlayer.m_jikaze == bombKaze))
			{
				score *= (BigInteger)2;
			}
			Debug.Log($"CalcPayScore {score} / {han}翻 / {fu}符 / {isOya}{isTsumo}");
			return score;
		}

		public static string GetYakuResultScore(int han, int fu, bool isTsumo, Kaze bombKaze, Player agariPlayer, Player houjuPlayer, List<Player> players)
		{
			Debug.Log("青天井DebugLog:役リザルトで青天井スコア計算機通るよ");
			bool isOya = agariPlayer.m_jikaze == Kaze.Ton;
			bool num = agariPlayer.m_cha == Cha.Jicha;
			BigInteger bigInteger = CalcScore(han, fu, isOya, isTsumo, bombKaze, agariPlayer, houjuPlayer, players);
			if (num)
			{
				((AotenjoEventGameData)GameDataManager.EventTaku.Current(MFG.Types.EventType.Aotenjo)).SetBestScore(bigInteger);
			}
			Debug.Log("青天井DebugLog:役リザルトで青天井スコア計算機通過できたよ");
			return GetScoreString(bigInteger, isYakuResult: true);
		}

		public static (BigInteger, BigInteger) GetScorePanelScore(ScoreMove score, int cha, List<Player> players, Cha bombPos)
		{
			BigInteger bigInteger = 0;
			BigInteger item = score.BeforeScore;
			bool isTsumo = AgariInfos[0].IsTsumo;
			AotenjoAgariInfo aotenjoAgariInfo = AgariInfos.Find((AotenjoAgariInfo x) => x.Cha == cha);
			bool num = aotenjoAgariInfo != null;
			bool flag = AgariInfos[0].Hoju == cha;
			Kaze jikaze = players.Find((Player x) => x.IsValid && x.m_cha == bombPos).m_jikaze;
			if (num)
			{
				Player agariPlayer = players.Find((Player x) => x.IsValid && x.m_cha == (Cha)cha);
				Player furikomiPlayer = (isTsumo ? null : players.Find((Player x) => x.IsValid && x.m_cha == (Cha)AgariInfos[0].Hoju));
				bigInteger = CalcScore(aotenjoAgariInfo.HanNum, aotenjoAgariInfo.FuNum, aotenjoAgariInfo.Kaze == Kaze.Ton, isTsumo, jikaze, agariPlayer, furikomiPlayer, players);
				item = bigInteger + score.BeforeScore + score.KyotakuScore + score.TsumifuScore;
			}
			else if (flag)
			{
				bigInteger = 0;
				foreach (AotenjoAgariInfo info in AgariInfos)
				{
					Player agariPlayer2 = players.Find((Player x) => x.IsValid && x.m_cha == (Cha)info.Cha);
					Player agararePlayer = players.Find((Player x) => x.IsValid && x.m_cha == (Cha)info.Hoju);
					bigInteger -= CalcPayScore(info.HanNum, info.FuNum, info.Kaze == Kaze.Ton, isTsumo, jikaze, agariPlayer2, agararePlayer);
				}
				item = bigInteger + score.BeforeScore + score.TsumifuScore;
			}
			else if (isTsumo)
			{
				Player agariPlayer3 = players.Find((Player x) => x.IsValid && x.m_cha == (Cha)AgariInfos[0].Cha);
				Player agararePlayer2 = players.Find((Player x) => x.IsValid && x.m_cha == (Cha)cha);
				bigInteger = -1 * CalcPayScore(AgariInfos[0].HanNum, AgariInfos[0].FuNum, AgariInfos[0].Kaze == Kaze.Ton, isTsumo, jikaze, agariPlayer3, agararePlayer2);
				item = bigInteger + score.BeforeScore + score.TsumifuScore;
			}
			return (bigInteger, item);
		}

		public static string GetScoreString(BigInteger score, bool isYakuResult = false, string color = "")
		{
			BigInteger bigInteger = BigInteger.Abs(score);
			if (bigInteger < (isYakuResult ? 1000000 : MaxScore))
			{
				return ((score < 0L) ? "－" : "") + bigInteger.ToString("N0");
			}
			if (bigInteger >= BigInteger.Pow(10, 72))
			{
				return GetScoreStringByPow10(score, isYakuResult);
			}
			return ToKetaAdditionalScore(score, isYakuResult, color);
		}

		private static string ToKetaAdditionalScore(BigInteger score, bool isYakuResult, string color = "")
		{
			bool flag = score < 0L;
			if (flag)
			{
				score *= (BigInteger)(-1);
			}
			string[] array = new string[18]
			{
				"",
				"万",
				"億",
				"兆",
				"京",
				"垓",
				isYakuResult ? "<sprite name=\"AotenjoJo\">" : (string.IsNullOrEmpty(color) ? "<sprite name=\"AotenjoSmall\">" : ("<sprite name=\"AotenjoSmall\" color=" + color + ">")),
				"穣",
				"溝",
				"澗",
				"正",
				"載",
				"極",
				"恒河沙",
				"阿僧祇",
				"那由多",
				"不可思議",
				"無量大数"
			};
			string text = score.ToString();
			int length = text.Length;
			int num = (length - 1) / 4;
			int num2 = 0;
			int num3 = 0;
			string text2 = "";
			bool flag2 = isYakuResult && num >= 2;
			if (flag2)
			{
				text2 = "</size>" + text2;
			}
			for (int num4 = length - 1; num4 >= 0; num4--)
			{
				if (num2 % 4 == 0)
				{
					if (flag2 && num3 == num - 1)
					{
						text2 = "\n<size=30>" + text2;
					}
					if (flag2)
					{
						text2 = "</color>" + text2;
					}
					text2 = array[num2 / 4] + text2;
					if (flag2)
					{
						text2 = "<color=orange>" + text2;
					}
					num3++;
				}
				text2 = text[num4] + text2;
				num2++;
			}
			if (!flag)
			{
				return text2;
			}
			return "－" + text2;
		}

		private static string GetScoreStringByPow10(BigInteger score, bool isYakuResult)
		{
			string text = BigInteger.Abs(score).ToString();
			int length = text.Length;
			int num = (isYakuResult ? 70 : 30);
			string text2 = $"{text[0]}.{text[1]}<size={num}>×</size>10<sup>{length - 1}</sup>";
			if (!(score < 0L))
			{
				return text2;
			}
			return "－" + text2;
		}

		private static BigInteger RoundScore(BigInteger score)
		{
			BigInteger bigInteger = score % 100;
			if (bigInteger == 0L)
			{
				return score;
			}
			return score + (100 - bigInteger);
		}
	}

	private static Dictionary<Yaku, (string kanji, string katakana)> YakuNames = new Dictionary<Yaku, (string, string)>
	{
		{
			Yaku.Tenho,
			("天和", "テンホー")
		},
		{
			Yaku.Chiho,
			("地和", "チーホー")
		},
		{
			Yaku.Renho,
			("人和", "レンホー")
		},
		{
			Yaku.Tyuren9,
			("九連宝燈", "チュウレンポウトウ 9面待ち")
		},
		{
			Yaku.TyurenTanki,
			("九連宝燈", "チュウレンポウトウ")
		},
		{
			Yaku.Chinroto,
			("清老頭", "チンロートー")
		},
		{
			Yaku.Tsuiso,
			("字一色", "ツーイーソー")
		},
		{
			Yaku.Ryuiso,
			("緑一色", "リューイーソー")
		},
		{
			Yaku.Kokushi13,
			("国士無双", "コクシムソウ 13面待ち")
		},
		{
			Yaku.KokushiTanki,
			("国士無双", "コクシムソウ")
		},
		{
			Yaku.Sukantsu,
			("四槓子", "スーカンツ")
		},
		{
			Yaku.Suanko,
			("四暗刻", "スーアンコ")
		},
		{
			Yaku.SuankoTanki,
			("四暗刻", "スーアンコ 単騎待ち")
		},
		{
			Yaku.Daisushi,
			("大四喜", "ダイスーシー")
		},
		{
			Yaku.Syosushi,
			("小四喜", "ショウスーシー")
		},
		{
			Yaku.Daisangen,
			("大三元", "ダイサンゲン")
		},
		{
			Yaku.Daisyarin,
			("大車輪", "ダイシャリン")
		},
		{
			Yaku.Surenko,
			("四連刻", "スーレンコ")
		},
		{
			Yaku.Parenchan,
			("八連荘", "パーレンチャン")
		},
		{
			Yaku.Kazoeyakuman,
			("数え役満", "数え役満")
		},
		{
			Yaku.Chiniso,
			("清一色", "チンイーソー")
		},
		{
			Yaku.Honroto,
			("混老頭", "ホンロートー")
		},
		{
			Yaku.Syosangen,
			("小三元", "ショウサンゲン")
		},
		{
			Yaku.Nagashimangan,
			("流し満貫", "流し満貫")
		},
		{
			Yaku.Shisanputo,
			("十三不塔", "シーサンプトウ")
		},
		{
			Yaku.Junchan,
			("純全帯公九", "ジュンチャンタ")
		},
		{
			Yaku.Ryanpeko,
			("二盃口", "リャンペーコー")
		},
		{
			Yaku.Honiso,
			("混一色", "ホンイーソー")
		},
		{
			Yaku.DoubleRichi,
			("ダブル立直", "ダブルリーチ")
		},
		{
			Yaku.Sankantsu,
			("三槓子", "サンカンツ")
		},
		{
			Yaku.Sananko,
			("三暗刻", "サンアンコ")
		},
		{
			Yaku.Toitoiho,
			("対々和", "トイトイホー")
		},
		{
			Yaku.Chitoitsu,
			("七対子", "チートイツ")
		},
		{
			Yaku.Sansyokudoko,
			("三色同刻", "サンショクドウコー")
		},
		{
			Yaku.Sansyokudojun,
			("三色同順", "サンショクドウジュン")
		},
		{
			Yaku.Chanta,
			("混全帯公九", "チャンタ")
		},
		{
			Yaku.Ikkitsukan,
			("一気通貫", "イッキツーカン")
		},
		{
			Yaku.Sanrenko,
			("三連刻", "サンレンコ")
		},
		{
			Yaku.Haitei,
			("海底撈月", "ハイテイ")
		},
		{
			Yaku.Hotei,
			("河底撈魚", "ホウテイ")
		},
		{
			Yaku.Chankan,
			("槍槓", "チャンカン")
		},
		{
			Yaku.Rinsyan,
			("嶺上開花", "リンシャンカイホー")
		},
		{
			Yaku.Ipeiko,
			("一盃口", "イーペーコー")
		},
		{
			Yaku.Tanyao,
			("断公九", "タンヤオ")
		},
		{
			Yaku.Pinfu,
			("平和", "ピンフ")
		},
		{
			Yaku.Richi,
			("立直", "リーチ")
		},
		{
			Yaku.Ippatsu,
			("一発", "イッパツ")
		},
		{
			Yaku.Menzen,
			("門前清自摸和", "メンゼンツモ")
		},
		{
			Yaku.Haku,
			("三元牌", "ヤクハイ(白)")
		},
		{
			Yaku.Hatsu,
			("三元牌", "ヤクハイ(發)")
		},
		{
			Yaku.Tyun,
			("三元牌", "ヤクハイ(中)")
		},
		{
			Yaku.Bakaze,
			("風牌", "ヤクハイ(場風牌)")
		},
		{
			Yaku.Jikaze,
			("風牌", "ヤクハイ(自風牌)")
		},
		{
			Yaku.Dora,
			("ドラ", "ドラ")
		},
		{
			Yaku.ChinisoNaki,
			("清一色", "チンイーソー")
		},
		{
			Yaku.JunchanNaki,
			("純全帯公九", "ジュンチャンタ")
		},
		{
			Yaku.HonisoNaki,
			("混一色", "ホンイーソー")
		},
		{
			Yaku.SansyokudojunNaki,
			("三色同順", "サンショクドウジュン")
		},
		{
			Yaku.ChantaNaki,
			("混全帯公九", "チャンタ")
		},
		{
			Yaku.IkkitsukanNaki,
			("一気通貫", "イッキツーカン")
		}
	};

	private static Dictionary<Yaku, int> YakuHans = new Dictionary<Yaku, int>
	{
		{
			Yaku.Tenho,
			13
		},
		{
			Yaku.Chiho,
			13
		},
		{
			Yaku.Renho,
			13
		},
		{
			Yaku.Tyuren9,
			26
		},
		{
			Yaku.TyurenTanki,
			13
		},
		{
			Yaku.Chinroto,
			13
		},
		{
			Yaku.Tsuiso,
			13
		},
		{
			Yaku.Ryuiso,
			13
		},
		{
			Yaku.Kokushi13,
			26
		},
		{
			Yaku.KokushiTanki,
			13
		},
		{
			Yaku.Sukantsu,
			13
		},
		{
			Yaku.Suanko,
			13
		},
		{
			Yaku.SuankoTanki,
			26
		},
		{
			Yaku.Daisushi,
			26
		},
		{
			Yaku.Syosushi,
			13
		},
		{
			Yaku.Daisangen,
			13
		},
		{
			Yaku.Daisyarin,
			13
		},
		{
			Yaku.Surenko,
			13
		},
		{
			Yaku.Parenchan,
			13
		},
		{
			Yaku.Kazoeyakuman,
			13
		},
		{
			Yaku.Chiniso,
			6
		},
		{
			Yaku.Honroto,
			2
		},
		{
			Yaku.Syosangen,
			2
		},
		{
			Yaku.Nagashimangan,
			4
		},
		{
			Yaku.Shisanputo,
			13
		},
		{
			Yaku.Junchan,
			3
		},
		{
			Yaku.Ryanpeko,
			3
		},
		{
			Yaku.Honiso,
			3
		},
		{
			Yaku.DoubleRichi,
			2
		},
		{
			Yaku.Sankantsu,
			2
		},
		{
			Yaku.Sananko,
			2
		},
		{
			Yaku.Toitoiho,
			2
		},
		{
			Yaku.Chitoitsu,
			2
		},
		{
			Yaku.Sansyokudoko,
			2
		},
		{
			Yaku.Sansyokudojun,
			2
		},
		{
			Yaku.Chanta,
			2
		},
		{
			Yaku.Ikkitsukan,
			2
		},
		{
			Yaku.Sanrenko,
			2
		},
		{
			Yaku.Haitei,
			1
		},
		{
			Yaku.Hotei,
			1
		},
		{
			Yaku.Chankan,
			1
		},
		{
			Yaku.Rinsyan,
			1
		},
		{
			Yaku.Ipeiko,
			1
		},
		{
			Yaku.Tanyao,
			1
		},
		{
			Yaku.Pinfu,
			1
		},
		{
			Yaku.Richi,
			1
		},
		{
			Yaku.Ippatsu,
			1
		},
		{
			Yaku.Menzen,
			1
		},
		{
			Yaku.Haku,
			1
		},
		{
			Yaku.Hatsu,
			1
		},
		{
			Yaku.Tyun,
			1
		},
		{
			Yaku.Bakaze,
			1
		},
		{
			Yaku.Jikaze,
			1
		},
		{
			Yaku.Dora,
			1
		},
		{
			Yaku.ChinisoNaki,
			5
		},
		{
			Yaku.JunchanNaki,
			2
		},
		{
			Yaku.HonisoNaki,
			2
		},
		{
			Yaku.SansyokudojunNaki,
			1
		},
		{
			Yaku.ChantaNaki,
			1
		},
		{
			Yaku.IkkitsukanNaki,
			1
		}
	};

	private static Dictionary<Han, string> HanNames = new Dictionary<Han, string>
	{
		{
			Han.Nothing,
			""
		},
		{
			Han.Han1,
			"一翻"
		},
		{
			Han.Han2,
			"二翻"
		},
		{
			Han.Han3,
			"三翻"
		},
		{
			Han.Han4,
			"四翻"
		},
		{
			Han.Mangan,
			"満貫"
		},
		{
			Han.Haneman,
			"跳満"
		},
		{
			Han.Baiman,
			"倍満"
		},
		{
			Han.Sanbaiman,
			"三倍満"
		},
		{
			Han.Yakuman,
			"役満"
		},
		{
			Han.Yakuman2,
			"二倍役満"
		},
		{
			Han.Yakuman3,
			"三倍役満"
		},
		{
			Han.Yakuman4,
			"四倍役満"
		},
		{
			Han.Yakuman5,
			"五倍役満"
		},
		{
			Han.Yakuman6,
			"六倍役満"
		},
		{
			Han.Yakuman7,
			"七倍役満"
		}
	};

	private static int[] m_openPaiNums = new int[34];

	public static bool m_isLoaded = false;

	public static bool m_isOpen = false;

	public static bool m_isHiSpeedMode = false;

	public static bool m_isTsumogiriMode = false;

	private static Dictionary<Pai, int> RestPaiNums = new Dictionary<Pai, int>();

	public static Kaze m_jichaStartKaze = Kaze.Ton;

	private static List<Pai> m_doras = new List<Pai>();

	private static List<Pai> m_omoteDoras = new List<Pai>();

	private static List<Pai> m_uraDoras = new List<Pai>();

	private static Pai m_lastAddDora = Pai.NONE;

	private static Tile m_touchedTile = null;

	public static Tile m_lastTargetTile = null;

	private static ViewType m_viewType = ViewType.Type1;

	private static int m_noInputCount = 0;

	public static long m_StartScore { get; set; }

	private static bool IsKokushiPossible(Pai pai)
	{
		foreach (Pai item in new List<Pai>
		{
			Pai.M1,
			Pai.M9,
			Pai.P1,
			Pai.P9,
			Pai.S1,
			Pai.S9,
			Pai.J1,
			Pai.J2,
			Pai.J3,
			Pai.J4,
			Pai.J5,
			Pai.J6,
			Pai.J7
		})
		{
			if (pai != item && RestPaiNums[item] == 0)
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsKokushiAnpai(Pai pai)
	{
		if (pai.ToHaiID().IsJihai() && !IsKokushiPossible(pai) && RestPaiNums[pai] == 0)
		{
			return true;
		}
		return false;
	}

	public static void SubRestPaiNum(Pai pai)
	{
		pai = AkaToNormal(pai);
		RestPaiNums[pai]--;
	}

	public static int GetRestPaiNum(Pai pai)
	{
		return RestPaiNums[pai];
	}

	public static void ResetOpenPaiNum()
	{
		RestPaiNums.Clear();
		foreach (Pai value in Enum.GetValues(typeof(Pai)))
		{
			RestPaiNums.Add(value, 4);
		}
	}

	public static void SetOpen(bool open)
	{
		m_isOpen = open;
	}

	public static bool IsOpen()
	{
		return m_isOpen;
	}

	public static string GetYakuName(Yaku yaku)
	{
		return YakuNames[yaku].kanji;
	}

	public static string GetYakuKanaName(Yaku yaku)
	{
		return YakuNames[yaku].katakana;
	}

	public static int GetHanNum(Yaku yaku, bool isFireReach, bool isOpenReach, bool isAtomicReach, bool isOpenFurikomi = false)
	{
		int num = YakuHans[yaku];
		switch (yaku)
		{
		case Yaku.DoubleRichi:
		case Yaku.Richi:
			if (GameDataManager.Mahjong.IsMentanpinMode)
			{
				num++;
			}
			if (isFireReach)
			{
				num += 4;
			}
			else if (isAtomicReach)
			{
				num += 2;
			}
			else if (isOpenReach)
			{
				num = ((!isOpenFurikomi) ? (num + 1) : 13);
			}
			break;
		case Yaku.Tanyao:
		case Yaku.Pinfu:
			if (GameDataManager.Mahjong.IsMentanpinMode)
			{
				num++;
			}
			break;
		case Yaku.Ryuiso:
			if (GameDataManager.Mahjong.IsAllGreenBonusMode)
			{
				num += 13;
			}
			break;
		}
		return num;
	}

	public static int GetNoNakiHanNum(Yaku yaku, bool isOpenReach, bool isFireReach, bool isAtomicReach)
	{
		Yaku yaku2 = yaku;
		switch (yaku)
		{
		case Yaku.ChinisoNaki:
			yaku2 = Yaku.Chiniso;
			break;
		case Yaku.JunchanNaki:
			yaku2 = Yaku.Junchan;
			break;
		case Yaku.HonisoNaki:
			yaku2 = Yaku.Honiso;
			break;
		case Yaku.SansyokudojunNaki:
			yaku2 = Yaku.Sansyokudojun;
			break;
		case Yaku.ChantaNaki:
			yaku2 = Yaku.Chanta;
			break;
		case Yaku.IkkitsukanNaki:
			yaku2 = Yaku.Ikkitsukan;
			break;
		}
		return GetHanNum(yaku2, isFireReach, isOpenReach, isAtomicReach);
	}

	public static string GetHanName(Han han)
	{
		return HanNames[han];
	}

	public static bool IsAkahai(Pai pai)
	{
		if (Pai.M1R <= pai)
		{
			return pai <= Pai.P9R;
		}
		return false;
	}

	public static bool IsIncludeAkahai(Pai pai)
	{
		return IsIncludeAkahai(pai, GameDataManager.Mahjong.EventType, GameDataManager.Mahjong.TakuType);
	}

	public static bool IsIncludeAkahai(Pai pai, MFG.Types.EventType eventType, TakuType takuType)
	{
		switch (takuType)
		{
		case TakuType.Tonpu:
			if (eventType == MFG.Types.EventType.Kirisame)
			{
				if (Pai.M1 > pai || pai > Pai.P9)
				{
					if (Pai.M1R <= pai)
					{
						return pai <= Pai.P9R;
					}
					return false;
				}
				return true;
			}
			if (pai != Pai.M5 && pai != Pai.S5 && pai != Pai.P5 && pai != Pai.M5R && pai != Pai.S5R)
			{
				return pai == Pai.P5R;
			}
			return true;
		case TakuType.Sanma:
			switch (eventType)
			{
			case MFG.Types.EventType.Mentanpin:
				if (pai != Pai.M5 && pai != Pai.M5R && pai != Pai.S5 && pai != Pai.P5 && pai != Pai.S5R)
				{
					return pai == Pai.P5R;
				}
				return true;
			case MFG.Types.EventType.Aotenjo:
			case MFG.Types.EventType.Kirisame:
				if ((Pai.S1 > pai || pai > Pai.P9) && (Pai.S1R > pai || pai > Pai.P9R) && pai != Pai.M1 && pai != Pai.M9 && pai != Pai.M1R)
				{
					return pai == Pai.M9R;
				}
				return true;
			default:
				if (pai != Pai.S5 && pai != Pai.P5 && pai != Pai.S5R)
				{
					return pai == Pai.P5R;
				}
				return true;
			}
		case TakuType.Nima:
			if (pai != Pai.S5 && pai != Pai.P5 && pai != Pai.S5R)
			{
				return pai == Pai.P5R;
			}
			return true;
		default:
			return false;
		}
	}

	public static Pai AkaToNormal(Pai pai)
	{
		if (IsAkahai(pai))
		{
			return pai - 64;
		}
		return pai;
	}

	public static Pai NormalToAka(Pai pai)
	{
		if (!IsAkahai(pai))
		{
			return pai + 64;
		}
		return pai;
	}

	public static Cha GetCha(Kaze jicha, Kaze tacha)
	{
		int num = tacha - jicha;
		if (num < 0)
		{
			num += Enum.GetNames(typeof(Cha)).Length;
		}
		if (GameDataManager.Mahjong.TakuType == TakuType.Nima && tacha != jicha)
		{
			num = 2;
		}
		return (Cha)num;
	}

	public static int GetWorstRank(TakuType taku)
	{
		int result = 0;
		switch (taku)
		{
		case TakuType.Tonpu:
		case TakuType.Hanchan:
			result = 3;
			break;
		case TakuType.Sanma:
			result = 2;
			break;
		case TakuType.Nima:
			result = 1;
			break;
		}
		return result;
	}

	public static RankType GetRankType(TakuType taku, Rank rank)
	{
		if (rank == (Rank)GetWorstRank(taku))
		{
			return RankType.RankTypeWorst;
		}
		if (rank == Rank.Rank1)
		{
			return RankType.RankTypeTop;
		}
		return RankType.RankTypeOther;
	}

	public static Pai DoraHyoujiToDora(Pai pai)
	{
		switch (pai)
		{
		case Pai.M9:
			return Pai.M1;
		case Pai.S9:
			return Pai.S1;
		case Pai.P9:
			return Pai.P1;
		case Pai.J4:
			return Pai.J1;
		case Pai.J7:
			return Pai.J5;
		case Pai.M5R:
			return Pai.M6;
		case Pai.S5R:
			return Pai.S6;
		case Pai.P5R:
			return Pai.P6;
		default:
			if (GameDataManager.Mahjong.IsKirisameMode)
			{
				if (GameDataManager.Mahjong.IsAotenjoMode && (pai == Pai.M1 || pai == Pai.M1R))
				{
					return Pai.M9;
				}
				if (GameDataManager.Mahjong.IsAotenjoMode && (pai == Pai.M9 || pai == Pai.M9R))
				{
					return Pai.M1;
				}
				return pai switch
				{
					Pai.M9R => Pai.M1, 
					Pai.S9R => Pai.S1, 
					Pai.P9R => Pai.P1, 
					Pai.J4R => Pai.J1, 
					Pai.J7R => Pai.J5, 
					_ => AkaToNormal(pai) + 1, 
				};
			}
			if (GameDataManager.Mahjong.TakuType == TakuType.Sanma && !GameDataManager.Mahjong.IsJihaiUnusedSanma)
			{
				if (pai == Pai.M1)
				{
					return Pai.M9;
				}
			}
			else if (GameDataManager.Mahjong.TakuType == TakuType.Nima && pai == Pai.J2)
			{
				return Pai.J1;
			}
			return pai + 1;
		}
	}

	public static bool IsDora(Pai pai, DoraRange doraRange = DoraRange.All)
	{
		if (IsPureDora(pai, doraRange))
		{
			return true;
		}
		if (IsAkahai(pai))
		{
			return true;
		}
		return false;
	}

	public static bool IsPureDora(Pai pai, DoraRange doraRange = DoraRange.All)
	{
		return GetPureDoraCount(pai, doraRange) > 0;
	}

	public static int GetPureDoraCount(Pai pai, DoraRange doraRange = DoraRange.All)
	{
		int num = 0;
		Pai pai2 = AkaToNormal(pai);
		if (doraRange == DoraRange.All || doraRange == DoraRange.Omote)
		{
			foreach (Pai omoteDora in m_omoteDoras)
			{
				if (omoteDora == pai2)
				{
					num++;
				}
			}
		}
		if (doraRange == DoraRange.All || doraRange == DoraRange.Ura)
		{
			foreach (Pai uraDora in m_uraDoras)
			{
				if (uraDora == pai2)
				{
					num++;
				}
			}
		}
		return num;
	}

	public static void AddDora(Pai pai, bool isOmote)
	{
		Debug.Log($"AddDora : {pai}");
		m_lastAddDora = pai;
		m_doras.Add(pai);
		if (isOmote)
		{
			m_omoteDoras.Add(pai);
		}
		else
		{
			m_uraDoras.Add(pai);
		}
	}

	public static Pai GetLastAddDora()
	{
		return m_lastAddDora;
	}

	public static void ResetDora()
	{
		m_doras.Clear();
		m_omoteDoras.Clear();
		m_uraDoras.Clear();
		m_lastAddDora = Pai.NONE;
	}

	public static Tile GetTouchedTile()
	{
		return m_touchedTile;
	}

	public static void SetTouchedTile(Tile item)
	{
		m_touchedTile = item;
	}

	public static bool IsYakuAgari(YakuData yaku, Yaku isYaku)
	{
		if ((yaku.YakuBit & (ulong)(1L << (int)isYaku)) != 0L)
		{
			return true;
		}
		return false;
	}

	public static bool IsReachAgari(YakuData yaku)
	{
		if ((yaku.YakuBit & 0x200000000000L) != 0L)
		{
			return true;
		}
		if ((yaku.YakuBit & 0x10000000) != 0L)
		{
			return true;
		}
		return false;
	}

	public static List<Yaku> CreateYakusFromYakuBit(ulong yakuBit)
	{
		List<Yaku> list = new List<Yaku>();
		for (int i = 0; i < 64; i++)
		{
			if ((yakuBit & (ulong)(1L << i)) != 0L)
			{
				list.Add((Yaku)i);
			}
		}
		return list;
	}

	public static bool IsKuitanEnable(MFG.Types.EventType eventType)
	{
		if (eventType == MFG.Types.EventType.Kirisame)
		{
			return false;
		}
		return true;
	}

	public static int GetPlayerNum(TakuType type)
	{
		return type switch
		{
			TakuType.Nima => 2, 
			TakuType.Sanma => 3, 
			_ => 4, 
		};
	}

	public static void SetLastTargetTile(Tile item)
	{
		m_lastTargetTile = item;
	}

	public static Tile GetLastTargetTile()
	{
		return m_lastTargetTile;
	}

	public static int GetBaseScore(Han hanType, int fu)
	{
		if (hanType >= Han.Yakuman)
		{
			return 8000 * (int)(hanType - 9 + 1);
		}
		switch (hanType)
		{
		case Han.Sanbaiman:
			return 6000;
		case Han.Baiman:
			return 4000;
		case Han.Haneman:
			return 3000;
		case Han.Mangan:
			return 2000;
		default:
		{
			int num = fu << (int)(hanType + 2);
			if (num >= 1920)
			{
				num = 2000;
			}
			return num;
		}
		}
	}

	public static Han GetHanType(int hanCount, int fu, int yakumanCount = 0)
	{
		if (yakumanCount > 0)
		{
			return (Han)(9 + yakumanCount - 1);
		}
		if (hanCount >= 13)
		{
			return Han.Yakuman;
		}
		if (hanCount >= 11)
		{
			return Han.Sanbaiman;
		}
		if (hanCount >= 8)
		{
			return Han.Baiman;
		}
		if (hanCount >= 6)
		{
			return Han.Haneman;
		}
		if (hanCount >= 5)
		{
			return Han.Mangan;
		}
		if (GetBaseScore((Han)hanCount, fu) >= 2000)
		{
			return Han.Mangan;
		}
		return (Han)hanCount;
	}

	public static (long totalPayment, long koPayment, long oyaPayment) GetScore(TakuType takuType, Han hanType, int fu, bool isOya, bool isTsumo)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int baseScore = GetBaseScore(hanType, fu);
		int num4 = RoundupTenthPlace((isOya ? 6 : 4) * baseScore);
		switch (takuType)
		{
		case TakuType.Nima:
			num = num4;
			num2 = num4;
			num3 = num4;
			break;
		case TakuType.Sanma:
			num = RoundupTenthPlace(num4 / 2);
			num2 = num;
			num3 = ((!isTsumo) ? num4 : (isOya ? (2 * num) : (num2 + num)));
			break;
		default:
			num = RoundupTenthPlace(((!isOya) ? 1 : 2) * baseScore);
			num2 = RoundupTenthPlace(2 * baseScore);
			num3 = ((!isTsumo) ? num4 : (isOya ? (3 * num) : (num2 + 2 * num)));
			break;
		}
		return (totalPayment: num3, koPayment: num, oyaPayment: num2);
	}

	private static int RoundupTenthPlace(int value)
	{
		return value / 100 * 100 + ((value % 100 > 0) ? 100 : 0);
	}

	public static bool IsHiSpeedMode()
	{
		if (!m_isHiSpeedMode)
		{
			return DebugFlag.IsActive(DebugFlag.Flag.HighSpeedMode);
		}
		return true;
	}

	public static async UniTask DelayIgnoreHighSpeedMode(int milliseconds, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!IsHiSpeedMode())
		{
			await UniTask.Delay(milliseconds, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		}
	}

	public static async UniTask DelayIgnoreHighSpeedMode(TimeSpan delayTimeSpan, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!IsHiSpeedMode())
		{
			await UniTask.Delay(delayTimeSpan, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		}
	}

	public static async UniTask CpuWait(Player player, bool isSute = false)
	{
		if (player.m_isCPU || GameDataManager.Mahjong.IsHaifuPlay)
		{
			await DelayIgnoreHighSpeedMode(isSute ? UnityEngine.Random.Range(500, 900) : 1000);
		}
	}

	public static ViewType GetViewType()
	{
		return m_viewType;
	}

	public static void SetViewType(ViewType type)
	{
		m_viewType = type;
	}

	public static int GetRemainingTurn(TakuType taku, int nokoriHaiNum)
	{
		return nokoriHaiNum / GetPlayerNum(taku);
	}

	public static void IncrementNoInputCount()
	{
		m_noInputCount++;
	}

	public static bool IsNoInputAutoTsumogiri()
	{
		return m_noInputCount >= 3;
	}

	public static void ResetNoInputCount()
	{
		m_noInputCount = 0;
	}
}
