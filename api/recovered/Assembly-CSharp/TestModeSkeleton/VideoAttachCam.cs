using KAMUNITY;
using UnityEngine;
using UnityEngine.Video;

namespace TestModeSkeleton;

[ExecuteInEditMode]
public class VideoAttachCam : MonoBehaviour
{
	public enum CameraKind
	{
		UI
	}

	public CameraKind cameraKind;

	private void OnEnable()
	{
		Attatch(UIAttachCam.FindCamera((UIAttachCam.CameraKind)cameraKind));
	}

	public void Attatch(Camera camera)
	{
		VideoPlayer component = base.gameObject.GetComponent<VideoPlayer>();
		if (component == null)
		{
			KAMUNITY.Logger.LogError("VideoPlayer not found [" + base.gameObject.name + "]");
		}
		else if (camera == null)
		{
			KAMUNITY.Logger.LogWarning($"VideoAttachCam: camera not found. CameraKind:{cameraKind}");
		}
		else
		{
			component.targetCamera = camera;
		}
	}

	private void OnDisable()
	{
		VideoPlayer component = base.gameObject.GetComponent<VideoPlayer>();
		if (component != null)
		{
			component.targetCamera = null;
		}
	}
}
