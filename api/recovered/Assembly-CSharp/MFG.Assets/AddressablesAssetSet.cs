using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MFG.Assets;

public class AddressablesAssetSet<TKey, AssetType> : Dictionary<TKey, AssetType>, IDisposableAssetSet<TKey, AssetType>, IDictionary<TKey, AssetType>, ICollection<KeyValuePair<TKey, AssetType>>, IEnumerable<KeyValuePair<TKey, AssetType>>, IEnumerable, IDisposable where AssetType : UnityEngine.Object
{
	private bool m_disposed;

	public AddressablesAssetSet(Dictionary<TKey, AssetType> keyValuePairs)
		: base((IDictionary<TKey, AssetType>)keyValuePairs)
	{
	}

	~AddressablesAssetSet()
	{
	}

	public void Dispose()
	{
		if (m_disposed)
		{
			return;
		}
		foreach (AssetType value in base.Values)
		{
			Addressables.Release(value);
		}
		Clear();
		m_disposed = true;
	}
}
