using System;
using System.Collections.Generic;
using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;
using MFG.Types;

namespace MFG.GameData;

public class PlayerTotalRecordInfo
{
	public Dictionary<SchoolID, int> ScrambleMatchTotalWinCount = new Dictionary<SchoolID, int>();

	public void Parse(XElement totalElem)
	{
		XElement xElement = totalElem.Element("g2_scramble");
		if (xElement == null)
		{
			return;
		}
		foreach (XElement item in xElement.Elements("history"))
		{
			SchoolID key = Enum.Parse<SchoolID>(item.Element("belong").Value);
			int value = item.Element("win_nr").ValueInt();
			ScrambleMatchTotalWinCount.TryAdd(key, value);
		}
	}
}
