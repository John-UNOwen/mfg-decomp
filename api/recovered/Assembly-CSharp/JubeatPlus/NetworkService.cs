using Konami.System.Net;
using UnityEngine;

namespace JubeatPlus;

public static class NetworkService
{
	public static bool IsInitialized { get; private set; } = false;


	public static bool IsOnline { get; private set; } = false;


	public static string NotificationToken { get; private set; } = null;


	public static ServiceStatus Service { get; private set; } = ServiceStatus.Default;


	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void InitializeNetworkService()
	{
		InitMobile();
	}

	private static void InitMobile()
	{
		GameObject gameObject = new GameObject("Network");
		if ((bool)gameObject)
		{
			gameObject.AddComponent<JubeatPlusRequest>();
			Object.DontDestroyOnLoad(gameObject);
			UserImplementsClass.Instance = new NetworkHandling();
		}
		IsInitialized = true;
	}
}
