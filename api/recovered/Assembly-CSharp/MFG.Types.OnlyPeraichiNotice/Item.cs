using System;

namespace MFG.Types.OnlyPeraichiNotice;

[Serializable]
public class Item
{
	public NoticeID ID;

	public GameEventType EventType;

	public string MethodName => $"{ID}Open";

	public bool IsEnable => GameDataManager.System.GameControlInfo.EventStatus.IsActive(EventType);

	public string AssetPath => $"OnlyPeraichiNotice/Prefabs/{ID}.prefab";

	public string BtHomePath => $"OnlyPeraichiNotice/Textures/{ID}/BtMode.png";

	public string DispDate()
	{
		GameEventData gameEventData = GameDataManager.System.GameControlInfo.EventStatus.GetGameEventData(EventType);
		if (gameEventData == null)
		{
			return "XXXX年YY月ZZ日（W）AA：BBまで";
		}
		return gameEventData.End.ToString("yyyy年MM月dd日（ddd）HH：mmまで");
	}
}
