using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MFG.Assets;

public interface IAssetLoader
{
	UniTask<IDisposableAsset<AssetType>> LoadAssetAsync<AssetType>(string path, IProgress<float> progress = null, CancellationToken cancellationToken = default(CancellationToken)) where AssetType : UnityEngine.Object;

	UniTask<IDisposableAssetSet<TKey, AssetType>> LoadAssetAsync<TKey, AssetType>(Dictionary<TKey, string> requestAssets, IProgress<float> progress = null, CancellationToken cancellationToken = default(CancellationToken)) where AssetType : UnityEngine.Object;
}
