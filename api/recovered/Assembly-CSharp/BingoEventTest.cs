using UnityEngine;

public class BingoEventTest : MonoBehaviour
{
	[SerializeField]
	private GameObject bingoEventDispPrefab;

	[SerializeField]
	private Canvas canvas;

	private void Start()
	{
		Object.Instantiate(bingoEventDispPrefab, base.transform);
	}

	private void Update()
	{
	}
}
