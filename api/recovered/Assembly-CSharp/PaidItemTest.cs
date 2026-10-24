using System;
using System.Collections.Generic;
using MFG.Types.PlayerPaidItem;
using UnityEngine;

public class PaidItemTest : MonoBehaviour
{
	[SerializeField]
	private GameObject paidItemDispPrefab;

	[SerializeField]
	private ScrollView scrollView;

	private void Start()
	{
		new List<GameObject>(Enum.GetNames(typeof(ItemID)).Length);
		new List<PaidItemDisp>(Enum.GetNames(typeof(ItemID)).Length);
		for (int i = 0; i < Enum.GetNames(typeof(ItemID)).Length; i++)
		{
			if (i != 0)
			{
				scrollView.Add(paidItemDispPrefab).GetComponent<PaidItemDisp>().SetItemID((ItemID)i);
			}
		}
	}

	private void Update()
	{
	}
}
