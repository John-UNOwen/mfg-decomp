using System.IO;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Shirahama;

public class DeliveryResolver : IRuntimeResolver
{
	private const string _contentsServerKey = "$CONTENTS_SERVER$";

	private ILogger _logger;

	public static string CacheCatalogPrimaryKey => "AddressablesMainContentCatalogCacheHash";

	public string HostServer { get; }

	public string CatalogCachePath { get; }

	public string AssetBundleCachePath { get; }

	public DeliveryResolver(string host, ILogger logger = null)
	{
		HostServer = host;
		CatalogCachePath = (InternalStorageResolver.ContentStoragePath + "/contents/catalog").Replace('\\', '/');
		AssetBundleCachePath = (InternalStorageResolver.ContentStoragePath + "/contents/data").Replace('\\', '/');
		_logger = logger;
	}

	public string Resolve(IResourceLocation location)
	{
		if (location.PrimaryKey == CacheCatalogPrimaryKey)
		{
			string fileName = Path.GetFileName(location.InternalId);
			string text = CatalogCachePath + "/" + fileName;
			_logger?.Log("[DeliveryResolver] convert cached catalog path " + location.InternalId + " to " + text);
			return text;
		}
		if (location is BundleHashCacheLocation)
		{
			string text2 = CatalogCachePath + "/" + location.InternalId;
			_logger?.Log("[DeliveryResolver] convert cached bundle hash table path(" + text2 + ")");
			return text2;
		}
		if (location is CachedAssetBundleLocation)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(location.InternalId);
			string text3 = AssetBundleCachePath + "/" + fileNameWithoutExtension;
			_logger?.Log("[DeliveryResolver] convert remote bundle path " + location.InternalId + " to " + text3);
			return text3;
		}
		string text4 = location.InternalId.Replace("$CONTENTS_SERVER$", HostServer);
		if (!text4.StartsWith("http"))
		{
			return location.InternalId;
		}
		_logger?.Log("[DeliveryResolver] convert " + location.InternalId + " to " + text4);
		return text4;
	}
}
