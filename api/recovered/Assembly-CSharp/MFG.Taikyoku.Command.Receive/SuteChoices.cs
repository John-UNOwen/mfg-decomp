using System.Xml.Linq;

namespace MFG.Taikyoku.Command.Receive;

public class SuteChoices : ReceiveCommand
{
	public ChoicesData Choices = new ChoicesData();

	public SuteChoices(RECEIVE_COMMAND_TYPE cmd, int seqNo)
		: base(cmd, seqNo)
	{
	}

	public override void Parse(XElement cellDataElement)
	{
		base.Parse(cellDataElement);
		Choices.ParseSuteChoices(cellDataElement);
	}
}
