using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class TenshiGowanWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject windowObj;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private TextMeshProUGUI wingNumText;

	[SerializeField]
	private TextMeshProUGUI dateText;

	public bool isOpen;

	private int wingNum;

	private async void Start()
	{
		wingNumText.text = GameDataManager.TenshiGowanEvent.TenshiWingNum.ToString();
		wingNum = GameDataManager.TenshiGowanEvent.TenshiWingNum;
		GameEventData gameEventData = GameDataManager.System.GameControlInfo.EventStatus.GetGameEventData(GameEventType.TenshiGowan);
		if (gameEventData != null)
		{
			dateText.text = gameEventData.End.ToString("yyyy年MM月dd日（ddd）HH：mmまで");
		}
		closeButton.OnClickAsObservable().Subscribe(delegate
		{
			CloseWindow().Forget();
		}).AddTo(this);
	}

	public void OpenWindow()
	{
		if (!isOpen)
		{
			isOpen = true;
			base.gameObject.SetActive(value: true);
			SoundManager.PlaySe("SFX_MJG_INFO1").Forget();
			windowObj.GetComponent<Animator>().SetTrigger("In");
		}
	}

	public async UniTask CloseWindow()
	{
		if (isOpen)
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			windowObj.GetComponent<Animator>().SetTrigger("Out");
			isOpen = false;
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		}
	}

	private void Update()
	{
	}
}
