using MFG.MasterGameData;
using MFG.Types;

namespace MFG.GameData;

public class MissionMemorialPrintNoticeInfo
{
	public CardConnectMissionID CCID;

	public CharaType Chara;

	public string ItemName;

	public string ItemOID;

	public string GetAddressablePath
	{
		get
		{
			if (CCID != CardConnectMissionID.CutinItem)
			{
				if (Chara < CharaType.Chara01)
				{
					return $"CardConnect/{CCID}/{ItemOID}";
				}
				return string.Format("CardConnectNotice/Chara{0}/{1}.png", ((int)(Chara + 1)).ToString("D2"), CCID);
			}
			return CutinItemMaster.GetItem(CutinItemMaster.GetItemID(ItemOID)).MemorialPrintGraphicPath;
		}
	}
}
