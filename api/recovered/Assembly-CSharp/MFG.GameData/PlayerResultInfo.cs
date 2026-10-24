using System;
using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;

namespace MFG.GameData;

[Serializable]
public class PlayerResultInfo
{
	public int Rank;

	public long Score;

	public int Uma;

	public void Parse(XElement playerResultElem)
	{
		try
		{
			Rank = playerResultElem.NeedNotElement("rank").ValueInt();
			Score = playerResultElem.NeedNotElement("score").ValueInt64();
			Uma = playerResultElem.NeedNotElement("uma").ValueInt();
		}
		catch (MustNeedNodeNotFoundException)
		{
		}
	}
}
