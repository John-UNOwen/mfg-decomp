using System;

namespace MFG.GameData;

[Serializable]
public class PaidItem
{
	public int ItemCount;

	public string ExpireDate;

	public DateTimeOffset Expire => DateTimeOffsetManager.GetJSTFromString(ExpireDate);

	public string FormatExpire
	{
		get
		{
			if (!(Expire == default(DateTimeOffset)))
			{
				return Expire.ToString("yyyy年MM月dd日 HH：mm");
			}
			return "";
		}
	}
}
