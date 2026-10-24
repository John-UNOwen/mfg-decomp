using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MFG.Types;

namespace MFG.MahjonPlay.Hand;

public class BlockDivider
{
	private HaiContextStack m_stack = new HaiContextStack();

	private BlockSetCache m_cache = new BlockSetCache();

	private bool[] m_isAvailableHaiID = new bool[34];

	private Counter m_tehaiCounter = new Counter();

	private Counter m_meldCounter;

	public async UniTask<List<List<StructuredHand>>> DivideAsOneByOneDiscarded(HaiID[] concealdHais, BlockSet meldedBlocks, bool flushCache = true)
	{
		List<List<StructuredHand>> ret = new List<List<StructuredHand>>();
		if (flushCache)
		{
			m_cache.Flush();
		}
		m_meldCounter = meldedBlocks.CreateCounter();
		int handItemCount = concealdHais.Length;
		int i = 0;
		while (i < handItemCount)
		{
			HaiID haiID = concealdHais[i];
			List<StructuredHand> list = new List<StructuredHand>();
			m_tehaiCounter.Clear();
			m_tehaiCounter.AddWithIgnoreIndex(concealdHais, i);
			ModifyAvailableHaiID(m_tehaiCounter, m_meldCounter);
			HaiMarkType mark = haiID.GetMark();
			m_cache.SetDirty(mark, isDirty: true);
			Divide(list, m_tehaiCounter, meldedBlocks);
			m_cache.SetDirty(mark, isDirty: true);
			ret.Add(list);
			if (i != 0 && i % 5 == 0)
			{
				await UniTask.Yield();
			}
			int num = i + 1;
			i = num;
		}
		m_cache.Flush();
		return ret;
	}

	public List<StructuredHand> Divide(HaiID[] concealdHais, BlockSet meldedBlocks, bool isIgnoreTsumoHai = false)
	{
		m_cache.Flush();
		int ignoreIndex = ((concealdHais.Length % 3 == 2 && isIgnoreTsumoHai) ? (concealdHais.Length - 1) : (-1));
		List<StructuredHand> list = new List<StructuredHand>();
		m_meldCounter = meldedBlocks.CreateCounter();
		m_tehaiCounter.Clear();
		m_tehaiCounter.AddWithIgnoreIndex(concealdHais, ignoreIndex);
		ModifyAvailableHaiID(m_tehaiCounter, m_meldCounter);
		Divide(list, m_tehaiCounter, meldedBlocks);
		m_cache.Flush();
		return list;
	}

	private void Divide(List<StructuredHand> ret, Counter tehai, BlockSet meldedBlocks)
	{
		DivideAsNormalHand(ret, tehai, meldedBlocks);
		DivideAsChiToitsuHand(ret, tehai, meldedBlocks);
		DivideAsKokushiHand(ret, tehai, meldedBlocks);
	}

	public List<StructuredHand> DivideAsNormalHand(Counter tehai, BlockSet meldedBlocks)
	{
		List<StructuredHand> list = new List<StructuredHand>();
		DivideAsNormalHand(list, tehai, meldedBlocks);
		return list;
	}

	private void DivideAsNormalHand(List<StructuredHand> ret, Counter tehai, BlockSet meldedBlocks)
	{
		for (int i = 0; i < 4; i++)
		{
			HaiMarkType markType = (HaiMarkType)i;
			if (m_cache.IsDirty(markType))
			{
				m_cache.PatternLists[i] = DivideAsNormalHandPartial(tehai, markType);
				m_cache.SetDirty(markType, isDirty: false);
			}
		}
		JoinBlockSet(ret, m_cache.PatternLists, meldedBlocks);
	}

