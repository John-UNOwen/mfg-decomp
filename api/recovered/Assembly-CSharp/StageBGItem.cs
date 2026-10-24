using System.Collections.Generic;
using UnityEngine;

public class StageBGItem : MonoBehaviour
{
	[SerializeField]
	[Header("アクティブ制御するオブジェクトを入れる。")]
	private List<GameObject> objects;

	[SerializeField]
	private Animator audience;

	[SerializeField]
	private GameObject backScreen;

	public void SetActive(bool isActive)
	{
		foreach (GameObject @object in objects)
		{
			@object.SetActive(isActive);
		}
	}

	public void AudienceSetTrigger(StageState state)
	{
		switch (state)
		{
		case StageState.None:
		case StageState.Start:
			audience.ResetTrigger("Lv1");
			audience.SetTrigger("Lv0");
			break;
		case StageState.Reach:
			audience.ResetTrigger("Lv0");
			audience.SetTrigger("Lv1");
			break;
		case StageState.Excite:
			audience.SetTrigger("Excit");
			break;
		case StageState.FinalResult:
			audience.SetTrigger("Lv0");
			break;
		}
	}

	public GameObject GetBackScreen()
	{
		return backScreen;
	}
}
