using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;

namespace MFG.Taikyoku.Command.Receive;

public class Tsumo : ReceiveCommand
{
	public int PlayerIndex;

	public int PaiID;

	public Tsumo(RECEIVE_COMMAND_TYPE cmd, int seqNo)
		: base(cmd, seqNo)
	{
	}

	public override void Parse(XElement cellDataElement)
	{
		base.Parse(cellDataElement);
		try
		{
			PlayerIndex = cellDataElement.NeedNotElement("pindex").ValueInt();
			PaiID = cellDataElement.NeedNotElement("pai").ValueInt();
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}
