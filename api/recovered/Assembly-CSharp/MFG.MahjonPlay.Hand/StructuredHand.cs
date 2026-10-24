using System.Collections.Generic;
using System.Linq;
using MFG.Types;

namespace MFG.MahjonPlay.Hand;

public class StructuredHand
{
	private enum AgariType
	{
		Ron,
		Tsumo,
		Max
	}

	public YakuInfo YakuInfo = new YakuInfo();

	public BlockSet Blocks = new BlockSet();

	private MachiType[] m_machiTypes;

	public List<ScoreInfo> m_scoreInfo;

	private const int INVALID_SHANTEN_COUNT = 14;

	private int m_shantenCount = 14;

	private Counter m_counter;

	private bool[] m_isAvailableIsolatedFlags;

	public MachiType AgariMachiType;

	public List<HaiID> AgariHaiIDs;

	public int CalculatedAgariHaiIDCount;

	public int ShantenCount()
	{
		if (m_shantenCount == 14)
		{
			m_shantenCount = EvaluateShantenCount();
		}
		return m_shantenCount;
	}

	private int EvaluateShantenCount()
	{
		return Blocks.DivideType switch
		{
			DivideType.Normal => EvaluateShantenNormalHand(), 
			DivideType.Chitoitsu => EvaluateShantenCountChitoitsuHand(), 
			DivideType.Kokushi => EvaluateShantenCountKokushiHand(), 
			_ => 14, 
		};
	}

	private int EvaluateShantenNormalHand()
	{
		int num = Blocks.ToitsuCount();
		int num2 = Blocks.MentsuCount();
		int num3 = Blocks.TatsuCount();
		int num4 = num + num3;
		int num5 = ((num > 0) ? 5 : 4) - num2;
		if (num4 > num5)
		{
			num4 = num5;
		}
		int num6 = Blocks.EffectiveIsolatedCount(GetAvailableIsolatedFlags());
		int num7 = 5 - num2 - num4 - num6;
		if (num7 < 0)
		{
			num7 = 0;
		}
		return 8 - num4 - 2 * num2 + num7;
	}

	private int EvaluateShantenCountChitoitsuHand()
	{
		int num = Blocks.ToitsuCount();
		int num2 = Blocks.IsolatedCount();
		int num3 = Blocks.Count();
		for (int i = 0; i < num3; i++)
		{
			Block block = Blocks[i];
			if (block.BlockType == BlockType.AnKoutsu)
			{
				num++;
				if (Blocks.Exists(block.HaiID, BlockType.Isolated))
				{
					num2--;
				}
			}
		}
		int num4 = 7 - num - num2;
		if (num4 < 0)
		{
			num4 = 0;
		}
		return 6 - num + num4;
	}

	private int EvaluateShantenCountKokushiHand()
	{
		Counter counter = GetCounter();
		int num = 0;
		bool flag = false;
		HaiID[] array = HaiUtility.YaochuHais();
		foreach (HaiID haiID in array)
		{
			int count = counter.GetCount(haiID);
			if (count > 0)
			{
				num++;
				if (!flag && count >= 2)
				{
					num++;
					flag = true;
				}
			}
		}
		return 13 - num;
	}

	public MachiType[] GetMachiTypes()
	{
		if (m_machiTypes == null)
		{
			m_machiTypes = EvaluateMachiTypes();
			CreateAgariMachiData();
		}
		return m_machiTypes;
	}

	private MachiType[] EvaluateMachiTypes()
	{
		return Blocks.DivideType switch
		{
			DivideType.Normal => EvaluateMachiTypesNormalHand(), 
			DivideType.Chitoitsu => EvaluateMachiTypesChitoitsuHand(), 
			DivideType.Kokushi => EvaluateMachiTypesKokushiHand(), 
			_ => null, 
		};
	}

