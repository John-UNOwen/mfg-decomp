using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.Catchphrase;
using TMPro;
using UniRx;
using UnityEngine;

namespace MFG.Scenes.MyRoom.Catchphrase;

public class CatchphraseUnlockTab : MonoBehaviour
{
	[SerializeField]
	private ScrollView _scrollView;

	[SerializeField]
	private TextMeshProUGUI _charaFightSpiritCout;

	[SerializeField]
	private TextMeshProUGUI _wildFightSpiritCout;

	[SerializeField]
	private FightSpiritNumDisp _fightSpiritNumDisp;

	[SerializeField]
	private FightSpiritConsumeWindow _fightSpiritConsumeWindow;

	[SerializeField]
	private FightSpiritConsumeResultWindow _fightSpiritConsumeResultWindow;

	[SerializeField]
	private GameObject _panelPrefab;

	private MyRoomFlow myRoomFlow;

	private CharaType _currentCharaType;

	private bool _isOpen;

	private void Start()
	{
		_fightSpiritConsumeWindow.OnConsume.Subscribe(delegate((ItemID itemID, CharaType charaType, int consumeCharaSpiritCount, int consumeWildSpiritCount) consumeResult)
		{
			FightSpiritUtility.ConsumeSpirit(consumeResult.charaType, consumeResult.consumeCharaSpiritCount, consumeResult.consumeWildSpiritCount, ItemConsumeType.Catchphrase);
			GameDataManager.Customize.CatchphraseItems.Add(new CatchphraseItemInfo
			{
				ItemID = consumeResult.itemID,
				IsNew = true
			});
			_fightSpiritConsumeWindow.Close(isPlaySE: false);
		}).AddTo(this);
		_fightSpiritConsumeWindow.OnCancel.Subscribe(delegate
		{
			_fightSpiritConsumeWindow.Close();
		}).AddTo(this);
	}

	public async UniTask Open(MyRoomFlow mrf, CharaType charaType, CancellationToken cancellationToken)
	{
		myRoomFlow = mrf;
		cancellationToken.ThrowIfCancellationRequested();
		_isOpen = true;
		_currentCharaType = charaType;
		_fightSpiritNumDisp.Reflesh(cancellationToken).Forget();
		myRoomFlow.SetFightSpirit(cancellationToken);
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
		IOrderedEnumerable<(ItemID itemID, CatchphraseUnlockPanel.PanelState panelState)> orderedEnumerable = from a in CatchphraseItemMaster.GetItemAll()
			where a.Available && a.CharaType == _currentCharaType && a.RequiredFightSpiritNum > 0
			select (itemID: a.ItemID, panelState: EvaluatePanelState(a)) into a
			orderby a.panelState, a.itemID
			select a;
		_scrollView.ResetScroll();
		foreach (var (itemID, panelState) in orderedEnumerable)
		{
			ListView_CreatePanel(itemID, panelState, cancellationToken).Forget();
		}
	}

	private async UniTask ListView_CreatePanel(ItemID itemID, CatchphraseUnlockPanel.PanelState panelState, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		GameObject panelInstance = _scrollView.Add(_panelPrefab);
		CanvasGroup canvasGroup = panelInstance.GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;
		canvasGroup.blocksRaycasts = false;
		CatchphraseUnlockPanel unlockPanel = panelInstance.GetComponentInChildren<CatchphraseUnlockPanel>(includeInactive: true);
		await unlockPanel.Reflesh(itemID, panelState, cancellationToken);
		canvasGroup.alpha = 1f;
		canvasGroup.blocksRaycasts = true;
		Item item = CatchphraseItemMaster.GetItem(itemID);
		unlockPanel.OnUnlocked.Subscribe(async delegate
		{
			_ = 2;
			try
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (item.CategoryType == Category.Chara)
				{
					CharaType charaType = item.CharaType;
					var (consumeCharaSpiritCount, consumeWildSpiritCount) = FightSpiritUtility.CalculateConsumeSpiritCount(charaType, item.RequiredFightSpiritNum);
					await _fightSpiritConsumeWindow.Open(itemID, charaType, consumeCharaSpiritCount, consumeWildSpiritCount, isPlaySE: true, cancellationToken);
					if (_fightSpiritConsumeWindow.GetResult() == FightSpiritConsumeWindow.ResultType.Consumed)
					{
						ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
						_fightSpiritNumDisp.Reflesh(cancellationToken).Forget();
						myRoomFlow.SetFightSpirit(cancellationToken);
						ListView_Create(cancellationToken);
						await _fightSpiritConsumeResultWindow.Open(itemID, cancellationToken);
						ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
					}
				}
				else
				{
					ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
					_fightSpiritNumDisp.Reflesh(cancellationToken).Forget();
					myRoomFlow.SetFightSpirit(cancellationToken);
					ListView_Create(cancellationToken);
					await _fightSpiritConsumeResultWindow.Open(itemID, cancellationToken);
					ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
				}
			}
			catch (OperationCanceledException)
			{
				Debug.Log("CatchphraseUnlockTab.ListView_CreatePanel OperationCanceledException");
			}
		}).AddTo(panelInstance);
	}

	private bool HasItem(ItemID itemID)
	{
		return GameDataManager.Customize.CatchphraseItems.Find((CatchphraseItemInfo x) => x.ItemID == itemID) != null;
	}

	private bool HasRequiredItem(Item item)
	{
		if (item.RequiredItemID != 0)
		{
			return HasItem(item.RequiredItemID);
		}
		return true;
	}

	private bool IsDebut(Item item)
	{
		if (item.CategoryType == Category.Chara)
		{
			return GameDataManager.Player.CharaData[(int)item.CharaType].IsDebut;
		}
		return true;
	}

	private bool HasEnoughFightSpirit(Item item)
	{
		if (item.RequiredFightSpiritNum <= 0)
		{
			return true;
		}
		return FightSpiritUtility.HasEnoughSpirit(item.CharaType, item.RequiredFightSpiritNum);
	}

	private CatchphraseUnlockPanel.PanelState EvaluatePanelState(Item item)
	{
		CatchphraseUnlockPanel.PanelState result = CatchphraseUnlockPanel.PanelState.None;
		bool num = HasItem(item.ItemID);
		bool flag = HasRequiredItem(item);
		bool flag2 = HasEnoughFightSpirit(item);
		bool flag3 = IsDebut(item);
		if (num)
		{
			result = CatchphraseUnlockPanel.PanelState.Unlocked;
		}
		else if (!flag)
		{
			result = CatchphraseUnlockPanel.PanelState.RequiredItemNotUnlock;
		}
		else if (!flag2)
		{
			result = CatchphraseUnlockPanel.PanelState.LackOfFightSpirit;
		}
		else if (!flag3)
		{
			result = CatchphraseUnlockPanel.PanelState.NotDebut;
		}
		return result;
	}
}
