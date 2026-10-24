using System;
using System.Collections.Generic;
using MFG.Types;
using MFG.Types.ScrambleFestival;
using UnityEngine;

[Serializable]
public class ReceiveOnlyGameData
{
	public const string ClientStateName = "receive_only";

	public List<KacEntryInfo> KacEntryInfos = new List<KacEntryInfo>();

	public PlayerLoginInfo PlayerInfo = new PlayerLoginInfo();

	public List<ImportantNoticeInfo> ImportantNotices = new List<ImportantNoticeInfo>();

	public List<ScrambleFestivalInfo> ScrambleFestivalScoreInfos = new List<ScrambleFestivalInfo>();

	private static List<ScrambleFestivalInfo> DebugScoreInfos = new List<ScrambleFestivalInfo>
	{
		new ScrambleFestivalInfo
		{
			EventType = GameEventType.ScrambleFestival,
			WinnerTeam = new List<TeamType>(),
			ScoreInfos = new List<ScrambleFestivalScore>
			{
				new ScrambleFestivalScore
				{
					Team = TeamType.Team01,
					Score = 12345,
					PrevScore = 999,
					ChoiceRate = 334,
					PrevChoiceRate = 334,
					MaList = new List<MahjongActorID> { MahjongActorID.Ma001 },
					PrevMaList = new List<MahjongActorID>()
				},
				new ScrambleFestivalScore
				{
					Team = TeamType.Team02,
					Score = 23456,
					PrevScore = 12345,
					ChoiceRate = 999,
					PrevChoiceRate = 999,
					MaList = new List<MahjongActorID> { MahjongActorID.Ma002 },
					PrevMaList = new List<MahjongActorID> { MahjongActorID.Ma002 }
				},
				new ScrambleFestivalScore
				{
					Team = TeamType.Team03,
					Score = 98424,
					PrevScore = 0,
					ChoiceRate = 566,
					PrevChoiceRate = 566,
					MaList = new List<MahjongActorID>
					{
						MahjongActorID.Ma003,
						MahjongActorID.Ma004,
						MahjongActorID.Ma005,
						MahjongActorID.Ma006,
						MahjongActorID.Ma007,
						MahjongActorID.Ma008
					},
					PrevMaList = new List<MahjongActorID> { MahjongActorID.Ma005 }
				}
			},
			RefleshDate = ""
		}
	};

	public bool IsPenaltyPlayer;

	public static ReceiveOnlyGameData LoadFromJson(string jsonString)
	{
		Debug.Log("ReceiveOnlyGameData Load : " + jsonString);
		ReceiveOnlyGameData receiveOnlyGameData = JsonUtility.FromJson<ReceiveOnlyGameData>(jsonString);
		if (receiveOnlyGameData == null)
		{
			receiveOnlyGameData = new ReceiveOnlyGameData();
		}
		return receiveOnlyGameData;
	}
}
