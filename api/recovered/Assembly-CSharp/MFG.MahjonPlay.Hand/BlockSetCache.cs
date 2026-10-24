using System;
using System.Collections.Generic;
using MFG.Types;
using UnityEngine;

namespace MFG.MahjonPlay.Hand;

[Serializable]
internal class BlockSetCache
{
	public List<BlockSet>[] PatternLists = new List<BlockSet>[4];

	[SerializeField]
	private bool[] m_isDirty = new bool[4];

	public BlockSetCache()
	{
		Flush();
	}

	public void SetDirty(HaiMarkType markType, bool isDirty)
	{
		m_isDirty[(int)markType] = isDirty;
	}

	public bool IsDirty(HaiMarkType markType)
	{
		return m_isDirty[(int)markType];
	}

	public void Flush()
	{
		for (int i = 0; i < 4; i++)
		{
			m_isDirty[i] = true;
		}
	}
}
