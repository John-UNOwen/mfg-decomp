using System;
using System.Collections.Generic;
using System.Linq;
using MFG.Types;

[Serializable]
public class YakuInfo
{
	public List<YakuPossibilityInfo> YakuPossibilityInfos = new List<YakuPossibilityInfo>();

	private static readonly Dictionary<Yaku, List<Yaku>> s_subordinates = new Dictionary<Yaku, List<Yaku>>
	{
		{
			Yaku.Ryanpeko,
			new List<Yaku> { Yaku.Ipeiko }
		},
		{
			Yaku.Chitoitsu,
			new List<Yaku>
			{
				Yaku.Ipeiko,
				Yaku.Bakaze
			}
		},
		{
			Yaku.Chiniso,
			new List<Yaku> { Yaku.Honiso }
		},
		{
			Yaku.Junchan,
			new List<Yaku> { Yaku.Chanta }
		},
		{
			Yaku.Honroto,
			new List<Yaku> { Yaku.Chanta }
		},
		{
			Yaku.Daisangen,
			new List<Yaku> { Yaku.Syosangen }
		},
		{
			Yaku.Daisushi,
			new List<Yaku> { Yaku.Syosushi }
		}
	};

	public YakuInfo Clone()
	{
		YakuInfo yakuInfo = new YakuInfo();
		yakuInfo.YakuPossibilityInfos = new List<YakuPossibilityInfo>();
		for (int i = 0; i < YakuPossibilityInfos.Count; i++)
		{
			yakuInfo.YakuPossibilityInfos.Add(YakuPossibilityInfos[i].Clone());
		}
		return yakuInfo;
	}

	public void DemoteToTakame(Yaku yaku)
	{
		int count = YakuPossibilityInfos.Count;
		for (int i = 0; i < count; i++)
		{
			YakuPossibilityInfo value = YakuPossibilityInfos[i];
			if (yaku == value.Yaku && value.Possibility == YakuPossibility.Established)
			{
				value.Possibility = YakuPossibility.EstablishedInTakame;
				YakuPossibilityInfos[i] = value;
			}
		}
	}

	public void DemoteToTakame(YakuInfo priorityYakuInfo)
	{
		int count = YakuPossibilityInfos.Count;
		for (int i = 0; i < count; i++)
		{
			YakuPossibilityInfo info = YakuPossibilityInfos[i];
			if (info.Possibility == YakuPossibility.Established && !priorityYakuInfo.YakuPossibilityInfos.Exists((YakuPossibilityInfo x) => x.Yaku == info.Yaku && x.Possibility == YakuPossibility.Established))
			{
				info.Possibility = YakuPossibility.EstablishedInTakame;
				YakuPossibilityInfos[i] = info;
			}
		}
	}

