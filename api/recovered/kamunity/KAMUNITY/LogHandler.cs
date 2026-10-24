using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace KAMUNITY;

public class LogHandler : ILogHandler, IDisposable
{
	private ILogHandler m_DefaultLogger;

	private StreamWriter m_File;

	private StringBuilder m_Builder;

	private bool m_Boot;

	private object mutex = new object();

	public bool outputDefaultLogger { get; set; } = true;


	public bool enableAvsLog { get; private set; }

	public bool enableStackTrace { get; set; } = true;


	public static string Eol { get; set; } = "\n";


	public event Action<LogType, string, string, UnityEngine.Object> appLogCallback;

	public LogHandler(bool enableAppLog = true, int builderSize = 1024, ILogHandler defaultLogger = null)
	{
		m_Builder = new StringBuilder(builderSize);
		if (enableAppLog)
		{
			DateTime now = DateTime.Now;
			m_File = new StreamWriter(Path.Combine(Utility.GetRawPath(), "log/applog_" + now.ToString("yyyyMMddHHmmss") + ".txt"));
			m_File.AutoFlush = true;
		}
		m_DefaultLogger = defaultLogger;
		m_Boot = true;
	}

	~LogHandler()
	{
		Shutdown();
	}

	private void Shutdown()
	{
		if (m_Boot)
		{
			this.appLogCallback = null;
			if (m_DefaultLogger != null)
			{
				m_DefaultLogger = null;
			}
			if (m_File != null)
			{
				m_File.Close();
				m_File = null;
			}
			m_Boot = false;
		}
	}

	public void LogException(Exception exception, UnityEngine.Object context)
	{
		string text = "";
		lock (mutex)
		{
			m_Builder.Clear();
			m_Builder.Append("Exception:" + exception.ToString() + Eol);
			string text2 = "";
			if (enableStackTrace)
			{
				text2 = StackTrace(exception);
				m_Builder.Append(text2 + Eol);
			}
			text = m_Builder.ToString();
			if (m_File != null)
			{
				m_File.Write(text);
			}
			this.appLogCallback?.Invoke(LogType.Exception, exception.ToString(), text2, context);
		}
		if (enableAvsLog)
		{
			Avs.AvsLogWarning("UNITY", text);
		}
		if (outputDefaultLogger && m_DefaultLogger != null)
		{
			m_DefaultLogger.LogException(exception, context);
		}
	}

	public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
	{
		string text = string.Format(format, args);
		lock (mutex)
		{
			m_Builder.Clear();
			m_Builder.Append(logType.ToString() + " : " + text + Eol);
			string text2 = "";
			if (enableStackTrace)
			{
				text2 = StackTrace();
				m_Builder.Append(text2 + Eol);
			}
			if (m_File != null)
			{
				m_File.Write(m_Builder.ToString());
			}
			this.appLogCallback?.Invoke(logType, text, text2, context);
		}
		if (enableAvsLog)
		{
			switch (logType)
			{
			case LogType.Log:
				Avs.AvsLogMisc("UNITY", text);
				break;
			case LogType.Error:
			case LogType.Assert:
			case LogType.Warning:
			case LogType.Exception:
				Avs.AvsLogWarning("UNITY", text);
				break;
			}
		}
		if (outputDefaultLogger && m_DefaultLogger != null)
		{
			m_DefaultLogger.LogFormat(logType, context, format, args);
		}
	}

	private string StackTrace(Exception e)
	{
		return StackTrace(StackTraceUtility.ExtractStringFromException(e));
	}

	private string StackTrace()
	{
		return StackTrace(StackTraceUtility.ExtractStackTrace());
	}

	private string StackTrace(string st)
	{
		StringBuilder stringBuilder = new StringBuilder(512);
		string[] array = st.Split('\n');
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].Trim().Replace("\r", "");
			if (text.Length != 0)
			{
				stringBuilder.Append(text + Eol);
			}
		}
		stringBuilder.Append(Eol);
		return stringBuilder.ToString();
	}

	public void Dispose()
	{
		Shutdown();
	}
}
