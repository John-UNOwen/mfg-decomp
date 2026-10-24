using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using KAMUNITY;
using UnityEngine;

namespace TestModeSkeleton;

[ExecuteInEditMode]
public class UIAttachCam : MonoBehaviour
{
	public enum CameraKind
	{
		UI
	}

	public CameraKind cameraKind;

	private void OnEnable()
	{
		AttatchWithWaitResidentInit(this.GetCancellationTokenOnDestroy()).Forget();
	}

	public void Attatch()
	{
		Attatch(FindCamera(cameraKind));
	}

	public async UniTask AttatchWithWaitResidentInit(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!ResidentFlow.IsAwaked)
		{
			await UniTask.WaitUntil(() => ResidentFlow.IsAwaked, PlayerLoopTiming.Update, cancellationToken);
		}
		Attatch(FindCamera(cameraKind));
	}

	public void Attatch(Camera camera)
	{
		Canvas component = base.gameObject.GetComponent<Canvas>();
		if (component == null)
		{
			KAMUNITY.Logger.LogError("Canvas not found [" + base.gameObject.name + "]");
			return;
		}
		if (camera == null)
		{
			KAMUNITY.Logger.LogWarning($"UIAttachCam: camera not found. CameraKind:{cameraKind}");
			return;
		}
		component.renderMode = RenderMode.ScreenSpaceCamera;
		component.worldCamera = camera;
	}

	private void OnDisable()
	{
		Canvas component = base.gameObject.GetComponent<Canvas>();
		if (component != null)
		{
			component.worldCamera = null;
		}
	}

	public static string GetCameraName(CameraKind cameraKind)
	{
		if (cameraKind == CameraKind.UI)
		{
			return GameUtility.IsCloudStreaming ? "StreamingCamera" : "LowerCamera";
		}
		throw new NotImplementedException();
	}

	public static Camera FindCamera(CameraKind cameraKind)
	{
		Camera result = null;
		string cameraName = GetCameraName(cameraKind);
		Camera[] array = UnityEngine.Object.FindObjectsOfType<Camera>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].name.CompareTo(cameraName) == 0)
			{
				result = array[i];
				break;
			}
		}
		return result;
	}
}
