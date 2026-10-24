using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;

namespace MFG.Taikyoku.Command.Receive;

public class KyokuEnd : ReceiveCommand
{
	public bool IsEnd;

	public KyokuEnd(RECEIVE_COMMAND_TYPE cmd, int seqNo)
		: base(cmd, seqNo)
	{
	}

	public override void Parse(XElement cellDataElement)
	{
		base.Parse(cellDataElement);
		try
		{
			IsEnd = cellDataElement.NeedNotElement("end_stat").ValueInt() == 1;
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}
