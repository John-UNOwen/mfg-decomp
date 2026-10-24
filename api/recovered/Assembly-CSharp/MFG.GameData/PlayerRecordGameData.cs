using System.Collections.Generic;
using System.Xml.Linq;
using MFG.GameRequest;
using MFG.Types;
using UnityEngine;

namespace MFG.GameData;

public class PlayerRecordGameData : GameDataBase
{
	public List<PlayerRecordInfo> PlayerRecordInfos = new List<PlayerRecordInfo>();

	public PlayerTotalRecordInfo PlayerTotalRecordInfo = new PlayerTotalRecordInfo();

	public void Parse(XElement elem)
	{
		Debug.Log(elem.ToString());
		XElement xElement = elem.Element("total");
		if (xElement != null)
		{
			PlayerTotalRecordInfo playerTotalRecordInfo = new PlayerTotalRecordInfo();
			playerTotalRecordInfo.Parse(xElement);
			PlayerTotalRecordInfo = playerTotalRecordInfo;
		}
		foreach (XElement item in elem.Elements("info"))
		{
			PlayerRecordInfo playerRecordInfo = new PlayerRecordInfo();
			playerRecordInfo.Parse(item);
			PlayerRecordInfos.Add(playerRecordInfo);
		}
	}

	public void Apply(GetRecord.ResultData resultData)
	{
		PlayerTotalRecordInfo = resultData.ResultPlayerTotalRecordInfo;
		PlayerRecordInfos = resultData.ResultPlayerRecordInfo;
	}

	public Dictionary<SchoolID, int> GetScrambleMatchTotalWinCount()
	{
		return PlayerTotalRecordInfo.ScrambleMatchTotalWinCount;
	}

	public ScrambleTrophyRank GetTrophyRank(SchoolID id)
	{
		Dictionary<SchoolID, int> scrambleMatchTotalWinCount = GetScrambleMatchTotalWinCount();
		if (scrambleMatchTotalWinCount.ContainsKey(id))
		{
			if (scrambleMatchTotalWinCount[id] >= 30)
			{
				return ScrambleTrophyRank.Gold;
			}
			if (scrambleMatchTotalWinCount[id] >= 10)
			{
				return ScrambleTrophyRank.Silver;
			}
			if (scrambleMatchTotalWinCount[id] >= 1)
			{
				return ScrambleTrophyRank.Bronze;
			}
		}
		return ScrambleTrophyRank.None;
	}
}
