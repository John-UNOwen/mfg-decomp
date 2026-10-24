using MFG.Types;

namespace MFG.Taikyoku.Command.Send;

public class Sutehai : SendCommand
{
	public Sutehai(int pai_pos, int pai_id, bool bRichi, bool bTsumoGiri, ReachLevel reachLevel)
	{
		Kind = SEND_COMMAND_TYPE.SUTE_PAI;
		TepaiPos = pai_pos;
		PaiID = pai_id;
		ReachFlag = (int)(bRichi ? reachLevel : ((ReachLevel)0));
		TsumoGiriFlag = (bTsumoGiri ? 1 : 0);
	}
}
