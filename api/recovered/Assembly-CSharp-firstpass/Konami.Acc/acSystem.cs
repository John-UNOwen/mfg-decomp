using System.Runtime.InteropServices;

namespace Konami.Acc;

public class acSystem
{
	public enum AC_LOG_LEVEL
	{
		AC_LOG_LEVEL_FATAL = 1,
		AC_LOG_LEVEL_WARN,
		AC_LOG_LEVEL_INFO,
		AC_LOG_LEVEL_MISC
	}

	[DllImport("libacc")]
	public static extern void acSystem_Initialize();

	[DllImport("libacc")]
	public static extern void acSystem_Finalize();

	[DllImport("libacc")]
	public static extern void acLogger_Report(uint i_Level, string i_pszSect, string i_pszMsg);

	public static void SystemInitialize()
	{
		acSystem_Initialize();
	}

	public static void SystemFinalize()
	{
		acSystem_Finalize();
	}

	public static void LogReport(AC_LOG_LEVEL i_Level, string i_pszSect, string i_pszMsg)
	{
		acLogger_Report((uint)i_Level, i_pszSect, i_pszMsg);
	}
}
