using System;
using System.Runtime.InteropServices;

public class EXECEXE
{
	public delegate IntPtr fnCreateFileCache(string i_lpFileName);

	public delegate void fnDebugPuts(string i_lpMsg);

	public static readonly IntPtr INVALID_HANDLE_VALUE;

	public const uint INFINITE = uint.MaxValue;

	public const uint WAIT_ABANDONED = 128u;

	public const uint WAIT_OBJECT_0 = 0u;

	public const uint WAIT_TIMEOUT = 258u;

	public const uint WAIT_FAILED = uint.MaxValue;

	public static readonly IntPtr m_hExecexe;

	public static readonly fnCreateFileCache m_fnCreateFileCache;

	public static readonly fnDebugPuts m_fnDebugPuts;

	static EXECEXE()
	{
		INVALID_HANDLE_VALUE = new IntPtr(-1);
		m_hExecexe = IntPtr.Zero;
		m_fnCreateFileCache = null;
		m_fnDebugPuts = null;
#if UNITY_WEBGL
		// execexe.dll is the shipped packer's native cache module; kernel32 P/Invokes do not
		// exist in the browser, and an unguarded DllNotFoundException here broke the boot
		// scene. Zero handles already mean the CreateFileCache/DebugPuts helpers no-op.
		if (true)
		{
			return;
		}
#endif
		m_hExecexe = GetModuleHandle("execexe.dll");
		if (!(m_hExecexe == IntPtr.Zero))
		{
			m_fnCreateFileCache = Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_hExecexe, "EXECEXE_CreateFileCache"), typeof(fnCreateFileCache)) as fnCreateFileCache;
			m_fnDebugPuts = Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_hExecexe, "EXECEXE_DebugPutsUtf8"), typeof(fnDebugPuts)) as fnDebugPuts;
		}
	}

	[DllImport("kernel32.dll")]
	public static extern IntPtr GetModuleHandleA(string i_lpModuleName);

	public static IntPtr GetModuleHandle(string i_lpModuleName)
	{
		return GetModuleHandleA(i_lpModuleName);
	}

	[DllImport("kernel32.dll")]
	public static extern IntPtr GetProcAddress(IntPtr i_hModule, string i_lpProcName);

	public static IntPtr CreateFileCache(string i_lpFileName)
	{
		if (m_fnCreateFileCache == null)
		{
			return IntPtr.Zero;
		}
		return m_fnCreateFileCache(i_lpFileName);
	}

	[DllImport("kernel32.dll")]
	public static extern uint WaitForSingleObject(IntPtr i_hHandle, uint i_dwMilliseconds);

	[DllImport("kernel32.dll")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool CloseHandle(IntPtr i_hObject);

	public static void DebugPuts(string i_lpMsg)
	{
		if (m_fnDebugPuts != null)
		{
			m_fnDebugPuts(i_lpMsg);
		}
	}
}
