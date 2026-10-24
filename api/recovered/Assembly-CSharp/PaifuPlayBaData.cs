using System.Collections.Generic;
using MFG.Taikyoku.Command.Receive;
using MFG.Types;

public class PaifuPlayBaData
{
	public Ba Ba;

	public int Kyoku;

	public int Honba;

	public List<ReceiveCommand> ReceivedCommands = new List<ReceiveCommand>();
}