	private MachiType[] EvaluateMachiTypesNormalHand()
	{
		MachiType[] array = new MachiType[34];
		int num = Blocks.MentsuCount();
		int num2 = Blocks.ToitsuCount();
		int num3 = Blocks.TatsuCount();
		int num4 = Blocks.Count();
		bool[] availableIsolatedFlags = GetAvailableIsolatedFlags();
		int num5 = Blocks.MentsuCount() + num2 + num3;
		for (int i = 0; i < num4; i++)
		{
			Block block = Blocks[i];
			HaiID haiID = block.HaiID;
			SuID su = haiID.GetSu();
			switch (block.BlockType)
			{
			case BlockType.Toitsu:
				if (num5 < 5 || num2 >= 2)
				{
					array[(int)haiID] |= MachiType.Shabo;
				}
				break;
			case BlockType.RyanmenTatsu:
				if (su > SuID.SU1)
				{
					array[(int)(haiID - 1)] |= MachiType.Ryanmen;
				}
				if (su < SuID.SU8)
				{
					array[(int)(haiID + 2)] |= MachiType.Ryanmen;
				}
				if (num5 >= 5 && num2 == 0)
				{
					if (availableIsolatedFlags[(int)haiID])
					{
						array[(int)haiID] |= MachiType.ShantenUp;
					}
					if (availableIsolatedFlags[(int)(haiID + 1)])
					{
						array[(int)(haiID + 1)] |= MachiType.ShantenUp;
					}
				}
				break;
			case BlockType.PenchanTatsu:
				if (su == SuID.SU1)
				{
					array[(int)(haiID + 2)] |= MachiType.Penchan;
				}
				if (su == SuID.SU8)
				{
					array[(int)(haiID - 1)] |= MachiType.Penchan;
				}
				if (num5 >= 5 && num2 == 0)
				{
					if (availableIsolatedFlags[(int)haiID])
					{
						array[(int)haiID] |= MachiType.ShantenUp;
					}
					if (availableIsolatedFlags[(int)(haiID + 1)])
					{
						array[(int)(haiID + 1)] |= MachiType.ShantenUp;
					}
				}
				break;
			case BlockType.KanchanTatsu:
				array[(int)(haiID + 1)] |= MachiType.Kanchan;
				if (num5 >= 5 && num2 == 0)
				{
					if (availableIsolatedFlags[(int)haiID])
					{
						array[(int)haiID] |= MachiType.ShantenUp;
					}
					if (availableIsolatedFlags[(int)(haiID + 2)])
					{
						array[(int)(haiID + 2)] |= MachiType.ShantenUp;
					}
				}
				break;
			case BlockType.Isolated:
				if (num5 >= 5)
				{
					break;
				}
				if (availableIsolatedFlags[(int)haiID])
				{
					array[(int)haiID] |= MachiType.Tanki;
				}
				if (haiID.IsSuhai() && (num5 < 4 || num2 > 0))
				{
					if (su > SuID.SU2 && availableIsolatedFlags[(int)(haiID - 2)])
					{
						array[(int)(haiID - 2)] |= MachiType.ShantenUp;
					}
					if (su > SuID.SU1 && availableIsolatedFlags[(int)(haiID - 1)])
					{
						array[(int)(haiID - 1)] |= MachiType.ShantenUp;
					}
					if (su < SuID.SU9 && availableIsolatedFlags[(int)(haiID + 1)])
					{
						array[(int)(haiID + 1)] |= MachiType.ShantenUp;
					}
					if (su < SuID.SU8 && availableIsolatedFlags[(int)(haiID + 2)])
					{
						array[(int)(haiID + 2)] |= MachiType.ShantenUp;
					}
				}
				break;
			}
		}
		int num6 = num2 + num3;
		int num7 = ((num2 > 0) ? 5 : 4) - num;
		if (num6 > num7)
		{
			num6 = num7;
		}
		if (num + num6 < 5)
		{
			int num8 = Blocks.EffectiveIsolatedCount(GetAvailableIsolatedFlags());
			if (5 - num - num6 - num8 > 0)
			{
				for (int j = 0; j < 34; j++)
				{
					if (availableIsolatedFlags[j])
					{
						array[j] |= MachiType.ShantenUp;
					}
				}
			}
		}
		EraseInvalidMachiHai(array);
		return array;
	}

