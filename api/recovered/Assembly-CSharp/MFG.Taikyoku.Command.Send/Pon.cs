namespace MFG.Taikyoku.Command.Send;

public class Pon : SendCommand
{
	public Pon(int pai_id, int tepai_pos, int tepai_id, int tepai_pos2, int tepai_id2)
	{
		Kind = SEND_COMMAND_TYPE.PON;
		PaiID = pai_id;
		TepaiPos = tepai_pos;
		TepaiPos2 = tepai_pos2;
		TepaiID = tepai_id;
		TepaiID2 = tepai_id2;
	}
}
