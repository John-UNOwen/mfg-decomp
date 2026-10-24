using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.Types.Customize.CustomizeItem;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class StampSelectWindow : MonoBehaviour
{
	[Serializable]
	private class IconListView
	{
		[SerializeField]
		private GameObject _iconPrefab;

		[SerializeField]
		private GameObject _instanceParent;

		[SerializeField]
		private ScrollRect _scrollRect;

		private List<ItemID> _iconItemIDs = new List<ItemID>();

		private List<StampIcon> _icons = new List<StampIcon>();

		private bool _isNeedReflesh;

		private Subject<ItemID> m_onSelected = new Subject<ItemID>();

		public IObservable<ItemID> OnSelected => m_onSelected.AsObservable();

		public void Init(List<ItemID> iconItemIDs)
		{
			_iconItemIDs = iconItemIDs;
			_isNeedReflesh = true;
		}

		public void Reflesh(bool isForce = false)
		{
			if (_scrollRect != null)
			{
				_scrollRect.verticalNormalizedPosition = 1f;
			}
			if (!_isNeedReflesh && !isForce)
			{
				return;
			}
			_icons.Clear();
			foreach (Transform item in _instanceParent.transform)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
			foreach (ItemID itemID in _iconItemIDs)
			{
				StampIcon component = UnityEngine.Object.Instantiate(_iconPrefab, _instanceParent.transform).GetComponent<StampIcon>();
				component.Reflesh(itemID).Forget();
				component.gameObject.GetComponent<Button>().OnClickAsObservable().Subscribe(delegate
				{
					m_onSelected.OnNext(itemID);
				})
					.AddTo(component.gameObject);
				_icons.Add(component);
			}
			_isNeedReflesh = false;
		}
	}

	[SerializeField]
	private IconListView _historyView = new IconListView();

	[SerializeField]
	private IconListView _tableView = new IconListView();

	[SerializeField]
	private ScrollView _scrollView;

	private bool _isOpen;

	private Subject<ItemID> m_onStampSelected = new Subject<ItemID>();

	private List<ItemID> _historyIonItemIDs = new List<ItemID>();

	private const int WindowWidth = 460;

	private const int HistoryCapacity = 12;

	private bool _isPlaying;

	private const int _stampIntervalSec = 2;

	public IObservable<ItemID> OnStampSelected => m_onStampSelected.AsObservable();

	public bool IsOpen => _isOpen;

	public void SetItem(List<ItemID> historyIonItemIDs, List<ItemID> tableIonItemIDs)
	{
		_historyIonItemIDs = historyIonItemIDs;
		_historyView.Init(historyIonItemIDs);
		_tableView.Init(tableIonItemIDs);
		_historyView.OnSelected.Subscribe(async delegate(ItemID itemID)
		{
			if (!_isPlaying)
			{
				_isPlaying = true;
				ModifyHistory(itemID);
				m_onStampSelected.OnNext(itemID);
				await UniTask.Delay(TimeSpan.FromSeconds(2.0));
				_isPlaying = false;
			}
		}).AddTo(this);
		_tableView.OnSelected.Subscribe(async delegate(ItemID itemID)
		{
			if (!_isPlaying)
			{
				_isPlaying = true;
				ModifyHistory(itemID);
				m_onStampSelected.OnNext(itemID);
				await UniTask.Delay(TimeSpan.FromSeconds(2.0));
				_isPlaying = false;
			}
		}).AddTo(this);
	}

	private void ModifyHistory(ItemID itemID)
	{
		_historyIonItemIDs.RemoveAll((ItemID x) => x == itemID);
		_historyIonItemIDs.Insert(0, itemID);
		if (_historyIonItemIDs.Count > 12)
		{
			_historyIonItemIDs.RemoveAt(_historyIonItemIDs.Count - 1);
		}
	}

	public async UniTask OpenClose(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (!_isOpen)
		{
			await Open(cancellationToken);
		}
		else
		{
			await Close(cancellationToken);
		}
	}

	public async UniTask Open(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (!_isOpen)
		{
			Reflesh();
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			RectTransform component = GetComponent<RectTransform>();
			await component.DOAnchorPos(new Vector3(720f, component.anchoredPosition.y, 0f), 0.1f).WithCancellation(cancellationToken);
			_isOpen = true;
		}
	}

	public async UniTask Close(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (_isOpen)
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			RectTransform component = GetComponent<RectTransform>();
			await component.DOAnchorPos(new Vector3(1186f, component.anchoredPosition.y, 0f), 0.1f).WithCancellation(cancellationToken);
			_isOpen = false;
		}
	}

	public void Reflesh()
	{
		_historyView.Reflesh(isForce: true);
		_tableView.Reflesh();
	}
}
