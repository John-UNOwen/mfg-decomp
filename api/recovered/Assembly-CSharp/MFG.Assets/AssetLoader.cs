using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MFG.Assets;

public class AssetLoader
{
	[SerializeField]
	[HideInInspector]
	private static IAssetLoader s_loader = new AddressablesAssetLoader();

	public static async UniTask<IDisposableAsset<T>> LoadAssetAsync<T>(string path, IProgress<float> progress = null, CancellationToken cancellationToken = default(CancellationToken)) where T : UnityEngine.Object
	{
		cancellationToken.ThrowIfCancellationRequested();
		return await s_loader.LoadAssetAsync<T>(path, progress, cancellationToken);
	}

	public static async UniTask<IDisposableAssetSet<TKey, TValue>> LoadAssetAsync<TKey, TValue>(Dictionary<TKey, string> requestAssets, IProgress<float> progress = null, CancellationToken cancellationToken = default(CancellationToken)) where TValue : UnityEngine.Object
	{
		cancellationToken.ThrowIfCancellationRequested();
		return await s_loader.LoadAssetAsync<TKey, TValue>(requestAssets, progress, cancellationToken);
	}

	public static async UniTask<bool> Exists(string pathToAsset)
	{
		return (await Addressables.LoadResourceLocationsAsync(pathToAsset)).Any();
	}
}
