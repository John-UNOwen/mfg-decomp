using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFG.MahjonPlay.Hand;

[Serializable]
internal class HaiContextStack
{
	private class Context
	{
		public Counter HaiCounter = new Counter();

		public Block[] Elements = new Block[14];

		public int ElementCount;

		public Context()
		{
			for (int i = 0; i < 14; i++)
			{
				Elements[i] = default(Block);
			}
		}
	}

	private Context[] m_stack = new Context[6];

	private int m_count;

	private const int STACK_SIZE = 6;

	private const int ELEMENT_MAX = 14;

	public HaiContextStack()
	{
		for (int i = 0; i < 6; i++)
		{
			m_stack[i] = new Context();
		}
	}

	public void Clear()
	{
		m_count = 0;
	}

	public void Push(Counter hai, List<Block> block)
	{
		Counter.Copy(hai, m_stack[m_count].HaiCounter);
		int count = block.Count;
		for (int i = 0; i < count; i++)
		{
			m_stack[m_count].Elements[i] = block[i];
		}
		m_stack[m_count].ElementCount = count;
		m_count++;
	}

	public void Peek(in Counter hai, in List<Block> block)
	{
		int num = m_count - 1;
		Counter.Copy(m_stack[num].HaiCounter, hai);
		block.Clear();
		int elementCount = m_stack[num].ElementCount;
		for (int i = 0; i < elementCount; i++)
		{
			block.Add(m_stack[num].Elements[i]);
		}
	}

	public void Pop(in Counter hai, in List<Block> block)
	{
		m_count--;
		Counter.Copy(m_stack[m_count].HaiCounter, hai);
		block.Clear();
		int elementCount = m_stack[m_count].ElementCount;
		for (int i = 0; i < elementCount; i++)
		{
			block.Add(m_stack[m_count].Elements[i]);
		}
	}

	private void CheckDepth(int count)
	{
		if (m_count != count)
		{
			Debug.Log($"invalid Stack {count}");
		}
	}
}
