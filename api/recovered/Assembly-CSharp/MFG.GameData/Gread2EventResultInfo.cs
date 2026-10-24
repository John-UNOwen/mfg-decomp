using System;
using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;
using MFG.Types;

namespace MFG.GameData;

[Serializable]
public class Gread2EventResultInfo
{
	public int EventID;

	public string SubCategory = "";

	public bool IsWin;

	public SchoolID SchoolID => Enum.Parse<SchoolID>(SubCategory);

	public void Parse(XElement resultElem)
	{
		EventID = resultElem.Element("id").ValueInt();
		SubCategory = resultElem.Element("sub_cate").Value;
		IsWin = resultElem.Element("win").ValueBool();
	}
}
