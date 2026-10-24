using System.Collections.Generic;
using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;

namespace MFG.Taikyoku.Command.Receive;

public class RyuKyoku : ReceiveCommand
{
	public int[] EndStatus = new int[Constants.TAKU_PLAYER_MAX];

	public int[][] MachiPaiID = new int[Constants.TAKU_PLAYER_MAX][];

	public List<ScoreData> Scores = new List<ScoreData>();

	public RyuKyoku(RECEIVE_COMMAND_TYPE cmd, int seqNo)
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
				XElement baseElement = cellDataElement.NeedNotElement("ryukyoku_status" + i);
				EndStatus[i] = baseElement.NeedNotElement("end_stat").ValueInt();
				MachiPaiID[i] = baseElement.NeedNotElement("machi_pai").ValueArrayInt();
				ScoreData scoreData = new ScoreData();
				scoreData.Parse(cellDataElement.NeedNotElement("calc_score" + i));
				Scores.Add(scoreData);
			}
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}
