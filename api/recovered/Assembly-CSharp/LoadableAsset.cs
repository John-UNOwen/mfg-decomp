using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using UnityEngine;

public class LoadableAsset<AssetType> : ILoadableAsset, IDisposable where AssetType : UnityEngine.Object
{
	private IDisposableAsset<AssetType> _asset;

	public bool IsLoadCalled { get; private set; }

	public bool IsLoading { get; private set; }

	public bool IsLoadCompleted
	{
		get
		{
			if (IsLoadCalled)
			{
				return !IsLoading;
			}
			return false;
		}
	}

	public async UniTask Load(string assetAdress, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (IsLoadCalled)
		{
			if (IsLoading)
			{
				await UniTask.WaitWhile(() => IsLoading, PlayerLoopTiming.Update, cancellationToken);
			}
			return;
		}
		try
		{
			IsLoadCalled = true;
			IsLoading = true;
			_asset = await AssetLoader.LoadAssetAsync<AssetType>(assetAdress, null, cancellationToken);
			IsLoading = false;
		}
		catch (OperationCanceledException)
		{
			IsLoadCalled = false;
			IsLoading = false;
			throw;
		}
	}

	public async UniTask<AssetType> Get(string assetAddress, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (IsLoadCompleted)
		{
			return Get();
		}
		while (_asset == null)
		{
			await Load(assetAddress, cancellationToken);
		}
		return _asset.Asset;
	}

	public AssetType Get()
	{
		return _asset.Asset;
	}

	public void Dispose()
	{
		_asset.Dispose();
	}
}
