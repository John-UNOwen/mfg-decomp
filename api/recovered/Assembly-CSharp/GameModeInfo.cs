using System;
using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;
using MFG.Taikyoku.Command;

[Serializable]
public class GameModeInfo
{
	public GAME_MODE GameMode;

	public int TakuClass;

	public int PaymentMode;

	public int TableType;

	public int PlayerMax;

	public int Tenbo;

	public bool IsPlayable;

	public int Rate;

	public int SuperiorBorder;

	public DateTime DateStart;

	public DateTime DateEnd;

	public void Parse(XElement playmodeElem)
	{
		GameMode = (GAME_MODE)playmodeElem.Element("gmode").ValueInt();
		TakuClass = playmodeElem.Element("taku_class").ValueInt();
		PaymentMode = playmodeElem.Element("payment_mode").ValueInt();
		TableType = playmodeElem.Element("table_type").ValueInt();
		PlayerMax = playmodeElem.Element("pmax").ValueInt();
		Tenbo = playmodeElem.Element("tenbo").ValueInt();
		IsPlayable = playmodeElem.Element("state").ValueInt() > 0;
		Rate = playmodeElem.Element("rate").ValueInt();
		SuperiorBorder = playmodeElem.Element("superior_border").ValueInt();
	}
}
