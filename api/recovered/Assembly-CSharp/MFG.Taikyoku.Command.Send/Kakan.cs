namespace MFG.Taikyoku.Command.Send;

public class Kakan : SendCommand
{
	public Kakan(int pai_id, int tepai_pos)
	{
		Kind = SEND_COMMAND_TYPE.KAKAN;
		PaiID = pai_id;
		TepaiPos = tepai_pos;
	}
}
