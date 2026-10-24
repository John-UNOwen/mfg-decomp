using System;
using System.Collections.Generic;
using MFG.Types;
using UnityEngine;

namespace MFG.MahjonPlay.Hand;

[Serializable]
public class BlockSet : IEquatable<BlockSet>
{
	private const int BLOCK_COUNT_MAX = 14;

	[SerializeField]
	private Block[] m_blocks = new Block[14];

	[SerializeField]
	private int m_count;

	private int m_mentsuCount;

	private int m_kantsuCount;

	private int m_toitsuCount;

	private int m_tatsuCount;

	private int m_isolatedCount;

	private long m_patternFlag;

	public DivideType DivideType;

	public Block this[int i]
	{
		get
		{
			return m_blocks[i];
		}
		set
		{
			m_blocks[i] = value;
		}
	}

	public BlockSet()
	{
	}

	public BlockSet(BlockType blockType, HaiID haiID)
	{
		AddBlock(blockType, haiID);
	}

	public BlockSet(List<Block> blocks)
	{
		AddBlock(blocks);
	}

	public BlockSet Clone()
	{
		BlockSet blockSet = new BlockSet();
		blockSet.m_count = m_count;
		blockSet.m_mentsuCount = m_mentsuCount;
		blockSet.m_kantsuCount = m_kantsuCount;
		blockSet.m_toitsuCount = m_toitsuCount;
		blockSet.m_tatsuCount = m_tatsuCount;
		blockSet.m_isolatedCount = m_isolatedCount;
		blockSet.m_patternFlag = m_patternFlag;
		blockSet.DivideType = DivideType;
		for (int i = 0; i < m_count; i++)
		{
			blockSet.m_blocks[i] = m_blocks[i];
		}
		return blockSet;
	}

	public bool Equals(BlockSet other)
	{
		return m_patternFlag == other.m_patternFlag;
	}

	public override int GetHashCode()
	{
		return m_count ^ m_mentsuCount ^ m_kantsuCount ^ m_toitsuCount ^ m_tatsuCount ^ m_isolatedCount;
	}

	public void Clear()
	{
		m_count = 0;
		m_patternFlag = 0L;
		m_mentsuCount = 0;
		m_kantsuCount = 0;
		m_toitsuCount = 0;
		m_tatsuCount = 0;
		m_isolatedCount = 0;
		DivideType = DivideType.None;
	}

	public void AddBlock(Block block)
	{
		m_blocks[m_count++] = block;
		m_patternFlag |= block.GetPatternFlag();
		switch (block.BlockType)
		{
		case BlockType.MinKantsu:
		case BlockType.AnKantsu:
			m_mentsuCount++;
			m_kantsuCount++;
			break;
		case BlockType.MinKoutsu:
		case BlockType.MinShuntsu:
		case BlockType.AnKoutsu:
		case BlockType.AnShuntsu:
			m_mentsuCount++;
			break;
		case BlockType.Toitsu:
			m_toitsuCount++;
			break;
		case BlockType.RyanmenTatsu:
		case BlockType.PenchanTatsu:
		case BlockType.KanchanTatsu:
			m_tatsuCount++;
			break;
		case BlockType.Isolated:
			m_isolatedCount++;
			break;
		}
	}

	private void AddBlock(BlockType blockType, HaiID haiID)
	{
		Block block = default(Block);
		block.HaiID = haiID;
		block.BlockType = blockType;
		Block block2 = block;
		AddBlock(block2);
	}

	public void AddBlock(List<Block> blocks)
	{
		if (blocks.Count > 0)
		{
			int count = blocks.Count;
			for (int i = 0; i < count; i++)
			{
				AddBlock(blocks[i]);
			}
		}
	}

	public void AddBlock(BlockSet blocks)
	{
		if (blocks.m_count > 0)
		{
			int count = blocks.m_count;
			for (int i = 0; i < count; i++)
			{
				AddBlock(blocks.m_blocks[i]);
			}
		}
	}

	public void AddKakan(HaiID haiID)
	{
		for (int i = 0; i < m_count; i++)
		{
			Block block = m_blocks[i];
			if (block.BlockType == BlockType.MinKoutsu && haiID == block.HaiID)
			{
				m_blocks[i].BlockType = BlockType.MinKantsu;
				break;
			}
		}
	}

	public void Sort()
	{
		Array.Sort(m_blocks);
	}

	public int Count()
	{
		return m_count;
	}

	public int Count(HaiID haiID, BlockType blockType)
	{
		int num = 0;
		for (int i = 0; i < m_count; i++)
		{
			Block block = m_blocks[i];
			if (block.BlockType == blockType && block.HaiID == haiID)
			{
				num++;
			}
		}
		return num;
	}

