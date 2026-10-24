using System;
using MFG.Types;

namespace MFG.GameData;

[Serializable]
public class VoteInfo
{
	public CharaType Chara;

	public int VoteCount;

	public string LastVoteDate;
}
