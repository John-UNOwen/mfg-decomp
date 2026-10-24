using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewHorizontal : MonoBehaviour, IDisposable
{
	[SerializeField]
	private GameObject m_content;

	[SerializeField]
	private Button m_listRightButton;

	[SerializeField]
	private Button m_listLeftButton;

	[SerializeField]
	private int SCROLL_SIZE;

	[SerializeField]
	private int AdjustX;

	private int m_scroll;

	private Dictionary<int, GameObject> m_objectList = new Dictionary<int, GameObject>();

	private List<GameObject> m_objects = new List<GameObject>();

	public void Dispose()
	{
	}

	private void OnDestroy()
	{
		Dispose();
	}

	private void Start()
	{
		m_listLeftButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			m_scroll = (int)m_content.GetComponent<RectTransform>().anchoredPosition.x;
			m_scroll += SCROLL_SIZE;
			if (m_scroll >= 0)
			{
				m_scroll = 0;
			}
			m_content.GetComponent<RectTransform>().DOAnchorPosX(m_scroll, 0.2f);
		}).AddTo(this);
		m_listRightButton.OnClickAsObservable().Subscribe(delegate
		{
			int num = CalcScollLimit();
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			m_scroll = (int)m_content.GetComponent<RectTransform>().anchoredPosition.x;
			m_scroll -= SCROLL_SIZE;
			if (m_scroll <= -num)
			{
				m_scroll = -num;
			}
			m_content.GetComponent<RectTransform>().DOAnchorPosX(m_scroll, 0.2f);
		}).AddTo(this);
		m_listRightButton.gameObject.SetActive(value: false);
		m_listLeftButton.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		int num = CalcScollLimit();
		if (num > 0)
		{
			m_listRightButton.gameObject.SetActive(value: true);
			m_listLeftButton.gameObject.SetActive(value: true);
			float x = m_content.GetComponent<RectTransform>().anchoredPosition.x;
			if (Mathf.Floor(x) <= (float)(-num))
			{
				m_listRightButton.interactable = false;
			}
			else
			{
				m_listRightButton.interactable = true;
			}
			if (x >= 0f)
			{
				m_listLeftButton.interactable = false;
			}
			else
			{
				m_listLeftButton.interactable = true;
			}
		}
	}

	public void ResetScroll()
	{
		m_scroll = 0;
		m_content.GetComponent<RectTransform>().DOAnchorPosX(m_scroll, 0f);
		m_listRightButton.gameObject.SetActive(value: false);
		m_listLeftButton.gameObject.SetActive(value: false);
	}

	private int CalcScollLimit()
	{
		float x = GetComponent<RectTransform>().sizeDelta.x;
		float x2 = m_content.GetComponent<RectTransform>().sizeDelta.x;
		int num = 0;
		if (m_content.GetComponent<HorizontalLayoutGroup>() != null)
		{
			num = m_content.GetComponent<HorizontalLayoutGroup>().padding.left;
		}
		int num2 = (int)(x2 - x + (float)num + (float)AdjustX);
		return (num2 >= 0) ? num2 : 0;
	}

	public GameObject Add(GameObject prefab)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(prefab, m_content.transform);
		m_objects.Add(gameObject);
		return gameObject;
	}

	public GameObject Add(GameObject prefab, int key)
	{
		GameObject gameObject = Add(prefab);
		m_objectList.Add(key, gameObject);
		return gameObject;
	}

	public List<GameObject> GetObjects()
	{
		return m_objects;
	}

	public GameObject GetObject(int key)
	{
		return m_objectList[key];
	}

	public void ResetList()
	{
		m_objects.Clear();
		m_objectList.Clear();
		foreach (Transform item in m_content.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		ResetScroll();
	}

	public GameObject GetListParent()
	{
		return m_content;
	}
}
