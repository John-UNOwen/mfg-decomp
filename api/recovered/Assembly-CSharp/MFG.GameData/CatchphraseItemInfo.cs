using System;
using System.Collections.Generic;
using MFG.MasterGameData;
using MFG.Types.Customize.Catchphrase;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class CatchphraseItemInfo : ISerializationCallbackReceiver
{
	[NonSerialized]
	public ItemID ItemID;

	[SerializeField]
	[HideInInspector]
	private string _objectID = "";

	[NonSerialized]
	public bool IsNew;

	public Item Master => CatchphraseItemMaster.GetItem(ItemID);

	public void OnBeforeSerialize()
	{
		_objectID = CatchphraseItemMaster.ToObjectID(ItemID);
	}

	public void OnAfterDeserialize()
	{
		ItemID = CatchphraseItemMaster.ToItemID(_objectID);
	}

	public static List<CatchphraseItemInfo> CreateDefaultItems()
	{
		List<CatchphraseItemInfo> list = new List<CatchphraseItemInfo>();
		foreach (ItemID defaultItemID in CatchphraseItemMaster.GetDefaultItemIDs())
		{
			list.Add(new CatchphraseItemInfo
			{
				ItemID = defaultItemID
			});
		}
		return list;
	}
}
