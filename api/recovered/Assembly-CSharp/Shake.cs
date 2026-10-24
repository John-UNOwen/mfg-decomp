using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Shake : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> m_objects2D = new List<GameObject>();

	[SerializeField]
	private List<GameObject> m_objects3D = new List<GameObject>();

	[SerializeField]
	private float duration = 2f;

	[SerializeField]
	private float strength = 0.1f;

	[SerializeField]
	private int vibrato = 30;

	[SerializeField]
	private float randomness = 90f;

	[SerializeField]
	private bool fadeOut = true;

	[SerializeField]
	private float startTime = 0.2f;

	private void Update()
	{
	}

	public void ShakeStart()
	{
		foreach (GameObject item in m_objects2D)
		{
			item.transform.DOShakePosition(duration, strength, vibrato, randomness, snapping: false, fadeOut).SetDelay(startTime);
		}
		foreach (GameObject item2 in m_objects3D)
		{
			item2.transform.DOShakePosition(duration, strength / 1000f, vibrato, randomness, snapping: false, fadeOut).SetDelay(startTime);
		}
	}
}
