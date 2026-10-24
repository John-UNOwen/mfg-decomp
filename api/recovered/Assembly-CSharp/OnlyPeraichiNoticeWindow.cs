using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using MFG.Types.OnlyPeraichiNotice;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class OnlyPeraichiNoticeWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject windowObj;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private TextMeshProUGUI dateText;

	[Header("任意のオブジェクト群")]
	[SerializeField]
	private List<GameObject> gameObjects;

	[NonSerialized]
	public bool isOpen;

	[NonSerialized]
	public Item Master;

	private async void Start()
	{
		closeButton.OnClickAsObservable().Subscribe(delegate
		{
			CloseWindow().Forget();
		}).AddTo(this);
	}

	public async UniTask OpenWindow()
	{
		if (!isOpen)
		{
			isOpen = true;
			if (dateText != null)
			{
				dateText.text = Master.DispDate();
			}
			await (UniTask)(GetType().GetMethod(Master.MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(this, null));
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
			base.gameObject.SetActive(value: false);
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		}
	}

	private async UniTask MFCSpSerialCodeOpen()
	{
		string serial = GameDataManager.ReceiveOnly.PlayerInfo.SerialCodeInfos?.Find((SerialCodeInfo x) => x.EventType == Master.EventType)?.SerialCode ?? "";
		gameObjects[1].transform.GetComponent<Button>().interactable = true;
		gameObjects[0].transform.GetComponent<TextMeshProUGUI>().text = "********";
		gameObjects[1].transform.GetComponent<Button>().OnClickAsObservable().Subscribe(delegate
		{
			gameObjects[1].transform.GetComponent<Button>().interactable = false;
			gameObjects[0].transform.GetComponent<TextMeshProUGUI>().text = serial;
		})
			.AddTo(this);
	}
}
