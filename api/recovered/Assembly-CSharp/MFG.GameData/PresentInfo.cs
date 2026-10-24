using System;
using MFG.Assets;
using MFG.Scenes.CommonSystem;

namespace MFG.GameData;

[Serializable]
public class PresentInfo : IAssetPath
{
	public ulong PresentID;

	public string Title;

	public string Text;

	public string Content;

	public int Count;

	public string LimitDate;

	public string ReceivedDate;

	public bool IsReceived;

	public bool IsAutoApply;

	public bool IsHideHistory;

	public string AddressableAssetPath => AssetPathResolver.ResolveGachaBannerAssets((int)PresentID);

	public string IEndDateString => "";
}