	private List<BlockSet> DivideAsNormalHandPartial(Counter _tehai, HaiMarkType markType)
	{
		Counter hai = _tehai.Clone();
		List<BlockSet> list = new List<BlockSet>(64);
		List<Block> elements = new List<Block>(14);
		if (markType == HaiMarkType.Jihai)
		{
			hai.ExtractAllBlockFromSamehai(in elements, HaiMarkType.Jihai, m_isAvailableHaiID);
			BlockSet blockSet = new BlockSet();
			blockSet.AddBlock(elements);
			list.Add(blockSet);
			return list;
		}
		if (hai.GetCountInMark(markType) <= 0)
		{
			return list;
		}
		bool flag = hai.GetMaxCountInMark(markType) >= 3;
		int num = (flag ? 32 : 16);
		long num2 = 0L;
		m_stack.Clear();
		m_stack.Push(hai, elements);
		for (int i = 0; i < 10; i++)
		{
			m_stack.Peek(in hai, in elements);
			if (i > 0)
			{
				HaiID haiID = (HaiID)markType.ToHaiID((SuID)(0 + (i - 1) % 9));
				if (hai.GetCount(haiID) < 2)
				{
					continue;
				}
				elements.Add(hai.Extract(BlockType.Toitsu, haiID));
			}
			m_stack.Push(hai, elements);
			for (int j = 0; j < 10; j++)
			{
				m_stack.Peek(in hai, in elements);
				if (j > 0)
				{
					HaiID haiID2 = (HaiID)markType.ToHaiID((SuID)(0 + (j - 1) % 9));
					if (hai.GetCount(haiID2) < 2)
					{
						continue;
					}
					if (!Block.Exists(elements, BlockType.Toitsu, haiID2))
					{
						elements.Add(hai.Extract(BlockType.Toitsu, haiID2));
					}
				}
				m_stack.Push(hai, elements);
				for (int k = 0; k < num; k++)
				{
					m_stack.Peek(in hai, in elements);
					if (flag && k < 16)
					{
						hai.ExtractAllAnkoInMark(in elements, markType);
					}
					hai.ExtractAllSyuntsuInMark(in elements, markType, (Counter.ExtractOrderType)(k % 8), k / 8 % 2 == 0, m_isAvailableHaiID);
					if (k >= 16)
					{
						hai.ExtractAllAnkoInMark(in elements, markType);
					}
					if (k % 16 == 0)
					{
						num2 = BlockSet.GetPatternFlag(elements);
					}
					else if (num2 == BlockSet.GetPatternFlag(elements))
					{
						continue;
					}
					m_stack.Push(hai, elements);
					for (int l = 0; l < 10; l++)
					{
						m_stack.Peek(in hai, in elements);
						if (l > 0)
						{
							HaiID haiID3 = (HaiID)markType.ToHaiID((SuID)(0 + (l - 1) % 9));
							if (hai.GetCount(haiID3) < 2)
							{
								continue;
							}
							if (!Block.Exists(elements, BlockType.Toitsu, haiID3))
							{
								elements.Add(hai.Extract(BlockType.Toitsu, haiID3));
							}
						}
						hai.ExtractAllPenchanInMark(elements, markType, m_isAvailableHaiID);
						m_stack.Push(hai, elements);
						for (int m = 0; m < 4; m++)
						{
							m_stack.Peek(in hai, in elements);
							if (hai.GetCountInMark(markType) <= 0)
							{
								StorePattern(list, elements);
								break;
							}
							if (m % 2 == 1)
							{
								hai.ExtractAllRyanmenInMark(elements, markType, m_isAvailableHaiID);
							}
							hai.ExtractAllKanchanInMark(elements, markType, (Counter.ExtractOrderType)(m / 2), m_isAvailableHaiID);
							if (m % 2 == 0)
							{
								hai.ExtractAllRyanmenInMark(elements, markType, m_isAvailableHaiID);
							}
							hai.ExtractAllBlockFromSamehai(in elements, markType, m_isAvailableHaiID);
							StorePattern(list, elements);
						}
						m_stack.Pop(in hai, in elements);
					}
					m_stack.Pop(in hai, in elements);
				}
				m_stack.Pop(in hai, in elements);
			}
			m_stack.Pop(in hai, in elements);
		}
		m_stack.Pop(in hai, in elements);
		return list;
		void StorePattern(List<BlockSet> _patterns, List<Block> _elements)
		{
			if (elements.Count > 0)
			{
				BlockSet blockSet2 = new BlockSet();
				blockSet2.AddBlock(_elements);
				if (!_patterns.Contains(blockSet2))
				{
					_patterns.Add(blockSet2);
				}
			}
		}
	}

