namespace MFG.Taikyoku.Command.Send;

public class Minkan : SendCommand
{
	public Minkan(int pai_id, int tepai_pos)
	{
		Kind = SEND_COMMAND_TYPE.MINKAN;
		PaiID = pai_id;
		TepaiPos = tepai_pos;
	}
}
