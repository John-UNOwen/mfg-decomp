public class BackupDataManager
{
	public enum BACKUP_KIND
	{
		TCS_BOOK_KEEPING,
		SERVICE_BUTTON_LOG,
		NUM
	}

	private static BackupDataManager instance;

	private BaseBackupData[] m_data;

	public bool Initialized { get; private set; }

	public static BackupDataManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new BackupDataManager();
			}
			return instance;
		}
		private set
		{
			instance = value;
		}
	}

	public void Initialize()
	{
		if (!Initialized)
		{
			m_data = new BaseBackupData[2];
			m_data[0] = new TCSBookKeeping();
			m_data[1] = new ServiceButtonLog();
			Initialized = true;
		}
	}

	public void Load()
	{
		BaseBackupData[] data = m_data;
		for (int i = 0; i < data.Length; i++)
		{
			data[i].Load();
		}
	}

	public void Save()
	{
		BaseBackupData[] data = m_data;
		for (int i = 0; i < data.Length; i++)
		{
			data[i].Save();
		}
	}

	public void Clear()
	{
		BaseBackupData[] data = m_data;
		for (int i = 0; i < data.Length; i++)
		{
			data[i].Clear();
		}
	}

	public void Recreate()
	{
		BaseBackupData[] data = m_data;
		foreach (BaseBackupData obj in data)
		{
			obj.RemoveFile();
			obj.Load();
		}
	}

	public void Reset(BACKUP_KIND kind)
	{
		if (kind < BACKUP_KIND.NUM)
		{
			m_data[(int)kind].Clear();
		}
	}

	public BACKUP_KIND IsError()
	{
		for (BACKUP_KIND bACKUP_KIND = BACKUP_KIND.TCS_BOOK_KEEPING; bACKUP_KIND < BACKUP_KIND.NUM; bACKUP_KIND++)
		{
			if (m_data[(int)bACKUP_KIND].IsError)
			{
				return bACKUP_KIND;
			}
		}
		return BACKUP_KIND.NUM;
	}

	public TCSBookKeeping TCSBookKeeping()
	{
		return m_data[0] as TCSBookKeeping;
	}

	public ServiceButtonLog ServiceButtonLog()
	{
		return m_data[1] as ServiceButtonLog;
	}
}
