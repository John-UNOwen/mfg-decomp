using System;
using MFG.Scenes.Home;
using MFG.Types.Mission;

namespace MFG.Types.EventTakuMaster;

public class Item
{
	public string EventName;

	public EventType EventType;

	public GameEventType CurrentFlagType;

	public bool[] EnableTakuType = new bool[3] { true, false, false };

	public GameEventType ConstancyFlagType;

	public GameEventType CompetitionFlagType;

	public Func<bool> IsDispConstancySchedule = () => false;

	public Type GameDataType;

	public string GameDataFieldName;

	public string HomeButtonIconSpritePath;

	public string HomeWindowPrefabPath;

	public HomeModePanelType HomeModePanelType;

	public string HomeModePanelPrefabPath;

	public HomeModePanelType HomeModePanelTypeSanma = HomeModePanelType.Sanma;

	public string HomeModePanelPrefabPathSanma = "";

	public HomeModePanelType HomeModePanelTypeConstancy;

	public string HomeModePanelPrefabPathConstancy;

	public string ResultWindowPrefabPath;

	public string TopTitlePrefabPath;

	public string[][][] BgmLableName;

	public string BgmName;

	public string[][][] BgmNames;

	public string[][][] ExtraBgmLableName;

	public string ExtraBgmName = "";

	public string HomeNoticeText;

	public string EventItemName;

	public string EventItemDisplayName;

	public string EventItemUnit;

	public MissionID MainMissionID;

	public string EventItemIconPath => "CustomizeIcon/Icon" + EventItemName + ".png";

	public (string, string) BgmLabelName(Ba ba, Kyoku kyoku, TakuType takuType, bool isExtra = false)
	{
		if (!isExtra || ExtraBgmLableName == null)
		{
			return (BgmLableName[(takuType == TakuType.Sanma) ? 1u : 0u][(int)ba][(int)kyoku], (BgmNames != null) ? BgmNames[(takuType == TakuType.Sanma) ? 1u : 0u][(int)ba][(int)kyoku] : BgmName);
		}
		return (ExtraBgmLableName[(takuType == TakuType.Sanma) ? 1u : 0u][(int)ba][(int)kyoku], ExtraBgmName);
	}
}