	public static void DemoteSubordinateYaku(ref List<YakuPossibilityInfo> infos)
	{
		foreach (Yaku superiorYaku in s_subordinates.Keys)
		{
			int num = infos.FindIndex((YakuPossibilityInfo x) => x.Yaku == superiorYaku && (x.Possibility == YakuPossibility.Established || x.Possibility == YakuPossibility.EstablishedInTakame));
			if (num >= 0 && infos[num].Possibility == YakuPossibility.Established)
			{
				List<Yaku> subordinates = s_subordinates[superiorYaku];
				infos.RemoveAll((YakuPossibilityInfo x) => subordinates.Contains(x.Yaku));
			}
		}
		if (infos.FindIndex((YakuPossibilityInfo x) => x.Yaku == Yaku.Suanko && x.Possibility == YakuPossibility.EstablishedInTakame) >= 0 && infos.FindIndex((YakuPossibilityInfo x) => x.Yaku == Yaku.Toitoiho && x.Possibility == YakuPossibility.Established) >= 0)
		{
			infos.RemoveAll((YakuPossibilityInfo x) => x.Yaku == Yaku.Ipeiko);
		}
		int num2 = infos.FindIndex((YakuPossibilityInfo x) => x.Yaku == Yaku.Sananko && x.Possibility >= YakuPossibility.EstablishedInTakame);
		int num3 = infos.FindIndex((YakuPossibilityInfo x) => x.Yaku == Yaku.Ryanpeko && x.Possibility >= YakuPossibility.EstablishedInTakame);
		int num4 = infos.FindIndex((YakuPossibilityInfo x) => x.Yaku == Yaku.Ipeiko && x.Possibility >= YakuPossibility.EstablishedInTakame);
		if (num2 >= 0 && (num4 >= 0 || num3 >= 0))
		{
			YakuPossibilityInfo value = infos[num2];
			value.Possibility = YakuPossibility.EstablishedInTakame;
			infos[num2] = value;
			if (num3 >= 0)
			{
				value = infos[num3];
				value.Possibility = YakuPossibility.EstablishedInTakame;
				infos[num3] = value;
				infos.RemoveAll((YakuPossibilityInfo x) => x.Yaku == Yaku.Ipeiko);
			}
			else
			{
				value = infos[num4];
				value.Possibility = YakuPossibility.EstablishedInTakame;
				infos[num4] = value;
			}
		}
		if (infos.Any((YakuPossibilityInfo x) => x.Possibility == YakuPossibility.Established && MahjongUtility.GetHanNum(x.Yaku, isFireReach: false, isOpenReach: false, isAtomicReach: false) >= 13))
		{
			infos.RemoveAll((YakuPossibilityInfo x) => MahjongUtility.GetHanNum(x.Yaku, isFireReach: false, isOpenReach: false, isAtomicReach: false) < 13);
		}
		else
		{
			infos.Any((YakuPossibilityInfo x) => x.Possibility == YakuPossibility.EstablishedInTakame && MahjongUtility.GetHanNum(x.Yaku, isFireReach: false, isOpenReach: false, isAtomicReach: false) >= 13);
		}
	}

	public static void DemoteKaratenInfo(ref List<YakuPossibilityInfo> infos)
	{
		int count = infos.Count;
		for (int i = 0; i < count; i++)
		{
			if (infos[i].Possibility == YakuPossibility.Established)
			{
				YakuPossibilityInfo value = infos[i];
				value.Karaten = KaratenInfo.None;
				infos[i] = value;
			}
		}
	}

	public static void DemoteOnebyoneKaraten(ref List<YakuPossibilityInfo> infos, Yaku yaku)
	{
		int count = infos.Count;
		for (int i = 0; i < count; i++)
		{
			if (infos[i].Yaku == yaku)
			{
				YakuPossibilityInfo value = infos[i];
				value.Karaten = KaratenInfo.None;
				infos[i] = value;
			}
		}
	}

	public static void TsumoMinogashiDemote(ref List<YakuPossibilityInfo> infos, List<Yaku> demoteYakus)
	{
		int count = infos.Count;
		for (int i = 0; i < count; i++)
		{
			YakuPossibilityInfo yakuPossibilityInfo = infos[i];
			if (demoteYakus.Contains(yakuPossibilityInfo.Yaku) && yakuPossibilityInfo.Possibility == YakuPossibility.Established)
			{
				YakuPossibilityInfo value = infos[i];
				value.Possibility = YakuPossibility.EstablishedInTakame;
				infos[i] = value;
			}
		}
	}

	public static void DemoteSubordinateYaku(ref List<Yaku> yakus)
	{
		if (yakus.Any((Yaku x) => MahjongUtility.GetHanNum(x, isFireReach: false, isOpenReach: false, isAtomicReach: false) >= 13))
		{
			yakus.RemoveAll((Yaku x) => MahjongUtility.GetHanNum(x, isFireReach: false, isOpenReach: false, isAtomicReach: false) < 13);
		}
		foreach (Yaku key in s_subordinates.Keys)
		{
			if (yakus.Contains(key))
			{
				List<Yaku> subordinates = s_subordinates[key];
				yakus.RemoveAll((Yaku x) => subordinates.Contains(x));
			}
		}
	}

	public override string ToString()
	{
		string text = "";
		foreach (YakuPossibilityInfo yakuPossibilityInfo in YakuPossibilityInfos)
		{
			text = text + "(" + MahjongUtility.GetYakuName(yakuPossibilityInfo.Yaku) + "," + yakuPossibilityInfo.Possibility.GetEnumName() + ")";
		}
		return text;
	}
}
