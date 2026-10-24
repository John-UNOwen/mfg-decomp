using System;
using MFG.MasterGameData;
using MFG.Types.PlayerItem;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class PlayerItemInfo
{
	public ItemID ItemID;

	[SerializeField]
	private int m_count;

	public int Count
	{
		get
		{
			return m_count;
		}
		set
		{
			m_count = value;
		}
	}

	public Item Master => PlayerItemMaster.GetItem(ItemID);

	public bool Add(int count = 1)
	{
		Count += count;
		return true;
	}

	public bool Consume(int consumeCount = 1)
	{
		Count -= consumeCount;
		if (Count < 0)
		{
			Count = 0;
			return false;
		}
		return true;
	}
}
