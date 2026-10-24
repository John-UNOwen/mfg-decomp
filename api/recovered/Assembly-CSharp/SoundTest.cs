using Cysharp.Threading.Tasks;
using USnd;
using UnityEngine;
using UnityEngine.UI;

public class SoundTest : MonoBehaviour
{
	[SerializeField]
	private GameObject parent;

	[SerializeField]
	private ScrollView scrollView;

	[SerializeField]
	private GameObject soundButtonPrefab;

	[SerializeField]
	private Button exitButton;

	private async void Start()
	{
		exitButton.onClick.AddListener(delegate
		{
			ReturnHome();
		});
		await Init();
	}

	public async UniTask Play()
	{
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
		parent.SetActive(value: true);
	}

	private async UniTask Init()
	{
		string[] labelNameList = AudioManager.GetLabelNameList();
		if (labelNameList != null)
		{
			Debug.Log(labelNameList.Length);
			scrollView.ResetList();
			for (int i = 0; i < labelNameList.Length; i++)
			{
				Debug.Log(labelNameList[i]);
				scrollView.Add(soundButtonPrefab).GetComponent<SoundButton>().Init(labelNameList[i]);
			}
		}
	}

	private void Update()
	{
		if (ResidentFlow.IsInitialized && Input.GetKeyDown(KeyCode.Escape))
		{
			ReturnHome();
		}
	}

	private async void ReturnHome()
	{
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
		parent.SetActive(value: false);
	}
}
