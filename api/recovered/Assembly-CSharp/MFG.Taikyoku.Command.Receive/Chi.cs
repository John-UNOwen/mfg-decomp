using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;

namespace MFG.Taikyoku.Command.Receive;

public class Chi : ReceiveCommand
{
	public int PlayerIndex;

	public int SutePlayerIndex;

	public int PaiID;

	public int[] ChiPaiID;

	public Chi(RECEIVE_COMMAND_TYPE cmd, int seqNo)
		: base(cmd, seqNo)
	{
	}

	public override void Parse(XElement cellDataElement)
	{
		base.Parse(cellDataElement);
		try
		{
			PlayerIndex = cellDataElement.NeedNotElement("pindex").ValueInt();
			SutePlayerIndex = cellDataElement.NeedNotElement("sute_pindex").ValueInt();
			PaiID = cellDataElement.NeedNotElement("pai").ValueInt();
			ChiPaiID = cellDataElement.NeedNotElement("chi_pai").ValueArrayInt();
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}
