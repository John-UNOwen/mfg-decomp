using System;
using MFG.Types;

namespace MFG.GameData;

[Serializable]
public class MahjongActorMatchRecord
{
	public MahjongActorID MahjongActorID;

	public int TotalWinCount;

	public int TotalCount;
}
