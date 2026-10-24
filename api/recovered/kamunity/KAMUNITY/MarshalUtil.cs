using System;
using System.Runtime.InteropServices;

namespace KAMUNITY;

public static class MarshalUtil
{
	public static T GetProcDelegate<T>(IntPtr module, string methodName) where T : class
	{
		T result = null;
		if (module != IntPtr.Zero)
		{
			IntPtr procAddress = NativeMethods.GetProcAddress(module, methodName);
			if (procAddress != IntPtr.Zero)
			{
				return Marshal.GetDelegateForFunctionPointer(procAddress, typeof(T)) as T;
			}
			Logger.LogErrorFormat("could not get method : [{0}] module[{1}]", methodName, module.ToString("X"));
		}
		else
		{
			Logger.LogErrorFormat("invalid module handle : [{0}]", module.ToString("X"));
		}
		return result;
	}

	public static Delegate GetProcDelegate(IntPtr module, string methodName, Type type)
	{
		Delegate result = null;
		if (module != IntPtr.Zero)
		{
			IntPtr procAddress = NativeMethods.GetProcAddress(module, methodName);
			if (procAddress != IntPtr.Zero)
			{
				result = Marshal.GetDelegateForFunctionPointer(procAddress, type);
			}
			else
			{
				Logger.LogErrorFormat("could not get method : [{0}] module[{1}]", methodName, module.ToString("X"));
			}
		}
		else
		{
			Logger.LogErrorFormat("invalid module handle : [{0}]", module.ToString("X"));
		}
		return result;
	}
}
