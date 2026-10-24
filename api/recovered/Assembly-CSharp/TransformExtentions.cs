using UnityEngine;

public static class TransformExtentions
{
	public static string GetFullPathName(this Transform transform)
	{
		if (transform.parent == null)
		{
			return transform.name;
		}
		return transform.parent.GetFullPathName() + "/" + transform.name;
	}
}
