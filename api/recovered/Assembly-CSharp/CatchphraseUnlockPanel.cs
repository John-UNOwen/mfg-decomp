using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.MasterGameData;
using MFG.Types.Customize.Catchphrase;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CatchphraseUnlockPanel : MonoBehaviour
{
	public enum PanelState
	{
		None,
		NotDebut,
		LackOfFightSpirit,
		RequiredItemNotUnlock,
		Unlocked
	}

	[SerializeField]
	private Button _unlockButton;

	[SerializeField]
	private CatchphrasePlate _catchphrasePlate;

	[SerializeField]
	private TextMeshProUGUI _unlockConditionText;

	[SerializeField]
	private GameObject _inactivePanel;

	private PanelState _panelState;

	private ItemID _itemID;

	private Subject<ItemID> _onUnlocked = new Subject<ItemID>();

	public IObservable<ItemID> OnUnlocked => _onUnlocked.AsObservable();

	private void Start()
	{
		_unlockButton.OnClickAsObservableSafety().Subscribe(delegate
		{
			_onUnlocked.OnNext(_itemID);
		}).AddTo(this);
	}

	public async UniTask Reflesh(ItemID itemID, PanelState panelState = PanelState.None, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		_itemID = itemID;
		_panelState = panelState;
		Item item = CatchphraseItemMaster.GetItem(_itemID);
		PanelState_Reflesh(panelState);
		await _catchphrasePlate.Reflesh(_itemID, cancellationToken);
		_unlockConditionText.text = item.UnlockConditionText;
	}

	private void PanelState_Reflesh(PanelState panelState)
	{
		Plate_SetActive(panelState != PanelState.Unlocked, panelState == PanelState.None);
		TextMeshProUGUI componentInChildren = _unlockButton.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
		if (componentInChildren != null)
		{
			TextMeshProUGUI textMeshProUGUI = componentInChildren;
			textMeshProUGUI.text = panelState switch
			{
				PanelState.Unlocked => "解禁済み", 
				PanelState.RequiredItemNotUnlock => "条件未達成", 
				PanelState.LackOfFightSpirit => "ファイト\nスピリット不足", 
				PanelState.NotDebut => "デビュー後\n解禁可能", 
				_ => "解禁", 
			};
		}
	}

	private void Plate_SetActive(bool isBaseActive, bool isButtonActive)
	{
		_unlockButton.interactable = isButtonActive;
		_inactivePanel.SetActive(!isBaseActive);
	}
}
