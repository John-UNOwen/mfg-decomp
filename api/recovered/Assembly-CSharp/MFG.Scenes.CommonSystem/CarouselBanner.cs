using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.Assets;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MFG.Scenes.CommonSystem;

[Serializable]
public class CarouselBanner<T> : MonoBehaviour where T : IAssetPath
{
	[SerializeField]
	protected ScrollRect m_scrollRect;

	[SerializeField]
	protected Button m_leftButton;

	[SerializeField]
	protected Button m_rightButton;

	[SerializeField]
	protected GameObject m_indicatorIconPrefab;

	[SerializeField]
	protected GameObject m_indicatorIconParent;

	[SerializeField]
	protected int ScrollWidth = 1040;

	[SerializeField]
	protected float Dulation = 0.35f;

	[SerializeField]
	protected bool AutoScrollEnable;

	[SerializeField]
	protected float AutoScrollInterval = 4f;

	protected List<GameObject> m_bannerPrefabs = new List<GameObject>();

	protected List<GameObject> m_bannerInstances = new List<GameObject>();

	protected List<GameObject> m_indicatorIconInstances = new List<GameObject>();

	protected List<T> m_bannerInfos;

	protected Subject<(T bannerInfo, bool isAutoMove)> m_onBannerChangeBegin = new Subject<(T, bool)>();

	protected Subject<(T bannerInfo, bool isAutoMove)> m_onBannerChangeEnd = new Subject<(T, bool)>();

	protected bool m_isIndicatorActive;

	protected bool m_isLeftRightButtonActive;

	protected int m_indexOfCenter;

	protected bool m_isMoving;

	protected float m_timeOfLastMove;

	protected bool m_isInitialized;

	private CancellationToken m_cancellationToken;

	public bool TouchEnable = true;

	public IObservable<(T bannerInfo, bool isAutoMove)> OnBannerChangeBegin => m_onBannerChangeBegin.AsObservable();

	public IObservable<(T bannerInfo, bool isAutoMove)> OnBannerChangeEnd => m_onBannerChangeEnd.AsObservable();

	public virtual void AutoScrollUnable()
	{
		AutoScrollEnable = false;
	}

	public virtual async UniTask Init(List<T> bannerInfos)
	{
		if (bannerInfos.Count <= 0)
		{
			return;
		}
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
		m_isInitialized = true;
	}

	private void Start()
	{
		m_cancellationToken = this.GetCancellationTokenOnDestroy();
	}

	private void OnDestroy()
	{
		m_bannerInstances.Clear();
	}

	private void Update()
	{
		if (m_isInitialized && AutoScrollEnable && !m_isMoving && Time.time - m_timeOfLastMove >= AutoScrollInterval)
		{
			MoveLeft(isAutoMove: true).Forget();
		}
	}

