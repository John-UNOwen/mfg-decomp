using System.Collections.Generic;
using MFG.Types;
using UnityEngine;

public class Wall : MonoBehaviour
{
	private TileManager tileManager;

	private List<Tile> Tiles = new List<Tile>();

	private void Start()
	{
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

	public void SetOpen(bool open)
	{
		foreach (Tile tile in Tiles)
		{
			tile.transform.rotation = Quaternion.Euler(180 * ((!open) ? 1 : 0), 0f, 0f);
		}
	}
}
