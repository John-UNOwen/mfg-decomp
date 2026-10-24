namespace MFG.Taikyoku.Command.Send;

public class Ankan : SendCommand
{
	public Ankan(int pai_id, int tepai_pos)
	{
		Kind = SEND_COMMAND_TYPE.ANKAN;
		PaiID = pai_id;
		TepaiPos = tepai_pos;
	}
}
