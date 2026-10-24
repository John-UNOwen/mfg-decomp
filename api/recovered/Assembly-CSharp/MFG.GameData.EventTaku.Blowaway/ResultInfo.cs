using System.Linq;
using MFG.Types;

namespace MFG.GameData.EventTaku.Blowaway;

public class ResultInfo : MFG.GameData.EventTaku.ResultInfo
{
	public void Score_Reflesh()
	{
		bool flag = base.BlowAwayInfo.ChaInfos[Cha.Jicha].BlowawayState == BlowawayState.Blowaway;
		int num = base.BlowAwayInfo.ChaInfos.Values.Sum((ChaInfo x) => (x.BlowawayState == BlowawayState.Blowawayed) ? 1 : 0);
		base.OtherBonuses.BonusInfo(BlowawayEventGameData.BonusType.Tobi.ToString()).BonusCount = (flag ? num : 0);
	}
}
