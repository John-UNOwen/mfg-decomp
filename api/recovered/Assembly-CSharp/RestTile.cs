using UnityEngine;

public class RestTile : MonoBehaviour
{
	public void Start()
	{
		GetComponent<Animator>().SetTrigger("Exit");
	}

	public void SetRestTileNum(int restTileNum)
	{
		Animator component = GetComponent<Animator>();
		switch (restTileNum)
		{
		case 20:
			component.ResetTrigger("Exit");
			component.SetTrigger("Limit1");
			break;
		case 10:
			component.ResetTrigger("Exit");
			component.SetTrigger("Limit2");
			break;
		}
	}
}
