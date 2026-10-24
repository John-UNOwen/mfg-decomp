using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Types.Customize.CutinItem;
using UnityEngine;

public class CutinAssetCache : IUniTaskAsyncDisposable
{
	private SemaphoreSlim _loadSemaphore = new SemaphoreSlim(1, 1);

	private bool m_isLoading;

	private bool m_isInstantiating;

	private Dictionary<CutinAssetKey, IDisposableAsset<Object>> m_prefabCaches = new Dictionary<CutinAssetKey, IDisposableAsset<Object>>();

	private Dictionary<CutinAssetKey, GameObject> m_instanceCaches = new Dictionary<CutinAssetKey, GameObject>();

	public bool IsBusy
	{
		get
		{
			if (!m_isLoading)
			{
				return m_isInstantiating;
			}
			return true;
		}
	}

	public async UniTask DisposeAsync()
	{
		await UnLoadAll();
	}

	private async UniTask Load(CutinAssetKey assetKey, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!IsLoaded(assetKey, cancellationToken))
		{
			m_isLoading = true;
			Item item = CutinItemMaster.GetItems((Item x) => x.ObjectID == assetKey.ObjectID)[0];
			IDisposableAsset<Object> value = await AssetLoader.LoadAssetAsync<Object>(AssetPathResolver.Resolve(PathType.CutinItem, item.CharaType, assetKey.CutinAssetType, item));
			m_prefabCaches[assetKey] = value;
			m_isLoading = false;
		}
	}

	public async UniTask PreLoad(CutinAssetKey assetKey, Transform instanceRoot, bool playOnActive = true, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		Debug.Log($"CutinPreLoad: {assetKey.ObjectID}, {assetKey.CutinAssetType}");
		CutinMovie component = (await GetInstance(assetKey, instanceRoot, playOnActive: true, cancellationToken)).GetComponent<CutinMovie>();
		if (component != null)
		{
			component.m_playOnActive = playOnActive;
		}
	}

	private bool IsLoaded(CutinAssetKey assetKey, CancellationToken cancellationToken = default(CancellationToken))
	{
		return m_prefabCaches.ContainsKey(assetKey);
	}

	private bool IsInstantiated(CutinAssetKey assetKey, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		return m_instanceCaches.ContainsKey(assetKey);
	}

	public async UniTask UnLoad(CutinAssetKey assetKey, CancellationToken cancellationToken = default(CancellationToken))
	{
		await _loadSemaphore.WaitAsync(cancellationToken);
		try
		{
			if (IsInstantiated(assetKey, cancellationToken))
			{
				Object.Destroy(m_instanceCaches[assetKey]);
				m_instanceCaches.Remove(assetKey);
			}
			if (IsLoaded(assetKey, cancellationToken))
			{
				m_prefabCaches[assetKey].Dispose();
				m_prefabCaches.Remove(assetKey);
			}
		}
		finally
		{
			_loadSemaphore.Release();
		}
	}

	public async UniTask UnLoadAll()
	{
		List<CutinAssetKey> list = m_prefabCaches.Keys.ToList();
		foreach (CutinAssetKey item in list)
		{
			await UnLoad(item);
		}
	}

	private async UniTask<Object> GetPrefab(CutinAssetKey assetKey, CancellationToken cancellationToken = default(CancellationToken))
	{
		await Load(assetKey, cancellationToken);
		return m_prefabCaches[assetKey].Asset;
	}

	public async UniTask<GameObject> GetInstance(CutinAssetKey assetKey, Transform instanceRoot, bool playOnActive = true, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await _loadSemaphore.WaitAsync(cancellationToken);
		try
		{
			if (!IsInstantiated(assetKey, cancellationToken))
			{
				m_isInstantiating = true;
				GameObject gameObject = (GameObject)Object.Instantiate(await GetPrefab(assetKey, cancellationToken), instanceRoot.transform, instantiateInWorldSpace: false);
				gameObject.name = $"{assetKey.ObjectID}_{assetKey.CutinAssetType}";
				m_instanceCaches[assetKey] = gameObject;
				m_isInstantiating = false;
			}
			GameObject gameObject2 = m_instanceCaches[assetKey];
			gameObject2.transform.SetParent(instanceRoot.transform, worldPositionStays: false);
			return gameObject2;
		}
		finally
		{
			_loadSemaphore.Release();
		}
	}
}
