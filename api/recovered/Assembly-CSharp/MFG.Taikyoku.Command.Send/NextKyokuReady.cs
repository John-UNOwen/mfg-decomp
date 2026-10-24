namespace MFG.Taikyoku.Command.Send;

public class NextKyokuReady : SendCommand
{
	public NextKyokuReady()
	{
		Kind = SEND_COMMAND_TYPE.NEXT_KYOKU_READY;
	}
}
