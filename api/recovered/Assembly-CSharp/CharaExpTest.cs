using System;
using System.Collections.Generic;
using MFG.Types;
using UnityEngine;

public class CharaExpTest : MonoBehaviour
{
	[SerializeField]
	private GameObject expDispPrefab;

	[SerializeField]
	private ScrollView scrollView;

	private async void Start()
	{
		new List<GameObject>(Enum.GetNames(typeof(CharaType)).Length);
		new List<CharaExpDisp>(Enum.GetNames(typeof(CharaType)).Length);
		for (int i = 0; i < Enum.GetNames(typeof(CharaType)).Length; i++)
		{
			scrollView.Add(expDispPrefab).GetComponent<CharaExpDisp>().SetCharaType((CharaType)i);
		}
	}
}
