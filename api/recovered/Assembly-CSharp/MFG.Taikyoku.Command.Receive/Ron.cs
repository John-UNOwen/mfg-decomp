using System.Collections.Generic;
using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;

namespace MFG.Taikyoku.Command.Receive;

public class Ron : ReceiveCommand
{
	public int PlayerIndex;

	public int OpenDoraNum;

	public int[] OmoteDoraData;

	public int[] UraDoraData;

	public bool HaveDoraData;

	public int[] RonFlag;

	public List<YakuData> Yakus = new List<YakuData>();

	public List<ScoreData> Scores = new List<ScoreData>();

	public Ron(RECEIVE_COMMAND_TYPE cmd, int seqNo)
		: base(cmd, seqNo)
	{
	}

	public override void Parse(XElement cellDataElement)
	{
		base.Parse(cellDataElement);
		try
		{
			PlayerIndex = cellDataElement.NeedNotElement("furikomi_pindex").ValueInt();
			RonFlag = cellDataElement.NeedNotElement("ron_flg").ValueArrayInt();
			ParseDora(cellDataElement);
			for (int i = 0; i < Constants.TAKU_PLAYER_MAX; i++)
			{
				YakuData yakuData = new YakuData();
				yakuData.Parse(cellDataElement.NeedNotElement("yaku" + i));
				Yakus.Add(yakuData);
				ScoreData scoreData = new ScoreData();
				scoreData.Parse(cellDataElement.NeedNotElement("calc_score" + i));
				Scores.Add(scoreData);
			}
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}

	public void ParseDora(XElement cellDataElement)
	{
		try
		{
			OpenDoraNum = cellDataElement.NeedNotElement("dora_open").ValueInt();
			OmoteDoraData = cellDataElement.NeedNotElement("dora").ValueArrayInt();
			UraDoraData = cellDataElement.NeedNotElement("ura_dora").ValueArrayInt();
			HaveDoraData = true;
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}
