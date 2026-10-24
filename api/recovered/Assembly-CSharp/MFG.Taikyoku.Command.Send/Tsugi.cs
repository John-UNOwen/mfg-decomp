namespace MFG.Taikyoku.Command.Send;

public class Tsugi : SendCommand
{
	public Tsugi()
	{
		Kind = SEND_COMMAND_TYPE.NAKINASHI;
	}
}
