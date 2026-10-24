using System.Runtime.InteropServices;
using System.Text;

namespace KAM;

public static class Win10ActLog
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct ACTIVATION_INFO
	{
		public uint lastGenerateTime;

		public uint count;
	}

	[DllImport("win10actlog", EntryPoint = "Win10ActLog_GetLastActLog")]
	public static extern void GetLastActLog(out ACTIVATION_INFO info);

	[DllImport("win10actlog", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "Win10ActLog_GetTimestampString")]
	public static extern void GetTimestampString(uint time, StringBuilder sb, int len);
}
