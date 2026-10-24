using System.Xml.Linq;

namespace MFG.Taikyoku.Command.Receive;

public class TsumoChoices : ReceiveCommand
{
	public ChoicesData Choices = new ChoicesData();

	public TsumoChoices(RECEIVE_COMMAND_TYPE cmd, int seqNo)
		: base(cmd, seqNo)
	{
	}

	public override void Parse(XElement cellDataElement)
	{
		base.Parse(cellDataElement);
		Choices.ParseTsumoChoices(cellDataElement);
	}
}
