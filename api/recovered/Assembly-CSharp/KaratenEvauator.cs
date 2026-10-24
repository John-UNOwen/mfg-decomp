using System.Collections.Generic;
using System.Linq;
using MFG.MahjonPlay.Hand;
using MFG.Types;

public class KaratenEvauator
{
	private Counter m_counter;

	public KaratenEvauator(Counter counter)
	{
		m_counter = counter;
	}

	private int IsRemainingPai(Pai pai)
	{
		return MahjongUtility.GetRestPaiNum(pai);
	}

	private int IsRemainingHai(HaiID id)
	{
		return IsRemainingPai(id.HaiIDToPai());
	}

	private int SumUsableHai(HaiID id)
	{
		return IsRemainingHai(id) + m_counter[id];
	}

	public KaratenInfo EvalYakuhaiKaraten(HaiID id)
	{
		if (IsRemainingHai(id) == 0 && m_counter[id] < 3)
		{
			return KaratenInfo.KaratenWait;
		}
		return KaratenInfo.None;
	}

	public KaratenInfo EvalPinfuKaraten(bool isReached, HaiID ryanmenID = HaiID.None)
	{
		SuID su = ryanmenID.GetSu();
		if (isReached && su >= SuID.SU2 && su <= SuID.SU7 && EvalTakameKaraten(isReached, ryanmenID - 1) == KaratenInfo.KaratenWait && EvalTakameKaraten(isReached, ryanmenID + 2) == KaratenInfo.KaratenWait)
		{
			return KaratenInfo.KaratenWait;
		}
		return KaratenInfo.None;
	}

	public KaratenInfo EvalTakameKaraten(bool isReached, HaiID takameID = HaiID.None)
	{
		if (isReached && takameID != HaiID.None && IsRemainingHai(takameID) <= 0)
		{
			return KaratenInfo.KaratenWait;
		}
		return KaratenInfo.None;
	}

	public KaratenInfo EvalSukantsu(List<HaiID> ponList, List<HaiID> ankoList = null)
	{
		if (CalcKantsuImpossiblePon(ponList) > 0)
		{
			return KaratenInfo.KaratenWait;
		}
		if (ankoList != null && CalcKantsuImpossiblePon(ankoList) > 0)
		{
			return KaratenInfo.KaratenWait;
		}
		return KaratenInfo.None;
	}

	public KaratenInfo EvalSankantsu(List<HaiID> ponList, List<HaiID> ankoList = null, int kantsuCount = 0)
	{
		if (CalcKantsuImpossiblePon(ponList) > 1)
		{
			return KaratenInfo.KaratenWait;
		}
		if (ankoList != null && CalcKantsuImpossiblePon(ankoList) > ankoList.Count + kantsuCount - 3)
		{
			return KaratenInfo.KaratenWait;
		}
		return KaratenInfo.None;
	}

	private int CalcKantsuImpossiblePon(List<HaiID> ponList)
	{
		if (ponList.Count <= 0)
		{
			return 0;
		}
		int num = 0;
		foreach (HaiID pon in ponList)
		{
			if (IsRemainingHai(pon) <= 0)
			{
				num++;
			}
		}
		return num;
	}

	public KaratenInfo EvalJunchanKaraten(bool isReached, HaiID takameID = HaiID.None)
	{
		if (isReached)
		{
			return EvalTakameKaraten(isReached, takameID);
		}
		int num = 0;
		HaiID[] array = HaiUtility.RoutouHais();
		foreach (HaiID id in array)
		{
			num += SumUsableHai(id);
		}
		if (num >= 6)
		{
			return KaratenInfo.None;
		}
		return KaratenInfo.KaratenWait;
	}

	public KaratenInfo EvalChantaKaraten(bool isReached, HaiID takameID = HaiID.None)
	{
		if (isReached)
		{
			return EvalTakameKaraten(isReached, takameID);
		}
		int num = 0;
		HaiID[] array = HaiUtility.YaochuHais();
		foreach (HaiID id in array)
		{
			num += SumUsableHai(id);
		}
		if (num >= 6)
		{
			return KaratenInfo.None;
		}
		return KaratenInfo.KaratenWait;
	}

