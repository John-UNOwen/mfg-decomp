using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MFG.Scenes.GachaSelect;

public class GachaInfoDialog : MonoBehaviour
{
	[SerializeField]
	private GameObject m_root;

	[SerializeField]
	private Button m_closeButton;

	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	private Button m_upButton;

	[SerializeField]
	private Button m_downButton;

	[SerializeField]
	private ScrollRect m_scrollRect;

	private void Start()
	{
		m_closeButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			m_root.SetActive(value: false);
		}).AddTo(this);
		m_upButton?.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			m_scrollRect.velocity = new Vector2(0f, -450f);
		}).AddTo(this);
		m_downButton?.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			m_scrollRect.velocity = new Vector2(0f, 450f);
		}).AddTo(this);
	}

	public void Open()
	{
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		m_root.SetActive(value: true);
	}
}
