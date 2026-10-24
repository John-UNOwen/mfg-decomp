using System;
using UnityEngine;

namespace KAMUNITY;

public class Logger
{
	private static long s_timestamp_old = 0L;

	private static string s_timestamp_buff = "";

	public static void Log(object message)
	{
		Log(message, null);
	}

	public static void Log(object message, UnityEngine.Object context)
	{
		if (KamunityInstance.isActive)
		{
			Avs.AvsLogMisc("UNITY", message.ToString());
		}
		Debug.Log(MakeMessage(message), context);
	}

	public static void LogFormat(string format, params object[] args)
	{
		Log(string.Format(format, args));
	}

	public static void LogFormat(UnityEngine.Object context, string format, params object[] args)
	{
		Log(string.Format(format, args), context);
	}

	public static void LogWarning(object message)
	{
		LogWarning(message, null);
	}

	public static void LogWarning(object message, UnityEngine.Object context)
	{
		if (KamunityInstance.isActive)
		{
			Avs.AvsLogWarning("UNITY", message.ToString());
		}
		Debug.LogWarning(MakeMessage(message), context);
	}

	public static void LogWarningFormat(string format, params object[] args)
	{
		LogWarning(string.Format(format, args));
	}

	public static void LogWarningFormat(UnityEngine.Object context, string format, params object[] args)
	{
		LogWarning(string.Format(format, args), context);
	}

	public static void LogError(object message)
	{
		LogError(message, null);
	}

	public static void LogError(object message, UnityEngine.Object context)
	{
		if (KamunityInstance.isActive)
		{
			Avs.AvsLogWarning("UNITY", message.ToString());
		}
		Debug.LogError(MakeMessage(message), context);
	}

	public static void LogErrorFormat(string format, params object[] args)
	{
		LogError(string.Format(format, args));
	}

	public static void LogErrorFormat(UnityEngine.Object context, string format, params object[] args)
	{
		LogError(string.Format(format, args), context);
	}

	public static void LogException(Exception e)
	{
		LogException(e, null);
	}

	public static void LogException(Exception e, UnityEngine.Object context)
	{
		if (KamunityInstance.isActive)
		{
			Avs.AvsLogWarning("UNITY", e.ToString());
		}
		Debug.LogException(e, context);
	}

	private static string GetTimestamp()
	{
		DateTime now = DateTime.Now;
		long num = now.Ticks / 10000000;
		if (s_timestamp_old != num)
		{
			s_timestamp_old = num;
			s_timestamp_buff = now.ToString("yyyy/MM/dd HH:mm:ss");
		}
		return s_timestamp_buff;
	}

	private static string MakeMessage(object message)
	{
		return $"[{GetTimestamp()}] {message}";
	}
}
