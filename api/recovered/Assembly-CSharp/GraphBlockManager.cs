using System.Collections.Generic;
using MFG.Types;
using UnityEngine;
using UnityEngine.UI;

public class GraphBlockManager : MonoBehaviour
{
	[SerializeField]
	private Image _point1;

	[SerializeField]
	private GameObject _point2;

	[SerializeField]
	private List<GameObject> _lines;

	public void SetGraph(Sprite spr, Rank nextRank, Rank rank)
	{
		if (spr != null)
		{
			_point1.sprite = spr;
		}
		if (nextRank != Rank.RankMax)
		{
			_lines[nextRank - rank + 3].SetActive(value: true);
		}
	}

	public void SetGraphNull()
	{
		SetGraph(null, Rank.RankMax, Rank.RankMax);
	}
}
