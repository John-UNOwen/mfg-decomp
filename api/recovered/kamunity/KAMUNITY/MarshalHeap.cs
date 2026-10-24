using System;
using System.Runtime.InteropServices;

namespace KAMUNITY;

public class MarshalHeap : IDisposable
{
	public IntPtr buffer { get; private set; } = IntPtr.Zero;


	public int size { get; private set; }

	public string ToStringAnsi()
	{
		if (buffer == IntPtr.Zero)
		{
			return "";
		}
		return Marshal.PtrToStringAnsi(buffer);
	}

	public MarshalHeap(int _size)
	{
		size = _size;
		buffer = Marshal.AllocCoTaskMem(size);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (buffer != IntPtr.Zero)
		{
			Marshal.FreeCoTaskMem(buffer);
			buffer = IntPtr.Zero;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
