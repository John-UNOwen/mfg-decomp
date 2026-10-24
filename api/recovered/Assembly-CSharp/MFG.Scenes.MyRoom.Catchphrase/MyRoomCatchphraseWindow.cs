using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Types;
using MFG.Types.Customize.Catchphrase;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MFG.Scenes.MyRoom.Catchphrase;

public class MyRoomCatchphraseWindow : MonoBehaviour
{
	private enum TabType
	{
		UnlockTab,
		EquipTab
	}

	[SerializeField]
	private List<GameObject> _tabButtons = new List<GameObject>();

	[SerializeField]
	private CatchphraseUnlockTab _unlockTab;

	[SerializeField]
	private CatchphraseEquipTab _equipTab;

	[SerializeField]
	private CatchphrasePlate _currentEquipPlate;

	private TabType _currentTab;

	private bool _isTabSelecting;

	private CharaType _currentCharaType;

	private Subject<ItemID> _onChange = new Subject<ItemID>();

	private MyRoomFlow myRoomFlow;

	[SerializeField]
	private Button m_closeButton;

	private bool _isOpen;

	public IObservable<ItemID> OnChange => _onChange.AsObservable();

	protected void Start()
	{
		CancellationToken cancellationToken = this.GetCancellationTokenOnDestroy();
		foreach (TabType tabType in Enum.GetValues(typeof(TabType)))
		{
			_tabButtons[(int)tabType].transform.Find("Text").GetComponent<TextMeshProUGUI>().color = MyRoomUtility.BtTextColor(tabType == TabType.EquipTab);
			_tabButtons[(int)tabType].GetComponent<Button>().OnClickAsObservable().Subscribe(async delegate
			{
				if (tabType == _currentTab || _isTabSelecting)
				{
					return;
				}
				try
				{
					_isTabSelecting = true;
					Tab_OpenClose(_currentTab, isOpenClose: false, isPlaySE: true, cancellationToken);
					_currentTab = tabType;
					Tab_OpenClose(_currentTab, isOpenClose: true, isPlaySE: true, cancellationToken);
					await UniTask.Delay(TimeSpan.FromSeconds(0.5), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
				}
				catch (OperationCanceledException)
				{
				}
				finally
				{
					_isTabSelecting = false;
				}
			})
				.AddTo(this);
		}
		m_closeButton.OnClickAsObservable().Subscribe(delegate
		{
			Close();
		}).AddTo(this);
		_equipTab.OnChange.Subscribe(delegate(ItemID itemID)
		{
			try
			{
				_currentEquipPlate.Reflesh(itemID, cancellationToken).Forget();
				_onChange.OnNext(itemID);
			}
			catch (OperationCanceledException)
			{
			}
		}).AddTo(this);
	}

	public void Close()
	{
		if (!_isOpen)
		{
			return;
		}
		SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
		foreach (TabType value in Enum.GetValues(typeof(TabType)))
		{
			Tab_OpenClose(value, isOpenClose: false, isPlaySE: false);
		}
		_isOpen = false;
	}

	public async UniTask Open(MyRoomFlow mrf, CharaType charaType, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (!_isOpen)
		{
			myRoomFlow = mrf;
			Debug.Log($"CatchphraseWindow.Open charaType:{charaType}");
			_isOpen = true;
			_currentCharaType = charaType;
			_currentTab = TabType.EquipTab;
			ItemID catchphraseItemEquip = GameDataManager.Player.CharaData[(int)_currentCharaType].CatchphraseItemEquip;
			_currentEquipPlate.EnableNoEquipPanelDisp = true;
			_currentEquipPlate.Reflesh(catchphraseItemEquip, cancellationToken).Forget();
			Tab_OpenClose(_currentTab, isOpenClose: true, isPlaySE: false, cancellationToken);
			await UniTask.WaitWhile(() => _isOpen, PlayerLoopTiming.Update, cancellationToken);
		}
	}

	private void Tab_OpenClose(TabType tabType, bool isOpenClose, bool isPlaySE = true, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (isPlaySE && isOpenClose)
		{
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
		}
		switch (tabType)
		{
		case TabType.UnlockTab:
			if (isOpenClose)
			{
				_tabButtons[(int)tabType].GetComponent<Animator>().SetTrigger("Selected");
				_tabButtons[(int)tabType].transform.Find("Text").GetComponent<TextMeshProUGUI>().color = MyRoomUtility.BtTextColor(isActive: true);
				_unlockTab.gameObject.SetActive(value: true);
				_unlockTab.Open(myRoomFlow, _currentCharaType, cancellationToken).Forget();
			}
			else
			{
				_tabButtons[(int)tabType].GetComponent<Animator>().SetTrigger("Normal");
				_tabButtons[(int)tabType].transform.Find("Text").GetComponent<TextMeshProUGUI>().color = MyRoomUtility.BtTextColor(isActive: false);
				_unlockTab.Close();
				_unlockTab.gameObject.SetActive(value: false);
			}
			break;
		case TabType.EquipTab:
			if (isOpenClose)
			{
				_tabButtons[(int)tabType].GetComponent<Animator>().SetTrigger("Selected");
				_tabButtons[(int)tabType].transform.Find("Text").GetComponent<TextMeshProUGUI>().color = MyRoomUtility.BtTextColor(isActive: true);
				_equipTab.gameObject.SetActive(value: true);
				_equipTab.Open(myRoomFlow, _currentCharaType, cancellationToken).Forget();
			}
			else
			{
				_tabButtons[(int)tabType].GetComponent<Animator>().SetTrigger("Normal");
				_tabButtons[(int)tabType].transform.Find("Text").GetComponent<TextMeshProUGUI>().color = MyRoomUtility.BtTextColor(isActive: false);
				_equipTab.Close();
				_equipTab.gameObject.SetActive(value: false);
			}
			break;
		}
	}
}
