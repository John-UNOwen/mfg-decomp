using System;
using MFG.Types;

namespace MFG.GameData;

[Serializable]
public class FriendInfo
{
	public long OdekakeDataID;

	public string FriendID;

	public string PlayerName;

	public DanLevelType Dan;

	public bool IsUsable;

	public CharaType CharaType;
}
