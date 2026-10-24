using System;
using MFG.MahjonPlay.Hand;
using MFG.Types;

[Serializable]
public struct YakuPossibilityInfo
{
	public Yaku Yaku;

	public YakuPossibility Possibility;

	public HaiID MachiHai0ID;

	public HaiID MachiHai1ID;

	public ushort RequiredBlockBitFlag;

	public BlockSet Blocks;

	public KaratenInfo Karaten;

	public YakuPossibilityInfo Clone()
	{
		YakuPossibilityInfo result = default(YakuPossibilityInfo);
		result.Yaku = Yaku;
		result.Possibility = Possibility;
		result.MachiHai0ID = MachiHai0ID;
		result.MachiHai1ID = MachiHai1ID;
		result.RequiredBlockBitFlag = RequiredBlockBitFlag;
		result.Blocks = Blocks.Clone();
		result.Karaten = Karaten;
		return result;
	}

	public bool IsComplete(HaiID haiID, MachiType machiType, bool isTsumo)
	{
		if ((Yaku == Yaku.Suanko || Yaku == Yaku.Sananko) && Possibility == YakuPossibility.EstablishedInTakame && machiType == MachiType.Shabo && !isTsumo)
		{
			return false;
		}
		if (Possibility < YakuPossibility.EstablishedInTakame)
		{
			return false;
		}
		if (Possibility == YakuPossibility.Established)
		{
			return true;
		}
		if (haiID != MachiHai0ID)
		{
			return haiID == MachiHai1ID;
		}
		return true;
	}

	public override string ToString()
	{
		return "(" + MahjongUtility.GetYakuName(Yaku) + "," + Possibility.GetEnumName() + ")";
	}
}
