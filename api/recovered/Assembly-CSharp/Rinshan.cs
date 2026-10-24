using System.Collections.Generic;
using MFG.Types;
using UnityEngine;

public class Rinshan : MonoBehaviour
{
	private TileManager tileManager;

	private List<Tile> Tiles = new List<Tile>();

	[SerializeField]
	private GameObject m_rinshanEffect;

	private void Start()
	{
		tileManager = GameObject.Find("TileManager").GetComponent<TileManager>();
		foreach (Transform item in base.transform)
		{
			if (item.gameObject.tag == "Debug")
			{
				Object.Destroy(item.gameObject);
			}
		}
	}

	private void Update()
	{
	}

	public void Add(Pai pai)
	{
	}

	public void Destruct()
	{
	}

	public void StartRinshanEffect()
	{
		m_rinshanEffect.SetActive(value: true);
		m_rinshanEffect.GetComponent<Animator>().SetTrigger("Next");
	}
}
