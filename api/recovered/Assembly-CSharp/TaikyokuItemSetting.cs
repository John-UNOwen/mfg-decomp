using System;
using System.Collections.Generic;
using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;
using MFG.Taikyoku.Command;

[Serializable]
public class TaikyokuItemSetting
{
	public class BasicSetting
	{
		public string ObjectID;

		public DateTime DateStart;

		public DateTime DateEnd;

		public string UserRankCodeMin;

		public string UserRankCodeMax;

		public void Parse(XElement settingElem)
		{
			ObjectID = settingElem.Element("object_id").Value;
			DateStart = settingElem.Element("date_start").ValueToDateTime();
			DateEnd = settingElem.Element("date_end").ValueToDateTime();
			UserRankCodeMin = settingElem.Element("urank_code_min").Value;
			UserRankCodeMax = settingElem.Element("urank_code_max").Value;
		}
	}

	private List<BasicSetting> BasicSettings = new List<BasicSetting>();

	private Dictionary<(int, int), List<string>> ModeSettings = new Dictionary<(int, int), List<string>>();

	public void Parse(XElement battleItemSettingElem)
	{
		foreach (XElement item3 in battleItemSettingElem.Element("basic_settings").Elements("setting"))
		{
			BasicSetting basicSetting = new BasicSetting();
			basicSetting.Parse(item3);
			BasicSettings.Add(basicSetting);
		}
		foreach (XElement item4 in battleItemSettingElem.Element("playmode_settings").Elements("setting"))
		{
			int item = item4.AttributeValueInt("gmode");
			int item2 = item4.AttributeValueInt("taku_class");
			List<string> list = new List<string>();
			foreach (XElement item5 in item4.Elements("object"))
			{
				list.Add(item5.Attribute("object_id").Value);
			}
			ModeSettings.Add((item, item2), list);
		}
	}

	public List<string> GetUsableItem(GAME_MODE gameMode, TAKU_CLASS takuClass)
	{
		return ModeSettings[((int)gameMode, (int)takuClass)];
	}
}
