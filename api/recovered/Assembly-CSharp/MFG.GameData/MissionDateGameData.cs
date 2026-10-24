using System;
using System.Xml.Linq;

namespace MFG.GameData;

[Serializable]
public class MissionDateGameData : GameDataBase
{
	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public MissionControlInfo ControlInfo = new MissionControlInfo();

	public void Parse(XElement dateResultElem)
	{
	}
}
