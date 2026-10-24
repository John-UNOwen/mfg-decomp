using System;
using System.Collections.Generic;
using UnityEngine;

public class ExCall : MonoBehaviour
{
	public enum Status
	{
		Tenpai,
		Noten,
		Nagashi,
		Kyusyu,
		Sukan,
		Sufu
	}

	[SerializeField]
	public List<GameObject> m_excalls = new List<GameObject>();

	public void Play(Status status, int index)
	{
		GameObject obj = m_excalls[index];
		obj.SetActive(value: true);
		obj.GetComponent<Animator>().SetTrigger("Start");
		foreach (Transform item in obj.transform)
		{
			if (item.name == Enum.GetName(typeof(Status), status))
			{
				item.gameObject.SetActive(value: true);
			}
			else
			{
				item.gameObject.SetActive(value: false);
			}
		}
	}

	public void Exit(int index)
	{
		GameObject gameObject = m_excalls[index];
		gameObject.GetComponent<Animator>().SetTrigger("End");
		foreach (Transform item in gameObject.transform)
		{
			item.gameObject.SetActive(value: false);
		}
		gameObject.SetActive(value: false);
	}
}
