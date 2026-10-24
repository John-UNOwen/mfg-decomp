using MFG.Taikyoku.Command.Receive;
using MFG.Types;

public class PaifuDump
{
	public DumpState State;

	public HaiID HaiID;

	public PaifuDump(Sutehai cmd)
	{
		State = (DumpState)((cmd.IsTsumoGiri ? 1 : 0) + (cmd.IsRichi ? 2 : 0));
		HaiID = ((Pai)cmd.PaiID).ToHaiID();
	}

	public void SetMeld()
	{
		State += 4;
	}

	public void SetRon()
	{
		State = (((int)State % 2 == 0) ? DumpState.RonPai : DumpState.TsumoGiriRonPai);
	}
}
