using System;
using System.Runtime.InteropServices;

namespace KAMUNITY;

internal static class NativeMethods
{
	[DllImport("kernel32", BestFitMapping = false, CharSet = CharSet.Ansi, SetLastError = true)]
	internal static extern IntPtr LoadLibraryA(string lpFileName);

	[DllImport("kernel32", SetLastError = true)]
	internal static extern int FreeLibrary(IntPtr hModule);

	[DllImport("kernel32", BestFitMapping = false, CharSet = CharSet.Ansi, SetLastError = true)]
	internal static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

	public static int GetLastError()
	{
		return Marshal.GetLastWin32Error();
	}
}
