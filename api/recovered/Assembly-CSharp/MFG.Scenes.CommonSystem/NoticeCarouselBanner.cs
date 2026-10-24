using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.Assets;
using MFG.GameData;
using MFG.Types.Notice;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MFG.Scenes.CommonSystem;

[Serializable]
public class NoticeCarouselBanner : CarouselBanner<NoticeInfo>
{
	[SerializeField]
	private HomeNoticeWindow _noticeWindow;

	[HideInInspector]
	public TenshiGowanWindow _tenshiGowanWindow;

	private List<NoticeInfo> _noticeInfos = new List<NoticeInfo>();

	private CancellationToken m_cancellationToken;

	private CanvasGroup _canvasGroup;

	private void Start()
	{
		m_cancellationToken = this.GetCancellationTokenOnDestroy();
	}

	public override async UniTask Init(List<NoticeInfo> bannerInfos)
	{
		if (bannerInfos.Count <= 0)
		{
			return;
		}
		_canvasGroup = GetComponent<CanvasGroup>();
		if (_canvasGroup != null)
		{
			_canvasGroup.alpha = 0f;
			_canvasGroup.blocksRaycasts = false;
		}
		_noticeInfos.Clear();
		m_bannerInfos = bannerInfos;
		await Banner_Create(m_bannerInfos);
		m_isIndicatorActive = bannerInfos.Count >= 2;
		m_isLeftRightButtonActive = m_isIndicatorActive;
		Indicator_Create(m_bannerInfos);
		m_leftButton?.gameObject.SetActive(m_isLeftRightButtonActive);
		(from _ in m_leftButton?.OnClickAsObservable()
			where TouchEnable
			select _).Subscribe(async delegate
		{
			if (!m_isMoving)
			{
				await MoveRight();
			}
		}).AddTo(this);
		m_rightButton?.gameObject.SetActive(m_isLeftRightButtonActive);
		(from _ in m_rightButton?.OnClickAsObservable()
			where TouchEnable
			select _).Subscribe(async delegate
		{
			if (!m_isMoving)
			{
				await MoveLeft();
			}
		}).AddTo(this);
		m_timeOfLastMove = Time.time;
		if (_canvasGroup != null)
		{
			_canvasGroup.DOFade(1f, 0.2f).ToUniTask().Forget();
			_canvasGroup.blocksRaycasts = true;
		}
		m_isInitialized = true;
	}

