using System;
using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;

namespace MFG.Taikyoku.Command.Receive;

[Serializable]
public class ReceiveCommand
{
	public RECEIVE_COMMAND_TYPE Kind;

	public PLAYER_INDEX_FLAG PlayerIndexFlag;

	public int SeqNo;

	public ReceiveCommand(RECEIVE_COMMAND_TYPE cmdType, int seqNo)
	{
		Kind = cmdType;
		PlayerIndexFlag = (PLAYER_INDEX_FLAG)0;
		SeqNo = seqNo;
	}

	public virtual void Parse(XElement cellDataElement)
	{
		ParsePlayerIndexFlag(cellDataElement);
	}

	protected void ParsePlayerIndexFlag(XElement cellDataElement)
	{
		PlayerIndexFlag = (PLAYER_INDEX_FLAG)0;
		if (cellDataElement.AttributeValueInt("pi0") == 1)
		{
			PlayerIndexFlag |= PLAYER_INDEX_FLAG.P0;
		}
		if (cellDataElement.AttributeValueInt("pi1") == 1)
		{
			PlayerIndexFlag |= PLAYER_INDEX_FLAG.P1;
		}
		if (cellDataElement.AttributeValueInt("pi2") == 1)
		{
			PlayerIndexFlag |= PLAYER_INDEX_FLAG.P2;
		}
		if (cellDataElement.AttributeValueInt("pi3") == 1)
		{
			PlayerIndexFlag |= PLAYER_INDEX_FLAG.P3;
		}
	}
}
