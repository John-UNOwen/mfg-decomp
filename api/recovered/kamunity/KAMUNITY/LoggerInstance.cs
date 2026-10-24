using System;
using UnityEngine;

namespace KAMUNITY;

public class LoggerInstance : MonoBehaviour
{
	public static LogHandler logger { get; protected set; }

	public static ILogHandler defaultLogger { get; protected set; }

	public static LoggerInstance Replace(LogHandler loghandler)
	{
		GameObject obj = UnityEngine.Object.FindObjectOfType<KamunityInstance>().gameObject;
		return Replace(loghandler, obj);
	}

	public static LoggerInstance Replace(LogHandler loghandler, GameObject obj)
	{
		LoggerInstance loggerInstance = UnityEngine.Object.FindObjectOfType<LoggerInstance>();
		if (loggerInstance != null)
		{
			Logger.LogWarning("already replace logger");
			return loggerInstance;
		}
		loggerInstance = obj.AddComponent<LoggerInstance>();
		logger = loghandler;
		KamunityInstance.shutdownCallback = (Action)Delegate.Combine(KamunityInstance.shutdownCallback, new Action(loggerInstance.Shutdown));
		defaultLogger = Debug.unityLogger.logHandler;
		Debug.unityLogger.logHandler = loghandler;
		Logger.Log("LoggerInstance Logger attach");
		return loggerInstance;
	}

	public void OnDestroy()
	{
		KamunityInstance.shutdownCallback = (Action)Delegate.Remove(KamunityInstance.shutdownCallback, new Action(Shutdown));
	}

	private void Shutdown()
	{
		if (logger != null)
		{
			Debug.unityLogger.logHandler = defaultLogger;
			defaultLogger = null;
			logger.Dispose();
			Logger.Log("LoggerInstance Logger detach");
		}
	}
}
