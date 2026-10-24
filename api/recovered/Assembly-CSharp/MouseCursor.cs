using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseCursor : MonoBehaviour
{
	[SerializeField]
	private GameObject m_cursorPrefab;

	[SerializeField]
	private int m_maxEffectNum = 5;

	[SerializeField]
	private Canvas m_canvas;

	private List<GameObject> m_effectPool = new List<GameObject>();

	private int m_index;

	private bool m_isEnable;

	public void SetEnable(bool enable)
	{
		m_isEnable = enable && GameUtility.IsCloudStreaming;
	}

	private void Start()
	{
		m_isEnable = GameUtility.IsCloudStreaming && GameDataManager.PlayerConfig.IsCursorAnimAllow;
		for (int i = 0; i < m_maxEffectNum; i++)
		{
			GameObject gameObject = Object.Instantiate(m_cursorPrefab, base.transform);
			gameObject.SetActive(value: false);
			m_effectPool.Add(gameObject);
		}
	}

	private void Update()
	{
		BaseInput customInput = ResidentFlow.CustomInput;
		if (!(customInput == null) && m_isEnable && GameDataManager.PlayerConfig.IsCursorAnimAllow)
		{
			if (!ResidentFlow.GetInstance().GetTouchEffect().IsHold() && !customInput.GetMouseButtonUp(0))
			{
				MakeEffect(customInput.mousePosition, m_canvas.transform as RectTransform);
			}
			else
			{
				ClearEffect();
			}
		}
	}

	private void MakeEffect(Vector2 mousePosition, RectTransform rectTransform)
	{
		GameObject obj = m_effectPool[m_index];
		obj.SetActive(value: false);
		obj.transform.localPosition = GetEffectPos(mousePosition, rectTransform);
		obj.SetActive(value: true);
		obj.GetComponent<Animator>().SetTrigger("Release");
		m_index = (m_index + 1) % m_maxEffectNum;
	}

	private Vector2 GetEffectPos(Vector2 mousePosition, RectTransform rectTransform)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mousePosition, m_canvas.worldCamera, out var localPoint);
		return localPoint;
	}

	private void ClearEffect()
	{
		try
		{
			foreach (GameObject item in m_effectPool)
			{
				item.SetActive(value: false);
			}
		}
		catch
		{
		}
	}
}
