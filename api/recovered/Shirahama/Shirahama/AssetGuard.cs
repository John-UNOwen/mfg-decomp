using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Shirahama;

public class AssetGuard<T> : IDisposable
{
	private AsyncOperationHandle<T>? _handle;

	public T Asset
	{
		get
		{
			if (!_handle.HasValue)
			{
				return default(T);
			}
			return _handle.Value.Result;
		}
	}

	public static explicit operator bool(AssetGuard<T> assetGuard)
	{
		if (assetGuard == null)
		{
			return false;
		}
		return assetGuard._handle?.Status == AsyncOperationStatus.Succeeded;
	}

	public static explicit operator T(AssetGuard<T> assetGuard)
	{
		if (assetGuard == null)
		{
			return default(T);
		}
		return assetGuard.Asset;
	}

	public AssetGuard(AsyncOperationHandle<T>? asset)
	{
		_handle = asset;
	}

	public void Dispose()
	{
		if (_handle.HasValue)
		{
			Addressables.Release(_handle.Value);
			_handle = null;
		}
	}
}
