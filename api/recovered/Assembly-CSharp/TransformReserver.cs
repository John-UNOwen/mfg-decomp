using UnityEngine;

public class TransformReserver : MonoBehaviour
{
	public Vector3 LocalPosition;

	public Vector3 LocalEulerAngle;

	public Vector3 LocalScale;

	private void Start()
	{
		ReserveSelf();
	}

	public void Reserve(Transform transform)
	{
		LocalPosition = transform.localPosition;
		LocalEulerAngle = transform.localEulerAngles;
		LocalScale = transform.localScale;
	}

	public void ReserveSelf()
	{
		LocalPosition = base.transform.localPosition;
		LocalEulerAngle = base.transform.localEulerAngles;
		LocalScale = base.transform.localScale;
	}

	public void Restore(Transform transform)
	{
		transform.localPosition = LocalPosition;
		transform.localEulerAngles = LocalEulerAngle;
		transform.localScale = LocalScale;
	}

	public void RestoreSelf()
	{
		base.transform.localPosition = LocalPosition;
		base.transform.localEulerAngles = LocalEulerAngle;
		base.transform.localScale = LocalScale;
	}

	public void RestoreLocalEulerAngle()
	{
		base.transform.localEulerAngles = LocalEulerAngle;
	}
}
