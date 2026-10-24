using System;
using System.Collections.Generic;
using System.Linq;
using MFG.Taikyoku.Command;
using MFG.Types;
using UnityEngine;

[Serializable]
public class ProStats
{
	[Serializable]
	public class ProStayData
	{
		public MahjongActorID MahjongActorID;

		public GAME_MODE LatestGameMode;
	}

	public const string ApliInfoKindName = "pro_stats";

	public bool now_pro_stats;

	public List<ProStayData> ProStayDatas = new List<ProStayData>();

	public bool AnyEventTakuStaying()
	{
		return ProStayDatas.Any(delegate(ProStayData x)
		{
			var (takuType, eventType) = x.LatestGameMode.GetTakuTypeAndEventType();
			return takuType >= TakuType.Tonpu && eventType != MFG.Types.EventType.None;
		});
	}

	public bool AnyNormalTakuStaying()
	{
		return ProStayDatas.Any(delegate(ProStayData x)
		{
			var (takuType, eventType) = x.LatestGameMode.GetTakuTypeAndEventType();
			return takuType >= TakuType.Tonpu && eventType == MFG.Types.EventType.None;
		});
	}

	public bool AnyTakuStaying(TakuType argTakuType, MFG.Types.EventType argEventType)
	{
		return ProStayDatas.Any(delegate(ProStayData x)
		{
			var (takuType, eventType) = x.LatestGameMode.GetTakuTypeAndEventType();
			return takuType == argTakuType && eventType == argEventType;
		});
	}

	public static ProStats LoadFromJson(string jsonString)
	{
		Debug.Log("ProStats Load : " + jsonString);
		ProStats proStats = JsonUtility.FromJson<ProStats>(jsonString);
		if (proStats == null)
		{
			proStats = new ProStats();
		}
		return proStats;
	}

	private static List<ProStayData> GenerateDebugProStayData()
	{
		return new List<ProStayData>
		{
			new ProStayData
			{
				MahjongActorID = MahjongActorID.Ma001,
				LatestGameMode = GAME_MODE.GLOBAL_TONPU
			},
			new ProStayData
			{
				MahjongActorID = MahjongActorID.Ma002,
				LatestGameMode = GAME_MODE.GLOBAL_SANMA
			},
			new ProStayData
			{
				MahjongActorID = MahjongActorID.Ma003,
				LatestGameMode = GAME_MODE.GLOBAL_TONPU_TOITOI_EVENT
			},
			new ProStayData
			{
				MahjongActorID = MahjongActorID.Ma004,
				LatestGameMode = GAME_MODE.GLOBAL_TONPU_FIREREACH_EVENT
			},
			new ProStayData
			{
				MahjongActorID = MahjongActorID.Ma005,
				LatestGameMode = GAME_MODE.GLOBAL_SANMA_TOITOI_EVENT
			},
			new ProStayData
			{
				MahjongActorID = MahjongActorID.Ma006,
				LatestGameMode = GAME_MODE.GLOBAL_TONPU_ACCELDORA_EVENT
			},
			new ProStayData
			{
				MahjongActorID = MahjongActorID.Ma007,
				LatestGameMode = GAME_MODE.GLOBAL_TONPU
			},
			new ProStayData
			{
				MahjongActorID = MahjongActorID.Ma008,
				LatestGameMode = GAME_MODE.GLOBAL_SANMA
			},
			new ProStayData
			{
				MahjongActorID = MahjongActorID.Ma011,
				LatestGameMode = GAME_MODE.GLOBAL_SANMA
			},
			new ProStayData
			{
				MahjongActorID = MahjongActorID.Ma012,
				LatestGameMode = GAME_MODE.GLOBAL_SANMA
			}
		};
	}
}
