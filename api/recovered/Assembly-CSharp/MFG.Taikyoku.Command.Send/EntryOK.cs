namespace MFG.Taikyoku.Command.Send;

public class EntryOK : SendCommand
{
	public EntryOK()
	{
		Kind = SEND_COMMAND_TYPE.ENTRY;
	}
}
