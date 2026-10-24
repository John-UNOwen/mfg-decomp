using UnityEngine;

public class DojoUnlockTest : MonoBehaviour
{
	[SerializeField]
	private GameObject dojoUnlockDispPrefab;

	[SerializeField]
	private Canvas canvas;

	private void Start()
	{
		Object.Instantiate(dojoUnlockDispPrefab, canvas.transform);
	}

	private void Update()
	{
	}
}
