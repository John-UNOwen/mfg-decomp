using UnityEngine;

namespace MFG.Sytem.Utilities;

public class CameraContext
{
	public Vector3 LocalPosition;

	public Vector3 LocalEulerAngle;

	public Vector3 LocalScale = Vector3.one;

	public float FieldOfView = 60f;

	public void Reserve(Camera camera)
	{
		if (!(camera == null))
		{
			Transform transform = camera.transform;
			LocalPosition = transform.localPosition;
			LocalEulerAngle = transform.localEulerAngles;
			LocalScale = transform.localScale;
			FieldOfView = camera.fieldOfView;
		}
	}

	public void Restore(Camera camera)
	{
		if (!(camera == null))
		{
			Transform transform = camera.transform;
			transform.localPosition = LocalPosition;
			transform.localEulerAngles = LocalEulerAngle;
			transform.localScale = LocalScale;
			camera.fieldOfView = FieldOfView;
		}
	}
}
