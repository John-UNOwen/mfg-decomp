using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;

namespace MFG.Taikyoku.Command.Receive;

public class FuroData
{
	public class PaiData
	{
		public int Muki;

		public int paiID;

		public void Parse(XElement paiElem)
		{
			Muki = paiElem.Element("pai_st").ValueInt();
			paiID = paiElem.Element("pai").ValueInt();
		}

		public void ParseFromReconnect(XElement paiElem)
		{
			Muki = paiElem.Element("pai_st").ValueInt();
			paiID = paiElem.Element("pai_id").ValueInt();
		}
	}

	public int MentsuType;

	public int KanType;

	public PaiData[] Pai = new PaiData[PAI_MAX];

	public static readonly int PAI_MAX = 4;

	public static readonly int FURO_MAX = 4;

	public void Parse(XElement nakiElem)
	{
		MentsuType = nakiElem.Element("type").ValueInt();
		KanType = nakiElem.Element("kantype").ValueInt();
		for (int i = 0; i < PAI_MAX; i++)
		{
			PaiData paiData = new PaiData();
			paiData.Parse(nakiElem.Element("pai" + i));
			Pai[i] = paiData;
		}
	}

	public void ParseFromReconnect(XElement nakiElem)
	{
		MentsuType = nakiElem.Element("mentsu_type").ValueInt();
		KanType = nakiElem.Element("kantype").ValueInt();
		for (int i = 0; i < PAI_MAX; i++)
		{
			PaiData paiData = new PaiData();
			XElement xElement = nakiElem.Element("pai" + i);
			if (xElement != null)
			{
				paiData.ParseFromReconnect(xElement);
			}
			Pai[i] = paiData;
		}
	}
}