	public int Count(Predicate<Block> match)
	{
		int num = 0;
		for (int i = 0; i < m_count; i++)
		{
			Block obj = m_blocks[i];
			if (match(obj))
			{
				num++;
			}
		}
		return num;
	}

	public bool[] CreateMatchedFlag(Predicate<Block> match)
	{
		bool[] array = new bool[34];
		for (int i = 0; i < m_count; i++)
		{
			Block obj = m_blocks[i];
			if (match(obj))
			{
				array[(int)obj.HaiID] = true;
			}
		}
		return array;
	}

	public int MentsuCount()
	{
		return m_mentsuCount;
	}

	public int ToitsuCount()
	{
		return m_toitsuCount;
	}

	public int TatsuCount()
	{
		return m_tatsuCount;
	}

	public int KantsuCount()
	{
		return m_kantsuCount;
	}

	public int IsolatedCount()
	{
		return m_isolatedCount;
	}

	public int EffectiveIsolatedCount(bool[] availableHaiIDFlags)
	{
		int num = 0;
		int num2 = m_mentsuCount + m_toitsuCount + m_tatsuCount;
		for (int i = 0; i < m_count; i++)
		{
			Block block = m_blocks[i];
			HaiID haiID = block.HaiID;
			haiID.GetSu();
			switch (block.BlockType)
			{
			case BlockType.RyanmenTatsu:
				if (num2 >= 5 && m_toitsuCount == 0)
				{
					if (availableHaiIDFlags[(int)haiID])
					{
						num++;
					}
					if (availableHaiIDFlags[(int)(haiID + 1)])
					{
						num++;
					}
				}
				break;
			case BlockType.PenchanTatsu:
				if (num2 >= 5 && m_toitsuCount == 0)
				{
					if (availableHaiIDFlags[(int)haiID])
					{
						num++;
					}
					if (availableHaiIDFlags[(int)(haiID + 1)])
					{
						num++;
					}
				}
				break;
			case BlockType.KanchanTatsu:
				if (num2 >= 5 && m_toitsuCount == 0)
				{
					if (availableHaiIDFlags[(int)haiID])
					{
						num++;
					}
					if (availableHaiIDFlags[(int)(haiID + 2)])
					{
						num++;
					}
				}
				break;
			case BlockType.Isolated:
				if (num2 < 5 && availableHaiIDFlags[(int)haiID])
				{
					num++;
				}
				break;
			}
		}
		return num;
	}

	public Counter CreateCounter()
	{
		Counter counter = new Counter();
		AddCounter(counter);
		return counter;
	}

	public void AddCounter(Counter counter)
	{
		for (int i = 0; i < m_count; i++)
		{
			Block block = m_blocks[i];
			HaiID haiID = block.HaiID;
			switch (block.BlockType)
			{
			case BlockType.MinKoutsu:
			case BlockType.AnKoutsu:
				counter.Add(haiID, 3);
				break;
			case BlockType.MinKantsu:
			case BlockType.AnKantsu:
				counter.Add(haiID, 4);
				break;
			case BlockType.MinShuntsu:
			case BlockType.AnShuntsu:
				counter.Add(haiID);
				counter.Add(haiID + 1);
				counter.Add(haiID + 2);
				break;
			case BlockType.Toitsu:
				counter.Add(haiID, 2);
				break;
			case BlockType.RyanmenTatsu:
			case BlockType.PenchanTatsu:
				counter.Add(haiID);
				counter.Add(haiID + 1);
				break;
			case BlockType.KanchanTatsu:
				counter.Add(haiID);
				counter.Add(haiID + 2);
				break;
			case BlockType.Isolated:
				counter.Add(haiID);
				break;
			}
		}
	}

	public bool Exists(HaiID haiID, BlockType blockType, int _count, List<int> exclusionBlockIndexes, out int retBlockIndex)
	{
		int num = 0;
		retBlockIndex = -1;
		for (int i = 0; i < m_count; i++)
		{
			Block block = m_blocks[i];
			if (block.BlockType != blockType || block.HaiID != haiID)
			{
				continue;
			}
			bool flag = false;
			foreach (int exclusionBlockIndex in exclusionBlockIndexes)
			{
				if (exclusionBlockIndex >= 0 && i == exclusionBlockIndex)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (num == 0)
				{
					retBlockIndex = i;
				}
				num++;
			}
		}
		return num >= _count;
	}

