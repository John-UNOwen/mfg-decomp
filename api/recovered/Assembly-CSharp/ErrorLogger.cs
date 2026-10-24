using System.Collections;
using KAMUNITY;
using UnityEngine;

public class ErrorLogger : MonoBehaviour
{
	private int errorCount;

	private void Start()
	{
		errorCount = 0;
	}

	private void OnEnable()
	{
		Application.logMessageReceived += HandleLog;
	}

	private void OnDisable()
	{
		Application.logMessageReceived -= HandleLog;
	}

	private void HandleLog(string logString, string stackTrace, LogType type)
	{
		if ((type == LogType.Error || type == LogType.Exception) && errorCount < 30)
		{
			errorCount++;
			StartCoroutine(CoSend(logString, stackTrace, type.ToString()));
		}
	}

	private IEnumerator CoSend(string message, string stackTrace, string type)
	{
		if (KamunityInstance.isEa3Boot)
		{
			bool end = false;
			XrpcCabinet.RequestError(message, stackTrace, type, delegate
			{
				end = true;
			}, null);
			yield return new WaitWhile(() => !end);
		}
	}
}
