using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Shirahama;

public class LocalResolver : IRuntimeResolver
{
	public static string CacheCatalogPrimaryKey => "AddressablesMainContentCatalogCacheHash";

	public string CatalogDirectoryPath => Application.streamingAssetsPath ?? "";

	public string Resolve(IResourceLocation location)
	{
		if (location.PrimaryKey == CacheCatalogPrimaryKey)
		{
			string fileName = Path.GetFileName(location.InternalId);
			return CatalogDirectoryPath + "/" + fileName;
		}
		if (location is CachedAssetBundleLocation)
		{
			string fileName2 = Path.GetFileName(location.InternalId);
			return Addressables.RuntimePath + "/" + fileName2;
		}
		return location.InternalId;
	}
}
