using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RemovableItems
{
	[Header("着脱指示インデックス※0がデフォルト")]
	public int Index;

	[Header("着脱可能にするオブジェクト")]
	public List<RemovableItem> Items;

	public void Set()
	{
		List<RemovableItem> items = Items;
		if (items == null || items.Count <= 0)
		{
			return;
		}
		foreach (RemovableItem item in Items)
		{
			item.Object.SetActive(item.IsDisp);
		}
	}
}
