using UnityEngine;

public class PlayerConfigTest : MonoBehaviour
{
	[SerializeField]
	private GameObject playerConfigDispPrefab;

	[SerializeField]
	private Canvas canvas;

	private void Start()
	{
		Object.Instantiate(playerConfigDispPrefab, base.transform);
	}

	private void Update()
	{
	}
}
