using UnityEngine;

public static class GameObjectExtensions
{
	public static void SetLayerRecursive(this GameObject gameObject, string layerName)
	{
		int layerID = LayerMask.NameToLayer(layerName);
		gameObject.SetLayerRecursive(layerID);
	}

	public static void SetLayerRecursive(this GameObject gameObject, int layerID)
	{
		gameObject.layer = layerID;
		foreach (Transform item in gameObject.transform)
		{
			item.gameObject.SetLayerRecursive(layerID);
		}
	}
}
