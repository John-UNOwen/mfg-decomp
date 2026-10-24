public class ErrorLogManager : LogManagerBase<ErrorLogData>
{
	private static ErrorLogManager instance;

	private ErrorLogManager()
		: base(BackupStorage.FORMAT.JSON, "ErrorLog", 1024)
	{
	}

	public static ErrorLogManager GetInstance()
	{
		if (instance == null)
		{
			instance = new ErrorLogManager();
			instance.Load();
		}
		return instance;
	}
}
