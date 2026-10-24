using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Types.Customize.CustomizeItem;
using UnityEngine;
using UnityEngine.UI;

public class StampBalloon : MonoBehaviour, IDisposable
{
	[SerializeField]
	private Image _iconImage;

	private IDisposableAsset<Sprite> _iconSprite;

	private ItemID _itemID;

	private Vector2 _initialPos;

	private void Awake()
	{
		_initialPos = GetComponent<RectTransform>().anchoredPosition;
	}

	private void OnDestroy()
	{
		Dispose();
	}

	public void Dispose()
	{
		if (_iconSprite != null)
		{
			_iconSprite.Dispose();
			_iconSprite = null;
		}
	}

	public async UniTask Play(ItemID itemID, Vector2 offsetPos = default(Vector2), CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		RectTransform component = GetComponent<RectTransform>();
		component.localScale = Vector3.zero;
		component.anchoredPosition = _initialPos + offsetPos;
		await RefleshIconGraphic(itemID, cancellationToken);
		cancellationToken.ThrowIfCancellationRequested();
		SoundManager.PlaySe("SFX_SYS_INPUT3").Forget();
		GetComponent<RectTransform>().localScale = Vector3.zero;
		await GetComponent<RectTransform>().DOScale(new Vector3(1f, 1f, 0f), 0.3f).SetEase(Ease.OutBack, 1.2f).ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);
	}

	private async UniTask RefleshIconGraphic(ItemID itemID, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (_itemID != itemID)
		{
			_iconSprite?.Dispose();
			_iconSprite = await AssetLoader.LoadAssetAsync<Sprite>(CustomizeItemMaster.GetItem(itemID).MahjongPlayDataPath, null, cancellationToken);
			_iconImage.sprite = _iconSprite.Asset;
			_itemID = itemID;
		}
	}
}
