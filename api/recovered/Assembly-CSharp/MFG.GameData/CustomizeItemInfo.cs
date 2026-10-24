using System;
using System.Collections.Generic;
using MFG.MasterGameData;
using MFG.Types.Customize.CustomizeItem;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class CustomizeItemInfo : ISerializationCallbackReceiver
{
	public ItemID ItemID;

	[NonSerialized]
	public bool IsNew;

	public Item Master => CustomizeItemMaster.GetItem(ItemID);

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
	}

	public static List<CustomizeItemInfo> CreateDefaultItems()
	{
		List<CustomizeItemInfo> list = new List<CustomizeItemInfo>();
		foreach (ItemID defaultItemID in CustomizeItemMaster.GetDefaultItemIDs())
		{
			list.Add(new CustomizeItemInfo
			{
				ItemID = defaultItemID
			});
		}
		return list;
	}
}
