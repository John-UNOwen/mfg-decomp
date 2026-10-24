using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MFG.Assets;

public class AddressablesAssetLoader : IAssetLoader
{
	public async UniTask<IDisposableAsset<AssetType>> LoadAssetAsync<AssetType>(string path, IProgress<float> progress = null, CancellationToken cancellationToken = default(CancellationToken)) where AssetType : UnityEngine.Object
	{
		cancellationToken.ThrowIfCancellationRequested();
		AsyncOperationHandle<AssetType> handle = Addressables.LoadAssetAsync<AssetType>(path);
		await handle.ToUniTask(progress, PlayerLoopTiming.Update, cancellationToken);
		return new AddressablesAsset<AssetType>(handle.Result);
	}

	public async UniTask<IDisposableAssetSet<TKey, AssetType>> LoadAssetAsync<TKey, AssetType>(Dictionary<TKey, string> requestAssets, IProgress<float> progress = null, CancellationToken cancellationToken = default(CancellationToken)) where AssetType : UnityEngine.Object
	{
		cancellationToken.ThrowIfCancellationRequested();
		Dictionary<TKey, AssetType> assets = new Dictionary<TKey, AssetType>();
		foreach (KeyValuePair<TKey, string> request in requestAssets)
		{
			AssetType value = await Addressables.LoadAssetAsync<AssetType>(request.Value);
			assets.Add(request.Key, value);
		}
		return new AddressablesAssetSet<TKey, AssetType>(assets);
	}
}
