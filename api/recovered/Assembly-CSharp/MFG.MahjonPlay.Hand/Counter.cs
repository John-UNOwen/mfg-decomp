using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MFG.Types;
using UnityEngine;

namespace MFG.MahjonPlay.Hand;

[Serializable]
public class Counter : IEquatable<Counter>
{
	public enum ExtractOrderType
	{
		FromLeft,
		FromRight,
		FromOutSide,
		FromInSide,
		FromRightInSide,
		FromLeftInSide,
		FromLeftTobi,
		FromRightTobi
	}

	private static readonly int[][] s_extractOrder = new int[8][]
	{
		new int[7] { 0, 1, 2, 3, 4, 5, 6 },
		new int[7] { 6, 5, 4, 3, 2, 1, 0 },
		new int[7] { 6, 0, 5, 1, 4, 2, 3 },
		new int[7] { 3, 2, 4, 1, 5, 0, 6 },
		new int[7] { 4, 3, 5, 2, 6, 1, 0 },
		new int[7] { 2, 3, 1, 4, 0, 5, 6 },
		new int[7] { 0, 3, 6, 1, 4, 2, 5 },
		new int[7] { 6, 3, 0, 5, 2, 4, 1 }
	};

	[SerializeField]
	private byte[] m_counts = new byte[34];

	public int this[HaiID haiID]
	{
		get
		{
			return m_counts[(int)haiID];
		}
		set
		{
			m_counts[(int)haiID] = (byte)value;
		}
	}

	public void Clear()
	{
		for (int i = 0; i < 34; i++)
		{
			m_counts[i] = 0;
		}
	}

	public bool Equals(Counter other)
	{
		for (int i = 0; i < 34; i++)
		{
			if (m_counts[i] != other.m_counts[i])
			{
				return false;
			}
		}
		return true;
	}

