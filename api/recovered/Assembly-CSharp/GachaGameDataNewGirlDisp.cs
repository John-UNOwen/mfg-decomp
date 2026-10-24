using System;
using MFG.GameData;
using UnityEngine;
using UnityEngine.UI;

public class GachaGameDataNewGirlDisp : MonoBehaviour
{
	[NonSerialized]
	public int seriesID;

	[NonSerialized]
	public GachaSeriesInfo gachaSeriesInfo;

	[SerializeField]
	private Button falseButton;

	private void Start()
	{
		falseButton.onClick.AddListener(delegate
		{
			ResetPickUpFirstDisp();
		});
	}

	private void Update()
	{
	}

	private void ResetPickUpFirstDisp()
	{
		gachaSeriesInfo.IsFirstDisplay = true;
	}
}
