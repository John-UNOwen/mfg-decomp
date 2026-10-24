using MFG.Assets;

namespace MFG.Types.MahjongActorMaster;

public class Item
{
	public MahjongActorID MahjongActorID;

	public CharaType CharaType;

	public string LastName;

	public string FirstName;

	public string FullName => LastName + FirstName;

	public string ActorIconPath => AssetPathResolver.MahjongActor.ResolveActorIconPath(MahjongActorID);
}
