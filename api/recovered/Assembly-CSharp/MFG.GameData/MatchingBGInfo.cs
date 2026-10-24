using System;
using System.Collections.Generic;
using MFG.MasterGameData;
using MFG.Types.Customize.MatchingBackground;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class MatchingBGInfo : ISerializationCallbackReceiver
{
	[NonSerialized]
	public ItemID ItemID;

	[SerializeField]
	[HideInInspector]
	private string _objectID = "";

	[NonSerialized]
	public bool IsNew;

	[SerializeField]
	private bool m_isOn = true;

	public Item Master => MatchingBackgroundMaster.GetItem(ItemID);

	public bool IsOn
	{
		get
		{
			return m_isOn;
		}
		set
		{
			m_isOn = value;
		}
	}

	public void OnBeforeSerialize()
	{
		_objectID = MatchingBackgroundMaster.ToObjectID(ItemID);
	}

	public void OnAfterDeserialize()
	{
		ItemID = MatchingBackgroundMaster.ToItemID(_objectID);
	}

	public static List<MatchingBGInfo> CreateDefaultItems()
	{
		List<MatchingBGInfo> list = new List<MatchingBGInfo>();
		foreach (ItemID defaultItemID in MatchingBackgroundMaster.GetDefaultItemIDs())
		{
			list.Add(new MatchingBGInfo
			{
				ItemID = defaultItemID,
				IsOn = MatchingBackgroundMaster.GetDefaultEquipItemIDs().Contains(defaultItemID)
			});
		}
		return list;
	}
}
