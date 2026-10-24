using System;
using System.Collections.Generic;
using MFG.MasterGameData;
using MFG.Types.Customize.Costume;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class CostumeItemInfo : ISerializationCallbackReceiver
{
	public ItemID ItemID;

	[NonSerialized]
	public bool IsNew;

	public Item Master => CostumeItemMaster.GetItem(ItemID);

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
	}

	public static List<CostumeItemInfo> CreateDefaultItems()
	{
		List<CostumeItemInfo> list = new List<CostumeItemInfo>();
		foreach (ItemID defaultItemID in CostumeItemMaster.GetDefaultItemIDs())
		{
			list.Add(new CostumeItemInfo
			{
				ItemID = defaultItemID
			});
		}
		return list;
	}
}