	private async UniTask Banner_Create(List<NoticeInfo> bannerInfos)
	{
		foreach (Transform item in m_scrollRect.content.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		foreach (NoticeInfo info in bannerInfos)
		{
			string assetPath = info.AddressableAssetPath;
			if (!string.IsNullOrEmpty(assetPath) && await AssetLoader.Exists(assetPath))
			{
				IDisposableAsset<GameObject> disposableAsset = await AssetLoader.LoadAssetAsync<GameObject>(assetPath);
				disposableAsset.AddTo(this);
				Debug.Log(disposableAsset.Asset.transform.name);
				_noticeInfos.Add(info);
				m_bannerPrefabs.Add(disposableAsset.Asset);
			}
		}
		if (m_bannerPrefabs.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < CalcListSize(); i++)
		{
			GameObject original = m_bannerPrefabs[GetIndexFromListPos(i)];
			NoticeInfo noticeP = _noticeInfos[GetIndexFromListPos(i)];
			GameObject gameObject = UnityEngine.Object.Instantiate(original, m_scrollRect.content.transform);
			(gameObject.transform as RectTransform).anchoredPosition = GetBannerPos(i);
			(from _ in gameObject.GetComponent<Button>().OnClickAsObservable()
				where TouchEnable
				select _).Subscribe(async delegate
			{
				await BannerClick(noticeP);
			}).AddTo(this);
			SetEndDateText(gameObject, noticeP.IEndDateString);
			m_bannerInstances.Add(gameObject);
		}
		m_bannerInstances[GetListCenterIndex()].GetComponent<Animator>()?.SetTrigger("In");
	}

	private void SetEndDateText(GameObject ins, string endStr)
	{
		Transform transform = ((ins.transform.Find("GachaText") != null) ? ins.transform.Find("GachaText") : null);
		transform = ((transform == null && ins.transform.Find("BingoText") != null) ? ins.transform.Find("BingoText") : transform);
		transform = ((transform == null && ins.transform.Find("Text") != null) ? ins.transform.Find("Text") : transform);
		if (transform != null)
		{
			transform.GetComponent<TextMeshProUGUI>().text = endStr;
		}
	}

	protected override async UniTask MoveLeft(bool isAutoMove = false)
	{
		try
		{
			if (!m_isMoving && m_bannerInstances.Count > 0)
			{
				m_isMoving = true;
				m_leftButton.enabled = false;
				m_rightButton.enabled = false;
				m_bannerInstances[GetListCenterIndex()].GetComponent<Animator>()?.SetTrigger("Out");
				m_indexOfCenter++;
				m_indexOfCenter = NormalizeIndex(m_indexOfCenter);
				m_onBannerChangeBegin.OnNext((m_bannerInfos[m_indexOfCenter], isAutoMove));
				GameObject original = m_bannerPrefabs[GetIndexFromListPos(CalcListSize() - 1)];
				NoticeInfo noticeP = _noticeInfos[GetIndexFromListPos(CalcListSize() - 1)];
				GameObject gameObject = UnityEngine.Object.Instantiate(original, m_scrollRect.content.transform, worldPositionStays: false);
				(gameObject.transform as RectTransform).anchoredPosition = GetBannerPos(CalcListSize());
				(from _ in gameObject.GetComponent<Button>().OnClickAsObservable()
					where TouchEnable
					select _).Subscribe(async delegate
				{
					await BannerClick(noticeP);
				}).AddTo(this);
				SetEndDateText(gameObject, noticeP.IEndDateString);
				m_bannerInstances.Add(gameObject);
				await m_scrollRect.content.DOAnchorPos(new Vector2(-ScrollWidth, 0f), Dulation).SetRelative(isRelative: true).SetEase(Ease.OutQuad)
					.ToAwaiter();
				m_cancellationToken.ThrowIfCancellationRequested();
				NormalizePosition(-ScrollWidth);
				GameObject obj = m_bannerInstances[0];
				m_bannerInstances.RemoveAt(0);
				UnityEngine.Object.Destroy(obj);
				Indicator_Reflesh();
				m_bannerInstances[GetListCenterIndex()].GetComponent<Animator>()?.SetTrigger("In");
				m_onBannerChangeEnd.OnNext((m_bannerInfos[m_indexOfCenter], isAutoMove));
				m_leftButton.enabled = true;
				m_rightButton.enabled = true;
				m_timeOfLastMove = Time.time;
				m_isMoving = false;
			}
		}
		catch (OperationCanceledException)
		{
			Debug.Log("NoticeCarouseBanner MoveLeft Canceled");
		}
	}

	protected override async UniTask MoveRight(bool isAutoMove = false)
	{
		try
		{
			if (!m_isMoving)
			{
				m_isMoving = true;
				m_leftButton.enabled = false;
				m_rightButton.enabled = false;
				m_bannerInstances[GetListCenterIndex()].GetComponent<Animator>()?.SetTrigger("Out");
				m_indexOfCenter--;
				m_indexOfCenter = NormalizeIndex(m_indexOfCenter);
				m_onBannerChangeBegin.OnNext((m_bannerInfos[m_indexOfCenter], isAutoMove));
				GameObject original = m_bannerPrefabs[GetIndexFromListPos(0)];
				NoticeInfo noticeP = _noticeInfos[GetIndexFromListPos(0)];
				GameObject gameObject = UnityEngine.Object.Instantiate(original, m_scrollRect.content.transform, worldPositionStays: false);
				(gameObject.transform as RectTransform).anchoredPosition = GetBannerPos(-1);
				(from _ in gameObject.GetComponent<Button>().OnClickAsObservable()
					where TouchEnable
					select _).Subscribe(async delegate
				{
					await BannerClick(noticeP);
				}).AddTo(this);
				SetEndDateText(gameObject, noticeP.IEndDateString);
				m_bannerInstances.Insert(0, gameObject);
				await m_scrollRect.content.DOAnchorPos(new Vector2(ScrollWidth, 0f), Dulation).SetRelative(isRelative: true).SetEase(Ease.OutQuad)
					.ToAwaiter();
				m_cancellationToken.ThrowIfCancellationRequested();
				NormalizePosition(ScrollWidth);
				GameObject obj = m_bannerInstances[m_bannerInstances.Count - 1];
				m_bannerInstances.RemoveAt(m_bannerInstances.Count - 1);
				UnityEngine.Object.Destroy(obj);
				Indicator_Reflesh();
				m_bannerInstances[GetListCenterIndex()].GetComponent<Animator>()?.SetTrigger("In");
				m_onBannerChangeEnd.OnNext((m_bannerInfos[m_indexOfCenter], isAutoMove));
				m_leftButton.enabled = true;
				m_rightButton.enabled = true;
				m_timeOfLastMove = Time.time;
				m_isMoving = false;
			}
		}
		catch (OperationCanceledException)
		{
			Debug.Log("NoticeCarouseBanner MoveRight Canceled");
		}
	}

	public async UniTask BannerClick(NoticeInfo info)
	{
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		switch (info.Category)
		{
		case Category.Update:
		case Category.Failure:
		case Category.Information:
		case Category.Gacha:
		case Category.Campaign:
			await _noticeWindow.BannerRefleshNoticeDetailOpen(info);
			break;
		case Category.Event:
			if (info.HomeBannerName.Equals("BtGowanBanner"))
			{
				if (_tenshiGowanWindow != null)
				{
					_tenshiGowanWindow.OpenWindow();
				}
			}
			else
			{
				await _noticeWindow.BannerRefleshNoticeDetailOpen(info);
			}
			break;
		case Category.GachaBanner:
			break;
		}
	}
}
