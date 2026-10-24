using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

[Serializable]
public class GameEventData : ISerializationCallbackReceiver
{
	public GameEventType EventType;

	public DateTimeOffset Begin;

	public DateTimeOffset End;

	[SerializeField]
	[HideInInspector]
	private string name;

	[SerializeField]
	[HideInInspector]
	private bool active;

	[SerializeField]
	[HideInInspector]
	private string begin;

	[SerializeField]
	[HideInInspector]
	private string end;

	[SerializeField]
	[HideInInspector]
	private string param;

	private Dictionary<string, string> _params = new Dictionary<string, string>();

	public bool IsActive
	{
		get
		{
			return active;
		}
		set
		{
			active = value;
		}
	}

	public string Param => param;

	public T ParamValue<T>(string key, T defaultValue = default(T))
	{
		TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
		if (converter == null || !_params.TryGetValue(key, out var value))
		{
			return defaultValue;
		}
		try
		{
			return (T)converter.ConvertFromString(value);
		}
		catch
		{
			Debug.LogWarning("GameEventData.ParamValue: 変換できませんでした。key=" + key);
			return defaultValue;
		}
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		if (!Enum.TryParse<GameEventType>(name, out EventType))
		{
			EventType = GameEventType.None;
			active = false;
			Begin = DateTimeOffset.MinValue;
			End = DateTimeOffset.MinValue;
			param = "";
			Debug.Log("GameEventData: 不明なイベント。" + DebugLogString());
			return;
		}
		if (string.IsNullOrEmpty(begin))
		{
			Begin = DateTimeOffset.MinValue;
		}
		else
		{
			Begin = DateTimeOffsetManager.GetJSTFromString(begin);
			if (Begin == default(DateTimeOffset))
			{
				Begin = DateTimeOffset.MinValue;
			}
		}
		if (string.IsNullOrEmpty(end))
		{
			End = DateTimeOffset.MaxValue;
		}
		else
		{
			End = DateTimeOffsetManager.GetJSTFromString(end);
			if (End == default(DateTimeOffset))
			{
				End = DateTimeOffset.MaxValue;
			}
		}
		if (!string.IsNullOrEmpty(param))
		{
			_params.Clear();
			_params = param.Split(",").ToDictionary((string x) => x.Split("=")[0], (string x) => x.Split("=")[1]);
		}
		Debug.Log("GameEventData: " + DebugLogString());
	}

	public string DebugLogString()
	{
		return $"name={name}, active={active}, begin={begin}, end={end}, param={param}";
	}
}
