using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ScrollView : MonoBehaviour, IDisposable
{
	[SerializeField]
	private GameObject m_content;

	[SerializeField]
	private Button m_listUpButton;

	[SerializeField]
	private Button m_listDownButton;

	[SerializeField]
	private int SCROLL_SIZE;

	[SerializeField]
	private int AdjustY;

	private int m_scroll;

	private Dictionary<int, GameObject> m_objectList = new Dictionary<int, GameObject>();

	public Dictionary<int, GameObject> ObjectList => m_objectList;

	public void Dispose()
	{
	}

	private void OnDestroy()
	{
		Dispose();
	}

	private void Start()
	{
		m_listUpButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			m_scroll = (int)m_content.GetComponent<RectTransform>().anchoredPosition.y;
			m_scroll -= SCROLL_SIZE;
			m_scroll = ((m_scroll >= 0) ? m_scroll : 0);
			m_content.GetComponent<RectTransform>().DOAnchorPosY(m_scroll, 0.2f);
		}).AddTo(this);
		m_listDownButton.OnClickAsObservable().Subscribe(delegate
		{
			float y = GetComponent<RectTransform>().sizeDelta.y;
			float y2 = m_content.GetComponent<RectTransform>().sizeDelta.y;
			int num = 0;
			if (m_content.GetComponent<VerticalLayoutGroup>() != null)
			{
				num = m_content.GetComponent<VerticalLayoutGroup>().padding.top;
			}
			int num2 = (int)(y2 - y + (float)num + (float)AdjustY);
			num2 = ((num2 >= 0) ? num2 : 0);
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			m_scroll = (int)m_content.GetComponent<RectTransform>().anchoredPosition.y;
			m_scroll += SCROLL_SIZE;
			m_scroll = ((m_scroll >= num2) ? num2 : m_scroll);
			m_content.GetComponent<RectTransform>().DOAnchorPosY(m_scroll, 0.2f);
		}).AddTo(this);
	}

	public void ResetScroll()
	{
		m_scroll = 0;
		m_content.GetComponent<RectTransform>().DOAnchorPosY(m_scroll, 0f);
	}

	public GameObject Add(GameObject prefab)
	{
		return UnityEngine.Object.Instantiate(prefab, m_content.transform);
	}

	public GameObject Add(GameObject prefab, int key)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(prefab, m_content.transform);
		m_objectList.Add(key, gameObject);
		return gameObject;
	}

	public GameObject GetObject(int key)
	{
		return m_objectList[key];
	}

	public void ResetList()
	{
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
