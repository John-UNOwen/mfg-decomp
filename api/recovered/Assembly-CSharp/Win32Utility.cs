using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

internal static class Win32Utility
{
	public struct RECT
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	public struct WINDOWINFO
	{
		public uint cbSize;

		public RECT rcWindow;

		public RECT rcClient;

		public uint dwStyle;

		public uint dwExStyle;

		public uint dwWindowStatus;

		public uint cxWindowBorders;

		public uint cyWindowBorders;

		public ushort atomWindowType;

		public short wCreatorVersion;
	}

	public delegate bool EnumWindowsDelegate(IntPtr hWnd, IntPtr lparam);

	public class WindowFinder
	{
		private IntPtr _hWndMain;

		private IntPtr _hWndSend;

		private IntPtr _hWndGame;

		private WINDOWINFO _windowInfo;

		private Predicate<string> _match;

		public IntPtr Find(Predicate<string> match)
		{
			_match = match;
			EnumWindows(EnumWindowCallBack, IntPtr.Zero);
			if (_hWndMain != IntPtr.Zero)
			{
				_hWndSend = _hWndMain;
			}
			UnityEngine.Debug.Log($"scope hWndMain {(int)_hWndMain:X8}");
			UnityEngine.Debug.Log($"scope hWndSend {(int)_hWndSend:X8}");
			return _hWndSend;
		}

		private bool EnumWindowCallBack(IntPtr hWnd, IntPtr lparam)
		{
			Process currentProcess = Process.GetCurrentProcess();
			GetWindowThreadProcessId(hWnd, out var lpdwProcessId);
			if (lpdwProcessId == currentProcess.Id)
			{
				int windowTextLength = GetWindowTextLength(hWnd);
				if (windowTextLength > 0)
				{
					StringBuilder stringBuilder = new StringBuilder(windowTextLength + 1);
					GetWindowText(hWnd, stringBuilder, stringBuilder.Capacity);
					if (_match(stringBuilder.ToString()))
					{
						_hWndMain = hWnd;
						GetWindowInfo(hWnd, out _windowInfo);
					}
				}
			}
			return true;
		}
	}

	[DllImport("User32.dll")]
	public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll")]
	public static extern IntPtr GetActiveWindow();

	[DllImport("user32.dll")]
	public static extern IntPtr FindWindowEx(IntPtr hParent, IntPtr hChildAfter, string pClassName, string pWindowName);

	[DllImport("User32.dll")]
	public static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetWindowInfo(IntPtr hWnd, out WINDOWINFO pwi);

	[DllImport("User32.dll", SetLastError = true)]
	public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern int GetWindowTextLength(IntPtr hWnd);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool EnumWindows(EnumWindowsDelegate lpEnumFunc, IntPtr lparam);

	public static int MinimizeWindow(IntPtr hWnd)
	{
		if (hWnd == IntPtr.Zero)
		{
			return 0;
		}
		return ShowWindow(hWnd, 6);
	}

	public static IntPtr FindWindow(Predicate<string> match)
	{
		return new WindowFinder().Find(match);
	}
}
