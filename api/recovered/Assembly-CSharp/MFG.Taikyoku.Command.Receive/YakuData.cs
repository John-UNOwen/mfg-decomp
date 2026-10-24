using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;

namespace MFG.Taikyoku.Command.Receive;

public class YakuData
{
	public int PaiID;

	public int HanID;

	public int HanNum;

	public int FuNum;

	public int DoraNum;

	public int BonusHan;

	public ulong YakuBit;

	public int[] TePai;

	public FuroData[] Furo = new FuroData[FURO_MAX];

	public static readonly int FURO_MAX = 4;

	public void Parse(XElement yakuElem)
	{
		PaiID = yakuElem.Element("pai").ValueInt();
		HanID = yakuElem.Element("yaku_han").ValueInt();
		HanNum = yakuElem.Element("han_num").ValueInt();
		FuNum = (byte)yakuElem.Element("fu_num").ValueInt();
		DoraNum = yakuElem.Element("dora_num").ValueInt();
		XElement xElement = yakuElem.Element("bonus_han");
		if (xElement != null)
		{
			BonusHan = xElement.ValueInt();
		}
		ulong num = yakuElem.Element("yaku1").ValueUInt();
		ulong num2 = yakuElem.Element("yaku2").ValueUInt();
		YakuBit = (num2 << 32) | num;
		TePai = yakuElem.Element("tepai").ValueArrayInt();
		for (int i = 0; i < FURO_MAX; i++)
		{
			FuroData furoData = new FuroData();
			furoData.Parse(yakuElem.Element("naki" + i));
			Furo[i] = furoData;
		}
	}
}
