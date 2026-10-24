using System;
using System.Collections.Generic;
using System.Linq;
using KAM.Usefully;
using KAMUNITY;
using UnityEngine;

public class ServiceButtonLog : BaseBackupData
{
	[Serializable]
	public class DailyData : ISerializationCallbackReceiver
	{
		[NonSerialized]
		public DateTime save_date;

		[SerializeField]
		private string _saveDate = "";

		[SerializeField]
		public int _count = -1;

		public DailyData(int offset)
		{
			save_date = GetTime().Date.AddDays(offset);
		}

		public void OnBeforeSerialize()
		{
			_saveDate = save_date.ToString();
		}

		public void OnAfterDeserialize()
		{
			save_date = DateTime.Parse(_saveDate);
		}
	}

	[Serializable]
	public class OneData : ISerializationCallbackReceiver
	{
		[NonSerialized]
		public DateTime timestamp;

		[SerializeField]
		private string _timestamp = "";

		public void OnBeforeSerialize()
		{
			_timestamp = timestamp.ToString();
		}

		public void OnAfterDeserialize()
		{
			timestamp = DateTime.Parse(_timestamp);
		}
	}

	[Serializable]
	private class SaveData : JsonSerializableFile
	{
		public List<DailyData> dailyLog = new List<DailyData>();

		public List<OneData> oneDataHistory = new List<OneData>();

		public SaveData()
			: base(isCrypt: true, isCompress: true)
		{
		}
	}

	private bool m_isDirty;

	private SaveData m_data = new SaveData();

	[NonSerialized]
	public int DAILY_MAX = 8;

	[NonSerialized]
	public int HISTORY_MAX = 20;

	public void AddServiceCount()
	{
		if (GameUtility.IsKamunityBootEnable())
		{
			DateTime time = GetTime();
			DateTime date = time.Date;
			GetLastDateData(date)._count++;
			OneData oneData = new OneData();
			oneData.timestamp = time;
			m_data.oneDataHistory.Insert(0, oneData);
			int num = m_data.oneDataHistory.Count - HISTORY_MAX;
			if (num > 0)
			{
				m_data.oneDataHistory.RemoveRange(m_data.oneDataHistory.Count - num, num);
			}
			m_isDirty = true;
		}
	}

	private DailyData GetLastDateData(DateTime now_time)
	{
		if (!m_data.dailyLog.Any())
		{
			m_data.dailyLog.Add(new DailyData(0));
		}
		else
		{
			DateTime date = m_data.dailyLog.First().save_date.Date;
			int num = (now_time - date).Days;
			if (num > DAILY_MAX)
			{
				num = DAILY_MAX;
			}
			for (int i = 0; i < num; i++)
			{
				int offset = num - i - 1;
				m_data.dailyLog.Insert(0, new DailyData(offset));
			}
		}
		while (m_data.dailyLog.Count > DAILY_MAX)
		{
			m_data.dailyLog.RemoveAt(m_data.dailyLog.Count - 1);
		}
		DailyData dailyData = m_data.dailyLog.First();
		if (dailyData._count < 0)
		{
			dailyData._count = 0;
		}
		return dailyData;
	}

	public List<int> GetDailyData()
	{
		List<int> list = new List<int>();
		GetLastDateData(GetTime().Date);
		for (int i = 0; i < m_data.dailyLog.Count; i++)
		{
			list.Add(m_data.dailyLog[i]._count);
		}
		return list;
	}

	public List<DateTime> GetOneDataHistory()
	{
		List<DateTime> list = new List<DateTime>();
		for (int i = 0; i < m_data.oneDataHistory.Count; i++)
		{
			list.Add(m_data.oneDataHistory[i].timestamp);
		}
		return list;
	}

	private static DateTime GetTime()
	{
		return Utility.Epoch2DateTime((long)Avs.TimeGet(AvsTypes.AVS_CLOCK.SERVER)) + new TimeSpan(9, 0, 0);
	}

	public ServiceButtonLog()
	{
		m_file = m_data;
	}

	public override string GetFileName()
	{
		return "ServiceButtonLog";
	}

	public override void Clear()
	{
		m_data.oneDataHistory.Clear();
		m_data.dailyLog.Clear();
		m_isDirty = true;
		Debug.LogWarning("SERVICE BUTTON LOG CLEAR");
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
