using System.Linq;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Shirahama;

public class CachedAssetBundleLocation : ResourceLocationBase
{
	public CachedAssetBundleLocation(IResourceLocation location)
		: base(location.PrimaryKey, location.InternalId, location.ProviderId, location.ResourceType, location.Dependencies?.ToArray() ?? new IResourceLocation[0])
	{
		base.Data = location.Data;
	}
}
