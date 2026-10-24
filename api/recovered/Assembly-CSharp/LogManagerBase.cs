using System;
using System.Collections.Generic;
using UnityEngine;

public class LogManagerBase<LOGDATA> where LOGDATA : LogData
{
	[Serializable]
	public class Logs
	{
		public List<LOGDATA> logDatas;
	}

	private BackupStorage.FORMAT m_format = BackupStorage.FORMAT.JSON;

	private string m_path;

	private int m_maxLogs = 1024;

	private List<LOGDATA> m_logDatas;

	private bool m_flush;

	public bool IsBroken { get; private set; }

	public int Count => m_logDatas.Count;

	protected LogManagerBase(BackupStorage.FORMAT format_, string filename, int maxLogs_)
	{
		m_format = format_;
		m_path = Application.persistentDataPath + "/" + filename + BackupStorage.GetFormatExt(m_format);
		m_maxLogs = maxLogs_;
		m_logDatas = new List<LOGDATA>();
		m_flush = false;
		IsBroken = false;
	}

	public void Flush()
	{
		if (m_flush)
		{
			Save();
			m_flush = false;
		}
	}

	public void Format()
	{
		m_logDatas = new List<LOGDATA>();
		m_flush = false;
		IsBroken = false;
		Save();
	}

	public bool Load()
	{
		Logs logs = BackupStorage.Load<Logs>(m_path, m_format);
		if (logs != null)
		{
			m_logDatas = logs.logDatas;
		}
		else
		{
			IsBroken = true;
			m_logDatas = new List<LOGDATA>();
		}
		return !IsBroken;
	}

	public bool Save()
	{
		if (IsBroken)
		{
			return false;
		}
		Logs logs = new Logs();
		logs.logDatas = new List<LOGDATA>(m_logDatas);
		return BackupStorage.Save(m_path, logs, m_format);
	}

	public void Set(LOGDATA log)
	{
		if (!IsBroken)
		{
			m_logDatas.Add(log);
			if (m_logDatas.Count > m_maxLogs)
			{
				m_logDatas.RemoveAt(0);
			}
			m_flush = true;
		}
	}

	public LOGDATA Get(int index)
	{
		if (index < m_logDatas.Count)
		{
			return m_logDatas[m_logDatas.Count - 1 - index];
		}
		return null;
	}
}
