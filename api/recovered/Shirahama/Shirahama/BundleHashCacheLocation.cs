using UnityEngine.ResourceManagement.ResourceLocations;

namespace Shirahama;

public class BundleHashCacheLocation : ResourceLocationBase
{
	public static readonly BundleHashCacheLocation Default = new BundleHashCacheLocation();

	public BundleHashCacheLocation(string name = "bundle_hash_table")
		: base(name, name, "BundleHashCacheLocation", typeof(string))
	{
	}
}
