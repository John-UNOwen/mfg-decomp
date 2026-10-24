using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MFG.GameRequest;
using MFG.Types;
using MFG.Types.ShopCompetition;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class ShopCompetitionGameData
{
	public List<ShopCompetitionInfo> ShopCompetitionInfos = new List<ShopCompetitionInfo>();

	[NonSerialized]
	public GameEventType CurrentGameEventType = GameEventType.ShopCompetition001;

	[NonSerialized]
	private List<ShopCompetitionInfo> _latestCompetitionInfos;

	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public const string ClientStateName = "shop_competition_game";

	public const string LatestResultClientStateName = "shop_competition_latest_result";

	public ShopCompetitionInfo GetInProgressCompetitionInfo()
	{
		return ShopCompetitionInfos.Find((ShopCompetitionInfo a) => a.IsEnable);
	}

	public ShopCompetitionInfo GetTrophyDisplayCompetitionInfo()
	{
		return ShopCompetitionInfos.Find((ShopCompetitionInfo a) => a.IsTrophyDisplay);
	}

	private ShopCompetitionInfo GetBeforeCompetitionInfo()
	{
		return GetInProgressCompetitionInfo();
	}

	public async UniTask<(ShopCompetitionInfo beforeCompetitionInfo, ShopCompetitionInfo afterCompetitionInfo)> GetLatestResult()
	{
		ShopCompetitionInfo beforeCompetitionInfo = GetBeforeCompetitionInfo();
		_latestCompetitionInfos = null;
		try
		{
			_latestCompetitionInfos = (await RequestApiBase<ClientStateRead, ClientStateRead.ResultData, ClientStateRead.Param>.Send(new ClientStateRead.Param
			{
				OneKindType = "shop_competition_latest_result"
			}, enableErrorScreenTransition: false)).LatestShopCompetition?.ShopCompetitionInfos;
		}
		catch
		{
			Debug.LogWarning("ClientStateRead: 大会スコア取得通信に失敗しました。");
		}
		ShopCompetitionInfo shopCompetitionInfo = _latestCompetitionInfos?.Find((ShopCompetitionInfo x) => x.CompetitionID == beforeCompetitionInfo.CompetitionID);
		if (shopCompetitionInfo == null)
		{
			Debug.LogWarning("大会スコアが見つかりませんでした。");
			(beforeCompetitionInfo, shopCompetitionInfo) = GenerateCompetitionInfoOnNetworkError();
		}
		Debug.Log("-- before scores--");
		beforeCompetitionInfo.Log_Scores();
		Debug.Log("-- after  scores--");
		shopCompetitionInfo.Log_Scores();
		return (beforeCompetitionInfo, shopCompetitionInfo);
	}

	public async UniTask Validate()
	{
	}

	public static ShopCompetitionInfo GenerateDebugCompetitionInfo(bool isAfter = false)
	{
		return new ShopCompetitionInfo
		{
			CompetitionID = CompetitionID.ShopCompetition001,
			Name = "最強店舗決定戦",
			ShopName = "KONAMIクリエイティブフロント東京ベイ店",
			CompetitionStatus = Status.InProgress,
			IsInTime = true,
			TaikyokuScore = (isAfter ? 438 : 0),
			PersonalScore = (isAfter ? 673 : 235),
			ShopScore = (isAfter ? 9120 : 8682),
			PersonalRank = (isAfter ? 8 : 10),
			ShopRank = (isAfter ? 345 : 347),
			TrophyCharaType = CharaType.Chara05
		};
	}

	private (ShopCompetitionInfo before, ShopCompetitionInfo after) GenerateCompetitionInfoOnNetworkError()
	{
		ShopCompetitionInfo shopCompetitionInfo = GetBeforeCompetitionInfo().Clone();
		ShopCompetitionInfo shopCompetitionInfo2 = GetBeforeCompetitionInfo().Clone();
		shopCompetitionInfo.Invalidate();
		shopCompetitionInfo2.Invalidate();
		return (before: shopCompetitionInfo, after: shopCompetitionInfo2);
	}

	public static async UniTask<ShopCompetitionGameData> LoadFromJson(string jsonString)
	{
		Debug.Log("ShopCompetitionGameData Load : " + jsonString);
		ShopCompetitionGameData gamedata = JsonUtility.FromJson<ShopCompetitionGameData>(jsonString);
		if (gamedata == null)
		{
			gamedata = new ShopCompetitionGameData();
		}
		await gamedata.Validate();
		return gamedata;
	}
}
