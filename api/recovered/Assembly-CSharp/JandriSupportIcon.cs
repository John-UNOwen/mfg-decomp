using UnityEngine;

public class JandriSupportIcon : MonoBehaviour
{
	private void Start()
	{
		Reflesh();
	}

	public void Reflesh(bool isEffective = true)
	{
		base.gameObject.SetActive(GameDataManager.IsLoadJandriAppear() && isEffective);
	}
}
