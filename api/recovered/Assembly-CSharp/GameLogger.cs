using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class GameLogger
{
	public class LogLine
	{
		public LogType logType;

		public string section;

		public string message;

		public LogLine(LogType _logType, string _section, string _message)
		{
			logType = _logType;
			section = _section;
			message = _message;
		}
	}

	private static List<LogLine> s_LogBuff = new List<LogLine>();

	private static ulong s_LogCount = 0uL;

	private static uint s_MaxLogBuff = 100u;

	private static long s_timestamp_old = 0L;

	private static string s_timestamp_buff = "";

	[RuntimeInitializeOnLoadMethod]
	private static void OnRuntimeMethodLoad()
	{
	}

	private static void InsertLog(LogLine log)
	{
		lock (s_LogBuff)
		{
			s_LogBuff.Add(log);
			if (s_LogBuff.Count > s_MaxLogBuff)
			{
				s_LogBuff.RemoveAt(0);
			}
			s_LogCount++;
		}
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

	private static string MakeMessage(string format, params object[] args)
	{
		return MakeMessage(string.Format(format, args));
	}

	private static string MakeMessage(object message)
	{
		_ = Application.platform;
		_ = 11;
		return $"[{GetTimestamp()}] {message}";
	}

	[DebuggerHidden]
	[Conditional("DEBUG_GAME_LOGGER")]
	[Conditional("DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public static void Log(object message, UnityEngine.Object context = null)
	{
	}

	[DebuggerHidden]
	[Conditional("DEBUG_GAME_LOGGER")]
	[Conditional("DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public static void LogSection(string section, object message, UnityEngine.Object context = null)
	{
		string message2 = MakeMessage(message);
		InsertLog(new LogLine(LogType.Log, section, message2));
		UnityEngine.Debug.Log(message2, context);
	}

	[DebuggerHidden]
	[Conditional("DEBUG_GAME_LOGGER")]
	[Conditional("DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public static void LogFormat(string format, params object[] args)
	{
	}

	[DebuggerHidden]
	[Conditional("DEBUG_GAME_LOGGER")]
	[Conditional("DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public static void LogSectionFormat(string section, string format, params object[] args)
	{
	}

	[DebuggerHidden]
	[Conditional("DEBUG_GAME_LOGGER")]
	[Conditional("DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public static void LogWarning(object message, UnityEngine.Object context = null)
	{
	}

	[DebuggerHidden]
	[Conditional("DEBUG_GAME_LOGGER")]
	[Conditional("DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public static void LogWarningSection(string section, object message, UnityEngine.Object context = null)
	{
		string message2 = MakeMessage(message);
		InsertLog(new LogLine(LogType.Warning, section, message2));
		UnityEngine.Debug.LogWarning(message2, context);
	}

	[DebuggerHidden]
	[Conditional("DEBUG_GAME_LOGGER")]
	[Conditional("DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public static void LogWarningFormat(string format, params object[] args)
	{
	}

	[DebuggerHidden]
	[Conditional("DEBUG_GAME_LOGGER")]
	[Conditional("DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public static void LogWarningSectionFormat(string section, string format, params object[] args)
	{
	}

	[DebuggerHidden]
	[Conditional("DEBUG_GAME_LOGGER")]
	[Conditional("DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public static void LogError(object message, UnityEngine.Object context = null)
	{
	}

	[DebuggerHidden]
	[Conditional("DEBUG_GAME_LOGGER")]
	[Conditional("DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public static void LogErrorSection(string section, object message, UnityEngine.Object context = null)
	{
		string message2 = MakeMessage(message);
		InsertLog(new LogLine(LogType.Error, section, message2));
		UnityEngine.Debug.LogError(message2, context);
	}

	[DebuggerHidden]
	[Conditional("DEBUG_GAME_LOGGER")]
	[Conditional("DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public static void LogErrorFormat(string format, params object[] args)
	{
	}

	[DebuggerHidden]
	[Conditional("DEBUG_GAME_LOGGER")]
	[Conditional("DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public static void LogException(Exception e, UnityEngine.Object context = null)
	{
	}

	[DebuggerHidden]
	[Conditional("DEBUG_GAME_LOGGER")]
	[Conditional("DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public static void LogExceptionSection(string section, Exception e, UnityEngine.Object context = null)
	{
		InsertLog(new LogLine(LogType.Exception, section, e.ToString()));
		UnityEngine.Debug.LogException(e, context);
	}

	[DebuggerHidden]
	[Conditional("DEBUG_GAME_LOGGER")]
	[Conditional("DEBUG")]
	[Conditional("UNITY_EDITOR")]
	public static void SetLogBuffMax(uint logmax)
	{
		lock (s_LogBuff)
		{
			s_MaxLogBuff = logmax;
			while (s_LogBuff.Count > s_MaxLogBuff)
			{
				s_LogBuff.RemoveAt(0);
			}
		}
	}

	[DebuggerHidden]
	public static ulong GetLogCounter()
	{
		lock (s_LogBuff)
		{
			return s_LogCount;
		}
	}

	[DebuggerHidden]
	public static List<LogLine> GetLogBuffer()
	{
		lock (s_LogBuff)
		{
			List<LogLine> list = new List<LogLine>(s_LogBuff.Count);
			for (int i = 0; i < s_LogBuff.Count; i++)
			{
				list.Add(s_LogBuff[i]);
			}
			return list;
		}
	}
}
