using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.MasterGameData;
using MFG.Types.Customize.Catchphrase;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CatchphrasePlate : MonoBehaviour, IDisposable
{
	[SerializeField]
	private Image _baseImage;

	[SerializeField]
	private TextMeshProUGUI _catchphraseText;

	[SerializeField]
	private TextMeshProUGUI _inactiveText;

	private ItemID _itemID;

	private SemaphoreSlim _loadSemaphore = new SemaphoreSlim(1, 1);

	public bool EnableNoEquipPanelDisp;

	public void Dispose()
	{
	}

	private void OnDestroy()
	{
		Dispose();
	}

	public async UniTask Reflesh(ItemID itemID, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (itemID == ItemID.None)
		{
			itemID = CatchphraseItemMaster.GetDefaultEquipItemID();
		}
		if (_itemID == itemID)
		{
			return;
		}
		await _loadSemaphore.WaitAsync();
		try
		{
			bool isDispImage = itemID != ItemID.Catchphrase_00_000 || EnableNoEquipPanelDisp;
			bool isDispCatchphraseText = itemID != ItemID.Catchphrase_00_000 && isDispImage;
			bool isDispInactiveText = itemID == ItemID.Catchphrase_00_000 && isDispImage;
			if (_baseImage != null)
			{
				await RefleshBaseImage(itemID, cancellationToken);
				_baseImage.enabled = isDispImage;
			}
			if (_catchphraseText != null)
			{
				Item item = CatchphraseItemMaster.GetItem(itemID);
				_catchphraseText.text = item.DispName;
				_catchphraseText.enabled = isDispCatchphraseText;
			}
			if (_inactiveText != null)
			{
				_inactiveText.enabled = isDispInactiveText;
			}
			_itemID = itemID;
		}
		finally
		{
			_loadSemaphore.Release();
		}
	}

	private async UniTask RefleshBaseImage(ItemID itemID, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (_itemID != itemID)
		{
			_itemID = itemID;
			_ = CatchphraseItemMaster.GetItem(_itemID).BaseImagePath;
			Image baseImage = _baseImage;
			baseImage.sprite = await CatchphraseItemMaster.GetItem(_itemID).BaseImage(cancellationToken);
		}
	}
}
