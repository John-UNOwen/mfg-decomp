using System;
using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;

namespace MFG.GameData;

[Serializable]
public class AffinityBonusInfo
{
	public string BonusTypeID;

	public int BonusKey;

	public int BonusValue;

	public int Count;

	public int Value;

	public void Parse(XElement bonusElem)
	{
		BonusTypeID = bonusElem.Attribute("bonus_type_id").Value;
		BonusKey = bonusElem.AttributeValueInt("bonus_key");
		BonusValue = bonusElem.AttributeValueInt("bonus_value");
		Count = bonusElem.Element("count").ValueInt();
		Value = bonusElem.Element("value").ValueInt();
	}
}
