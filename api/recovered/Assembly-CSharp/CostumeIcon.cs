using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Types.Customize.Costume;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CostumeIcon : MonoBehaviour, IDisposable
{
	[SerializeField]
	private Image _costumeImage;

	[SerializeField]
	private TextMeshProUGUI _catchphraseText;

	private ItemID _itemID;

	private IDisposableAsset<Sprite> _costumeImageSprite;

	private SemaphoreSlim _loadSemaphore = new SemaphoreSlim(1, 1);

	public void Dispose()
	{
		if (_costumeImageSprite != null)
		{
			_costumeImageSprite.Dispose();
			_costumeImageSprite = null;
		}
	}

	private void OnDestroy()
	{
		Dispose();
	}

	public async UniTask Reflesh(ItemID itemID, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (itemID == ItemID.None || _itemID == itemID)
		{
			return;
		}
		await _loadSemaphore.WaitAsync();
		try
		{
			Item item = CostumeItemMaster.GetItem(itemID);
			await UniTask.WhenAll(UniTask.Create(async delegate
			{
				if (_costumeImage != null)
				{
					_costumeImage.SetAlpha(0f);
					await RefleshCostumeImage(itemID, cancellationToken);
				}
			}), UniTask.Create(async delegate
			{
				if (_catchphraseText != null)
				{
					_catchphraseText.SetAlpha(0f);
					_catchphraseText.text = item.Name;
				}
			}));
			await UniTask.WhenAll(UniTask.Create(async delegate
			{
				if (_costumeImage != null)
				{
					await _costumeImage.DOFade(1f, 0.05f).ToUniTask(TweenCancelBehaviour.Kill, cancellationToken);
				}
			}), UniTask.Create(async delegate
			{
				if (_catchphraseText != null)
				{
					await _catchphraseText.DOFade(1f, 0.05f).ToUniTask(TweenCancelBehaviour.Kill, cancellationToken);
				}
			}));
			_itemID = itemID;
		}
		finally
		{
			_loadSemaphore.Release();
		}
	}

	private async UniTask RefleshCostumeImage(ItemID itemID, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (_itemID != itemID)
		{
			_itemID = itemID;
			_costumeImageSprite?.Dispose();
			_costumeImageSprite = await AssetLoader.LoadAssetAsync<Sprite>(CostumeItemMaster.GetItem(_itemID).IconGraphicPath, null, cancellationToken);
			_costumeImage.sprite = _costumeImageSprite.Asset;
		}
	}
}
