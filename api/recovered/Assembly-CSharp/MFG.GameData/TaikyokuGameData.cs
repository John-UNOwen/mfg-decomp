using System;
using System.Collections.Generic;
using MFG.GameRequest;
using MFG.Taikyoku;

namespace MFG.GameData;

[Serializable]
public class TaikyokuGameData : GameDataBase
{
	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public EntryTakuInfo TakuInfo = new EntryTakuInfo();

	public List<PlayerInfo> PlayerInfos = new List<PlayerInfo>();

	public bool IsScrambleMatch;

	public bool IsMotherWars;

	public bool IsValidCompetition => PlayerInfos[TakuInfo.PlayerIndex].IsValidCompetition;

	public bool IsValidShopCompetition => PlayerInfos[TakuInfo.PlayerIndex].IsValidShopCompetition;

	public TaikyokuGameData()
	{
		PlayerInfos.Clear();
		PlayerInfos.Add(new PlayerInfo());
		PlayerInfos.Add(new PlayerInfo());
		PlayerInfos.Add(new PlayerInfo());
		PlayerInfos.Add(new PlayerInfo());
	}

	public void Apply(EntryTakuInfo takuInfo)
	{
		TakuInfo = takuInfo;
	}

	public void Apply(GetMenuData.ReconnectData recon)
	{
		TakuInfo.GameServiceUrl = recon.GameServiceUrl;
		TakuInfo.GameServiceID = int.Parse(recon.GameServiceUrl.Split('/')[3]);
	}

	public void Apply(HaifuData haifuData)
	{
		PlayerInfos = haifuData.PlayerInfos;
		TakuInfo.PlayerIndex = haifuData.PlayerIndex;
	}
}
