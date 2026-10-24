using Dx11HighLevelScopeControllerPlugin;
using UnityEngine;

public class PluginDriver : MonoBehaviour
{
	public Camera gCamera;

	private PluginAccess mPluginAccess;

	private void Start()
	{
		mPluginAccess = PluginSingleton.Instance.gPluginAccess;
		mPluginAccess.Initiate(gCamera, gCamera.pixelWidth, gCamera.scaledPixelHeight);
	}

	private void OnPostRender()
	{
		mPluginAccess.VideoCaptureAndEncode();
	}

	private void OnApplicationQuit()
	{
		mPluginAccess.Terminate(gCamera);
	}
}
