using System;
using System.Collections.Generic;
using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;
using MFG.Types.Customize.CustomizeItem;

namespace MFG.Taikyoku;

public class StampData
{
	public class Element
	{
		public long SeqID;

		public int MobileID;

		public int PlayerIndex;

		public DateTime SendTime;

		public string PlayerName = "";

		public ItemID TableStickerItemID;

		public string Param = "";

		public void Parse(XElement stampElem)
		{
			SeqID = stampElem.AttributeValueInt64("idx");
			MobileID = stampElem.AttributeValueInt("mid");
			PlayerIndex = stampElem.AttributeValueInt("pindex");
			SendTime = stampElem.AttributeValueJavaCurrentTimeMillsToDateTime("time");
			PlayerName = stampElem.Element("name").Value;
			if (!Enum.TryParse<ItemID>(stampElem.Element("contents").Value, out TableStickerItemID))
			{
				TableStickerItemID = ItemID.None;
			}
			Param = stampElem.Element("param").Value;
		}
	}

	public List<Element> Elements = new List<Element>();

	public void Parse(XElement stampBaseElem)
	{
		foreach (XElement item in stampBaseElem.Elements("d"))
		{
			if (item != null)
			{
				Element element = new Element();
				element.Parse(item);
				Elements.Add(element);
			}
		}
	}
}
