namespace MFG.Taikyoku.Command.Send;

public class Reconnect : SendCommand
{
	public Reconnect()
	{
		Kind = SEND_COMMAND_TYPE.RECONNECT;
	}
}
