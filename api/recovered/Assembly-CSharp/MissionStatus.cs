using System;
using System.Collections.Generic;
using MFG.Types.Mission;
using UnityEngine;

[Serializable]
public class MissionStatus : ISerializationCallbackReceiver
{
	public const string ApliInfoKindName = "missions";

	private Dictionary<Category, MissionData> _missionDatas = new Dictionary<Category, MissionData>();

	[SerializeField]
	[HideInInspector]
	private List<MissionData> list = new List<MissionData>();

	public bool IsActive(Category category)
	{
		return GetMissionData(category)?.IsActive ?? false;
	}

	public MissionData GetMissionData(Category category)
	{
		if (_missionDatas.TryGetValue(category, out var value))
		{
			return value;
		}
		return null;
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		_missionDatas.Clear();
		foreach (Category value in Enum.GetValues(typeof(Category)))
		{
			_missionDatas[value] = new MissionData
			{
				Category = value,
				IsActive = false,
				Begin = DateTimeOffset.MinValue,
				End = DateTimeOffset.MinValue
			};
		}
		foreach (MissionData item in list)
		{
			if (item.Category != 0)
			{
				_missionDatas[item.Category] = item;
			}
		}
	}

	public static MissionStatus LoadFromJson(string jsonString)
	{
		Debug.Log("MissionStatus Load : " + jsonString);
		MissionStatus missionStatus = JsonUtility.FromJson<MissionStatus>(jsonString);
		if (missionStatus == null)
		{
			missionStatus = new MissionStatus();
		}
		return missionStatus;
	}
}