	private MachiType[] EvaluateMachiTypesChitoitsuHand()
	{
		MachiType[] array = new MachiType[34];
		Counter counter = GetCounter();
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < 34; i++)
		{
			HaiID haiID = (HaiID)i;
			int count = counter.GetCount(haiID);
			if (count == 1)
			{
				num++;
			}
			else if (count >= 2)
			{
				num2++;
			}
		}
		bool flag = 7 - num2 - num > 0;
		for (int j = 0; j < 34; j++)
		{
			int count2 = counter.GetCount((HaiID)j);
			if (count2 == 1 || (flag && count2 == 0))
			{
				array[j] |= MachiType.Tanki;
			}
		}
		EraseInvalidMachiHai(array);
		return array;
	}

	private MachiType[] EvaluateMachiTypesKokushiHand()
	{
		MachiType[] array = new MachiType[34];
		bool flag = false;
		Counter counter = GetCounter();
		HaiID[] array2 = HaiUtility.YaochuHais();
		foreach (HaiID haiID in array2)
		{
			if (counter.GetCount(haiID) >= 2)
			{
				flag = true;
				break;
			}
		}
		array2 = HaiUtility.YaochuHais();
		foreach (HaiID haiID2 in array2)
		{
			if (!flag || counter.GetCount(haiID2) == 0)
			{
				array[(int)haiID2] |= MachiType.Tanki;
			}
		}
		EraseInvalidMachiHai(array);
		return array;
	}

	private void EraseInvalidMachiHai(MachiType[] machiTypes)
	{
		Counter counter = GetCounter();
		for (int i = 0; i < 34; i++)
		{
			HaiID haiID = (HaiID)i;
			if (!haiID.IsAvailable())
			{
				machiTypes[i] = MachiType.None;
			}
			else if (counter[haiID] >= 4)
			{
				machiTypes[i] = MachiType.None;
			}
		}
	}

	private Counter GetCounter()
	{
		if (m_counter == null)
		{
			m_counter = Blocks.CreateCounter();
		}
		return m_counter;
	}

	private bool[] GetAvailableIsolatedFlags()
	{
		if (m_isAvailableIsolatedFlags == null)
		{
			m_isAvailableIsolatedFlags = new bool[34];
			Counter counter = GetCounter();
			bool flag = Blocks.MentsuCount() + Blocks.TatsuCount() >= 4 && Blocks.ToitsuCount() == 0;
			for (int i = 0; i < 34; i++)
			{
				HaiID haiID = (HaiID)i;
				m_isAvailableIsolatedFlags[i] = counter[haiID] < 4 && (!(haiID.IsJihai() || flag) || counter[haiID] < (haiID.IsJihai() ? 2 : 4));
			}
		}
		return m_isAvailableIsolatedFlags;
	}

	private void CreateAgariMachiData()
	{
		if (m_shantenCount > 0)
		{
			return;
		}
		AgariHaiIDs = new List<HaiID>();
		for (int i = 0; i < 34; i++)
		{
			if (m_machiTypes[i] > MachiType.None)
			{
				AgariMachiType = m_machiTypes[i];
				AgariHaiIDs.Add((HaiID)i);
			}
		}
		CalculatedAgariHaiIDCount = AgariHaiIDs.Count;
	}

	public List<ScoreInfo> EvaluateScore(TaikyokuState taikyokuState, int doraCount)
	{
		HaiID bakazeHaiID = taikyokuState.GetBakazeHaiID();
		HaiID jikazeHaiID = taikyokuState.GetJikazeHaiID();
		bool isFireReach = taikyokuState.IsFireReach;
		bool isOpenReach = taikyokuState.IsOpenReach;
		bool isAtomicReach = taikyokuState.IsAtomicReach;
		if (m_scoreInfo != null)
		{
			return m_scoreInfo;
		}
		List<ScoreInfo> list = new List<ScoreInfo>();
		if (m_shantenCount > 0)
		{
			return list;
		}
		bool isOya = jikazeHaiID == HaiID.J1;
		MachiType[] machiTypes = GetMachiTypes();
		bool isMenzen = Blocks.IsMenzen();
		for (HaiID haiID = HaiID.M1; haiID < HaiID.Max; haiID++)
		{
			MachiType machiType = machiTypes[(int)haiID];
			if (machiType != 0)
			{
				for (AgariType agariType = AgariType.Ron; agariType < AgariType.Max; agariType++)
				{
					ScoreInfo scoreInfo = new ScoreInfo();
					bool isTsumo = agariType == AgariType.Tsumo;
					scoreInfo.AgariHaiID = haiID;
					scoreInfo.MachiType = machiType;
					scoreInfo.IsTsumo = isTsumo;
					scoreInfo.IsOya = isOya;
					scoreInfo.DoraCount = doraCount + (MahjongUtility.IsPureDora(haiID.HaiIDToPai()) ? 1 : 0);
					scoreInfo.BonusCount = taikyokuState.GetBonusCount(haiID);
					foreach (YakuPossibilityInfo yakuPossibilityInfo in YakuInfo.YakuPossibilityInfos)
					{
						if (yakuPossibilityInfo.IsComplete(haiID, machiType, isTsumo))
						{
							scoreInfo.Yakus.Add(yakuPossibilityInfo.Yaku);
						}
					}
					if (agariType == AgariType.Tsumo && isMenzen)
					{
						scoreInfo.Yakus.Add(Yaku.Menzen);
					}
					YakuInfo.DemoteSubordinateYaku(ref scoreInfo.Yakus);
					scoreInfo.YakumanCount = scoreInfo.Yakus.Sum((Yaku yaku) => MahjongUtility.GetHanNum(yaku, isFireReach, isOpenReach, isAtomicReach) / 13);
					scoreInfo.HanCount = scoreInfo.Yakus.Sum(delegate(Yaku yaku)
					{
						int num = MahjongUtility.GetHanNum(yaku, isFireReach, isOpenReach, isAtomicReach);
						if (num >= 13)
						{
							return 0;
						}
						if (!isMenzen)
						{
							switch (yaku)
							{
							case Yaku.Chiniso:
							case Yaku.Junchan:
							case Yaku.Honiso:
							case Yaku.Sansyokudojun:
							case Yaku.Chanta:
							case Yaku.Ikkitsukan:
								num--;
								break;
							}
						}
						return num;
					});
					if (scoreInfo.YakumanCount != 0 || scoreInfo.HanCount != 0)
					{
						if (scoreInfo.YakumanCount == 0 && scoreInfo.HanCount + scoreInfo.DoraCount + scoreInfo.BonusCount >= 13)
						{
							scoreInfo.Yakus.Add(Yaku.Kazoeyakuman);
							scoreInfo.YakumanCount = 1;
						}
						scoreInfo.Fu = Blocks.GetFu(haiID, machiType, bakazeHaiID, jikazeHaiID, isTsumo);
						scoreInfo.Han = MahjongUtility.GetHanType(scoreInfo.HanCount + scoreInfo.DoraCount + scoreInfo.BonusCount, scoreInfo.Fu, scoreInfo.YakumanCount);
						(long, long, long) score = MahjongUtility.GetScore(GameDataManager.Mahjong.TakuType, scoreInfo.Han, scoreInfo.Fu, scoreInfo.IsOya, scoreInfo.IsTsumo);
						scoreInfo.TotalPayment = score.Item1;
						scoreInfo.OyaPaymentByTsumo = score.Item3;
						scoreInfo.KoPaymentByTsumo = score.Item2;
						list.Add(scoreInfo);
					}
				}
			}
		}
		m_scoreInfo = list;
		return list;
	}

	public bool IsTenpai()
	{
		return m_shantenCount <= 0;
	}

	public int MentsuCount()
	{
		return Blocks.MentsuCount();
	}

	public int ToitsuCount()
	{
		return Blocks.ToitsuCount();
	}

	public int TatsuCount()
	{
		return Blocks.TatsuCount();
	}

	public int KantsuCount()
	{
		return Blocks.KantsuCount();
	}

	public int GetCount(HaiID haiID, BlockType blockType)
	{
		return Blocks.Count(haiID, blockType);
	}
}
