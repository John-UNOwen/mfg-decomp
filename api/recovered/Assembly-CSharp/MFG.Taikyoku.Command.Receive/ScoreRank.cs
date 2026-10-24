using System.Collections.Generic;
using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;

namespace MFG.Taikyoku.Command.Receive;

public class ScoreRank : ReceiveCommand
{
	public class RichiAfterData
	{
		public long score;

		public int rank;

		public void Parse(XElement richiAfterElem)
		{
			try
			{
				score = richiAfterElem.NeedNotElement("score").ValueInt64();
				rank = richiAfterElem.NeedNotElement("rank").ValueInt();
			}
			catch (NeedNotNodeNotFoundException)
			{
			}
		}
	}

	public int Kyotaku;

	public List<RichiAfterData> RichiAfterDatas = new List<RichiAfterData>();

	public ScoreRank(RECEIVE_COMMAND_TYPE cmd, int seqNo)
		: base(cmd, seqNo)
	{
	}

	public override void Parse(XElement cellDataElement)
	{
		base.Parse(cellDataElement);
		try
		{
			for (int i = 0; i < Constants.TAKU_PLAYER_MAX; i++)
			{
				string s = "riti_after" + i;
				if (cellDataElement.IsExistElement(s))
				{
					RichiAfterData richiAfterData = new RichiAfterData();
					richiAfterData.Parse(cellDataElement.NeedNotElement(s));
					RichiAfterDatas.Add(richiAfterData);
				}
			}
			if (cellDataElement.IsExistElement("kyoutaku"))
			{
				Kyotaku = cellDataElement.NeedNotElement("kyoutaku").ValueInt();
			}
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}
