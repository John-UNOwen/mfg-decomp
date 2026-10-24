using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.Types;
using UnityEngine;
using UnityEngine.UI;

public class SchoolEmblemIcon : MonoBehaviour, IDisposable
{
	[SerializeField]
	private Image _baseImage;

	private SchoolID _schoolID;

	private SemaphoreSlim _loadSemaphore = new SemaphoreSlim(1, 1);

	private static LoadableAssetSet<Sprite> _baseImageAssetSet = new LoadableAssetSet<Sprite>();

	public void Dispose()
	{
	}

	private void OnDestroy()
	{
		Dispose();
	}

	public async UniTask Reflesh(SchoolID schoolID, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (_schoolID == schoolID)
		{
			return;
		}
		await _loadSemaphore.WaitAsync();
		try
		{
			if (_baseImage != null && schoolID != 0)
			{
				await RefleshBaseImage(schoolID, cancellationToken);
			}
			if (_baseImage != null)
			{
				_baseImage.enabled = schoolID != SchoolID.None;
			}
			_schoolID = schoolID;
		}
		finally
		{
			_loadSemaphore.Release();
		}
	}

	private async UniTask RefleshBaseImage(SchoolID schoolID, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (_schoolID != schoolID)
		{
			_schoolID = schoolID;
			if (_baseImage != null)
			{
				Image baseImage = _baseImage;
				baseImage.sprite = await BaseImage(schoolID, cancellationToken);
			}
		}
	}

	public static async UniTask ResidentLoad()
	{
		List<string> assetAddresses = (from x in Enum.GetValues(typeof(SchoolID)).OfType<SchoolID>()
			where x != SchoolID.None
			select AssetPathResolver.SchoolEmblemIcon.ResolveBaseImage(x)).ToList();
		await _baseImageAssetSet.Load(assetAddresses);
	}

	public async UniTask<Sprite> BaseImage(SchoolID schoolID, CancellationToken cancellationToken = default(CancellationToken))
	{
		return await _baseImageAssetSet.Get(AssetPathResolver.SchoolEmblemIcon.ResolveBaseImage(schoolID), cancellationToken);
	}
}
