using System;
using System.Collections.Generic;
using MFG.GameData.EventTaku;
using MFG.Types;
using UnityEngine;

[Serializable]
public class GameEventStatus : ISerializationCallbackReceiver
{
	public const string ApliInfoKindName = "events";

	private Dictionary<GameEventType, GameEventData> _gameEventDatas = new Dictionary<GameEventType, GameEventData>();

	[SerializeField]
	[HideInInspector]
	private List<GameEventData> list = new List<GameEventData>();

	public bool IsActive(GameEventType gameEventType)
	{
		return GetGameEventData(gameEventType)?.IsActive ?? false;
	}

	public GameEventData GetGameEventData(GameEventType gameEventType)
	{
		if (_gameEventDatas.TryGetValue(gameEventType, out var value))
		{
			return value;
		}
		return null;
	}

	public static GameEventType ToGameEventType(MFG.Types.EventType eventType)
	{
		try
		{
			if (eventType == MFG.Types.EventType.None)
			{
				return GameEventType.None;
			}
			BaseEventGameData baseEventGameData = GameDataManager.EventTaku.Current(eventType);
			return GameDataManager.Mahjong.IsConstancyTaku ? baseEventGameData.Master.ConstancyFlagType : baseEventGameData.FlagType;
		}
		catch
		{
		}
		return GameEventType.None;
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		_gameEventDatas.Clear();
		foreach (GameEventType value in Enum.GetValues(typeof(GameEventType)))
		{
			_gameEventDatas[value] = new GameEventData
			{
				EventType = value,
				IsActive = false,
				Begin = DateTimeOffset.MinValue,
				End = DateTimeOffset.MinValue
			};
		}
		foreach (GameEventData item in list)
		{
			if (item.EventType != 0)
			{
				_gameEventDatas[item.EventType] = item;
			}
		}
	}

	public static GameEventStatus LoadFromJson(string jsonString)
	{
		Debug.Log("GameEventStatus Load : " + jsonString);
		GameEventStatus gameEventStatus = JsonUtility.FromJson<GameEventStatus>(jsonString);
		if (gameEventStatus == null)
		{
			gameEventStatus = new GameEventStatus();
		}
		return gameEventStatus;
	}
}
