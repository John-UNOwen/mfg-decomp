using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LoadableAssetSet<AssetType> : IDisposable where AssetType : UnityEngine.Object
{
	private Dictionary<string, LoadableAsset<AssetType>> _lodableAssets = new Dictionary<string, LoadableAsset<AssetType>>();

	private SemaphoreSlim _loadSemaphore = new SemaphoreSlim(1, 1);

	public async UniTask Load(List<string> assetAddresses, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		foreach (string assetAddress in assetAddresses)
		{
			await Get(assetAddress, cancellationToken);
		}
	}

	public async UniTask<AssetType> Get(string assetAddress, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (string.IsNullOrEmpty(assetAddress))
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		await _loadSemaphore.WaitAsync(cancellationToken);
		try
		{
			if (_lodableAssets.TryGetValue(assetAddress, out var value))
			{
				return value.Get();
			}
		}
		finally
		{
			_loadSemaphore.Release();
		}
		LoadableAsset<AssetType> loadableAsset = new LoadableAsset<AssetType>();
		await loadableAsset.Load(assetAddress, cancellationToken);
		await _loadSemaphore.WaitAsync(cancellationToken);
		try
		{
			_lodableAssets.TryAdd(assetAddress, loadableAsset);
			return loadableAsset.Get();
		}
		finally
		{
			_loadSemaphore.Release();
		}
	}

	public void Dispose()
	{
		foreach (LoadableAsset<AssetType> value in _lodableAssets.Values)
		{
			value.Dispose();
		}
		_lodableAssets.Clear();
	}
}
