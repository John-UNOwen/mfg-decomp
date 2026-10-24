using System;
using System.Collections.Generic;
using System.Linq;
using MFG.MasterGameData;
using MFG.Types.Customize.Voice;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class VoiceInfo : ISerializationCallbackReceiver
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

	public Item Master => VoiceMaster.GetItem(ItemID);

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
		_objectID = VoiceMaster.ToObjectID(ItemID);
	}

	public void OnAfterDeserialize()
	{
		ItemID = VoiceMaster.ToItemID(_objectID);
	}

	public static List<VoiceInfo> CreateDefaultItems()
	{
		List<VoiceInfo> list = new List<VoiceInfo>();
		foreach (ItemID item in from x in VoiceMaster.GetItems((Item x) => x.ObtainType == ObtainType.Default)
			select x.ItemID)
		{
			list.Add(new VoiceInfo
			{
				ItemID = item,
				IsOn = true
			});
		}
		return list;
	}
}
