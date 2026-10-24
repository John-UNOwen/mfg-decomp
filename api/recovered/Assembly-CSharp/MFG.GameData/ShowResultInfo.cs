using System;
using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;

namespace MFG.GameData;

[Serializable]
public class ShowResultInfo
{
	public int FightVoltage;

	public float ContributionRate;

	public float CardEffectRate;

	public float ItemEffectRate;

	public int PunishmentBonus;

	public int AcquiredPopularityExp;

	public void Parse(XElement showResultElem)
	{
		try
		{
			FightVoltage = showResultElem.Element("voltage")?.ValueInt() ?? 0;
			ContributionRate = ((float?)showResultElem.Element("contribute_percent")?.ValueInt() * 0.01f) ?? 1f;
			CardEffectRate = ((float?)showResultElem.Element("card_effect_percent")?.ValueInt() * 0.01f + 1f) ?? 1f;
			ItemEffectRate = ((float?)showResultElem.Element("item_effect_percent")?.ValueInt() * 0.01f + 1f) ?? 1f;
			PunishmentBonus = showResultElem.Element("bonus")?.ValueInt() ?? 0;
			AcquiredPopularityExp = showResultElem.Element("get_point")?.ValueInt() ?? 0;
			showResultElem.Element("old");
			showResultElem.Element("new");
		}
		catch (MustNeedNodeNotFoundException)
		{
		}
	}
}
