using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.GameRequest;
using MFG.Types.Notice;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class HomeNoticeWindow : MonoBehaviour
{
	[SerializeField]
	private ScrollView _scrollView;

	[SerializeField]
	private GameObject _noticePrefab;

	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private Button _noticeDetailCloseButton;

	[SerializeField]
	private Button _noticeDetailBackButton;

	[SerializeField]
	private Button _noticeDetailCloseAllButton;

	[SerializeField]
	private InformationDetailWindow _informationDetailWindow;

	[SerializeField]
	private GameObject _noNoticeText;

	[SerializeField]
	private GameObject _window;

	[SerializeField]
	private GameObject _detailMask;

	private bool _isForcedDisp;

	private List<int> m_receivedNotices = new List<int>();

	private CancellationToken _cancellationToken;

	private List<NoticeInfo> m_noticeInfos = new List<NoticeInfo>();

	private bool _isCloseAllPushed;

	public bool _isOpen { get; private set; }

	public bool _isTimeup { get; private set; }

	private async void Start()
	{
		await UniTask.WaitUntil(() => ResidentFlow.IsInitialized);
		try
		{
			this.GetCancellationTokenOnDestroy();
			_closeButton.OnClickAsObservable().Subscribe(async delegate
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
				_isOpen = false;
			}).AddTo(this);
			_noticeDetailCloseAllButton.OnClickAsObservable().Subscribe(async delegate
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
				await _informationDetailWindow.Close();
				_isCloseAllPushed = true;
				foreach (NoticeInfo noticeInfo in m_noticeInfos)
				{
					noticeInfo.IsNew = false;
					m_receivedNotices.Add(noticeInfo.NoticeID);
				}
			}).AddTo(this);
			_noticeDetailCloseButton.OnClickAsObservable().Subscribe(async delegate
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
				await _informationDetailWindow.Close();
				if (!_isForcedDisp)
				{
					_isOpen = false;
					await Close();
				}
			}).AddTo(this);
			_noticeDetailBackButton.OnClickAsObservable().Subscribe(async delegate
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
				await CreateNoticeList();
				m_noticeInfos.Find((NoticeInfo a) => a.IsNew);
				_isForcedDisp = false;
				await _informationDetailWindow.Close();
			}).AddTo(this);
		}
		catch (OperationCanceledException ex)
		{
			Debug.Log("OperationCanceledException Catched. " + ex.Message);
		}
	}

	public async UniTask Play(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_isOpen)
		{
			_isOpen = true;
			_isForcedDisp = false;
			_isTimeup = false;
			base.gameObject.SetActive(value: true);
			GetComponent<Animator>().SetTrigger("In");
			base.transform.Find("Base").gameObject.SetActive(value: true);
			await CreateNoticeList();
			await UniTask.WaitWhile(() => _isOpen, PlayerLoopTiming.Update, cancellationToken);
			await Close();
		}
	}

	public async UniTask ForcePlay(CancellationToken cancellation = default(CancellationToken))
	{
		if (_isOpen)
		{
			return;
		}
		_cancellationToken = cancellation;
		_isTimeup = false;
		m_noticeInfos = (from a in GameDataManager.Notice.NoticeInfos
			where a.Category != Category.GachaBanner
			select a into x
			orderby x.NoticeID descending
			select x).Take(99).ToList();
		List<NoticeInfo> newNoticeInfos = m_noticeInfos.FindAll((NoticeInfo a) => a.IsNew);
		Debug.Log("newNoticeNum:" + newNoticeInfos?.Count);
		if (newNoticeInfos == null)
		{
			return;
		}
		_isOpen = true;
		_isForcedDisp = true;
		await CreateNoticeList();
		_detailMask.SetActive(value: true);
		base.gameObject.SetActive(value: true);
		GetComponent<Animator>().SetTrigger("In");
		base.transform.Find("Base").gameObject.SetActive(value: true);
		foreach (NoticeInfo item in newNoticeInfos)
		{
			if (item.IsForcedDisplayNotice())
			{
				Debug.Log("newNotice:" + item.NoticeID);
				await OpenDetailWindow(item, _isForcedDisp, cancellation);
			}
			if (_isTimeup)
			{
				return;
			}
			if (_isCloseAllPushed)
			{
				break;
			}
		}
		_isForcedDisp = false;
		await Close();
	}

	public async UniTask Close(bool isTimeup = false)
	{
		_isTimeup = isTimeup;
		GetComponent<Animator>().SetTrigger("Out");
		base.gameObject.SetActive(value: false);
		if (_informationDetailWindow._isOpen)
		{
			_informationDetailWindow.Close().Forget();
		}
		_detailMask.SetActive(value: false);
		if (m_receivedNotices.Count > 0)
		{
			await RequestApiBase<NoticeDone, NoticeDone.ResultData, List<int>>.Send(m_receivedNotices);
		}
		_isOpen = false;
	}

	private async UniTask CreateNoticeList()
	{
		_scrollView.ResetList();
		m_noticeInfos = (from a in GameDataManager.Notice.NoticeInfos
			where a.Category != Category.GachaBanner
			select a into x
			orderby x.NoticeID descending
			select x).Take(99).ToList();
		foreach (NoticeInfo notice in m_noticeInfos)
		{
			GameObject obj = _scrollView.Add(_noticePrefab);
			Transform trs = obj.transform.Find("Bt");
			obj.GetComponent<Button>().OnClickAsObservable().Subscribe(async delegate
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				await OpenDetailWindow(notice);
			})
				.AddTo(this);
			SetupNoticeBanner(notice, trs);
		}
		_noNoticeText.SetActive(m_noticeInfos.Count() == 0);
		_scrollView.ResetScroll();
	}

	public async UniTask BannerRefleshNoticeDetailOpen(NoticeInfo notice)
	{
		if (!_isOpen)
		{
			_isOpen = true;
			_isForcedDisp = false;
			base.gameObject.SetActive(value: true);
			GetComponent<Animator>().SetTrigger("In");
			base.transform.Find("Base").gameObject.SetActive(value: true);
			await CreateNoticeList();
			if (m_noticeInfos.Find((NoticeInfo a) => a == notice) != null)
			{
				await OpenDetailWindow(notice);
			}
			await UniTask.WaitWhile(() => _isOpen);
			await Close();
		}
	}

	public async UniTask OpenDetailWindow(NoticeInfo notice, bool isForced = false, CancellationToken cancellation = default(CancellationToken))
	{
		await _informationDetailWindow.OpenDetail(notice, isForced);
		if (notice.IsNew)
		{
			notice.IsNew = false;
			m_receivedNotices.Add(notice.NoticeID);
		}
		await UniTask.WaitWhile(() => _informationDetailWindow._isOpen && !_isTimeup, PlayerLoopTiming.Update, cancellation);
	}

	public void SetupNoticeBanner(NoticeInfo notice, Transform trs)
	{
		trs.Find("TextMain").GetComponent<TextMeshProUGUI>().text = notice.Title;
		trs.Find("New").gameObject.SetActive(notice.IsNew);
		foreach (Transform item in trs.Find("Type").transform)
		{
			item.gameObject.SetActive(value: false);
		}
		switch (notice.Category)
		{
		case Category.Update:
			trs.Find("Type").Find("Type1").gameObject.SetActive(value: true);
			break;
		case Category.Failure:
			trs.Find("Type").Find("Type2").gameObject.SetActive(value: true);
			break;
		case Category.Information:
			trs.Find("Type").Find("Type3").gameObject.SetActive(value: true);
			break;
		case Category.Event:
			trs.Find("Type").Find("Type4").gameObject.SetActive(value: true);
			break;
		case Category.Gacha:
			trs.Find("Type").Find("Type5").gameObject.SetActive(value: true);
			break;
		case Category.Campaign:
			trs.Find("Type").Find("Type6").gameObject.SetActive(value: true);
			break;
		}
		trs.Find("Date").GetComponent<TextMeshProUGUI>().text = notice.ReleaseDate;
		if (notice.UpdateDate != null)
		{
			trs.Find("Date2").GetComponent<TextMeshProUGUI>().text = "【更新】" + notice.UpdateDate;
		}
		else
		{
			trs.Find("Date2").gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		_ = ResidentFlow.IsInitialized;
	}
}
