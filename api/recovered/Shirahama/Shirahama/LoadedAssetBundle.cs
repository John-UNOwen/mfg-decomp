using System;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Shirahama;

public class LoadedAssetBundle : IAssetBundleResource, IDisposable
{
	private AssetBundle _assetBundle;

	private readonly string _bundleName;

	public string BundleName => _bundleName;

	public LoadedAssetBundle(AssetBundle assetBundle, string bundleName)
	{
		_assetBundle = assetBundle;
		_bundleName = bundleName;
	}

	public void Dispose()
	{
		if (_assetBundle != null)
		{
			_assetBundle.Unload(unloadAllLoadedObjects: false);
			_assetBundle = null;
		}
	}

	public AssetBundle GetAssetBundle()
	{
		return _assetBundle;
	}
}
