namespace MFG.Types.PlayerPaidItem;

public class Item
{
	public string ObjectID;

	public ItemID ItemID;

	public Category Category;

	public string Name;

	public string Description;

	public bool IsTicketConvertEnable;

	public bool Available = true;

	public string IconGraphicPath => $"CustomizeIcon/Icon{ItemID}.png";
}
