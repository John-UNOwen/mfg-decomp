using System;
using System.Collections.Generic;
using MFG.Types;

namespace MFG.MahjonPlay.Hand;

[Serializable]
public struct Block : IEquatable<Block>, IComparable
{
	public BlockType BlockType;

	public HaiID HaiID;

	public Block(BlockType blockType, HaiID haiID)
	{
		BlockType = blockType;
		HaiID = haiID;
	}

	public static Block CreatePon(Cha cha, Tile item, Tile item1, Tile item2)
	{
		Block result = default(Block);
		result.BlockType = BlockType.MinKoutsu;
		result.HaiID = (HaiID)Math.Min((int)item.pai.ToHaiID(), Math.Min((int)item1.pai.ToHaiID(), (int)item2.pai.ToHaiID()));
		return result;
	}

	public static Block CreateChi(Tile item, Tile item1, Tile item2)
	{
		Block result = default(Block);
		result.BlockType = BlockType.MinShuntsu;
		result.HaiID = (HaiID)Math.Min((int)item.pai.ToHaiID(), Math.Min((int)item1.pai.ToHaiID(), (int)item2.pai.ToHaiID()));
		return result;
	}

	public static Block CreateMinkan(Cha cha, Tile item, Tile item1, Tile item2, Tile item3)
	{
		Block result = default(Block);
		result.BlockType = BlockType.MinKantsu;
		result.HaiID = item.pai.ToHaiID();
		return result;
	}

	public static Block CreateAnkan(Tile item1, Tile item2, Tile item3, Tile item4)
	{
		Block result = default(Block);
		result.BlockType = BlockType.AnKantsu;
		result.HaiID = item1.pai.ToHaiID();
		return result;
	}

	public bool Equals(Block other)
	{
		if (BlockType == other.BlockType)
		{
			return HaiID == other.HaiID;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return BlockType.GetHashCode() ^ HaiID.GetHashCode();
	}

	public int CompareTo(object obj)
	{
		Block block = this;
		Block block2 = (Block)obj;
		HaiMarkType mark = block.HaiID.GetMark();
		HaiMarkType mark2 = block2.HaiID.GetMark();
		if (mark == mark2)
		{
			SuID su = block.HaiID.GetSu();
			SuID su2 = block2.HaiID.GetSu();
			if (su == su2)
			{
				return block.BlockType - block2.BlockType;
			}
			return su - su2;
		}
		return mark - mark2;
	}

	public void Set(BlockType blockType, HaiID haiID)
	{
		BlockType = blockType;
		HaiID = haiID;
	}

	public bool IsValid()
	{
		return BlockType != BlockType.None;
	}

	public bool IsMentsu()
	{
		BlockType blockType = BlockType;
		if ((uint)(blockType - 1) <= 5u)
		{
			return true;
		}
		return false;
	}

	public bool IsToitsu()
	{
		if (BlockType == BlockType.Toitsu)
		{
			return true;
		}
		return false;
	}

	public bool IsTatsu()
	{
		BlockType blockType = BlockType;
		if ((uint)(blockType - 8) <= 2u)
		{
			return true;
		}
		return false;
	}

	public bool IsKantsu()
	{
		BlockType blockType = BlockType;
		if (blockType == BlockType.MinKantsu || blockType == BlockType.AnKantsu)
		{
			return true;
		}
		return false;
	}

	public bool IsMenzen()
	{
		BlockType blockType = BlockType;
		if ((uint)(blockType - 1) <= 2u)
		{
			return false;
		}
		return true;
	}

	public bool IsMelded()
	{
		BlockType blockType = BlockType;
		if ((uint)(blockType - 1) <= 3u)
		{
			return true;
		}
		return false;
	}

	public long GetPatternFlag()
	{
		long num = 0L;
		switch (BlockType)
		{
		case BlockType.AnKoutsu:
			num = 17179869184L;
			break;
		case BlockType.AnShuntsu:
			num = 8796093022208L;
			break;
		case BlockType.Toitsu:
			num = 512L;
			break;
		case BlockType.RyanmenTatsu:
			num = 33554432L;
			break;
		case BlockType.PenchanTatsu:
			num = 4294967296L;
			break;
		case BlockType.KanchanTatsu:
			num = 262144L;
			break;
		case BlockType.Isolated:
			num = 1L;
			break;
		}
		SuID su = HaiID.GetSu();
		if (BlockType == BlockType.PenchanTatsu && su == SuID.SU8)
		{
			return 8589934592L;
		}
		return num << (int)su;
	}

	public override string ToString()
	{
		HaiMarkType mark = HaiID.GetMark();
		int num = (int)(HaiID.GetSu() + 1);
		string[] obj = new string[4] { "M", "S", "P", "J" };
		string text = "";
		string text2 = "";
		string text3 = obj[(int)mark];
		switch (BlockType)
		{
		case BlockType.None:
			return "";
		case BlockType.MinKantsu:
		case BlockType.AnKantsu:
			return $"{text}{text3}{num}{num}{num}{num}{text2}|";
		case BlockType.MinKoutsu:
		case BlockType.AnKoutsu:
			return $"{text}{text3}{num}{num}{num}{text2}|";
		case BlockType.MinShuntsu:
		case BlockType.AnShuntsu:
			return $"{text}{text3}{num}{num + 1}{num + 2}{text2}|";
		case BlockType.Toitsu:
			return $"{text}{text3}{num}{num}{text2}|";
		case BlockType.RyanmenTatsu:
		case BlockType.PenchanTatsu:
			return $"{text}{text3}{num}{num + 1}{text2}|";
		case BlockType.KanchanTatsu:
			return $"{text}{text3}{num}{num + 2}{text2}|";
		case BlockType.Isolated:
			return $"{text}{text3}{num}{text2}|";
		default:
			return "";
		}
	}

	public List<HaiID> ToHaiID()
	{
		switch (BlockType)
		{
		case BlockType.MinKoutsu:
		case BlockType.AnKoutsu:
			return new List<HaiID> { HaiID, HaiID, HaiID };
		case BlockType.MinKantsu:
		case BlockType.AnKantsu:
			return new List<HaiID> { HaiID, HaiID, HaiID, HaiID };
		case BlockType.MinShuntsu:
		case BlockType.AnShuntsu:
			return new List<HaiID>
			{
				HaiID,
				HaiID + 1,
				HaiID + 2
			};
		case BlockType.Toitsu:
			return new List<HaiID> { HaiID, HaiID };
		case BlockType.RyanmenTatsu:
		case BlockType.PenchanTatsu:
			return new List<HaiID>
			{
				HaiID,
				HaiID + 1
			};
		case BlockType.KanchanTatsu:
			return new List<HaiID>
			{
				HaiID,
				HaiID + 2
			};
		case BlockType.Isolated:
			return new List<HaiID> { HaiID };
		default:
			return new List<HaiID>();
		}
	}

	public static bool Exists(List<Block> blocks, BlockType blockType, HaiID haiID)
	{
		int count = blocks.Count;
		for (int i = 0; i < count; i++)
		{
			Block block = blocks[i];
			if (block.BlockType == blockType && block.HaiID == haiID)
			{
				return true;
			}
		}
		return false;
	}
}
