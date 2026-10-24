using System.Collections.Generic;
using System.Linq;
using MFG.Types;

namespace MFG.GameData.EventTaku;

public class BlowAwayChaInfo
{
	public Dictionary<Cha, ChaInfo> ChaInfos = new Dictionary<Cha, ChaInfo>();

	private const Cha Cha_None = (Cha)(-1);

	public void Init(List<Player> players)
	{
		ChaInfos.Clear();
		foreach (Player player in players)
		{
			ChaInfos.Add(player.m_cha, new ChaInfo
			{
				Cha = player.m_cha,
				BlowawayState = BlowawayState.None,
				CharaType = player.m_charaType
			});
		}
	}

	public void BlowawayState_Reflesh(Cha blowawayCha, List<Player> players)
	{
		if (!players.Any((Player x) => x.m_score <= 0))
		{
			return;
		}
		if (blowawayCha != (Cha)(-1))
		{
			ChaInfos[blowawayCha].BlowawayState = BlowawayState.Blowaway;
		}
		foreach (Player player in players)
		{
			if (player.m_cha != blowawayCha)
			{
				ChaInfos[player.m_cha].BlowawayState = ((player.m_score <= 0) ? BlowawayState.Blowawayed : BlowawayState.None);
			}
		}
	}
}
