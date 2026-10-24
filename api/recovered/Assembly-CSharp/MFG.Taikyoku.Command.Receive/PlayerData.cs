using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;

namespace MFG.Taikyoku.Command.Receive;

public class PlayerData
{
	public int Jikaze;

	public int[] TePai;

	public long Score;

	public int Rank;

	public int TsumoPai;

	public int[] SutePai;

	public int[] SutePaiState;

	public FuroData[] Furo = new FuroData[FuroData.FURO_MAX];

	public bool IsRichi;

	public bool IsOkkakeRichi;

	public void Parse(XElement dataElem)
	{
		ParseBasicData(dataElem);
	}

	public void ParseFromReconnect(XElement dataElem)
	{
		ParseBasicData(dataElem);
		try
		{
			TsumoPai = dataElem.NeedNotElement("tsumo_pai").ValueInt();
			SutePai = dataElem.NeedNotElement("sute_pai").ValueArrayInt();
			SutePaiState = dataElem.NeedNotElement("sute_pai_state").ValueArrayInt();
			IsRichi = ((dataElem.NeedNotElement("reechi").ValueInt() != 0) ? true : false);
			IsOkkakeRichi = ((dataElem.NeedNotElement("okkake_reechi").ValueInt() != 0) ? true : false);
			for (int i = 0; i < FuroData.FURO_MAX; i++)
			{
				FuroData furoData = new FuroData();
				furoData.ParseFromReconnect(dataElem.Element("naki" + i));
				Furo[i] = furoData;
			}
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}

	protected void ParseBasicData(XElement dataElem)
	{
		try
		{
			Jikaze = dataElem.NeedNotElement("jikaze").ValueInt();
			TePai = dataElem.NeedNotElement("tepai").ValueArrayInt();
			Score = dataElem.NeedNotElement("score").ValueInt64();
			Rank = dataElem.NeedNotElement("rank").ValueInt();
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}
