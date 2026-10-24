using System;

namespace MFG.Types.OneShotNotice;

[Serializable]
public class Item
{
	public NoticeID ID;

	public GameEventType EventType;

	public string MethodName => $"{ID}Open";

	public bool IsEnable
	{
		get
		{
			if (!GameDataManager.PlayerConfig.OneShotNoticeFinishIDs.Contains(ID))
			{
				return GameDataManager.System.GameControlInfo.EventStatus.IsActive(EventType);
			}
			return false;
		}
	}

	public string AssetPath => $"OneShotNotice/Prefabs/{ID}.prefab";
}
