using System;
using System.Collections.Generic;
using MFG.Types;
using UnityEngine;

public class CharaFanTest : MonoBehaviour
{
	[SerializeField]
	private GameObject fansDispPrefab;

	[SerializeField]
	private ScrollView scrollView;

	private void Start()
	{
		new List<GameObject>(Enum.GetNames(typeof(CharaType)).Length * Enum.GetNames(typeof(FanType)).Length);
		new List<CharaFansDisp>(Enum.GetNames(typeof(CharaType)).Length * Enum.GetNames(typeof(FanType)).Length);
		for (int i = 0; i < Enum.GetNames(typeof(CharaType)).Length; i++)
		{
			for (int j = 0; j < Enum.GetNames(typeof(FanType)).Length; j++)
			{
				GameObject obj = scrollView.Add(fansDispPrefab);
				obj.GetComponent<CharaFansDisp>().SetCharaType((CharaType)i);
				obj.GetComponent<CharaFansDisp>().SetFanType((FanType)j);
			}
		}
	}

	private void Update()
	{
	}
}
