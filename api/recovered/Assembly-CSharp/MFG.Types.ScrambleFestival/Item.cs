using System.Collections.Generic;
using MFG.Types.Customize.Catchphrase;

namespace MFG.Types.ScrambleFestival;

public class Item
{
	public GameEventType FlagType;

	public GameEventType FinishFlagType;

	public FestivalType FesType;

	public bool IsOnlyJanpon;

	public string Theme;

	public Dictionary<TeamType, string> Teams;

	public Dictionary<TeamType, (ItemID Normal, ItemID Extra)> CatchPhrases;

	public int LotteryConsumeMedal;

	public (int Medal, int Spirit) SpiritLotteryConsume;

	public int SpiritExchangeMedal;

	public int ChoiceBonusCount;

	public ExtraPlayerItemMaster LotteryItem;

	public string HomeWindowPath => $"ScrambleFestival/{FesType}/Prefabs/HomeWindow.prefab";

	public string TeamMatchHomeWindowPath => $"ScrambleFestival/{FesType}/Prefabs/TeamMatchHomeWindow.prefab";

	public string LotteryHomeWindowPath => $"ScrambleFestival/{FesType}/Prefabs/LotteryHomeWindow.prefab";

	public string BtHomePath => $"ScrambleFestival/{FesType}/Textures/BtMode.png";

	public string BtTeamMatchPath => $"ScrambleFestival/{FesType}/Textures/BtTeamMatch.png";

	public string BtLotteryPath => $"ScrambleFestival/{FesType}/Textures/BtLottery.png";

	public string DispDate()
	{
		GameEventData gameEventData = GameDataManager.System.GameControlInfo.EventStatus.GetGameEventData(FlagType);
		if (gameEventData == null)
		{
			return "XXXX年YY月ZZ日（W）AA：BBまで";
		}
		return gameEventData.End.ToString("yyyy年MM月dd日（ddd）HH：mmまで");
	}
}