	public Counter Clone()
	{
		Counter counter = new Counter();
		for (int i = 0; i < 34; i++)
		{
			counter.m_counts[i] = m_counts[i];
		}
		return counter;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Copy(Counter src, Counter dst)
	{
		for (int i = 0; i < 34; i++)
		{
			dst.m_counts[i] = src.m_counts[i];
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Copy(Counter haiCounter)
	{
		for (int i = 0; i < 34; i++)
		{
			m_counts[i] = haiCounter.m_counts[i];
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Copy(int[] haiIDs)
	{
		int num = haiIDs.Length;
		for (int i = 0; i < num; i++)
		{
			m_counts[i] = (byte)haiIDs[i];
		}
	}

	public static Counter Create(HaiID[] haiIDs, int ignoreIndex = -1)
	{
		Counter counter = new Counter();
		int num = haiIDs.Length;
		for (int i = 0; i < num; i++)
		{
			if (i != ignoreIndex)
			{
				counter.Add(haiIDs[i]);
			}
		}
		return counter;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetCount(HaiID haiID)
	{
		return m_counts[(int)haiID];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsExist(HaiID haiID)
	{
		return m_counts[(int)haiID] > 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool CheckCount(HaiID haiID, int count)
	{
		return m_counts[(int)haiID] >= count;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool CheckCount(HaiID haiID0, HaiID haiID1, int count)
	{
		if (m_counts[(int)haiID0] >= count)
		{
			return m_counts[(int)haiID1] >= count;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool CheckCount(HaiID haiID0, HaiID haiID1, HaiID haiID2, int count)
	{
		if (m_counts[(int)haiID0] >= count && m_counts[(int)haiID1] >= count)
		{
			return m_counts[(int)haiID2] >= count;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(Counter counter)
	{
		for (int i = 0; i < 34; i++)
		{
			m_counts[i] += counter.m_counts[i];
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(HaiID haiID, int value = 1)
	{
		m_counts[(int)haiID] += (byte)value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AddWithIgnoreIndex(HaiID[] haiIDs, int ignoreIndex)
	{
		int num = haiIDs.Length;
		for (int i = 0; i < num; i++)
		{
			if (i != ignoreIndex)
			{
				Add(haiIDs[i]);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Dec(Counter counter)
	{
		for (int i = 0; i < 34; i++)
		{
			m_counts[i] -= counter.m_counts[i];
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Dec(HaiID haiID, int value = 1)
	{
		m_counts[(int)haiID] -= (byte)value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void DecMultiple(HaiID haiID, HaiID haiID1, HaiID value)
	{
		m_counts[(int)haiID] -= (byte)value;
		m_counts[(int)haiID1] -= (byte)value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void DecMultiple(HaiID haiID, HaiID haiID1, HaiID haiID2, int value)
	{
		m_counts[(int)haiID] -= (byte)value;
		m_counts[(int)haiID1] -= (byte)value;
		m_counts[(int)haiID2] -= (byte)value;
	}

	public Block Extract(BlockType blockType, HaiID haiID)
	{
		Block result = default(Block);
		result.BlockType = blockType;
		result.HaiID = haiID;
		switch (blockType)
		{
		case BlockType.MinKantsu:
			m_counts[(int)haiID] -= 4;
			break;
		case BlockType.MinKoutsu:
			m_counts[(int)haiID] -= 3;
			break;
		case BlockType.MinShuntsu:
			m_counts[(int)haiID]--;
			m_counts[(int)(haiID + 1)]--;
			m_counts[(int)(haiID + 2)]--;
			break;
		case BlockType.AnKantsu:
			m_counts[(int)haiID] -= 4;
			break;
		case BlockType.AnKoutsu:
			m_counts[(int)haiID] -= 3;
			break;
		case BlockType.AnShuntsu:
			m_counts[(int)haiID]--;
			m_counts[(int)(haiID + 1)]--;
			m_counts[(int)(haiID + 2)]--;
			break;
		case BlockType.Toitsu:
			m_counts[(int)haiID] -= 2;
			break;
		case BlockType.RyanmenTatsu:
			m_counts[(int)haiID]--;
			m_counts[(int)(haiID + 1)]--;
			break;
		case BlockType.PenchanTatsu:
			m_counts[(int)haiID]--;
			m_counts[(int)(haiID + 1)]--;
			break;
		case BlockType.KanchanTatsu:
			m_counts[(int)haiID]--;
			m_counts[(int)(haiID + 2)]--;
			break;
		case BlockType.Isolated:
			m_counts[(int)haiID]--;
			break;
		}
		return result;
	}

	public void ExtractAllBlockFromSamehai(in List<Block> blocks, HaiMarkType type, bool[] availableHaiIDFlags)
	{
		(HaiID, HaiID) range = HaiUtility.GetRange(type);
		HaiID item = range.Item1;
		HaiID item2 = range.Item2;
		for (int i = (int)item; i <= (int)item2; i++)
		{
			switch (m_counts[i])
			{
			case 1:
				blocks.Add(Extract(BlockType.Isolated, (HaiID)i));
				break;
			case 2:
				if (!Block.Exists(blocks, BlockType.Toitsu, (HaiID)i))
				{
					blocks.Add(Extract(BlockType.Toitsu, (HaiID)i));
					break;
				}
				blocks.Add(Extract(BlockType.Isolated, (HaiID)i));
				blocks.Add(Extract(BlockType.Isolated, (HaiID)i));
				break;
			case 3:
				blocks.Add(Extract(BlockType.AnKoutsu, (HaiID)i));
				break;
			case 4:
				blocks.Add(Extract(BlockType.AnKoutsu, (HaiID)i));
				blocks.Add(Extract(BlockType.Isolated, (HaiID)i));
				break;
			}
		}
	}

	public List<Block> ExtractAllAnkoInMark(HaiMarkType type)
	{
		List<Block> blocks = new List<Block>(4);
		ExtractAllAnkoInMark(in blocks, type);
		return blocks;
	}

	public void ExtractAllAnkoInMark(in List<Block> blocks, HaiMarkType type)
	{
		int num = type.ToHaiID(SuID.SU1);
		for (int i = 0; i <= 8; i++)
		{
			int num2 = num + i;
			if (m_counts[num2] >= 3)
			{
				blocks.Add(Extract(BlockType.AnKoutsu, (HaiID)num2));
			}
		}
	}

	public void ExtractAllSyuntsuInMark(in List<Block> blocks, HaiMarkType type, ExtractOrderType orderType, bool isTrySyuntsuAfterKanchan, bool[] availableHaiIDFlags)
	{
		int num = type.ToHaiID(SuID.SU1);
		for (int i = 0; i <= 6; i++)
		{
			int num2 = num + s_extractOrder[(int)orderType][i];
			while (m_counts[num2] >= 1 && m_counts[num2 + 2] >= 1)
			{
				if (m_counts[num2 + 1] > 0)
				{
					blocks.Add(Extract(BlockType.AnShuntsu, (HaiID)num2));
					continue;
				}
				if (!isTrySyuntsuAfterKanchan || (availableHaiIDFlags != null && !availableHaiIDFlags[num2 + 1]))
				{
					break;
				}
				blocks.Add(Extract(BlockType.KanchanTatsu, (HaiID)num2));
			}
		}
	}

	public void ExtractAllPenchanInMark(List<Block> blocks, HaiMarkType type, bool[] availableHaiIDFlags)
	{
		int num = type.ToHaiID(SuID.SU1);
		int num2 = num;
		while (m_counts[num2] >= 1 && m_counts[num2 + 1] >= 1 && (availableHaiIDFlags == null || availableHaiIDFlags[num2 + 2]))
		{
			blocks.Add(Extract(BlockType.PenchanTatsu, (HaiID)num2));
		}
		num2 = num + 7;
		while (m_counts[num2] >= 1 && m_counts[num2 + 1] >= 1 && (availableHaiIDFlags == null || availableHaiIDFlags[num2 - 1]))
		{
			blocks.Add(Extract(BlockType.PenchanTatsu, (HaiID)num2));
		}
	}

	public void ExtractAllRyanmenInMark(List<Block> blocks, HaiMarkType type, bool[] availableHaiIDFlags)
	{
		int num = type.ToHaiID(SuID.SU1);
		for (int i = 1; i <= 6; i++)
		{
			int num2 = num + i;
			while (m_counts[num2] >= 1 && m_counts[num2 + 1] >= 1 && (availableHaiIDFlags == null || availableHaiIDFlags[num2 - 1] || availableHaiIDFlags == null || availableHaiIDFlags[num2 + 2]))
			{
				blocks.Add(Extract(BlockType.RyanmenTatsu, (HaiID)num2));
			}
		}
	}

	public void ExtractAllKanchanInMark(List<Block> blocks, HaiMarkType type, ExtractOrderType orderType, bool[] availableHaiIDFlags)
	{
		int num = type.ToHaiID(SuID.SU1);
		for (int i = 0; i <= 6; i++)
		{
			int num2 = num + s_extractOrder[(int)orderType][i];
			while (m_counts[num2] >= 1 && m_counts[num2 + 2] >= 1 && (availableHaiIDFlags == null || availableHaiIDFlags[num2 + 1]))
			{
				blocks.Add(Extract(BlockType.KanchanTatsu, (HaiID)num2));
			}
		}
	}

	public int GetCountInMark(HaiMarkType markType)
	{
		int num = 0;
		(HaiID, HaiID) range = HaiUtility.GetRange(markType);
		HaiID item = range.Item1;
		HaiID item2 = range.Item2;
		for (int i = (int)item; i <= (int)item2; i++)
		{
			num += m_counts[i];
		}
		return num;
	}

	public int GetMaxCountInMark(HaiMarkType markType)
	{
		int num = 0;
		(HaiID, HaiID) range = HaiUtility.GetRange(markType);
		HaiID item = range.Item1;
		HaiID item2 = range.Item2;
		for (int i = (int)item; i <= (int)item2; i++)
		{
			if (m_counts[i] > num)
			{
				num = m_counts[i];
			}
		}
		return num;
	}

	public int GetDistinctCount()
	{
		int num = 0;
		for (int i = 0; i < 34; i++)
		{
			if (m_counts[i] > 0)
			{
				num++;
			}
		}
		return num;
	}

	private int GetHaiIndex(ExtractOrderType _orderType, int _index)
	{
		return s_extractOrder[(int)_orderType][_index];
	}
}
