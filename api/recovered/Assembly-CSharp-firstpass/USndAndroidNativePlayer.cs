using UnityEngine;

public class USndAndroidNativePlayer
{
	private static USndAndroidNativePlayer player;

	private static AndroidJavaObject plugin;

	public static void Initialize(int maxNum)
	{
		if (player == null)
		{
			player = new USndAndroidNativePlayer();
			if (Application.platform == RuntimePlatform.Android && plugin == null)
			{
				plugin = new AndroidJavaObject("com.konami.usndplugin.USndSoundPoolPlayer");
				plugin.Call("Init", maxNum);
			}
		}
	}

	public static void Terminate()
	{
		if (player != null)
		{
			if (plugin != null)
			{
				plugin.Call("Release");
				plugin = null;
			}
			player = null;
		}
	}

	public static int LoadData(string saveName, string className, string funcName)
	{
		if (plugin == null)
		{
			return 0;
		}
		return plugin.Call<int>("LoadSound", new object[3] { className, funcName, saveName });
	}

	public static int Play(int soundId, float volume, float rate)
	{
		if (plugin == null)
		{
			return 0;
		}
		return plugin.Call<int>("Play", new object[3] { soundId, volume, rate });
	}

	public static void Stop(int streamId)
	{
		if (plugin != null)
		{
			plugin.Call("Stop", streamId);
		}
	}

	public static void Unload(int soundId)
	{
		if (plugin != null)
		{
			plugin.Call("Unload", soundId);
		}
	}
}
