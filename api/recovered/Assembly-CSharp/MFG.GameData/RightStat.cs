using System;

namespace MFG.GameData;

[Serializable]
public class RightStat
{
	public int PurchaseId;

	public string ObjectID;

	public bool Available;

	public string ExpireAt;

	public bool IsAvailable(bool isRealTime = false)
	{
		if (!isRealTime || string.IsNullOrEmpty(ExpireAt))
		{
			return Available;
		}
		return DateTimeOffsetManager.GetJSTFromString(ExpireAt) > DateTimeOffsetManager.GetJSTTime();
	}
}
