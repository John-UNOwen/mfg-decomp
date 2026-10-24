using System.Collections.Generic;
using MFG.GameData;
using UnityEngine;

public class GachaGameDataTest : MonoBehaviour
{
	[SerializeField]
	private GameObject gachaGameDataDispPrefab;

	[SerializeField]
	private GameObject gachaGameDataNewGirlDispPrefab;

	[SerializeField]
	private ScrollView scrollView;

	private void Start()
	{
		new List<GachaGameDataDisp>(new List<GameObject>(GameDataManager.Gacha.GachaSeriesInfos.Count).Count);
		for (int i = 0; i < GameDataManager.GachaInfo.GachaSeriesInfosFromServer.Count; i++)
		{
			GameObject obj = scrollView.Add(gachaGameDataDispPrefab);
			int id = GameDataManager.GachaInfo.GachaSeriesInfosFromServer[i].SeriesID;
			obj.GetComponent<GachaGameDataDisp>().seriesID = id;
			obj.GetComponent<GachaGameDataDisp>().gachaSeriesInfo = GameDataManager.Gacha.GachaSeriesInfos.Find((GachaSeriesInfo x) => x.SeriesID == id);
			if (GameDataManager.GachaInfo.GachaSeriesInfosFromServer[i].PickUpCharas.Count > 0)
			{
				GameObject obj2 = scrollView.Add(gachaGameDataNewGirlDispPrefab);
				obj2.GetComponent<GachaGameDataNewGirlDisp>().seriesID = id;
				obj2.GetComponent<GachaGameDataNewGirlDisp>().gachaSeriesInfo = GameDataManager.Gacha.GachaSeriesInfos.Find((GachaSeriesInfo x) => x.SeriesID == id);
			}
		}
	}

	private void Update()
	{
	}
}
