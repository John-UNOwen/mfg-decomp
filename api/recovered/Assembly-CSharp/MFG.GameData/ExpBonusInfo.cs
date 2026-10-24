using System;
using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;

namespace MFG.GameData;

[Serializable]
public class ExpBonusInfo
{
	public int BonusID;

	public int Exp;

	public void Parse(XElement bonusElem)
	{
		BonusID = bonusElem.AttributeValueInt("bonus_id");
		Exp = bonusElem.Element("exp").ValueInt();
	}
}
