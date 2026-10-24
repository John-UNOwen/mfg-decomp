using MFG.GameData;
using MFG.MasterGameData;

namespace MFG.Types.Omikuji;

public class Item
{
	public OmikujiType OmikujiType;

	public GameEventType FlagType;

	public CardConnectMissionID CardConnectMissionID;

	public bool IsDuplicationEnable;

	public CharaType MaxCharaType;

	public string ItemName = "";

	public ExtraPlayerItemMaster LotteryItem;

	public string HomeWindowPath => $"Omikuji/{OmikujiType}/Prefabs/HomeWindow.prefab";

	public string BtHomePath => $"Omikuji/{OmikujiType}/Textures/BtMode.png";

	public string GetItemName(CharaType chara)
	{
		return ItemName + "「" + MasterGameDataManager.Character.Properties(chara).Profile.Name + "」";
	}

	public string ItemImagePath(CharaType chara)
	{
		return string.Format("CardConnectNotice/Chara{0}/{1}.png", ((int)(chara + 1)).ToString("D2"), CardConnectMissionID);
	}

	public string DispDate()
	{
		GameEventData gameEventData = GameDataManager.System.GameControlInfo.EventStatus.GetGameEventData(FlagType);
		if (gameEventData == null)
		{
			return "XXXX年YY月ZZ日（W）AA：BBまで";
		}
		return DateTimeOffsetManager.GetJSTFromString(gameEventData.ParamValue("end_date", "")).ToString("yyyy年MM月dd日（ddd）HH：mmまで");
	}
}
