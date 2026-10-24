using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using UnityEngine;

namespace MFG.MahjongPlay.Cutin;

public class CutinAssetContainer : IDisposable
{
	public GameObject Instance;

	private IDisposableAsset<GameObject> _subCutinPrefab;

	private IPlayerInteractor _playerInteractor;

	private CutinEventNotifier _eventNotifier;

	public async UniTask LoadAndInstantiate(string assetPath, IPlayerInteractor plyaerInteractor, CutinEventNotifier targetEventNotifier = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		_playerInteractor = plyaerInteractor;
		_subCutinPrefab = await AssetLoader.LoadAssetAsync<GameObject>(assetPath, null, cancellationToken);
		Instance = UnityEngine.Object.Instantiate(_subCutinPrefab.Asset, plyaerInteractor.CutinParent().transform);
		Instance.transform.SetAsLastSibling();
		_eventNotifier = targetEventNotifier;
		if (_eventNotifier != null)
		{
			_eventNotifier.m_sourcePlayerInteractor = plyaerInteractor;
			_eventNotifier.m_subCutinParent = Instance;
		}
	}

	public void Dispose()
	{
		if ((bool)Instance)
		{
			UnityEngine.Object.Destroy(Instance);
		}
		_subCutinPrefab?.Dispose();
	}
}
