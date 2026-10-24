using System.Collections.Generic;
using MFG.Assets;
using MFG.MasterGameData;

namespace MFG.Types.Vote;

public class Item
{
	public GameEventType FlagType;

	public List<CharaType> Characters;

	public bool IsOnlyEnableChara;

	public bool IsHomeVote;

	public bool IsTaikyokuBonusEnable;

	public bool IsPointMaxEnable;

	public int VoteMaxToOneChara = -1;

	public bool IsCutinPlayInResult;

	public List<string> RewardItemList = new List<string>();

	public bool IsOneShotVote;

	public bool IsUnableIcon;

	public int[][] RankBonus;

	public int[] ProBonus;

	public ExtraPlayerItemMaster VoteItem;

	public bool IsCardConnectEnable;

	public string ConsumeWindowIconPathRoot;

	public string VoteCompleteWindowItemNamePathRoot;

	public string VoteCompleteWindowIconPathRoot;

	public string VoteWindowPath => $"VoteEvent/{FlagType}/Prefabs/VoteWindow.prefab";

	public string ResultWindowPath => $"VoteEvent/{FlagType}/Prefabs/ResultWindow.prefab";

	public string HomeWindowPath => $"VoteEvent/{FlagType}/Prefabs/HomeWindow.prefab";

	public string BtHomePath => $"VoteEvent/{FlagType}/Textures/BtMode.png";

	public string ConsumeWindowIconPath(CharaType chara)
	{
		if (ConsumeWindowIconPathRoot.Equals("RewardItemIcon"))
		{
			return ItemUtility.ItemIconGraphicPath(RewardItemList[Characters.IndexOf(chara)]);
		}
		return ConsumeWindowIconPathRoot.Replace("【chara】", chara.ToString()).Replace("【chara:D2】", ((int)(chara + 1)).ToString("D2"));
	}

	public string VoteCompleteWindowItemNamePath(CharaType chara)
	{
		return VoteCompleteWindowItemNamePathRoot.Replace("【chara】", chara.ToString()).Replace("【chara:D2】", ((int)(chara + 1)).ToString("D2"));
	}

	public string VoteCompleteWindowIconPath(CharaType chara)
	{
		return VoteCompleteWindowIconPathRoot.Replace("【chara】", chara.ToString()).Replace("【chara:D2】", ((int)(chara + 1)).ToString("D2"));
	}

	public int GetRankBonus(TakuType takuType, Rank rank)
	{
		int num = GameDataManager.PopularityVoteAdjustRankBonus(takuType, rank);
		if (num != -1)
		{
			return num;
		}
		return RankBonus[(int)takuType][(int)rank];
	}

	public int GetProBonus(bool isOverRank)
	{
		return ProBonus[isOverRank ? 1 : 0];
	}

	public int AvailablePointMax()
	{
		return 200;
	}

	public string DispDate()
	{
		GameEventData gameEventData = GameDataManager.System.GameControlInfo.EventStatus.GetGameEventData(FlagType);
		if (gameEventData == null)
		{
			return "XXXX年YY月ZZ日（W）AA：BBまで";
		}
		return gameEventData.End.ToString("yyyy年MM月dd日（ddd）HH：mmまで");
	}

	public string GetItemName(CharaType chara)
	{
		CharacterProperties characterProperties = MasterGameDataManager.Character.Properties(chara);
		if (FlagType == GameEventType.Xmas2024)
		{
			return characterProperties.Profile.Name + "のプレゼント";
		}
		return ItemUtility.ItemName(RewardItemList[Characters.IndexOf(chara)]);
	}
}