	private void JoinBlockSet(List<StructuredHand> ret, List<BlockSet>[] blockSetsByMark, BlockSet meldedBlocks)
	{
		for (int i = 0; i <= 2; i++)
		{
			List<BlockSet> list = blockSetsByMark[i];
			if (list.Count == 0)
			{
				list.Add(new BlockSet(BlockType.None, HaiID.M1));
			}
		}
		new List<StructuredHand>(64);
		BlockSet blockSet = new BlockSet();
		BlockSet blockSet2 = blockSetsByMark[3][0];
		int count = blockSetsByMark[0].Count;
		int count2 = blockSetsByMark[1].Count;
		int count3 = blockSetsByMark[2].Count;
		for (int j = 0; j < count; j++)
		{
			BlockSet blockSet3 = blockSetsByMark[0][j];
			for (int k = 0; k < count2; k++)
			{
				BlockSet blockSet4 = blockSetsByMark[1][k];
				for (int l = 0; l < count3; l++)
				{
					BlockSet blockSet5 = blockSetsByMark[2][l];
					blockSet.Clear();
					blockSet.DivideType = DivideType.Normal;
					if (blockSet3.IsValid())
					{
						blockSet.AddBlock(blockSet3);
					}
					if (blockSet4.IsValid())
					{
						blockSet.AddBlock(blockSet4);
					}
					if (blockSet5.IsValid())
					{
						blockSet.AddBlock(blockSet5);
					}
					if (blockSet2.IsValid())
					{
						blockSet.AddBlock(blockSet2);
					}
					if (meldedBlocks.IsValid())
					{
						blockSet.AddBlock(meldedBlocks);
					}
					if (blockSet.IsValid())
					{
						StructuredHand structuredHand = new StructuredHand();
						structuredHand.Blocks = blockSet.Clone();
						ret.Add(structuredHand);
					}
				}
			}
		}
	}

	private void DivideAsChiToitsuHand(List<StructuredHand> ret, Counter _tehai, BlockSet meldedBlocks)
	{
		if (meldedBlocks.Count() > 0)
		{
			return;
		}
		Counter counter = _tehai.Clone();
		StructuredHand structuredHand = new StructuredHand();
		structuredHand.Blocks.DivideType = DivideType.Chitoitsu;
		for (int i = 0; i < 34; i++)
		{
			HaiID haiID = (HaiID)i;
			switch (counter.GetCount(haiID))
			{
			case 1:
				structuredHand.Blocks.AddBlock(counter.Extract(BlockType.Isolated, haiID));
				break;
			case 2:
				structuredHand.Blocks.AddBlock(counter.Extract(BlockType.Toitsu, haiID));
				break;
			case 3:
				structuredHand.Blocks.AddBlock(counter.Extract(BlockType.AnKoutsu, haiID));
				break;
			case 4:
				structuredHand.Blocks.AddBlock(counter.Extract(BlockType.Isolated, haiID));
				structuredHand.Blocks.AddBlock(counter.Extract(BlockType.AnKoutsu, haiID));
				break;
			}
		}
		ret.Add(structuredHand);
	}

	private void DivideAsKokushiHand(List<StructuredHand> ret, Counter _tehai, BlockSet meldedBlocks)
	{
		if (meldedBlocks.Count() > 0 || GameDataManager.Mahjong.TakuType == TakuType.Nima)
		{
			return;
		}
		Counter counter = _tehai.Clone();
		StructuredHand structuredHand = new StructuredHand();
		structuredHand.Blocks.DivideType = DivideType.Kokushi;
		for (int i = 0; i < 34; i++)
		{
			HaiID haiID = (HaiID)i;
			int count = counter.GetCount(haiID);
			if (!haiID.IsYaochu())
			{
				for (int j = 0; j < count; j++)
				{
					structuredHand.Blocks.AddBlock(counter.Extract(BlockType.Isolated, haiID));
				}
				continue;
			}
			switch (count)
			{
			case 1:
				structuredHand.Blocks.AddBlock(counter.Extract(BlockType.Isolated, haiID));
				break;
			case 2:
				structuredHand.Blocks.AddBlock(counter.Extract(BlockType.Toitsu, haiID));
				break;
			case 3:
				structuredHand.Blocks.AddBlock(counter.Extract(BlockType.AnKoutsu, haiID));
				break;
			case 4:
				structuredHand.Blocks.AddBlock(counter.Extract(BlockType.Isolated, haiID));
				structuredHand.Blocks.AddBlock(counter.Extract(BlockType.AnKoutsu, haiID));
				break;
			}
		}
		if (structuredHand.Blocks.Count() > 0)
		{
			ret.Add(structuredHand);
		}
	}

	private void ModifyAvailableHaiID(Counter tehaiCounter, Counter meldCounter)
	{
		for (int i = 0; i < 34; i++)
		{
			HaiID haiID = (HaiID)i;
			m_isAvailableHaiID[i] = haiID.IsAvailable() && tehaiCounter[haiID] + meldCounter[haiID] < 4;
		}
	}
}
