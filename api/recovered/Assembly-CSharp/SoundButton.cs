using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoundButton : MonoBehaviour
{
	[SerializeField]
	private Button button;

	private string label;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Init(string str)
	{
		label = str;
		button.onClick.AddListener(delegate
		{
			PlaySound();
		});
		button.transform.Find("Text (TMP)").gameObject.GetComponent<TextMeshProUGUI>().text = label;
	}

	private void PlaySound()
	{
		SoundManager.StopAll().Forget();
		SoundManager.PlayBgm(label).Forget();
	}
}
