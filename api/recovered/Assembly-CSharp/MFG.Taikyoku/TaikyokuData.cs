using System;
using System.Collections.Generic;
using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;
using MFG.Taikyoku.Command;
using MFG.Taikyoku.Command.Receive;

namespace MFG.Taikyoku;

[Serializable]
public class TaikyokuData
{
	public int NextFetchSeqNo;

	public List<ReceiveCommand> ReceivedCommands = new List<ReceiveCommand>();

	public bool[] IsDaisou = new bool[4];

	public void Parse(XElement taikyokuElem)
	{
		try
		{
			XElement xElement = taikyokuElem.Element("cell_info");
			XAttribute xAttribute = xElement.Attribute("available");
			for (int i = 0; i < 4; i++)
			{
				XElement xElement2 = taikyokuElem.Element($"pinfo_{i}");
				if (xElement2 == null)
				{
					continue;
				}
				XElement xElement3 = xElement2.Element("con_state");
				if (xElement3 != null && xElement3.ValueInt() != 0)
				{
					XElement xElement4 = xElement2.Element("app_player_state");
					if (xElement4 != null && xElement4.ValueInt() == 1)
					{
						IsDaisou[i] = true;
					}
				}
			}
			if (xAttribute == null || int.Parse(xAttribute.Value) == 1)
			{
				XElement xElement5 = xElement.NeedNotElement("cell_sno");
				int num = 0;
				XAttribute xAttribute2 = xElement5.Attribute("start");
				if (xAttribute2 != null)
				{
					num = int.Parse(xAttribute2.Value);
				}
				int num2 = xElement5.AttributeValueInt("count");
				for (int j = 0; j < num2; j++)
				{
					int seqNo = j + num;
					XElement cellDataElem = xElement.NeedNotElement("cell_data_" + seqNo);
					ParseCellData(cellDataElem, seqNo);
				}
				NextFetchSeqNo = num + num2;
			}
		}
		catch (MustNeedNodeNotFoundException)
		{
		}
	}

	protected void ParseCellData(XElement cellDataElem, int seqNo)
	{
		ReceiveCommand receiveCommand = ReceiveCommandFactory.Create((RECEIVE_COMMAND_TYPE)cellDataElem.AttributeValueInt("kind"), seqNo);
		if (receiveCommand != null)
		{
			receiveCommand.Parse(cellDataElem);
			ReceivedCommands.Add(receiveCommand);
		}
	}
}
