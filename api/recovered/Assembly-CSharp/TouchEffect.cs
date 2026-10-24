using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchEffect : MonoBehaviour
{
	[SerializeField]
	private GameObject m_effectPrefab;

	[SerializeField]
	private int m_maxEffectNum = 5;

	[SerializeField]
	private Canvas m_canvas;

	private List<GameObject> m_effectPool = new List<GameObject>();

	private int m_index;

	private bool m_isEnable = true;

	[SerializeField]
	private float _thresholdTime = 0.5f;

	private float _touchDuration;

	private GameObject _latestHoldEffect;

	public bool IsHold()
	{
		if (_latestHoldEffect != null)
		{
			return _latestHoldEffect.activeSelf;
		}
		return false;
	}

	public void SetEnable(bool enable)
	{
		m_isEnable = enable;
	}

	private void Start()
	{
		for (int i = 0; i < m_maxEffectNum; i++)
		{
			GameObject gameObject = Object.Instantiate(m_effectPrefab, base.transform);
			gameObject.SetActive(value: false);
			m_effectPool.Add(gameObject);
		}
	}

	private void Update()
	{
		BaseInput customInput = ResidentFlow.CustomInput;
		if (customInput == null || !m_isEnable)
		{
			HoldEffect_Clear();
			return;
		}
		if (customInput.GetMouseButtonUp(0))
		{
			HoldEffect_Clear();
			MakeEffect(customInput.mousePosition, m_canvas.transform as RectTransform, isHold: false);
		}
		if (customInput.GetMouseButton(0))
		{
			_touchDuration += Time.deltaTime;
		}
		else
		{
			_touchDuration = 0f;
		}
		if (_latestHoldEffect == null && _touchDuration >= _thresholdTime)
		{
			MakeEffect(customInput.mousePosition, m_canvas.transform as RectTransform, isHold: true);
		}
		if (_latestHoldEffect != null)
		{
			_latestHoldEffect.transform.localPosition = GetEffectPos(customInput.mousePosition, m_canvas.transform as RectTransform);
		}
	}

	private void MakeEffect(Vector2 mousePosition, RectTransform rectTransform, bool isHold)
	{
		GameObject gameObject = m_effectPool[m_index];
		gameObject.SetActive(value: false);
		gameObject.transform.localPosition = GetEffectPos(mousePosition, rectTransform);
		gameObject.SetActive(value: true);
		gameObject.GetComponent<Animator>().SetTrigger(isHold ? "Hold" : "Release");
		if (isHold)
		{
			_latestHoldEffect = gameObject;
		}
		m_index = (m_index + 1) % m_maxEffectNum;
	}

	private Vector2 GetEffectPos(Vector2 mousePosition, RectTransform rectTransform)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mousePosition, m_canvas.worldCamera, out var localPoint);
		return localPoint;
	}

	private void HoldEffect_Clear()
	{
		if (_latestHoldEffect != null)
		{
			_latestHoldEffect.SetActive(value: false);
			_latestHoldEffect = null;
		}
		_touchDuration = 0f;
	}
}
