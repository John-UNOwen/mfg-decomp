using Dx11HighLevelScopeControllerPlugin;
using UnityEngine;
using UnityEngine.Rendering;

public class SampleDriver : MonoBehaviour
{
	public Camera gCamera;

	private PluginAccess mPluginAccess;

	public static bool useCapture = true;

	private bool _initialized;

	public void Initialize(int width, int height)
	{
		if (!_initialized)
		{
			base.gameObject.GetComponent<SampleDriver>().enabled = true;
			mPluginAccess = PluginSingleton.Instance.gPluginAccess;
			Debug.Log("StreamingDriver: Initialize");
			mPluginAccess.Initiate(gCamera, width, height);
			RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
			_initialized = true;
		}
	}

	private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
	{
		OnPostRender();
	}

	private void OnPostRender()
	{
		mPluginAccess.VideoCaptureAndEncode();
	}

	private void OnDestroy()
	{
		RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
		if (mPluginAccess != null)
		{
			mPluginAccess.Terminate(gCamera);
		}
	}
}
