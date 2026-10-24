using System;
using MFG.MasterGameData;
using MFG.Types.Coop;

namespace MFG.GameData;

[Serializable]
public class CoopInfo
{
	public int PurchaseId;

	public string ObjectID;

	public int Amount;

	public CoopID CoopID => CoopMaster.GetCoopID(ObjectID);
}
