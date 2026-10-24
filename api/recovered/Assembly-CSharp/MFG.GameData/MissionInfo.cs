using System;

namespace MFG.GameData;

[Serializable]
public class MissionInfo
{
	public string ObjectID;

	public int ID;

	public bool IsClear;

	public int Progress;

	public string LimitDate = "";

	public int LoopCount;
}
