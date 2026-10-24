using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MFG.Assets;

internal class AddressablesAsset<AssetType> : IDisposableAsset<AssetType>, IDisposable where AssetType : UnityEngine.Object
{
	private bool m_disposed;

	public AssetType Asset { get; protected set; }

	public AddressablesAsset(AssetType asset)
	{
		Asset = asset;
	}

	~AddressablesAsset()
	{
	}

	public void Dispose()
	{
		if (!m_disposed && Asset != null)
		{
			Addressables.Release(Asset);
			Asset = null;
		}
	}
}
