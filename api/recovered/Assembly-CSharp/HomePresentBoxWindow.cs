using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.Types;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class HomePresentBoxWindow : MonoBehaviour
{
	[Serializable]
	private class TabButton
	{
		public List<Button> TabButtons = new List<Button>();
	}

	[SerializeField]
	private List<TabButton> _tabButtons = new List<TabButton>();

	[SerializeField]
	private List<GameObject> _tabObjects = new List<GameObject>();

	[SerializeField]
	private GameObject _presentBannerPrefab;

	[SerializeField]
	private ScrollView _boxScrollView;

	[SerializeField]
	private ScrollView _historyScrollView;

	[SerializeField]
	private Button _boxCloseButton;

	[SerializeField]
	private Button _boxBundleReceiveButton;

	[SerializeField]
	private Button _historyCloseButton;

	[SerializeField]
	private GameObject _getWindow;

	[SerializeField]
	private InfoWindow _infoWindow;

	[SerializeField]
	private GameObject _noBoxText;

	[SerializeField]
	private GameObject _noHistoryText;

	[SerializeField]
	private ItemPopupManager _itemPopUp;

	private CancellationToken _token;

	private List<int> m_receivedPresents = new List<int>();

	public bool _isOpen { get; private set; }

	private async void Start()
	{
		await UniTask.WaitUntil(() => ResidentFlow.IsInitialized);
		try
		{
			_token = this.GetCancellationTokenOnDestroy();
			_boxCloseButton.OnClickAsObservable().Subscribe(async delegate
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
				_isOpen = false;
			}).AddTo(this);
			_boxBundleReceiveButton.OnClickAsObservable().Subscribe(async delegate
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				List<PresentInfo> list = (from a in GameDataManager.PresentBox.PresentInfos
					where !a.IsReceived
					orderby a.PresentID descending
					select a).Take(99).ToList();
				int idx = 0;
				List<PresentInfo> getList = new List<PresentInfo>();
				string receiveDateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
				ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
				foreach (PresentInfo item in list)
				{
					item.IsReceived = true;
					m_receivedPresents.Add((int)item.PresentID);
					item.ReceivedDate = receiveDateTime;
					getList.Add(item);
					if (list.Count <= 10)
					{
						_boxBundleReceiveButton.interactable = false;
					}
					idx++;
					if (idx % 10 == 0 || idx == list.Count)
					{
						RefleshPresentList();
						await PresentGetList(getList);
						getList.Clear();
					}
				}
				ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
			}).AddTo(this);
			_tabButtons[0].TabButtons[1].OnClickAsObservable().Subscribe(async delegate
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				_tabObjects[0].SetActive(value: false);
				_tabObjects[1].SetActive(value: true);
			}).AddTo(this);
			_tabButtons[1].TabButtons[0].OnClickAsObservable().Subscribe(async delegate
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				_tabObjects[0].SetActive(value: true);
				_tabObjects[1].SetActive(value: false);
			}).AddTo(this);
			_historyCloseButton.OnClickAsObservable().Subscribe(async delegate
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
				_isOpen = false;
			}).AddTo(this);
		}
		catch (OperationCanceledException ex)
		{
			Debug.Log("OperationCanceledException Catched. " + ex.Message);
		}
	}

	public async UniTask Play()
	{
		if (!_isOpen)
		{
			_isOpen = true;
			base.gameObject.SetActive(value: true);
			_tabObjects[0].SetActive(value: true);
			_tabObjects[1].SetActive(value: false);
			base.gameObject.GetComponent<Animator>().SetTrigger("In");
			RefleshPresentList();
			await UniTask.WaitWhile(() => _isOpen);
			await Close();
		}
	}

	public async UniTask Close()
	{
		base.gameObject.GetComponent<Animator>().SetTrigger("Out");
		base.gameObject.SetActive(value: false);
		await GameDataManager.PresentBox.SaveTask(m_receivedPresents);
		_isOpen = false;
	}

	private async UniTask SetPresentBanner(PresentInfo info, GameObject obj)
	{
		Transform trs = obj.transform.Find("Bg");
		trs.Find("TextMain").GetComponent<TextMeshProUGUI>().text = SaniTize.SanitizedString(GetPresentNameWithCount(info));
		trs.Find("TextDetail").GetComponent<TextMeshProUGUI>().text = SaniTize.SanitizedString(info.Text);
		if (await AssetLoader.Exists(ItemUtility.ItemIconGraphicPath(info.Content)))
		{
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(ItemUtility.ItemIconGraphicPath(info.Content));
			disposableAsset.AddTo(this);
			trs.Find("IconImage").Find("Icon").GetComponent<Image>()
				.sprite = disposableAsset.Asset;
		}
		trs.Find("IconImage").GetComponent<Button>().OnClickAsObservable()
			.Subscribe(async delegate
			{
				RectTransform component = trs.Find("IconImage").GetComponent<RectTransform>();
				await _itemPopUp.Open(info.Content, component.position, component.sizeDelta);
			})
			.AddTo(this);
		if (info.IsReceived)
		{
			trs.Find("TextDate").Find("Limit").gameObject.SetActive(value: false);
			trs.Find("TextDate").Find("Receive").gameObject.SetActive(value: true);
			trs.Find("Date").GetComponent<TextMeshProUGUI>().text = info.ReceivedDate;
		}
		else
		{
			bool flag = string.IsNullOrEmpty(info.LimitDate);
			trs.Find("TextDate").Find("Limit").gameObject.SetActive(!flag);
			trs.Find("TextDate").Find("Receive").gameObject.SetActive(value: false);
			trs.Find("Date").gameObject.SetActive(!flag);
			if (!flag)
			{
				trs.Find("Date").GetComponent<TextMeshProUGUI>().text = info.LimitDate;
			}
		}
		if (info.IsReceived)
		{
			trs.Find("BtGet").gameObject.SetActive(value: false);
			return;
		}
		trs.Find("BtGet").gameObject.SetActive(value: true);
		trs.Find("BtGet").GetComponent<Button>().OnClickAsObservable()
			.Subscribe(async delegate
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				info.IsReceived = true;
				m_receivedPresents.Add((int)info.PresentID);
				info.ReceivedDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
				RefleshPresentList();
				List<PresentInfo> getList = new List<PresentInfo> { info };
				await PresentGetList(getList);
			})
			.AddTo(this);
	}

	private void RefleshPresentList()
	{
		List<PresentInfo> list = (from a in GameDataManager.PresentBox.PresentInfos
			where !a.IsReceived
			orderby a.PresentID descending
			select a).Take(99).ToList();
		_boxScrollView.ResetList();
		foreach (PresentInfo item in list)
		{
			GameObject obj = _boxScrollView.Add(_presentBannerPrefab);
			SetPresentBanner(item, obj).Forget();
		}
		_noBoxText.SetActive(list.Count() == 0);
		if (list.Count == 0)
		{
			_boxBundleReceiveButton.interactable = false;
		}
		list = (from a in GameDataManager.PresentBox.PresentInfos
			where a.IsReceived && !a.IsHideHistory
			orderby a.PresentID descending
			orderby a.ReceivedDate descending
			select a).Take(99).ToList();
		_historyScrollView.ResetList();
		foreach (PresentInfo item2 in list)
		{
			GameObject obj2 = _historyScrollView.Add(_presentBannerPrefab);
			SetPresentBanner(item2, obj2).Forget();
		}
		_noHistoryText.SetActive(list.Count() == 0);
	}

	private async UniTask PresentGetList(List<PresentInfo> getList)
	{
		_infoWindow.ResetItem();
		int idx = 0;
		foreach (PresentInfo present in getList)
		{
			ItemUtility.AddItem(present.Content, present.Count, CharaType.Chara01, present.LimitDate);
			if (await AssetLoader.Exists(ItemUtility.ItemIconGraphicPath(present.Content)))
			{
				IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(ItemUtility.ItemIconGraphicPath(present.Content));
				disposableAsset.AddTo(this);
				string text = SaniTize.SanitizedString(GetPresentNameWithCount(present));
				await _infoWindow.AddItem(text, disposableAsset.Asset, SaniTize.SanitizedString(GetPresentName(present)), present.Content);
			}
			idx++;
			if (idx >= 10)
			{
				break;
			}
		}
		if (getList.Count > 0)
		{
			await _infoWindow.Open();
			if (_infoWindow.m_isOpen)
			{
				await _infoWindow.Close();
			}
		}
	}

	private string GetPresentNameWithCount(PresentInfo info)
	{
		string content = info.Content;
		if (ItemUtility.IsPlayerItem(content) || ItemUtility.IsVoteItem(content) || ItemUtility.IsEventTaku(content))
		{
			return GetPresentName(info) + "×" + info.Count;
		}
		return GetPresentName(info);
	}

	private string GetPresentName(PresentInfo info)
	{
		string text = ItemUtility.ItemName(info.Content);
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return info.Title;
	}
}
