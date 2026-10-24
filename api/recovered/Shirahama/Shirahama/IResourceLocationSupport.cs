using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Shirahama;

public static class IResourceLocationSupport
{
	public static LocationType GetLocationType(this ResourceManager manager, IResourceLocation location)
	{
		if (location == null)
		{
			return LocationType.Unknown;
		}
		if (location.ResourceType != typeof(IAssetBundleResource))
		{
			return LocationType.Unknown;
		}
		if (manager.TransformInternalId(location).StartsWith("http"))
		{
			return LocationType.Remote;
		}
		return LocationType.Local;
	}
}
