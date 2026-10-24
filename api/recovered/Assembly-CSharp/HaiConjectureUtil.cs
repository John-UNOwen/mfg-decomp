using System.Collections.Generic;
using System.Linq;
using MFG.Types;

public class HaiConjectureUtil
{
	private static bool IsNimaSanma1Man9Man(Pai pai)
	{
		if ((GameDataManager.Mahjong.TakuType == TakuType.Nima || GameDataManager.Mahjong.TakuType == TakuType.Sanma) && (pai == Pai.M1 || pai == Pai.M9))
		{
			return true;
		}
		return false;
	}

	public static List<Pai> GetSujiHai(Pai pai)
	{
		HaiID haiID = pai.ToHaiID();
		if (haiID.IsJihai() || IsNimaSanma1Man9Man(pai))
		{
			return new List<Pai> { Pai.NONE };
		}
		SuID su = haiID.GetSu();
		HaiMarkType mark = haiID.GetMark();
		List<int> list = new List<int>();
		for (int i = -1; i < 2; i++)
		{
			SuID suID = su + i * 3;
			if (suID.IsValidSu() && suID != su)
			{
				list.Add((int)suID);
			}
		}
		if (list.Count <= 0)
		{
			return new List<Pai> { Pai.NONE };
		}
		return list.Select((int num) => HaiUtility.ToHaiID(mark, (SuID)num).HaiIDToPai()).ToList();
	}

	public static List<Pai> GetMusujiHai(Pai pai)
	{
		if (pai.ToHaiID().IsJihai() || IsNimaSanma1Man9Man(pai))
		{
			return new List<Pai> { Pai.NONE };
		}
		List<Pai> sujiHai = GetSujiHai(pai);
		List<Pai> list = new List<Pai>();
		HaiID haiID = HaiID.M1;
		if (GameDataManager.Mahjong.TakuType == TakuType.Nima || GameDataManager.Mahjong.TakuType == TakuType.Sanma)
		{
			haiID = HaiID.S1;
		}
		for (int i = (int)haiID; i <= 26; i++)
		{
			Pai pai2 = ((HaiID)i).HaiIDToPai();
			if (pai2 != pai && !sujiHai.Contains(pai2))
			{
				list.Add(pai2);
			}
		}
		if (list.Count <= 0)
		{
			return new List<Pai> { Pai.NONE };
		}
		return list;
	}

	public static List<Pai> CalcMusujiHaiFromSujiList(List<Pai> paiList)
	{
		List<Pai> list = new List<Pai>();
		foreach (Pai pai in paiList)
		{
			if (pai != 0 && !pai.ToHaiID().IsJihai() && !IsNimaSanma1Man9Man(pai))
			{
				List<Pai> list2 = (from p in GetMusujiHai(pai)
					where !paiList.Contains(p) && p != Pai.NONE
					select p).ToList();
				if (list2 != null)
				{
					list.AddRange(list2);
				}
			}
		}
		if (list.Count <= 0)
		{
			return new List<Pai> { Pai.NONE };
		}
		list.Distinct().ToList();
		return list;
	}
}