	public KaratenInfo EvalSanshokudokoKaraten(bool isSanma, bool isReached, HaiID takameID = HaiID.None)
	{
		if (isReached)
		{
			return EvalTakameKaraten(isReached, takameID);
		}
		for (SuID suID = SuID.SU1; suID <= SuID.SU9; suID++)
		{
			if (!isSanma || suID <= SuID.SU1 || suID >= SuID.SU9)
			{
				bool num = SumUsableHai(HaiUtility.ToHaiID(HaiMarkType.Manzu, suID)) >= 3;
				bool flag = SumUsableHai(HaiUtility.ToHaiID(HaiMarkType.Souzu, suID)) >= 3;
				bool flag2 = SumUsableHai(HaiUtility.ToHaiID(HaiMarkType.Pinzu, suID)) >= 3;
				if (num && flag && flag2)
				{
					return KaratenInfo.None;
				}
			}
		}
		return KaratenInfo.KaratenWait;
	}

	public KaratenInfo EvalRyuisoKaraten()
	{
		KaratenInfo result = KaratenInfo.None;
		Dictionary<HaiID, int> shuntsuCannotUse = new Dictionary<HaiID, int>
		{
			{
				HaiID.S2,
				0
			},
			{
				HaiID.S3,
				0
			},
			{
				HaiID.S4,
				0
			}
		};
		Dictionary<HaiID, int> dictionary = new Dictionary<HaiID, int>
		{
			{
				HaiID.S6,
				0
			},
			{
				HaiID.S8,
				0
			},
			{
				HaiID.J6,
				0
			}
		};
		HaiID[] array = HaiUtility.RyuisoHais();
		foreach (HaiID haiID in array)
		{
			if (shuntsuCannotUse.ContainsKey(haiID))
			{
				shuntsuCannotUse[haiID] = 4 - m_counter[haiID] - IsRemainingHai(haiID);
			}
			else
			{
				dictionary[haiID] = 4 - m_counter[haiID] - IsRemainingHai(haiID);
			}
		}
		int num = dictionary.Where((KeyValuePair<HaiID, int> x) => x.Value >= 2).Count();
		int num2 = dictionary.Where((KeyValuePair<HaiID, int> x) => x.Value >= 3).Count();
		if ((Eval234UseingCount(3) >= 2 || Eval234UseingCount(2) >= 3) && (Eval234UseingCount(4) >= 1 || Eval234UseingCount(2) >= 3))
		{
			result = KaratenInfo.KaratenWait;
		}
		else if (Eval234UseingCount(2) >= 2 && (Eval234UseingCount(4) >= 1 || Eval234UseingCount(1) >= 3) && Eval234UseingCount(3) >= 1)
		{
			if (num > 0)
			{
				result = KaratenInfo.KaratenWait;
			}
		}
		else if ((Eval234UseingCount(3) >= 1 || Eval234UseingCount(2) >= 2) && (Eval234UseingCount(4) >= 1 || Eval234UseingCount(2) >= 2 || Eval234UseingCount(1) >= 3) && (Eval234UseingCount(3) >= 1 || Eval234UseingCount(1) >= 3))
		{
			if (num2 >= 1 || num >= 2)
			{
				result = KaratenInfo.KaratenWait;
			}
		}
		else if (Eval234UseingCount(2) > 0)
		{
			if (Eval234UseingCount(4) >= 1 || Eval234UseingCount(1) >= 2)
			{
				if (num >= 2)
				{
					result = KaratenInfo.KaratenWait;
				}
			}
			else if (num2 >= 2 || num >= 3)
			{
				result = KaratenInfo.KaratenWait;
			}
		}
		else if (Eval234UseingCount(1) >= 1)
		{
			if (num >= 3)
			{
				result = KaratenInfo.KaratenWait;
			}
		}
		else if (num2 >= 3)
		{
			result = KaratenInfo.KaratenWait;
		}
		return result;
		int Eval234UseingCount(int unableCount)
		{
			return shuntsuCannotUse.Where((KeyValuePair<HaiID, int> x) => x.Value >= unableCount).Count();
		}
	}

	public KaratenInfo EvalKokushiKaraten()
	{
		KaratenInfo result = KaratenInfo.None;
		List<HaiID> list = new List<HaiID>();
		HaiID[] array = HaiUtility.YaochuHais();
		foreach (HaiID haiID in array)
		{
			if (m_counter[haiID] <= 0)
			{
				list.Add(haiID);
				if (IsRemainingHai(haiID) <= 0)
				{
					result = KaratenInfo.KaratenWait;
					break;
				}
			}
		}
		return result;
	}

	public KaratenInfo EvalChurenKaraten(HaiMarkType markType)
	{
		KaratenInfo karatenInfo = KaratenInfo.None;
		for (SuID suID = SuID.SU1; suID <= SuID.SU9; suID++)
		{
			HaiID id = HaiUtility.ToHaiID(markType, suID);
			if (suID == SuID.SU1 || suID == SuID.SU9)
			{
				if (SumUsableHai(id) < 3)
				{
					karatenInfo = KaratenInfo.KaratenWait;
				}
			}
			else if (SumUsableHai(id) <= 0)
			{
				karatenInfo = KaratenInfo.KaratenWait;
			}
			if (karatenInfo != 0)
			{
				break;
			}
		}
		return karatenInfo;
	}

