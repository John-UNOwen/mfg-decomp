using MFG.MasterGameData;

namespace MFG.Types.Customize.Voice;

public class Item
{
	public ItemID ItemID;

	public string Label;

	public string Name;

	public Category Category;

	public bool Available = true;

	public ObtainType ObtainType;

	public int NeedUnlockFavo;

	public int GachaItemNo;

	public string ObjectID => VoiceMaster.ToObjectID(ItemID);

	public string DispName => "ボイス「" + SaniTize.SanitizedString(Name) + "」";
}
