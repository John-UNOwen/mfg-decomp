using System;
using MFG.GameData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaGameDataDisp : MonoBehaviour
{
	[NonSerialized]
	public int seriesID;

	[NonSerialized]
	public GachaSeriesInfo gachaSeriesInfo;

	[SerializeField]
	private TextMeshProUGUI exchangePointText;

	[SerializeField]
	private Button up1Button;

	[SerializeField]
	private Button down1Button;

	[SerializeField]
	private Button up10Button;

	[SerializeField]
	private Button down10Button;

	[SerializeField]
	private Button up100Button;

	[SerializeField]
	private Button down100Button;

	private void Start()
	{
		up1Button.onClick.AddListener(delegate
		{
			ChangeExchangePoint(1);
		});
		down1Button.onClick.AddListener(delegate
		{
			ChangeExchangePoint(-1);
		});
		up10Button.onClick.AddListener(delegate
		{
			ChangeExchangePoint(10);
		});
		down10Button.onClick.AddListener(delegate
		{
			ChangeExchangePoint(-10);
		});
		up100Button.onClick.AddListener(delegate
		{
			ChangeExchangePoint(100);
		});
		down100Button.onClick.AddListener(delegate
		{
			ChangeExchangePoint(-100);
		});
		DispExchangePoint();
	}

	private void Update()
	{
	}

	private void ChangeExchangePoint(int change)
	{
		gachaSeriesInfo.ExchangePoint += change;
		if (gachaSeriesInfo.ExchangePoint < 0)
		{
			gachaSeriesInfo.ExchangePoint = 0;
		}
		DispExchangePoint();
	}

	private void DispExchangePoint()
	{
		exchangePointText.text = GameDataManager.GachaInfo.GachaSeriesInfosFromServer?.Find((GachaSeriesServerInfo x) => x.SeriesID == seriesID)?.Label ?? ("の交換点：" + (gachaSeriesInfo?.ExchangePoint ?? 0));
	}
}
