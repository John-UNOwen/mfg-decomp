using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class DialogWindow : PopupWindow
{
	[SerializeField]
	private TextMeshProUGUI m_message;

	[SerializeField]
	private GameObject m_buttonRoot;

	[SerializeField]
	private GameObject m_srcButton;

	[SerializeField]
	private Transform m_buttonParent;

	[SerializeField]
	[Header("アセット")]
	private Sprite m_spriteCancelButtonImage;

	private bool m_isSkip;

	private bool m_forceExit;

	public bool IsSelected { get; private set; }

	public string SelectedButton { get; private set; } = "";


	public void SetMessage(string text)
	{
		m_message.text = text;
	}

	public void Open(float disp_time = 3f, bool enable_skip = true, UnityAction onEnd = null)
	{
		IsSelected = false;
		SelectedButton = "";
		StartCoroutine(CoOpen(disp_time, enable_skip, onEnd));
	}

	private IEnumerator CoOpen(float disp_time, bool enable_skip, UnityAction onEnd)
	{
		Animator animator = base.transform.Find("WindowYN").GetComponent<Animator>();
		animator.SetTrigger("In");
		float wait_time = 0.5f;
		yield return new WaitForSecondsRealtime(wait_time);
		disp_time -= wait_time;
		float time = Time.time;
		m_isSkip = false;
		yield return new WaitUntil(() => (disp_time > 0f && Time.time - time > disp_time) || (enable_skip && m_isSkip));
		animator.SetTrigger("Out");
		onEnd?.Invoke();
		Exit();
	}

	public void OnClick()
	{
		m_isSkip = true;
	}

	public void Exit()
	{
		base.gameObject.SetActive(value: false);
		Object.Destroy(base.gameObject);
	}

	public void ChangeFontSize(int font_size)
	{
		m_message.fontSize = font_size;
	}
}
