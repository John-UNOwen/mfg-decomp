using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FreeConsumeButton : MonoBehaviour
{
	[SerializeField]
	private Button m_button;

	private UnityAction m_onClick;

	private UnityAction<bool, ConsumeWindow.EndType> m_onEnd;

	private bool m_isDecide;

	private float m_agingCount;

	public void SetInteractable(bool interactable)
	{
		m_button.interactable = interactable;
	}

	public void Set(UnityAction onClick, UnityAction<bool, ConsumeWindow.EndType> onEnd)
	{
		m_onClick = onClick;
		m_onEnd = onEnd;
		m_button.onClick.AddListener(OnClick);
		m_agingCount = 0f;
	}

	private void Update()
	{
		_ = m_isDecide;
	}

	private void OnClick()
	{
		if (!m_isDecide)
		{
			m_isDecide = true;
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			m_onClick?.Invoke();
			m_onEnd?.Invoke(arg0: true, ConsumeWindow.EndType.Success);
		}
	}
}
