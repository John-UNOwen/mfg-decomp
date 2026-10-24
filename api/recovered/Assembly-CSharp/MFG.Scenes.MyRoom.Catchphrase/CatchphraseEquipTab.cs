using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.Types;
using MFG.Types.Customize.Catchphrase;
using UniRx;
using UnityEngine;

namespace MFG.Scenes.MyRoom.Catchphrase;

public class CatchphraseEquipTab : MonoBehaviour
{
	[SerializeField]
	private ScrollView _scrollView;

	[SerializeField]
	private GameObject _panelPrefab;

	private CharaType _currentCharaType;

	private Subject<ItemID> _onChange = new Subject<ItemID>();

	private bool _isOpen;

	private bool _isSelecting;

	private MyRoomFlow myRoomFlow;

	public IObservable<ItemID> OnChange => _onChange.AsObservable();

	public async UniTask Open(MyRoomFlow mrf, CharaType charaType, CancellationToken cancellationToken = default(CancellationToken))
	{
		myRoomFlow = mrf;
		cancellationToken.ThrowIfCancellationRequested();
		_isOpen = true;
		_currentCharaType = charaType;
		ListView_Create(cancellationToken);
		await UniTask.WaitWhile(() => _isOpen, PlayerLoopTiming.Update, cancellationToken);
	}

	public void Close()
	{
		if (_isOpen)
		{
			_isOpen = false;
		}
	}

	private void ListView_Create(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		_scrollView.ResetList();
		IEnumerable<ItemID> enumerable = from a in GameDataManager.Customize.CatchphraseItems
			where a.Master.Available && (a.Master.CategoryType == Category.Common || a.Master.CharaType == _currentCharaType)
			orderby a.Master.Sort descending
			orderby a.ItemID == ItemID.Catchphrase_00_000 descending, a.Master.CategoryType descending, a.ItemID
			select a.ItemID;
		_scrollView.ResetScroll();
		ItemID catchphraseItemEquip = GameDataManager.Player.CharaData[(int)_currentCharaType].CatchphraseItemEquip;
		foreach (ItemID item in enumerable)
		{
			ListView_CreatePanel(item, catchphraseItemEquip, cancellationToken).Forget();
		}
	}

	private async UniTask ListView_CreatePanel(ItemID itemID, ItemID currentEquipItemID, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		GameObject panelInstance = _scrollView.Add(_panelPrefab, (int)itemID);
		panelInstance.SetActive(value: false);
		CatchphraseEquipPanel equipPanel = panelInstance.GetComponentInChildren<CatchphraseEquipPanel>(includeInactive: true);
		await equipPanel.Reflesh(itemID, itemID == currentEquipItemID, enableNoEquipPanelDisp: true, cancellationToken);
		panelInstance.SetActive(value: true);
		equipPanel.OnSelected.Subscribe(async delegate
		{
			if (!_isSelecting)
			{
				ItemID catchphraseItemEquip = GameDataManager.Player.CharaData[(int)_currentCharaType].CatchphraseItemEquip;
				if (catchphraseItemEquip != itemID)
				{
					try
					{
						_isSelecting = true;
						SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
						GameDataManager.Player.CharaData[(int)_currentCharaType].CatchphraseItemEquip = itemID;
						_onChange.OnNext(itemID);
						try
						{
							_scrollView.GetObject((int)catchphraseItemEquip).GetComponent<CatchphraseEquipPanel>().SetSelected(isSelected: false);
						}
						catch
						{
						}
						equipPanel.SetSelected(isSelected: true);
						myRoomFlow.SetCatchphrase(itemID, cancellationToken);
						await UniTask.Delay(TimeSpan.FromSeconds(0.5), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
					}
					catch (OperationCanceledException)
					{
					}
					finally
					{
						_isSelecting = false;
					}
				}
			}
		}).AddTo(panelInstance);
	}
}
