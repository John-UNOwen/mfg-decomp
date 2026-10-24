using MFG.Assets;

namespace MFG.Types.PlayerItem;

public class Item
{
	public string ObjectID;

	public ItemID ItemID;

	public Category Category;

	public int SubCategory;

	public string Name;

	public string Description;

	public bool Available = true;

	public int LimitedCount = -1;

	public bool IsUpperLimited => LimitedCount >= 1;

	public string IconGraphicPath => AssetPathResolver.ResolvePlayersItemIcon(ItemID);
}
