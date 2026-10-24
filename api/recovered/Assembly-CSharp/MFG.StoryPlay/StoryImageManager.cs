using UnityEngine;

namespace MFG.StoryPlay;

public class StoryImageManager : MonoBehaviour
{
	public static Transform CreateImage()
	{
		return Object.Instantiate(StoryPlayFlow.GetInstance().imagePrefab, StoryPlayFlow.GetInstance().imageTop.transform).transform;
	}
}
