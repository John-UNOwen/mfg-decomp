using System;
using System.Collections.Generic;
using System.Linq;
using GameContents;
using KAM.Usefully;
using TestModeSkeleton;
using UnityEngine;

public class TCSBookKeeping : BaseBackupData
{
	[Serializable]
	public class CountData
	{
		public uint[] count = new uint[9];
	}

	[Serializable]
	public class LogData : ISerializationCallbackReceiver
	{
		[NonSerialized]
		public DateTime save_date;

		[NonSerialized]
		public Dictionary<string, CountData> data = new Dictionary<string, CountData>();

		[SerializeField]
		private string _saveDate = "";

		[SerializeField]
		private List<string> _keys = new List<string>();

		[SerializeField]
		private List<CountData> _values = new List<CountData>();

		public LogData()
		{
			if (GameUtility.IsKamunityBootEnable())
			{
				save_date = CabinetManager.Clock.GetTime();
			}
			else
			{
				save_date = DateTime.Now;
			}
		}

		public void OnBeforeSerialize()
		{
			_saveDate = save_date.ToString();
			_keys.Clear();
			_values.Clear();
			foreach (KeyValuePair<string, CountData> datum in data)
			{
				_keys.Add(datum.Key);
				_values.Add(datum.Value);
			}
		}

		public void OnAfterDeserialize()
		{
			save_date = DateTime.Parse(_saveDate);
			data.Clear();
			for (int i = 0; i < _values.Count; i++)
			{
				if (_values[i].count.Length < 9)
				{
					int num = _values[i].count.Length;
					Array.Resize(ref _values[i].count, 9);
					for (int j = num; j < _values[i].count.Length; j++)
					{
						_values[i].count[j] = 0u;
					}
				}
			}
			for (int k = 0; k < _keys.Count; k++)
			{
				data.Add(_keys[k], _values[k]);
			}
		}
	}

	[Serializable]
	private class SaveData : JsonSerializableFile
	{
		public List<LogData> list = new List<LogData>();

		public SaveData()
			: base(isCrypt: true, isCompress: true)
		{
		}
	}

	private bool m_isDirty;

	private SaveData m_data = new SaveData();

	[NonSerialized]
	public int DAILY_MAX = 8;

	public IReadOnlyCollection<LogData> ReserveStack => m_data.list;

	public void AddCount(string contents_id, GAME_TYPE type, uint count = 1u)
	{
		if (GameUtility.IsKamunityBootEnable() && CabinetManager.Book.Data.BookKeeping != 0)
		{
			LogData logData = Rotate();
			if (!logData.data.ContainsKey(contents_id))
			{
				logData.data.Add(contents_id, new CountData());
			}
			logData.data[contents_id].count[(int)type] += count;
			m_isDirty = true;
		}
	}

	private LogData Rotate()
	{
		DateTime date = CabinetManager.Clock.GetTime().Date;
		if (!m_data.list.Any())
		{
			m_data.list.Add(new LogData());
		}
		else
		{
			DateTime date2 = m_data.list.First().save_date.Date;
			int days = (date - date2).Days;
			for (int i = 0; i < days; i++)
			{
				m_data.list.Insert(0, new LogData());
			}
		}
		while (m_data.list.Count > DAILY_MAX)
		{
			m_data.list.RemoveAt(m_data.list.Count - 1);
		}
		return m_data.list.First();
	}

	public TCSBookKeeping()
	{
		m_file = m_data;
	}

	public override string GetFileName()
	{
		return "TCSBookKeepingInfo";
	}

	public override void Clear()
	{
		m_data.list.Clear();
		m_isDirty = true;
	}

	protected override bool IsDirty()
	{
		return m_isDirty;
	}

	protected override void PostSave()
	{
		m_isDirty = false;
	}
}