	protected virtual async UniTask MoveLeft(bool isAutoMove = false)
	{
		try
		{
			if (!m_isMoving)
			{
				m_isMoving = true;
				m_leftButton.enabled = false;
				m_rightButton.enabled = false;
				m_bannerInstances[GetListCenterIndex()].GetComponent<Animator>()?.SetTrigger("Out");
				m_indexOfCenter++;
				m_indexOfCenter = NormalizeIndex(m_indexOfCenter);
				m_onBannerChangeBegin.OnNext((m_bannerInfos[m_indexOfCenter], isAutoMove));
				GameObject gameObject = UnityEngine.Object.Instantiate(m_bannerPrefabs[GetIndexFromListPos(CalcListSize() - 1)], m_scrollRect.content.transform, worldPositionStays: false);
				(gameObject.transform as RectTransform).anchoredPosition = GetBannerPos(CalcListSize());
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
			Debug.Log("CarouseBanner MoveLeft Canceled");
		}
	}

	protected virtual async UniTask MoveRight(bool isAutoMove = false)
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
				GameObject gameObject = UnityEngine.Object.Instantiate(m_bannerPrefabs[GetIndexFromListPos(0)], m_scrollRect.content.transform, worldPositionStays: false);
				(gameObject.transform as RectTransform).anchoredPosition = GetBannerPos(-1);
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
			Debug.Log("CarouseBanner MoveRight Canceled");
		}
	}

	protected void NormalizePosition(int correctionValue)
	{
		foreach (GameObject bannerInstance in m_bannerInstances)
		{
			RectTransform rectTransform = bannerInstance.transform as RectTransform;
			if (rectTransform != null)
			{
				Vector2 anchoredPosition = rectTransform.anchoredPosition;
				anchoredPosition.x += correctionValue;
				rectTransform.anchoredPosition = anchoredPosition;
			}
		}
		Vector2 anchoredPosition2 = m_scrollRect.content.anchoredPosition;
		anchoredPosition2.x -= correctionValue;
		m_scrollRect.content.anchoredPosition = anchoredPosition2;
	}

	private int GetMaxVisibleCount()
	{
		if (m_bannerInfos.Count <= 1)
		{
			return m_bannerInfos.Count;
		}
		int num = Mathf.CeilToInt(m_scrollRect.viewport.rect.width / (float)ScrollWidth) + 1;
		if (num % 2 != 0)
		{
			return num;
		}
		return num + 1;
	}

	protected int CalcListSize()
	{
		if (m_bannerInfos.Count <= 1)
		{
			return m_bannerInfos.Count;
		}
		return GetMaxVisibleCount() + 2;
	}

	protected int GetListCenterIndex()
	{
		if (m_bannerInfos.Count <= 1)
		{
			return 0;
		}
		return GetMaxVisibleCount() / 2 + 1;
	}

	protected Vector2 GetBannerPos(int listPos)
	{
		return new Vector2(ScrollWidth * (listPos - GetListCenterIndex()), 0f);
	}

	protected int GetIndexFromListPos(int listPos)
	{
		return NormalizeIndex(m_indexOfCenter + (listPos - GetListCenterIndex()));
	}

	protected int NormalizeIndex(int index)
	{
		index %= m_bannerPrefabs.Count;
		if (index < 0)
		{
			index += m_bannerPrefabs.Count;
		}
		return index;
	}

	private async UniTask Banner_Create(List<T> bannerInfos)
	{
		foreach (Transform item in m_scrollRect.content.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		foreach (T info in bannerInfos)
		{
			string assetPath = info.AddressableAssetPath;
			if (string.IsNullOrEmpty(assetPath) || !(await AssetLoader.Exists(assetPath)))
			{
				continue;
			}
			IDisposableAsset<GameObject> disposableAsset = await AssetLoader.LoadAssetAsync<GameObject>(assetPath);
			disposableAsset.AddTo(this);
			if (!string.IsNullOrEmpty(info.IEndDateString))
			{
				try
				{
					disposableAsset.Asset.transform.Find("GachaText").GetComponent<TextMeshProUGUI>().text = info.IEndDateString;
				}
				catch
				{
				}
			}
			m_bannerPrefabs.Add(disposableAsset.Asset);
		}
		for (int i = 0; i < CalcListSize(); i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_bannerPrefabs[GetIndexFromListPos(i)], m_scrollRect.content.transform);
			(gameObject.transform as RectTransform).anchoredPosition = GetBannerPos(i);
			m_bannerInstances.Add(gameObject);
		}
		m_bannerInstances[GetListCenterIndex()].GetComponent<Animator>()?.SetTrigger("In");
	}

	protected void Indicator_Create(List<T> bannerInfos)
	{
		foreach (Transform item2 in m_indicatorIconParent.transform)
		{
			UnityEngine.Object.Destroy(item2.gameObject);
		}
		m_indicatorIconInstances.Clear();
		m_indicatorIconParent.SetActive(m_isIndicatorActive);
		if (m_isIndicatorActive)
		{
			for (int i = 0; i < m_bannerPrefabs.Count; i++)
			{
				GameObject item = UnityEngine.Object.Instantiate(m_indicatorIconPrefab, m_indicatorIconParent.transform);
				m_indicatorIconInstances.Add(item);
			}
		}
		Indicator_Reflesh();
	}

	protected void Indicator_Reflesh()
	{
		int num = 0;
		foreach (GameObject indicatorIconInstance in m_indicatorIconInstances)
		{
			Indicator_SetOnOff(indicatorIconInstance, num == m_indexOfCenter);
			num++;
		}
	}

	private void Indicator_SetOnOff(GameObject gameObject, bool isOn)
	{
		gameObject.transform.Find("On").gameObject.SetActive(isOn);
	}
}
