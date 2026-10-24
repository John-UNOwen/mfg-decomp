using UnityEngine;

public class FightStarTest : MonoBehaviour
{
	[SerializeField]
	private GameObject fightStarDispPrefab;

	[SerializeField]
	private Canvas canvas;

	private void Start()
	{
		Object.Instantiate(fightStarDispPrefab, base.transform);
	}

	private void Update()
	{
	}
}
