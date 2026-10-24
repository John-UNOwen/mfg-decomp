using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static class MouseInputWrapper
{
	private class NativeMethods
	{
		[DllImport("user32.dll", SetLastError = true)]
		public static extern int SendInput(int nInputs, ref Input pInputs, int cbsize);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr GetMessageExtraInfo();

		[DllImport("user32.dll")]
		public static extern bool SetCursorPos(int x, int y);
	}

	private struct MouseInput
	{
		public int X;

		public int Y;

		public int Data;

		public int Flags;

		public int Time;

		public IntPtr ExtraInfo;
	}

	private struct KeyboardInput
	{
		public short VirtualKey;

		public short ScanCode;

		public int Flags;

		public int Time;

		public IntPtr ExtraInfo;
	}

	private struct HardwareInput
	{
		public int uMsg;

		public short wParamL;

		public short wParamH;
	}

	private struct Input
	{
		public int Type;

		public InputUnion ui;
	}

	[StructLayout(LayoutKind.Explicit)]
	private struct InputUnion
	{
		[FieldOffset(0)]
		public MouseInput Mouse;

		[FieldOffset(0)]
		public KeyboardInput Keyboard;

		[FieldOffset(0)]
		public HardwareInput Hardware;
	}

	private const int MOUSEEVENTF_MOVE = 1;

	private const int MOUSEEVENTF_LEFTDOWN = 2;

	private const int MOUSEEVENTF_LEFTUP = 4;

	private const int MOUSEEVENTF_VIRTUALDESK = 16384;

	private const int MOUSEEVENTF_ABSOLUTE = 32768;

	private static Input MouseMoveData(float x, float y, IntPtr extraInfo)
	{
		Vector2 touchPosition = Bi2xInput.Instance.GetTouchPosition();
		x = touchPosition.x / 1920f * 65535f;
		y = (1080f - touchPosition.y) / 1080f * 65535f;
		Input result = default(Input);
		result.Type = 0;
		result.ui.Mouse.Flags = 32769;
		result.ui.Mouse.Data = 0;
		result.ui.Mouse.X = (int)x;
		result.ui.Mouse.Y = (int)y;
		result.ui.Mouse.Time = 0;
		result.ui.Mouse.ExtraInfo = extraInfo;
		return result;
	}

	private static Input MouseDataWithoutMove(int flags, IntPtr extraInfo)
	{
		Input result = default(Input);
		result.Type = 0;
		result.ui.Mouse.Flags = flags;
		result.ui.Mouse.Data = 0;
		result.ui.Mouse.X = 0;
		result.ui.Mouse.Y = 0;
		result.ui.Mouse.Time = 0;
		result.ui.Mouse.ExtraInfo = extraInfo;
		return result;
	}

	public static void SendMouseMove(float x, float y)
	{
		IntPtr messageExtraInfo = NativeMethods.GetMessageExtraInfo();
		Input pInputs = MouseMoveData(x, y, messageExtraInfo);
		NativeMethods.SetCursorPos(pInputs.ui.Mouse.X, pInputs.ui.Mouse.Y);
		NativeMethods.SendInput(1, ref pInputs, Marshal.SizeOf(pInputs));
	}

	public static void SendMouseDown()
	{
		IntPtr messageExtraInfo = NativeMethods.GetMessageExtraInfo();
		Input pInputs = MouseDataWithoutMove(2, messageExtraInfo);
		NativeMethods.SendInput(1, ref pInputs, Marshal.SizeOf(pInputs));
	}

	public static void SendMouseUp()
	{
		IntPtr messageExtraInfo = NativeMethods.GetMessageExtraInfo();
		Input pInputs = MouseDataWithoutMove(4, messageExtraInfo);
		NativeMethods.SendInput(1, ref pInputs, Marshal.SizeOf(pInputs));
	}
}
