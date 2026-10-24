using System;
using System.Collections.Generic;

namespace MFG.GameData;

[Serializable]
public class MusicGachaInfo
{
	public int GachaID;

	public bool IsComplete;

	public int PossessionCount;

	public int GachaSupplyCount;

	public List<string> PossessionItemOIDs = new List<string>();
}
