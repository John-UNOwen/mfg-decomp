using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;

namespace MFG.Taikyoku.Command.Receive;

public class ScoreData
{
	public long BeforeScore;

	public long YakuScore;

	public int KyotakuScore;

	public int TsumifuScore;

	public long NewScore;

	public int WhereFore;

	public void Parse(XElement cellDataElement)
	{
		BeforeScore = cellDataElement.Element("before_score").ValueInt64();
		YakuScore = cellDataElement.Element("yaku_score").ValueInt64();
		KyotakuScore = cellDataElement.Element("kyotaku_score").ValueInt();
		TsumifuScore = cellDataElement.Element("tumifu_score").ValueInt();
		NewScore = cellDataElement.Element("new_score").ValueInt64();
		WhereFore = cellDataElement.Element("wherefore").ValueInt();
	}
}
