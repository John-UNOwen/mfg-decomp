using System.Collections.Generic;
using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;
using UnityEngine;

namespace MFG.Taikyoku.Command.Receive;

public class KyokuStart : ReceiveCommand
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

	public KyokuStart(RECEIVE_COMMAND_TYPE cmd, int seqNo)
		: base(cmd, seqNo)
	{
	}

	public override void Parse(XElement cellDataElement)
	{
		base.Parse(cellDataElement);
		try
		{
			ChicyaPlayerIndex = cellDataElement.NeedNotElement("chicya").ValueInt();
			OyaPlayerIndex = cellDataElement.NeedNotElement("oya").ValueInt();
			Sai = cellDataElement.NeedNotElement("sai").ValueArrayInt();
			Ba = cellDataElement.NeedNotElement("ba").ValueInt();
			Kyoku = cellDataElement.NeedNotElement("kyoku").ValueInt();
			IsAllLast = cellDataElement.NeedNotElement("all_last").ValueInt() == 1;
			Honba = cellDataElement.NeedNotElement("honba").ValueInt();
			Rencyan = cellDataElement.NeedNotElement("rencyan").ValueInt();
			Kyotaku = cellDataElement.NeedNotElement("kyoutaku").ValueInt();
			Nokori = cellDataElement.NeedNotElement("nokori").ValueInt();
			ParseDora(cellDataElement);
			ParseYama(cellDataElement);
			ParseRinshan(cellDataElement);
			ParsePlayer(cellDataElement);
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}

	public void ParseDora(XElement cellDataElement)
	{
		try
		{
			OpenDoraNum = cellDataElement.NeedNotElement("dora_open").ValueInt();
			OmoteDoraData = cellDataElement.NeedNotElement("dora").ValueArrayInt();
			Debug.Log("dora_check:" + string.Join(',', OmoteDoraData));
			UraDoraData = cellDataElement.NeedNotElement("ura_dora").ValueArrayInt();
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}

	public void ParseYama(XElement cellDataElement)
	{
		try
		{
			YamaNum = cellDataElement.NeedNotElement("yama_cnt").ValueInt();
			YamaPai = cellDataElement.NeedNotElement("yama").ValueArrayInt();
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}

	public void ParseRinshan(XElement cellDataElement)
	{
		try
		{
			RinshanPai = cellDataElement.NeedNotElement("rinshan").ValueArrayInt();
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}

	public void ParsePlayer(XElement cellDataElement)
	{
		try
		{
			for (int i = 0; i < Constants.TAKU_PLAYER_MAX; i++)
			{
				PlayerData playerData = new PlayerData();
				playerData.Parse(cellDataElement.NeedNotElement("player_info" + i));
				Players.Add(playerData);
			}
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}
