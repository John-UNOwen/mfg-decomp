using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameContents;
using MFG.GameRequest;
using MFG.Types;
using MFG.Types.FightGirlNews;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class GachaGameData
{
	private const int CurrentDataVersion = 2;

	public int DataVersion = 2;

	public const string ClientStateName = "gacha";

	public List<GachaSeriesInfo> GachaSeriesInfos = new List<GachaSeriesInfo>();

	[NonSerialized]
	public GachaPlayMode SelectedMode;

	[NonSerialized]
	public GachaSeriesServerInfo SelectedSeriesInfo;

	[NonSerialized]
	public CONSUME_MODE ConsumeMode;

	[NonSerialized]
	public GachaSeriesType EntryGachaSelectSceneMode = GachaSeriesType.Pickup;

	[NonSerialized]
	public GachaResultInfo GachaResult = new GachaResultInfo();

	public async UniTask Save(CancellationToken cancellationToken = default(CancellationToken))
	{
		string text = JsonUtility.ToJson(this);
		Debug.Log("GachaGameData Save : " + text);
		await RequestApiBase<ClientStateWrite, ClientStateWrite.ResultData, (string, string)>.Send(("gacha", text), enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
	}

	public static GachaGameData LoadFromJson(string jsonString)
	{
		Debug.Log("GachaGameData Load : " + jsonString);
		GachaGameData gachaGameData = JsonUtility.FromJson<GachaGameData>(jsonString);
		if (gachaGameData == null)
		{
			gachaGameData = new GachaGameData();
		}
		gachaGameData.Validate();
		Debug.Log(jsonString);
		return gachaGameData;
	}

	public void Validate()
	{
		List<GachaSeriesServerInfo> gachaSeriesInfosFromServer = GameDataManager.GachaInfo.GachaSeriesInfosFromServer;
		if (gachaSeriesInfosFromServer != null)
		{
			foreach (GachaSeriesServerInfo gacha in gachaSeriesInfosFromServer)
			{
				if (!GachaSeriesInfos.Exists((GachaSeriesInfo a) => a.SeriesID == gacha.SeriesID))
				{
					GachaSeriesInfos.Add(new GachaSeriesInfo
					{
						SeriesID = gacha.SeriesID,
						ExchangePoint = 0
					});
				}
			}
		}
		if (DataVersion != 1)
		{
			return;
		}
		foreach (GachaSeriesName seriesName in new List<GachaSeriesName>
		{
			GachaSeriesName.PickupShiori,
			GachaSeriesName.PickupChaos,
			GachaSeriesName.PickupClear,
			GachaSeriesName.PickupIyo,
			GachaSeriesName.PickupGrimAroe,
			GachaSeriesName.PickupCocoa,
			GachaSeriesName.PickupDia,
			GachaSeriesName.PickupDoubriel,
			GachaSeriesName.PickupIppatsu,
			GachaSeriesName.PickupShiroe,
			GachaSeriesName.PickupJun
		})
		{
			try
			{
				GachaSeriesInfos.Find((GachaSeriesInfo x) => x.SeriesID == (int)seriesName).ExchangePoint = 0;
			}
			catch
			{
			}
		}
		DataVersion = 2;
	}

	public static int GetRequiredGachaTicket(int gachaNum = 1)
	{
		return 10 * gachaNum;
	}

	public void PlayerConfigUpdate()
	{
		if ((!(GachaSeriesInfos?.Find((GachaSeriesInfo x) => x.SeriesID == 52)?.IsFirstDisplay)) ?? false)
		{
			GameDataManager.PlayerConfig.FightGirlNewsFinishIDs.Add(NewsID.PickupUniformSen2);
		}
	}
}
