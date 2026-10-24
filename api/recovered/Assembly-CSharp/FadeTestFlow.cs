using Cysharp.Threading.Tasks;
using UnityEngine;

public class FadeTestFlow : MonoBehaviour
{
	private bool m_isInitializeFinished;

	private async void Start()
	{
		await UniTask.WaitUntil(() => ResidentFlow.IsInitialized);
		m_isInitializeFinished = true;
	}

	private async void Update()
	{
		if (ResidentFlow.IsInitialized && m_isInitializeFinished && Input.GetKeyDown(KeyCode.Escape))
		{
			ResidentFlow.GetInstance().LoadScene("FadeTest");
		}
	}

	private void OnDestroy()
	{
	}
}
