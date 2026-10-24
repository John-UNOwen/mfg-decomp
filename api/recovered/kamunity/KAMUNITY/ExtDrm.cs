using System;
using System.Runtime.InteropServices;

namespace KAMUNITY;

[KamunityDllApiClass]
public static class ExtDrm
{
	[KamunityDllApiClass]
	internal class InternalMethod
	{
		public delegate IntPtr Func_EdrmCheckCreateInstance();

		public delegate void Func_EdrmCheckReleaseInstance([In] IntPtr handle);

		public delegate void Func_EdrmCheckStart([In] IntPtr handle, [In] CheckMode mode);

		public delegate void Func_EdrmCheckAbort([In] IntPtr handle);

		public delegate Result Func_EdrmCheckGetResult([In] IntPtr handle);

		public delegate ulong Func_EdrmCheckGetTotalFileSize([In] IntPtr handle);

		public delegate ulong Func_EdrmCheckGetProcessedFileSize([In] IntPtr handle);

		public delegate ulong Func_EdrmCheckGetTotalFiles([In] IntPtr handle);

		public delegate ulong Func_EdrmCheckGetProcessedFiles([In] IntPtr handle);

		public static Func_EdrmCheckCreateInstance EdrmCheckCreateInstance { get; private set; }

		public static Func_EdrmCheckReleaseInstance EdrmCheckReleaseInstance { get; private set; }

		public static Func_EdrmCheckStart EdrmCheckStart { get; private set; }

		public static Func_EdrmCheckAbort EdrmCheckAbort { get; private set; }

		public static Func_EdrmCheckGetResult EdrmCheckGetResult { get; private set; }

		public static Func_EdrmCheckGetTotalFileSize EdrmCheckGetTotalFileSize { get; private set; }

		public static Func_EdrmCheckGetProcessedFileSize EdrmCheckGetProcessedFileSize { get; private set; }

		public static Func_EdrmCheckGetTotalFiles EdrmCheckGetTotalFiles { get; private set; }

		public static Func_EdrmCheckGetProcessedFiles EdrmCheckGetProcessedFiles { get; private set; }
	}

	public enum CheckMode
	{
		Simple,
		CheckSum,
		Full
	}

	public enum Result
	{
		Idle,
		InProgress,
		Ok,
		Bad,
		Aborted
	}

	private static IntPtr m_romCheckerInstance = IntPtr.Zero;

	public static void CreateInstance()
	{
		if (m_romCheckerInstance == IntPtr.Zero)
		{
			m_romCheckerInstance = InternalMethod.EdrmCheckCreateInstance?.Invoke() ?? IntPtr.Zero;
		}
		else
		{
			Logger.LogWarningFormat("extd: CreateInstance already has instance");
		}
	}

	public static void ReleaseInstance()
	{
		if (m_romCheckerInstance != IntPtr.Zero)
		{
			InternalMethod.EdrmCheckReleaseInstance?.Invoke(m_romCheckerInstance);
			m_romCheckerInstance = IntPtr.Zero;
		}
	}

	public static void CheckStart(CheckMode mode)
	{
		InternalMethod.EdrmCheckStart?.Invoke(m_romCheckerInstance, mode);
	}

	public static void CheckAbort()
	{
		InternalMethod.EdrmCheckAbort?.Invoke(m_romCheckerInstance);
	}

	public static Result GetResult()
	{
		return InternalMethod.EdrmCheckGetResult?.Invoke(m_romCheckerInstance) ?? Result.Bad;
	}

	public static ulong GetTotalFileSize()
	{
		return InternalMethod.EdrmCheckGetTotalFileSize?.Invoke(m_romCheckerInstance) ?? 0;
	}

	public static ulong GetProcessedFileSize()
	{
		return InternalMethod.EdrmCheckGetProcessedFileSize?.Invoke(m_romCheckerInstance) ?? 0;
	}

	public static ulong GetTotalFiles()
	{
		return InternalMethod.EdrmCheckGetTotalFiles?.Invoke(m_romCheckerInstance) ?? 0;
	}

	public static ulong GetProcessedFiles()
	{
		return InternalMethod.EdrmCheckGetProcessedFiles?.Invoke(m_romCheckerInstance) ?? 0;
	}
}
