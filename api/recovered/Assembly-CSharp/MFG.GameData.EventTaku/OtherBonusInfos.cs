using System.Collections.Generic;
using System.Linq;

namespace MFG.GameData.EventTaku;

public class OtherBonusInfos
{
	public Dictionary<string, BonusInfo> _datas = new Dictionary<string, BonusInfo>();

	public int TotalBonusPoint => _datas.Sum((KeyValuePair<string, BonusInfo> x) => x.Value.TotalBonusPoint);

	public BonusInfo BonusInfo(string bonusTypeKey)
	{
		return _datas[bonusTypeKey];
	}

	public bool ContainsKey(string bonusTypeKey)
	{
		return _datas.ContainsKey(bonusTypeKey);
	}
}
