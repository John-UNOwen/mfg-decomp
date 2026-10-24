using UnityEngine;

namespace KAMUNITY;

public static class Kamunity
{
	public static void Startup()
	{
		GameObject gameObject = KamunityInstance.Startup();
		gameObject.AddComponent<XrpcService>();
		gameObject.AddComponent<HttpDownloadService>();
		if (KamunityInstance.isEa3Boot)
		{
			XrpcModuleCardmng.AddModule();
		}
	}
}
