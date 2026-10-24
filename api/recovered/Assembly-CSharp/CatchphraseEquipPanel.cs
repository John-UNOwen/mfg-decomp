using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Types.Customize.Catchphrase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CatchphraseEquipPanel : MonoBehaviour
{
	[SerializeField]
	private Button _panelButton;

	[SerializeField]
	private CatchphrasePlate _catchphrasePlate;

	[SerializeField]
	private GameObject _selected;

	private ItemID _itemID;

	private Subject<ItemID> _onSelected = new Subject<ItemID>();

	private bool _enableNoEquipPanelDisp;

	public IObservable<ItemID> OnSelected => _onSelected.AsObservable();

	private void Start()
	{
		_panelButton.OnClickAsObservableSafety().Subscribe(delegate
		{
			_onSelected.OnNext(_itemID);
		}).AddTo(this);
	}

	public async UniTask Reflesh(ItemID itemID, bool isSelected = false, bool enableNoEquipPanelDisp = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		_itemID = itemID;
		_enableNoEquipPanelDisp = enableNoEquipPanelDisp;
		SetSelected(isSelected);
		_catchphrasePlate.EnableNoEquipPanelDisp = _enableNoEquipPanelDisp;
		await _catchphrasePlate.Reflesh(_itemID, cancellationToken);
	}

	public void SetSelected(bool isSelected)
	{
		_selected.SetActive(isSelected);
	}
}
