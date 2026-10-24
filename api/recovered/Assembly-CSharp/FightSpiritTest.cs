using System;
using System.Collections.Generic;
using MFG.Types.PlayerItem;
using UnityEngine;

public class FightSpiritTest : MonoBehaviour
{
	[SerializeField]
	private GameObject fightSpiritDispPrefab;

	[SerializeField]
	private ScrollView scrollView;

	private void Start()
	{
		new List<GameObject>(Enum.GetNames(typeof(ItemID)).Length);
		new List<FightSpiritDisp>(Enum.GetNames(typeof(ItemID)).Length);
		for (int i = 0; i < Enum.GetNames(typeof(ItemID)).Length; i++)
		{
			if (i != 0)
			{
				scrollView.Add(fightSpiritDispPrefab).GetComponent<FightSpiritDisp>().SetItemID((ItemID)i);
			}
		}
	}

	private void Update()
	{
	}
}
