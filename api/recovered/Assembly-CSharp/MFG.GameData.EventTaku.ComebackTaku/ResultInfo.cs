using System.Linq;
using MFG.Types;

namespace MFG.GameData.EventTaku.ComebackTaku;

public class ResultInfo : MFG.GameData.EventTaku.ResultInfo
{
	public void SetBonusCount(OtherBonusInfos otherBonusInfos, TakuType takuType)
	{
		bool flag = base.BlowAwayInfo.ChaInfos[Cha.Jicha].BlowawayState == BlowawayState.Blowaway;
		int num = base.BlowAwayInfo.ChaInfos.Values.Sum((ChaInfo x) => (x.BlowawayState == BlowawayState.Blowawayed) ? 1 : 0);
		base.OtherBonuses.BonusInfo(ComebackTakuEventGameData.BonusType.Tobi.ToString()).BonusCount = (flag ? num : 0) * ((takuType == TakuType.Sanma) ? 3 : 4);
	}
}
