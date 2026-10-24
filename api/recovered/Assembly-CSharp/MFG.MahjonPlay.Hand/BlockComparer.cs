using System.Collections.Generic;

namespace MFG.MahjonPlay.Hand;

public class BlockComparer : EqualityComparer<Block>
{
	public override bool Equals(Block a, Block b)
	{
		if (a.BlockType == b.BlockType)
		{
			return a.HaiID == b.HaiID;
		}
		return false;
	}

	public override int GetHashCode(Block a)
	{
		return (int)a.BlockType ^ (int)a.HaiID;
	}
}