	public Dictionary<Yaku, KaratenInfo> EvalSushihoKaraten()
	{
		Dictionary<Yaku, KaratenInfo> dictionary = new Dictionary<Yaku, KaratenInfo>
		{
			{
				Yaku.Daisushi,
				KaratenInfo.None
			},
			{
				Yaku.Syosushi,
				KaratenInfo.None
			}
		};
		bool flag = false;
		HaiID[] array = HaiUtility.KazeHais();
		foreach (HaiID id in array)
		{
			switch (SumUsableHai(id))
			{
			case 0:
			case 1:
				dictionary[Yaku.Daisushi] = KaratenInfo.KaratenWait;
				dictionary[Yaku.Syosushi] = KaratenInfo.KaratenWait;
				return dictionary;
			case 2:
				dictionary[Yaku.Daisushi] = KaratenInfo.KaratenWait;
				if (flag)
				{
					dictionary[Yaku.Syosushi] = KaratenInfo.KaratenWait;
					return dictionary;
				}
				flag = true;
				break;
			}
		}
		return dictionary;
	}

	public Dictionary<Yaku, KaratenInfo> EvalDaiShoSangenKaraten()
	{
		Dictionary<Yaku, KaratenInfo> dictionary = new Dictionary<Yaku, KaratenInfo>
		{
			{
				Yaku.Daisangen,
				KaratenInfo.None
			},
			{
				Yaku.Syosangen,
				KaratenInfo.None
			}
		};
		bool flag = false;
		HaiID[] array = HaiUtility.SangenHais();
		foreach (HaiID id in array)
		{
			switch (SumUsableHai(id))
			{
			case 0:
			case 1:
				dictionary[Yaku.Daisangen] = KaratenInfo.KaratenWait;
				dictionary[Yaku.Syosangen] = KaratenInfo.KaratenWait;
				return dictionary;
			case 2:
				dictionary[Yaku.Daisangen] = KaratenInfo.KaratenWait;
				if (flag)
				{
					dictionary[Yaku.Syosangen] = KaratenInfo.KaratenWait;
					return dictionary;
				}
				flag = true;
				break;
			}
		}
		return dictionary;
	}

	public Dictionary<Yaku, KaratenInfo> EvalTsuisoChirotoHonrotoKaraten()
	{
		Dictionary<Yaku, KaratenInfo> dictionary = new Dictionary<Yaku, KaratenInfo>
		{
			{
				Yaku.Tsuiso,
				KaratenInfo.None
			},
			{
				Yaku.Chinroto,
				KaratenInfo.None
			},
			{
				Yaku.Honroto,
				KaratenInfo.None
			}
		};
		int[] nimaiGireCount = new int[2];
		int[] sanmaiGireCount = new int[2];
		HaiID[] array = HaiUtility.YaochuHais();
		foreach (HaiID haiID in array)
		{
			if (m_counter[haiID] < 3)
			{
				if (SumUsableHai(haiID) < 2)
				{
					AddImpossibleCount(isSanmaiGire: true, haiID.IsJihai());
				}
				else if (SumUsableHai(haiID) < 3)
				{
					AddImpossibleCount(isSanmaiGire: false, haiID.IsJihai());
				}
			}
		}
		bool flag = sanmaiGireCount[0] == 0;
		bool flag2 = sanmaiGireCount.Sum() <= 6;
		if ((nimaiGireCount[1] >= 4 || sanmaiGireCount[1] >= 3) && !flag)
		{
			dictionary[Yaku.Tsuiso] = KaratenInfo.KaratenWait;
		}
		if (nimaiGireCount[0] >= 3 || sanmaiGireCount[0] >= 2)
		{
			dictionary[Yaku.Chinroto] = KaratenInfo.KaratenWait;
		}
		if ((nimaiGireCount.Sum() >= 10 || sanmaiGireCount.Sum() >= 9) && !flag2)
		{
			dictionary[Yaku.Honroto] = KaratenInfo.KaratenWait;
		}
		return dictionary;
		void AddImpossibleCount(bool isSanmaiGire, bool isJihai)
		{
			int num = (isJihai ? 1 : 0);
			nimaiGireCount[num]++;
			if (isSanmaiGire)
			{
				sanmaiGireCount[num]++;
			}
		}
	}
}