	public int Count(HaiID haiID, BlockType blockType, List<int> exclusionBlockIndexes)
	{
		int num = 0;
		for (int i = 0; i < m_count; i++)
		{
			Block block = m_blocks[i];
			if (block.BlockType != blockType || block.HaiID != haiID)
			{
				continue;
			}
			bool flag = false;
			foreach (int exclusionBlockIndex in exclusionBlockIndexes)
			{
				if (exclusionBlockIndex >= 0 && i == exclusionBlockIndex)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				num++;
			}
		}
		return num;
	}

	public Block? Find(Predicate<Block> match)
	{
		for (int i = 0; i < m_count; i++)
		{
			Block block = m_blocks[i];
			if (match(block))
			{
				return block;
			}
		}
		return null;
	}

	public ushort FindAllIndex(Predicate<Block> match)
	{
		ushort num = 0;
		for (int i = 0; i < m_count; i++)
		{
			Block obj = m_blocks[i];
			if (match(obj))
			{
				num |= (ushort)(1 << i);
			}
		}
		return num;
	}

	public List<Block> FindAll(Predicate<Block> match)
	{
		List<Block> list = new List<Block>();
		for (int i = 0; i < m_count; i++)
		{
			Block block = m_blocks[i];
			if (match(block))
			{
				list.Add(block);
			}
		}
		return list;
	}

	public bool Exists(HaiID haiID, BlockType blockType)
	{
		return Count(haiID, blockType) > 0;
	}

	public bool Exists(Predicate<Block> match)
	{
		for (int i = 0; i < m_count; i++)
		{
			Block obj = m_blocks[i];
			if (match(obj))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsValid()
	{
		if (m_count == 0 || m_blocks[0].BlockType == BlockType.None)
		{
			return false;
		}
		return true;
	}

	public bool IsMenzen()
	{
		int num = 0;
		for (int i = 0; i < m_count; i++)
		{
			Block block = m_blocks[i];
			if (!block.IsMenzen())
			{
				num++;
			}
		}
		return num == 0;
	}

	public int GetFu(HaiID agariHaiID, MachiType machiType, HaiID baKazeHaiID, HaiID jiKazeHaiID, bool isTsumo)
	{
		if (DivideType == DivideType.Kokushi)
		{
			return 0;
		}
		if (DivideType == DivideType.Chitoitsu)
		{
			return 25;
		}
		bool flag = IsMenzen();
		int num = 20;
		if (isTsumo)
		{
			num += 2;
		}
		else if (flag)
		{
			num += 10;
		}
		int num2 = 0;
		if (machiType == MachiType.Kanchan || machiType == MachiType.Penchan || machiType == MachiType.Tanki)
		{
			num2 = 2;
		}
		num += num2;
		int num3 = 0;
		for (int i = 0; i < m_count; i++)
		{
			Block block = m_blocks[i];
			HaiID haiID = block.HaiID;
			bool flag2 = haiID.IsYaochu();
			switch (block.BlockType)
			{
			case BlockType.AnKantsu:
				num3 += (flag2 ? 32 : 16);
				break;
			case BlockType.MinKantsu:
				num3 += (flag2 ? 16 : 8);
				break;
			case BlockType.AnKoutsu:
				num3 += (flag2 ? 8 : 4);
				break;
			case BlockType.MinKoutsu:
				num3 += (flag2 ? 4 : 2);
				break;
			case BlockType.Toitsu:
				if (machiType == MachiType.Shabo && m_toitsuCount == 2 && haiID == agariHaiID)
				{
					num3 = ((!isTsumo) ? (num3 + (flag2 ? 4 : 2)) : (num3 + (flag2 ? 8 : 4)));
				}
				else if (haiID == HaiID.J5 || haiID == HaiID.J6 || haiID == HaiID.J7 || haiID == baKazeHaiID || haiID == jiKazeHaiID)
				{
					num3 += 2;
				}
				break;
			}
		}
		num += num3;
		if (num2 + num3 == 0)
		{
			num = ((!(flag && isTsumo)) ? 30 : 20);
		}
		return (num + 8) / 10 * 10;
	}

	public override string ToString()
	{
		string text = "|";
		Block[] blocks = m_blocks;
		for (int i = 0; i < blocks.Length; i++)
		{
			Block block = blocks[i];
			text += block.ToString();
		}
		return text;
	}

	private long GetPatternFlag()
	{
		return m_patternFlag;
	}

	public static long GetPatternFlag(List<Block> blocks)
	{
		long num = 0L;
		int count = blocks.Count;
		for (int i = 0; i < count; i++)
		{
			num |= blocks[i].GetPatternFlag();
		}
		return num;
	}
}
