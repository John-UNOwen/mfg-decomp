namespace MFG.Taikyoku.Command.Send;

public class Chi : SendCommand
{
	public Chi(int pai_id, int tepai_pos, int tepai_id, int tepai_pos2, int tepai_id2, int chi_naki)
	{
		Kind = SEND_COMMAND_TYPE.CHI;
		TepaiPos = tepai_pos;
		TepaiPos2 = tepai_pos2;
		PaiID = pai_id;
		TepaiID = tepai_id;
		TepaiID2 = tepai_id2;
		ChiNaki = chi_naki;
	}
}
