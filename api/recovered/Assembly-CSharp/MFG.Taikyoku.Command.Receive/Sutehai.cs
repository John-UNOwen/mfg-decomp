using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;

namespace MFG.Taikyoku.Command.Receive;

public class Sutehai : ReceiveCommand
{
	public int PlayerIndex;

	public int PaiID;

	public int CpuPaiID;

	public bool IsRichi;

	public bool IsTsumoGiri;

	public bool IsFireRichi;

	public bool IsOpenRichi;

	public bool IsAtomicRichi;

	public Sutehai(RECEIVE_COMMAND_TYPE cmd, int seqNo)
		: base(cmd, seqNo)
	{
	}

	public override void Parse(XElement cellDataElement)
	{
		base.Parse(cellDataElement);
		try
		{
			PlayerIndex = cellDataElement.NeedNotElement("pindex").ValueInt();
			PaiID = cellDataElement.NeedNotElement("pai").ValueInt();
			int num = cellDataElement.NeedNotElement("stat").ValueInt();
			IsRichi = ((((uint)num & (true ? 1u : 0u)) != 0) ? true : false);
			IsTsumoGiri = ((((uint)num & 2u) != 0) ? true : false);
			IsFireRichi = ((((uint)num & 4u) != 0) ? true : false);
			IsOpenRichi = ((((uint)num & 8u) != 0) ? true : false);
			IsAtomicRichi = ((((uint)num & 0x10u) != 0) ? true : false);
		}
		catch (NeedNotNodeNotFoundException)
		{
			return;
		}
		CpuPaiID = cellDataElement.Element("cpu_pai")?.ValueInt() ?? 0;
	}
}
