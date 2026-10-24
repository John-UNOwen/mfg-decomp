using System.Collections.Generic;
using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;

namespace MFG.Taikyoku.Command.Receive;

public class TaikyokuStartInfo
{
	public int ChicyaPlayerIndex;

	public int OyaPlayerIndex;

	public int[] Sai;

	public int Ba;

	public int Kyoku;

	public bool IsAllLast;

	public int Honba;

	public int Rencyan;

	public int Kyotaku;

	public int Nokori;

	public int OpenDoraNum;

	public int[] OmoteDoraData;

	public int[] UraDoraData;

	public int YamaNum;

	public int[] YamaPai;

	public int[] RinshanPai;

	public List<PlayerData> Players = new List<PlayerData>();

	public int NextSeqNo;

	public bool IsNotDoraOpenAfterMinkan;

	public int LastChouKouNum;

	public int ChouKouNum;

	public void Parse(XElement dataElem)
	{
		try
		{
			ParseBasicData(dataElem);
			ParseDora(dataElem);
			ParseYama(dataElem);
			ParseRinshan(dataElem);
			ParsePlayer(dataElem);
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}

	public void ParseFromReconnect(XElement dataElem)
	{
		try
		{
			ParseBasicData(dataElem);
			ParseDora(dataElem);
			ParseYama(dataElem);
			ParseRinshan(dataElem);
			ParsePlayerFromReconnect(dataElem);
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}

	public void ParseBasicData(XElement dataElem)
	{
		try
		{
			ChicyaPlayerIndex = dataElem.NeedNotElement("chicya").ValueInt();
			OyaPlayerIndex = dataElem.NeedNotElement("oya").ValueInt();
			Sai = dataElem.NeedNotElement("sai").ValueArrayInt();
			Ba = dataElem.NeedNotElement("ba").ValueInt();
			Kyoku = dataElem.NeedNotElement("kyoku").ValueInt();
			IsAllLast = dataElem.NeedNotElement("all_last").ValueInt() == 1;
			Honba = dataElem.NeedNotElement("honba").ValueInt();
			Rencyan = dataElem.NeedNotElement("rencyan").ValueInt();
			Kyotaku = dataElem.NeedNotElement("kyoutaku").ValueInt();
			Nokori = dataElem.NeedNotElement("nokori").ValueInt();
		}
		catch (NeedNotNodeNotFoundException)
		{
			return;
		}
		XElement xElement = dataElem.Element("next_sno");
		if (xElement != null)
		{
			NextSeqNo = xElement.ValueInt();
		}
		XElement xElement2 = dataElem.Element("last_cyoukou_num");
		if (xElement2 != null)
		{
			LastChouKouNum = xElement2.ValueInt();
		}
		XElement xElement3 = dataElem.Element("cyoukou_num");
		if (xElement3 != null)
		{
			ChouKouNum = xElement3.ValueInt();
		}
		XElement xElement4 = dataElem.Element("minkan_flag");
		if (xElement4 != null)
		{
			IsNotDoraOpenAfterMinkan = ((xElement4.ValueInt() != 0) ? true : false);
		}
	}

	public void ParseDora(XElement dataElem)
	{
		try
		{
			OpenDoraNum = dataElem.NeedNotElement("dora_open").ValueInt();
			OmoteDoraData = dataElem.NeedNotElement("dora").ValueArrayInt();
			UraDoraData = dataElem.NeedNotElement("ura_dora").ValueArrayInt();
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}

	public void ParseYama(XElement dataElem)
	{
		try
		{
			YamaNum = dataElem.NeedNotElement("yama_cnt").ValueInt();
			YamaPai = dataElem.NeedNotElement("yama").ValueArrayInt();
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}

	public void ParseRinshan(XElement dataElem)
	{
		try
		{
			RinshanPai = dataElem.NeedNotElement("rinshan").ValueArrayInt();
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}

	public void ParsePlayer(XElement dataElem)
	{
		try
		{
			for (int i = 0; i < Constants.TAKU_PLAYER_MAX; i++)
			{
				PlayerData playerData = new PlayerData();
				playerData.Parse(dataElem.NeedNotElement("player_info" + i));
				Players.Add(playerData);
			}
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}

	public void ParsePlayerFromReconnect(XElement dataElem)
	{
		try
		{
			for (int i = 0; i < Constants.TAKU_PLAYER_MAX; i++)
			{
				PlayerData playerData = new PlayerData();
				playerData.Parse(dataElem.NeedNotElement("player" + i));
				Players.Add(playerData);
			}
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}
