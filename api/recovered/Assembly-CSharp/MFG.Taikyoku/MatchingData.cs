using System;
using System.Collections.Generic;
using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;

namespace MFG.Taikyoku;

[Serializable]
public class MatchingData
{
	public bool IsMatchingEnd;

	public int PlayerIndex;

	public int HumanPlayerNum;

	public int CpuPlayerNum;

	public int PlayerNum;

	public List<PlayerInfo> PlayerInfos;

	public bool IsScrambleMatch;

	public bool IsMotherWars;

	public void Parse(XElement mwaitElem)
	{
		try
		{
			int num = mwaitElem.NeedNotElement("status").ValueInt();
			IsMatchingEnd = ((num != 0) ? true : false);
			HumanPlayerNum = mwaitElem.NeedNotElement("pnum").ValueInt();
			CpuPlayerNum = mwaitElem.NeedNotElement("cpu_num").ValueInt();
			PlayerNum = HumanPlayerNum + CpuPlayerNum;
			if (mwaitElem.Element("pindex") != null)
			{
				mwaitElem.Element("pindex").ValueInt();
			}
			ParseEntryPlayerNode(mwaitElem);
			MatchingEndNode(mwaitElem);
		}
		catch (MustNeedNodeNotFoundException)
		{
		}
	}

	protected void ParseEntryPlayerNode(XElement mwaitElem)
	{
		try
		{
			for (int i = 0; i < HumanPlayerNum; i++)
			{
				XElement xElement = mwaitElem.Element("epdata_" + i);
				PlayerInfos[i].PlayerIndex = i;
				if (xElement != null)
				{
					PlayerInfos[i].ParseFromEntryPlayerNode(xElement);
				}
			}
		}
		catch (MustNeedNodeNotFoundException)
		{
		}
	}

	protected void MatchingEndNode(XElement mwaitElem)
	{
		XElement xElement = mwaitElem.Element("mend");
		if (xElement == null)
		{
			return;
		}
		try
		{
			for (int i = 0; i < PlayerNum; i++)
			{
				XElement playerElem = xElement.Element("player_" + i);
				PlayerInfos[i].PlayerIndex = i;
				PlayerInfos[i].ParseFromMatchingEndNode(playerElem);
			}
		}
		catch (MustNeedNodeNotFoundException)
		{
			return;
		}
		XElement xElement2 = xElement.Element("g2event");
		if (xElement2 == null)
		{
			return;
		}
		using IEnumerator<XElement> enumerator = xElement2.Elements("e").GetEnumerator();
		while (enumerator.MoveNext())
		{
			switch (enumerator.Current.Element("id").ValueInt())
			{
			case 1:
				IsScrambleMatch = true;
				break;
			case 2:
				IsMotherWars = true;
				break;
			}
		}
	}

	public MatchingData()
	{
		PlayerInfos = new List<PlayerInfo>();
		PlayerInfos.Add(new PlayerInfo());
		PlayerInfos.Add(new PlayerInfo());
		PlayerInfos.Add(new PlayerInfo());
		PlayerInfos.Add(new PlayerInfo());
	}
}
