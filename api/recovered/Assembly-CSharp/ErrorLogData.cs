using System;

[Serializable]
public class ErrorLogData : LogData
{
	public ERROR m_code;

	public uint m_option;

	public ERROR_LEVEL m_level = ERROR_LEVEL.HALT;

	public ErrorLogData()
	{
	}

	public ErrorLogData(ERROR code_, uint option_, ERROR_LEVEL level_)
	{
		m_code = code_;
		m_option = option_;
		m_level = level_;
	}
}
