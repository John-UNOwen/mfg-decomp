using System;
using System.Collections.Generic;
using MFG.MasterGameData;
using MFG.Types.Customize.CutinItem;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class CutinItemInfo : ISerializationCallbackReceiver
{
	public ItemID ItemID;

	[SerializeField]
	private bool m_isOn = true;

	[NonSerialized]
	public bool IsNew;

	public int AcquiredCountInGacha5;

	public bool IsOn
	{
		get
		{
			if (!m_isOn)
			{
				return !Master.IsDetachable;
			}
			return true;
		}
		set
		{
			m_isOn = value;
		}
	}

	public Item Master => CutinItemMaster.GetItem(ItemID);

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
	}

	public static List<CutinItemInfo> CreateDefaultItems()
	{
		List<CutinItemInfo> list = new List<CutinItemInfo>();
		foreach (ItemID defaultItemID in CutinItemMaster.GetDefaultItemIDs())
		{
			bool isOn = true;
			CutinItemMaster.GetItem(defaultItemID);
			list.Add(new CutinItemInfo
			{
				ItemID = defaultItemID,
				IsOn = isOn
			});
		}
		return list;
	}
}
