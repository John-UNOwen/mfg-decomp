using UnityEngine;

public class USndPlugin : MonoBehaviour
{
	public static bool isSetAudioFocus;

	private static USndPlugin_abstract plugin;

	public static void Init(string objName, string funcName)
	{
		_ = plugin;
	}

	public static void UpdateAndroidMusicStatus()
	{
	}

	private void OnDestroy()
	{
		if (plugin != null)
		{
			plugin.Delete();
			plugin = null;
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		if (focus && plugin != null)
		{
			plugin.SetAudioFocus();
		}
	}

	public static bool IsMusicPlaying()
	{
		if (plugin != null)
		{
			return plugin.IsMusicPlaying();
		}
		return false;
	}

	public static bool IsOtherAudioPlaying()
	{
		if (plugin != null)
		{
			return plugin.IsOtherAudioPlaying();
		}
		return false;
	}

	public static bool IsSpeaker()
	{
		if (plugin != null)
		{
			return plugin.IsSpeaker();
		}
		return false;
	}

	public static bool IsMannerMode()
	{
		if (plugin != null)
		{
			return plugin.IsMannerMode();
		}
		return false;
	}

	public static void SetAudioFocus()
	{
		if (plugin != null && isSetAudioFocus)
		{
			plugin.SetAudioFocus();
		}
	}
}
